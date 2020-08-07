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

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, Func<CallerInfo, object[], CallTargetState>> BeginMethodDelegates =
            new ConcurrentDictionary<RuntimeTypeHandle, Func<CallerInfo, object[], CallTargetState>>();

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, DuckTyping.ValueTuple<Func<object, Exception, CallTargetState, object>, Func<object, Exception, CallTargetState, object>>> EndMethodDelegates =
            new ConcurrentDictionary<RuntimeTypeHandle, DuckTyping.ValueTuple<Func<object, Exception, CallTargetState, object>, Func<object, Exception, CallTargetState, object>>>();

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
            try
            {
                CallerInfo callerInfo = new CallerInfo(instanceTypeHandle, instance);
                var onMethodBeginDelegate = BeginMethodDelegates.GetOrAdd(wrapperTypeHandle, handle => CreateMethodBeginDelegate(handle, "OnMethodBegin"));
                if (onMethodBeginDelegate != null)
                {
                    Log.Information("Calling OnMethodBegin delegate");
                    return onMethodBeginDelegate(callerInfo, arguments);
                }
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"BeginMethod error: {ex.Message}");
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
            try
            {
                var endMethodDelegates = EndMethodDelegates.GetOrAdd(wrapperTypeHandle, handle => new DuckTyping.ValueTuple<Func<object, Exception, CallTargetState, object>, Func<object, Exception, CallTargetState, object>>(
                    CreateMethodEndDelegate(handle, "OnMethodEnd"),
                    CreateMethodEndDelegate(handle, "OnMethodEndAsync")));

                if (endMethodDelegates.Item1 != null)
                {
                    Log.Information("Calling OnMethodEnd delegate");
                    returnValue = endMethodDelegates.Item1(returnValue, exception, state);
                }

                if (endMethodDelegates.Item2 != null)
                {
                    Log.Information("Calling OnMethodEndAsync delegate");
                    returnValue = returnValue = AsyncTool.AddContinuation(
                        returnValue,
                        exception,
                        new DuckTyping.ValueTuple<Func<object, Exception, CallTargetState, object>, CallTargetState>(endMethodDelegates.Item2, state),
                        (rValue, ex, tuple) => tuple.Item1(rValue, ex, tuple.Item2));
                }

                /*
                Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
                MethodInfo onMethodEndMethodInfo = wrapperType.GetMethod("OnMethodEnd");
                MethodInfo onMethodEndAsyncMethodInfo = wrapperType.GetMethod("OnMethodEndAsync");

                if (onMethodEndMethodInfo != null)
                {
                    Type expectedReturnValueType = onMethodEndMethodInfo.GetParameters()[0].ParameterType;
                    if (!(expectedReturnValueType == typeof(object) || returnValue is null || expectedReturnValueType.IsValueType || expectedReturnValueType.IsAssignableFrom(returnValue.GetType())))
                    {
                        Log.Information($"Trying to ducktype {returnValue.GetType()} as {expectedReturnValueType}");
                        returnValue = DuckType.Create(expectedReturnValueType, returnValue);
                    }

                    returnValue = onMethodEndMethodInfo.Invoke(null, new object[] { returnValue, exception, state });
                    if (returnValue is DuckType dType)
                    {
                        returnValue = dType.Instance;
                    }
                }

                if (onMethodEndAsyncMethodInfo != null)
                {
                    returnValue = AsyncTool.AddContinuation(
                        returnValue,
                        exception,
                        new DuckTyping.ValueTuple<MethodInfo, CallTargetState>(onMethodEndAsyncMethodInfo, state),
                        (rValue, ex, tuple) =>
                        {
                            MethodInfo mInfo = tuple.Item1;
                            Type expectedReturnValueType = mInfo.GetParameters()[0].ParameterType;
                            if (!(expectedReturnValueType == typeof(object) || rValue is null || expectedReturnValueType.IsValueType || expectedReturnValueType.IsAssignableFrom(rValue.GetType())))
                            {
                                Log.Information($"Trying to ducktype {rValue.GetType()} as {expectedReturnValueType}");
                                rValue = rValue.As(expectedReturnValueType);
                            }

                            rValue = mInfo.Invoke(null, new object[] { rValue, ex, tuple.Item2 });
                            if (rValue is DuckType dType)
                            {
                                rValue = dType.Instance;
                            }

                            return rValue;
                        });
                }
                */
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"EndMethod error: {ex.Message}");
            }

            return returnValue;
        }

        private static Func<CallerInfo, object[], CallTargetState> CreateMethodBeginDelegate(RuntimeTypeHandle wrapperTypeHandle, string methodName)
        {
            Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
            if (wrapperType is null)
            {
                Log.Error("Wrapper type is null, the wrapperTypeHandle.Value is empty");
                return null;
            }

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

            return (Func<CallerInfo, object[], CallTargetState>)callMethod.CreateDelegate(typeof(Func<CallerInfo, object[], CallTargetState>));
        }

        private static Func<object, Exception, CallTargetState, object> CreateMethodEndDelegate(RuntimeTypeHandle wrapperTypeHandle, string methodName)
        {
            Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
            if (wrapperType is null)
            {
                Log.Error("Wrapper type is null, the wrapperTypeHandle.Value is empty");
                return null;
            }

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

            // Load arguments
            for (var i = 0; i < parameters.Length; i++)
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
                ILHelpers.WriteIlIntValue(ilWriter, i);
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
            ilWriter.EmitCall(OpCodes.Call, onMethodEndMethodInfo, null);

            // Unwrap in case the return value is a duck type
            ilWriter.EmitCall(OpCodes.Call, UnWrapReturnValueMethodInfo, null);
            ilWriter.Emit(OpCodes.Ret);

            return (Func<object, Exception, CallTargetState, object>)callMethod.CreateDelegate(typeof(Func<object, Exception, CallTargetState, object>));
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

            Log.Information($"Ducktyping {valueType.FullName} as {conversionType.FullName}");
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
    }
}
