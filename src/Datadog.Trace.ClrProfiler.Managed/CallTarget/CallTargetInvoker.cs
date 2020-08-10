using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Datadog.Trace.ClrProfiler.Integrations.Testing;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.CallTarget
{
    internal delegate CallTargetState MethodBeginDelegate(CallerInfo callerInfo, object[] arguments);

    internal delegate object MethodEndDelegate(object returnValue, Exception exception, CallTargetState state);

    /// <summary>
    /// Call target integration helper
    /// </summary>
    public static class CallTargetInvoker
    {
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(CallTargetInvoker));
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle");
        private static readonly MethodInfo EnumToObjectMethodInfo = typeof(Enum).GetMethod("ToObject", new[] { typeof(Type), typeof(object) });
        private static readonly MethodInfo ConvertTypeMethodInfo = typeof(CallTargetInvoker).GetMethod("ConvertType", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo UnWrapReturnValueMethodInfo = typeof(CallTargetInvoker).GetMethod("UnWrapReturnValue", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, CallTargetIntegration> Integrations = new ConcurrentDictionary<RuntimeTypeHandle, CallTargetIntegration>();

        /// <summary>
        /// Call target static begin method helper
        /// </summary>
        /// <param name="instanceTypeHandle">Instance type handle</param>
        /// <param name="instance">Object instance</param>
        /// <param name="arguments">Arguments</param>
        /// <param name="wrapperTypeHandle">Wrapper type handle</param>
        /// <returns>CallTargetBeginReturn instance</returns>
        public static CallTargetState BeginMethod(RuntimeTypeHandle instanceTypeHandle, object instance, object[] arguments, RuntimeTypeHandle wrapperTypeHandle)
        {
            var integration = GetCallTargetIntegration(wrapperTypeHandle);
            if (integration.OnMethodBeginDelegate != null)
            {
                return integration.OnMethodBeginDelegate(new CallerInfo(instanceTypeHandle, instance), arguments);
            }

            return new CallTargetState(null);
        }

        /// <summary>
        /// Call target static end method helper
        /// </summary>
        /// <param name="returnValue">Original method return value</param>
        /// <param name="exception">Original method exception</param>
        /// <param name="state">State from the BeginMethod</param>
        /// <param name="wrapperTypeHandle">Wrapper type handle</param>
        /// <returns>Return value</returns>
        public static object EndMethod(object returnValue, Exception exception, CallTargetState state, RuntimeTypeHandle wrapperTypeHandle)
        {
            var integration = GetCallTargetIntegration(wrapperTypeHandle);

            if (integration.OnMethodEndDelegate != null)
            {
                returnValue = integration.OnMethodEndDelegate(returnValue, exception, state);
            }

            if (integration.OnMethodEndAsyncDelegate != null)
            {
                returnValue = returnValue = AsyncTool.AddContinuation(
                    returnValue,
                    exception,
                    new DuckTyping.ValueTuple<MethodEndDelegate, CallTargetState>(integration.OnMethodEndAsyncDelegate, state),
                    (rValue, ex, tuple) =>
                    {
                        try
                        {
                            return tuple.Item1(rValue, ex, tuple.Item2);
                        }
                        catch (Exception cEx)
                        {
                            LogException(cEx);
                        }

                        return rValue;
                    });
            }

            return returnValue;
        }

        /// <summary>
        /// Logs the exception to the CallTarget Logger
        /// </summary>
        /// <param name="ex">Exception instance</param>
        public static void LogException(Exception ex)
        {
            if (ex != null)
            {
                Log.SafeLogError(ex, ex.Message);
            }
        }

        private static CallTargetIntegration GetCallTargetIntegration(RuntimeTypeHandle wrapperTypeHandle)
        {
            return Integrations.GetOrAdd(wrapperTypeHandle, handle =>
            {
                return new CallTargetIntegration(
                    CreateMethodBeginDelegate(handle, "OnMethodBegin"),
                    CreateMethodEndDelegate(handle, "OnMethodEnd"),
                    CreateMethodEndDelegate(handle, "OnMethodEndAsync"));
            });
        }

        private static MethodBeginDelegate CreateMethodBeginDelegate(RuntimeTypeHandle wrapperTypeHandle, string methodName)
        {
            try
            {
                Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
                if (wrapperType is null)
                {
                    Log.Error("Wrapper type is null, the wrapperTypeHandle.Value is empty");
                    return null;
                }

                Log.Information($"Creating MethodBegin Delegate: {methodName} in {wrapperType.FullName}");
                MethodInfo onMethodBeginMethodInfo = wrapperType.GetMethod(methodName);
                if (onMethodBeginMethodInfo is null)
                {
                    Log.Warning($"Couldn't find the method: {methodName} in type: {wrapperType.FullName}");
                    return null;
                }

                if (onMethodBeginMethodInfo.ReturnType != typeof(CallTargetState))
                {
                    Log.Error($"The return type of the method: {methodName} in type: {wrapperType.FullName} is not {nameof(CallTargetState)}");
                    return null;
                }

                DynamicMethod callMethod = new DynamicMethod(
                    $"{onMethodBeginMethodInfo.DeclaringType.Name}.{onMethodBeginMethodInfo.Name}",
                    typeof(CallTargetState),
                    new Type[] { typeof(CallerInfo), typeof(object[]) },
                    onMethodBeginMethodInfo.Module);
                ILGenerator ilWriter = callMethod.GetILGenerator();

                ParameterInfo[] parameters = onMethodBeginMethodInfo.GetParameters();
                bool hasCallerInfo = (parameters.Length > 0 && parameters[0].ParameterType == typeof(CallerInfo));

                // Load caller info
                if (hasCallerInfo)
                {
                    ilWriter.Emit(OpCodes.Ldarg_0);
                }

                // Load arguments
                for (var i = (hasCallerInfo ? 1 : 0); i < parameters.Length; i++)
                {
                    Type pType = parameters[i].ParameterType;
                    Type rType = DuckTyping.Util.GetRootType(pType);
                    bool callEnum = false;
                    if (rType.IsEnum)
                    {
                        ilWriter.Emit(OpCodes.Ldtoken, rType);
                        ilWriter.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                        callEnum = true;
                    }

                    ilWriter.Emit(OpCodes.Ldarg_1);
                    ILHelpers.WriteIlIntValue(ilWriter, i - (hasCallerInfo ? 1 : 0));
                    ilWriter.Emit(OpCodes.Ldelem_Ref);

                    if (callEnum)
                    {
                        ilWriter.EmitCall(OpCodes.Call, EnumToObjectMethodInfo, null);
                    }
                    else
                    {
                        ilWriter.Emit(OpCodes.Ldtoken, rType);
                        ilWriter.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                        ilWriter.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null);
                    }

                    if (pType.IsValueType)
                    {
                        ilWriter.Emit(OpCodes.Unbox_Any, pType);
                    }
                    else if (pType != typeof(object))
                    {
                        ilWriter.Emit(OpCodes.Castclass, pType);
                    }
                }

                // Call method
                ilWriter.EmitCall(OpCodes.Call, onMethodBeginMethodInfo, null);
                ilWriter.Emit(OpCodes.Ret);

                return (MethodBeginDelegate)callMethod.CreateDelegate(typeof(MethodBeginDelegate));
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"Error while creating the MethodBegin delegate for {methodName}");
            }

            return null;
        }

        private static MethodEndDelegate CreateMethodEndDelegate(RuntimeTypeHandle wrapperTypeHandle, string methodName)
        {
            try
            {
                Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
                if (wrapperType is null)
                {
                    Log.Error("Wrapper type is null, the wrapperTypeHandle.Value is empty");
                    return null;
                }

                Log.Information($"Creating MethodEnd Delegate: {methodName} in {wrapperType.FullName}");
                MethodInfo onMethodEndMethodInfo = wrapperType.GetMethod(methodName);
                if (onMethodEndMethodInfo is null)
                {
                    Log.Warning($"Couldn't find the method: {methodName} in type: {wrapperType.FullName}");
                    return null;
                }

                ParameterInfo[] parameters = onMethodEndMethodInfo.GetParameters();
                if (parameters.Length != 3)
                {
                    Log.Error($"The method: {methodName} in type: {wrapperType.FullName} should have exactly 3 parameters.");
                    return null;
                }

                DynamicMethod callMethod = new DynamicMethod(
                    $"{onMethodEndMethodInfo.DeclaringType.Name}.{onMethodEndMethodInfo.Name}",
                    typeof(object),
                    new Type[] { typeof(object), typeof(Exception), typeof(CallTargetState) },
                    onMethodEndMethodInfo.Module);
                ILGenerator ilWriter = callMethod.GetILGenerator();

                // Load the return value
                Type pType = parameters[0].ParameterType;
                Type rType = DuckTyping.Util.GetRootType(pType);
                bool callEnum = false;
                if (rType.IsEnum)
                {
                    ilWriter.Emit(OpCodes.Ldtoken, rType);
                    ilWriter.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    callEnum = true;
                }

                ILHelpers.WriteLoadArgument(0, ilWriter, true);
                if (callEnum)
                {
                    ilWriter.EmitCall(OpCodes.Call, EnumToObjectMethodInfo, null);
                }
                else
                {
                    ilWriter.Emit(OpCodes.Ldtoken, rType);
                    ilWriter.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    ilWriter.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null);
                }

                if (pType.IsValueType)
                {
                    ilWriter.Emit(OpCodes.Unbox_Any, pType);
                }
                else if (pType != typeof(object))
                {
                    ilWriter.Emit(OpCodes.Castclass, pType);
                }

                // Load exception
                ILHelpers.WriteLoadArgument(1, ilWriter, true);

                // Load state
                ILHelpers.WriteLoadArgument(2, ilWriter, true);

                // Call method
                ilWriter.EmitCall(OpCodes.Call, onMethodEndMethodInfo, null);

                // Unwrap in case the return value is a duck type
                ilWriter.EmitCall(OpCodes.Call, UnWrapReturnValueMethodInfo, null);
                ilWriter.Emit(OpCodes.Ret);

                return (MethodEndDelegate)callMethod.CreateDelegate(typeof(MethodEndDelegate));
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"Error while creating the MethodEnd delegate for {methodName}");
            }

            return null;
        }

        private static object ConvertType(object value, Type conversionType)
        {
            if (value is null || conversionType == typeof(object))
            {
                return value;
            }

            Type valueType = value.GetType();
            if (valueType == conversionType || conversionType.IsAssignableFrom(valueType))
            {
                return value;
            }

            if (value is IConvertible)
            {
                return Convert.ChangeType(value, conversionType, CultureInfo.CurrentCulture);
            }

            return value.As(conversionType);
        }

        private static object UnWrapReturnValue(object returnValue)
        {
            if (returnValue is DuckType dType)
            {
                return dType.Instance;
            }

            return returnValue;
        }

        private readonly struct CallTargetIntegration
        {
            public readonly MethodBeginDelegate OnMethodBeginDelegate;
            public readonly MethodEndDelegate OnMethodEndDelegate;
            public readonly MethodEndDelegate OnMethodEndAsyncDelegate;

            public CallTargetIntegration(MethodBeginDelegate beginDelegate, MethodEndDelegate endDelegate, MethodEndDelegate endAsyncDelegate)
            {
                OnMethodBeginDelegate = beginDelegate;
                OnMethodEndDelegate = endDelegate;
                OnMethodEndAsyncDelegate = endAsyncDelegate;
            }
        }
    }
}
