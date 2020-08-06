using System;
using System.Diagnostics;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.Integrations
{
    /// <inheritdoc/>
    public class CallTargetSampleIntegration : CallTargetState
    {
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(CallTargetSampleIntegration));

        /// <summary>
        /// Gets or sets stopwatch instance (this stopwatch instance is valid for the whole transaction)
        /// </summary>
        public Stopwatch Watch { get; set; }

        /// <inheritdoc/>
        public override void OnStartMethodCall(ArraySegment<object> args)
        {
            Log.Information($"BeginMethod was called: [InstanceType:{InstanceType}|Instance:{Instance}|Arguments Count:{args.Count}]");
            Watch = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public override object OnBeforeEndMethodCall(object returnValue, Exception exception)
        {
            Log.Information($"Before running continuations: [ReturnValue:{returnValue}|Exception:{exception}] " +
                $"==> Elapsed = {Watch.Elapsed.TotalMilliseconds} ms");
            return returnValue;
        }

        /// <inheritdoc/>
        public override object OnEndMethodCall(object returnValue, Exception exception)
        {
            Watch.Stop();
            Log.Information($"EndMethod continuation was completed: [ReturnValue:{returnValue}|Exception:{exception}] " +
                $"==> Elapsed = {Watch.Elapsed.TotalMilliseconds} ms");
            Watch = null;
            return returnValue;
        }
    }
}
