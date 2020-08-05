using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.Integrations.Testing;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler
{
    /// <summary>
    /// Call target integration helper
    /// </summary>
    public static class CallTarget
    {
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(CallTarget));

        /// <summary>
        /// Call target static begin method helper
        /// </summary>
        /// <param name="type">Instance type</param>
        /// <param name="instance">Object instance</param>
        /// <param name="arguments">Arguments</param>
        /// <param name="function_token">Function token</param>
        /// <returns>CallTargetBeginReturn instance</returns>
        public static CallTargetState BeginMethod(Type type, object instance, object[] arguments, uint function_token)
        {
            Log.Information($"BeginMethod was called: [Type:{type}|Instance:{instance}|Arguments Count:{arguments?.Length ?? 0}|FunctionToken:{function_token}]");
            var sw = Stopwatch.StartNew();
            return new SampleState { Watch = sw };
        }

        /// <summary>
        /// Call target static end method helper
        /// </summary>
        /// <param name="returnValue">Original method return value</param>
        /// <param name="exception">Original method exception</param>
        /// <param name="state">State from the BeginMethod</param>
        /// <param name="function_token">Function token</param>
        /// <returns>Return value</returns>
        public static object EndMethod(object returnValue, Exception exception, CallTargetState state, uint function_token)
        {
            return AsyncTool.AddContinuation(returnValue, exception, state, (rValue, ex, s) =>
            {
                var sampleState = (SampleState)s;
                Log.Information($"EndMethod continuation was completed: [State:{sampleState}|ReturnValue:{rValue}|Exception:{ex}|FunctionToken:{function_token}] " +
                    $"==> Elapsed = {sampleState.Watch.Elapsed.TotalMilliseconds} ms");

                return rValue;
            });
        }

        private class SampleState : CallTargetState
        {
            /// <summary>
            /// Gets or sets stopwatch instance
            /// </summary>
            public Stopwatch Watch { get; set; }
        }
    }
}
