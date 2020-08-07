using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Datadog.Trace.ClrProfiler.Helpers;
using Datadog.Trace.Logging;

#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements must be documented

namespace Datadog.Trace.ClrProfiler.Integrations
{
    public class HttpClientHandlerCallTargetIntegration
    {
        private const string IntegrationName = "HttpClientHandler";
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(HttpClientHandlerCallTargetIntegration));

        public static CallTargetState OnMethodBegin(CallerInfo caller, HttpRequestMessage requestMessage, CancellationTokenSource cancellationTokenSource)
        {
            Scope scope = null;

            if (IsTracingEnabled(requestMessage.Headers))
            {
                scope = ScopeFactory.CreateOutboundHttpScope(Tracer.Instance, requestMessage.Method.Method, requestMessage.RequestUri, IntegrationName);
                if (scope != null)
                {
                    scope.Span.SetTag("http-client-handler-type", caller.Type.FullName);

                    // add distributed tracing headers to the HTTP request
                    SpanContextPropagator.Instance.Inject(scope.Span.Context, new ReflectionHttpHeadersCollection(requestMessage.Headers.Instance));
                }
            }

            return new CallTargetState(scope);
        }

        public static object OnMethodEndAsync(HttpResponseMessage responseMessage, Exception exception, CallTargetState state)
        {
            Scope scope = (Scope)state.State;

            if (scope is null)
            {
                Log.Information($"No scope: [ReturnValue:{responseMessage}|Exception:{exception}] ");
                return responseMessage;
            }

            if (exception is null)
            {
                scope.Span.SetTag(Tags.HttpStatusCode, responseMessage.StatusCode.ToString());
            }
            else
            {
                scope.Span.SetException(exception);
            }

            scope.Dispose();
            scope = null;
            return responseMessage;
        }

        private static bool IsTracingEnabled(RequestHeaders headers)
        {
            if (headers.Contains(HttpHeaderNames.TracingEnabled))
            {
                var headerValues = headers.GetValues(HttpHeaderNames.TracingEnabled);
                if (headerValues != null && headerValues.Any(s => string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)))
                {
                    // tracing is disabled for this request via http header
                    return false;
                }
            }

            return true;
        }

        public class HttpRequestMessage : DuckType
        {
            public virtual HttpMethod Method { get; }

            public virtual Uri RequestUri { get; }

            public virtual Version Version { get; }

            public virtual RequestHeaders Headers { get; }
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

        public class RequestHeaders : DuckType
        {
            public virtual bool Contains(string name) => true;

            public virtual IEnumerable<string> GetValues(string name) => Enumerable.Empty<string>();
        }

        public class HttpResponseMessage : DuckType
        {
            public int StatusCode { get; }
        }
    }
}
