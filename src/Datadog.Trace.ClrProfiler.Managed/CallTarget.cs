using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static object BeginMethod(object type, object instance, object[] arguments, uint function_token)
        {
            Log.Information($"BeginMethod was called: [{type}|{instance}|{arguments?.Length ?? 0}|{function_token}]");
            return new CallTargetBeginReturn();
        }
    }
}
