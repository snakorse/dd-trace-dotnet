using System;
using System.Collections.Generic;
using System.Linq;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Datadog.Trace.ClrProfiler.Helpers;
using Datadog.Trace.Logging;

#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements must be documented

namespace Datadog.Trace.ClrProfiler.Integrations
{
    /// <inheritdoc/>
    public class HttpClientHandlerCallTargetIntegration : CallTargetState
    {
        private const string IntegrationName = "HttpClientHandler";
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(HttpClientHandlerCallTargetIntegration));

        private Scope _currentScope = null;

        /// <inheritdoc/>
        public override void OnStartMethodCall(ArraySegment<object> args)
        {
            HttpRequestMessage requestMessage = args.Array[0].As<HttpRequestMessage>();

            if (!IsTracingEnabled(requestMessage.Headers))
            {
                return;
            }

            _currentScope = ScopeFactory.CreateOutboundHttpScope(Tracer.Instance, requestMessage.Method.Method, requestMessage.RequestUri, IntegrationName);
            if (_currentScope != null)
            {
                _currentScope.Span.SetTag("http-client-handler-type", InstanceType.FullName);

                // add distributed tracing headers to the HTTP request
                SpanContextPropagator.Instance.Inject(_currentScope.Span.Context, new ReflectionHttpHeadersCollection(requestMessage.Headers.Instance));
            }
        }

        /// <inheritdoc/>
        public override object OnEndMethodCall(object returnValue, Exception exception)
        {
            if (_currentScope is null)
            {
                Log.Information($"No scope: [ReturnValue:{returnValue}|Exception:{exception}] ");
                return returnValue;
            }

            if (exception is null)
            {
                HttpResponseMessage responseMessage = returnValue.As<HttpResponseMessage>();
                _currentScope.Span.SetTag(Tags.HttpStatusCode, responseMessage.StatusCode.ToString());
            }
            else
            {
                _currentScope.Span.SetException(exception);
            }

            _currentScope.Dispose();
            _currentScope = null;
            return returnValue;
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
