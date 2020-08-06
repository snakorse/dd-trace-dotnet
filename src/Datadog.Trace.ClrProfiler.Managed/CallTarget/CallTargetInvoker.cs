using System;
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
            CallTargetState state = null;
            try
            {
                state = (CallTargetState)Activator.CreateInstance(Type.GetTypeFromHandle(wrapperTypeHandle));
                state.Init(instanceTypeHandle, instance);
                state.OnStartMethodCall(new ArraySegment<object>(arguments));
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"BeginMethod error: {ex.Message}");
            }

            return state;
        }

        /// <summary>
        /// Call target static end method helper
        /// </summary>
        /// <param name="returnValue">Original method return value</param>
        /// <param name="exception">Original method exception</param>
        /// <param name="state">State from the BeginMethod</param>
        /// <returns>Return value</returns>
        public static object EndMethod(object returnValue, Exception exception, CallTargetState state)
        {
            if (state is null)
            {
                return returnValue;
            }

            try
            {
                returnValue = state.OnBeforeEndMethodCall(returnValue, exception);
                return AsyncTool.AddContinuation(returnValue, exception, state, (rValue, ex, s) => s.OnEndMethodCall(rValue, ex));
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"EndMethod error: {ex.Message}");
            }

            return returnValue;
        }
    }
}
