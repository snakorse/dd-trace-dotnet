using System;
using System.Reflection;
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
            Log.Information($"BeginMethod [{instanceTypeHandle}|{instance}|{arguments.Length}|{Type.GetTypeFromHandle(wrapperTypeHandle)}]");
            try
            {
                CallerInfo callerInfo = new CallerInfo(instanceTypeHandle, instance);

                Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
                MethodInfo onMethodBeginMethodInfo = wrapperType.GetMethod("OnMethodBegin");
                if (onMethodBeginMethodInfo != null)
                {
                    if (onMethodBeginMethodInfo.ReturnType != typeof(CallTargetState))
                    {
                        Log.Error($"OnMethodBegin method from {wrapperType.FullName} has an invalid signature.");
                        return new CallTargetState(null);
                    }

                    ParameterInfo[] parameters = onMethodBeginMethodInfo.GetParameters();
                    bool requiresCallerInfo = (parameters.Length > 0 && parameters[0].ParameterType == typeof(CallerInfo));
                    int currentParametersLength = parameters.Length - (requiresCallerInfo ? 1 : 0);
                    int expectedParametersLength = arguments?.Length ?? 0;

                    if (currentParametersLength != expectedParametersLength)
                    {
                        Log.Error($"OnMethodBegin method from {wrapperType.FullName} doesn't have the expected number of parameters. [Current={currentParametersLength}, Expected={expectedParametersLength}]");
                        return new CallTargetState(null);
                    }

                    object[] onBeginArguments = new object[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        ParameterInfo currentParameter = parameters[i];
                        int argumentIndex = i - (requiresCallerInfo ? 1 : 0);

                        if (currentParameter.ParameterType == typeof(CallerInfo))
                        {
                            onBeginArguments[i] = callerInfo;
                        }
                        else if (currentParameter.ParameterType == typeof(object) ||
                            arguments[argumentIndex] is null ||
                            currentParameter.ParameterType.IsValueType ||
                            currentParameter.ParameterType.IsAssignableFrom(arguments[argumentIndex].GetType()))
                        {
                            onBeginArguments[i] = arguments[argumentIndex];
                        }
                        else
                        {
                            Log.Information($"Trying to ducktype {arguments[argumentIndex].GetType()} as {currentParameter.ParameterType}");
                            onBeginArguments[i] = arguments[argumentIndex].As(currentParameter.ParameterType);
                        }
                    }

                    return (CallTargetState)onMethodBeginMethodInfo.Invoke(null, onBeginArguments);
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
            Log.Information($"EndMethod [{returnValue}|{exception}]");

            try
            {
                Type wrapperType = Type.GetTypeFromHandle(wrapperTypeHandle);
                MethodInfo onMethodEndMethodInfo = wrapperType.GetMethod("OnMethodEnd");
                MethodInfo onMethodEndAsyncMethodInfo = wrapperType.GetMethod("OnMethodEndAsync");

                if (onMethodEndMethodInfo != null)
                {
                    Type expectedReturnValueType = onMethodEndMethodInfo.GetParameters()[0].ParameterType;
                    if (!(expectedReturnValueType == typeof(object) || returnValue is null || expectedReturnValueType.IsValueType || expectedReturnValueType.IsAssignableFrom(returnValue.GetType())))
                    {
                        Log.Information($"Trying to ducktype {returnValue.GetType()} as {expectedReturnValueType}");
                        returnValue = returnValue.As(expectedReturnValueType);
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
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"EndMethod error: {ex.Message}");
            }

            return returnValue;
        }
    }
}
