using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Datadog.Trace.Logging;

#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements must be documented

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
            // Ducktype the object arg0 to the custom HttpRequestMessage below
            HttpRequestMessage requestMessage = args.Array[0].As<HttpRequestMessage>();

            Log.Information($"BeginMethod was called: [InstanceType:{InstanceType}|Instance:{Instance}|Arguments Count:{args.Count}] \r\n " +
                $"Method = {requestMessage.Method.Method} | Uri = {requestMessage.RequestUri} | Version = {requestMessage.Version}");
            Watch = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public override object OnBeforeEndMethodCall(object returnValue, Exception exception)
        {
            // If the return value is a task we can add here custom continuations

            Log.Information($"Before running continuations: [ReturnValue:{returnValue}|Exception:{exception}] " +
                $"==> Elapsed = {Watch.Elapsed.TotalMilliseconds} ms");
            return returnValue;
        }

        /// <inheritdoc/>
        public override object OnEndMethodCall(object returnValue, Exception exception)
        {
            // If the return value is a task this will run after the original task has been completed

            Watch.Stop();
            Log.Information($"EndMethod continuation was completed: [ReturnValue:{returnValue}|Exception:{exception}] " +
                $"==> Elapsed = {Watch.Elapsed.TotalMilliseconds} ms");
            Watch = null;
            return returnValue;
        }

        public class HttpRequestMessage : DuckType
        {
            public virtual HttpMethod Method { get; }

            public virtual Uri RequestUri { get; }

            public virtual Version Version { get; }
        }

        public class HttpMethod : DuckType
        {
            public virtual string Method { get; }
        }

        public class Version : DuckType
        {
            public virtual int Major { get; }

            public virtual int Minor { get; }

            public virtual int Build { get; }
        }
    }
}
