#if NETCOREAPP

using System;
using System.Reflection;
using Datadog.Trace.DuckTyping;
using Datadog.Trace.ExtensionMethods;
using Microsoft.AspNetCore.Http;

namespace Datadog.Trace.DiagnosticListeners
{
    internal class HttpRequestStopDiagnosticObserver : CompositeDiagnosticObserver
    {
        private string _hostingHttpRequestInStopEventKey;

        public HttpRequestStopDiagnosticObserver(Tracer tracer)
            : base(tracer)
        {
        }

        protected override string ListenerName => nameof(HttpRequestStopDiagnosticObserver);

        protected override void OnNext(string eventName, object arg)
        {
            if (_hostingHttpRequestInStopEventKey != null && ReferenceEquals(eventName, _hostingHttpRequestInStopEventKey))
            {
                OnHostingHttpRequestInStop(arg);
            }
            else
            {
                throw new Exception();
            }
        }

        protected override bool IsEventEnabled(string eventName)
        {
            var lastChar = eventName[^1];

            if (lastChar == 'p')
            {
                if (ReferenceEquals(eventName, _hostingHttpRequestInStopEventKey))
                {
                    return true;
                }

                if (eventName.AsSpan().Slice(PrefixLength).SequenceEqual("Hosting.HttpRequestIn.Stop"))
                {
                    _hostingHttpRequestInStopEventKey = eventName;
                    return true;
                }
            }

            return false;
        }

        private void OnHostingHttpRequestInStop(object arg)
        {
            var tracer = Tracer ?? Tracer.Instance;

            if (!tracer.Settings.IsIntegrationEnabled(IntegrationName))
            {
                return;
            }

            var scope = tracer.ActiveScope;

            if (scope != null)
            {
                HttpContext httpContext = arg.As<HttpRequestInStopStruct>().HttpContext;

                scope.Span.SetServerStatusCode(httpContext.Response.StatusCode);
                scope.Dispose();
            }
        }

        [DuckCopy]
        public struct HttpRequestInStopStruct
        {
            [Duck(BindingFlags = DuckAttribute.DefaultFlags | BindingFlags.IgnoreCase)]
            public HttpContext HttpContext;
        }
    }
}
#endif
