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
        public static object BeginMethod(object type, object instance, object[] arguments, uint function_token)
        {
            var sw = Stopwatch.StartNew();
            Log.Information($"BeginMethod was called: [Type:{type}|Instance:{instance}|Arguments Count:{arguments?.Length ?? 0}|FunctionToken:{function_token}]");

            var endMethodObject = new CallTargetBeginReturn();
            endMethodObject.SetDelegate((returnValue, ex) =>
            {
                Log.Information($"EndMethod was called: [Type:{type}|Instance:{instance}|ReturnValue:{returnValue}|Exception:{ex}] ==> Using closure to calculate roundtrip: {sw.Elapsed.TotalMilliseconds}ms");

                return AsyncTool.AddContinuation(returnValue, ex, sw, (r, e, s) =>
                {
                    var sw = (Stopwatch)s;
                    Log.Information($"Continuation was completed: [Type:{type}|Instance:{instance}|ReturnValue:{returnValue}|Exception:{ex}] ==> Using closure to calculate roundtrip: {sw.Elapsed.TotalMilliseconds}ms");
                    return r;
                });
            });
            return endMethodObject;
        }
    }
}
