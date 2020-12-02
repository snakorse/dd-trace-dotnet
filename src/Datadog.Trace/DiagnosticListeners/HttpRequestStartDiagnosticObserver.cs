#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Datadog.Trace.DuckTyping;
using Datadog.Trace.ExtensionMethods;
using Datadog.Trace.Headers;
using Datadog.Trace.Logging;
using Datadog.Trace.Tagging;
using Datadog.Trace.Util;
using Microsoft.AspNetCore.Http;

namespace Datadog.Trace.DiagnosticListeners
{
    internal class HttpRequestStartDiagnosticObserver : CompositeDiagnosticObserver
    {
        private const string NoHostSpecified = "UNKNOWN_HOST";
        private const string HttpRequestInOperationName = "aspnet_core.request";

        private string _hostingHttpRequestInStartEventKey;

        public HttpRequestStartDiagnosticObserver(Tracer tracer)
            : base(tracer)
        {
        }

        protected override string ListenerName => nameof(HttpRequestStartDiagnosticObserver);

        protected override void OnNext(string eventName, object arg)
        {
            if (_hostingHttpRequestInStartEventKey != null && ReferenceEquals(eventName, _hostingHttpRequestInStartEventKey))
            {
                OnHostingHttpRequestInStart(arg);
            }
            else
            {
                throw new Exception();
            }
        }

        protected override bool IsEventEnabled(string eventName)
        {
            var lastChar = eventName[^1];

            if (lastChar == 't')
            {
                if (ReferenceEquals(eventName, _hostingHttpRequestInStartEventKey))
                {
                    return true;
                }

                if (eventName.AsSpan().Slice(PrefixLength).SequenceEqual("Hosting.HttpRequestIn.Start"))
                {
                    _hostingHttpRequestInStartEventKey = eventName;
                    return true;
                }
            }

            return false;
        }

        private static SpanContext ExtractPropagatedContext(HttpRequest request)
        {
            try
            {
                // extract propagation details from http headers
                var requestHeaders = request.Headers;

                if (requestHeaders != null)
                {
                    return SpanContextPropagator.Instance.Extract(new HeadersCollectionAdapter(requestHeaders));
                }
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, "Error extracting propagated HTTP headers.");
            }

            return null;
        }

        private static IEnumerable<KeyValuePair<string, string>> ExtractHeaderTags(HttpRequest request, IDatadogTracer tracer)
        {
            var settings = tracer.Settings;

            if (!settings.HeaderTags.IsEmpty())
            {
                try
                {
                    // extract propagation details from http headers
                    var requestHeaders = request.Headers;

                    if (requestHeaders != null)
                    {
                        return SpanContextPropagator.Instance.ExtractHeaderTags(new HeadersCollectionAdapter(requestHeaders), settings.HeaderTags);
                    }
                }
                catch (Exception ex)
                {
                    Log.SafeLogError(ex, "Error extracting propagated HTTP headers.");
                }
            }

            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        private static string GetUrl(HttpRequest request)
        {
            if (request.Host.HasValue)
            {
                return $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}{request.Path.Value}";
            }

            // HTTP 1.0 requests are not required to provide a Host to be valid
            // Since this is just for display, we can provide a string that is
            // not an actual Uri with only the fields that are specified.
            // request.GetDisplayUrl(), used above, will throw an exception
            // if request.Host is null.
            return $"{request.Scheme}://{NoHostSpecified}{request.PathBase.Value}{request.Path.Value}";
        }

        private void OnHostingHttpRequestInStart(object arg)
        {
            var tracer = Tracer ?? Tracer.Instance;

            if (!tracer.Settings.IsIntegrationEnabled(IntegrationName))
            {
                return;
            }

            HttpRequest request = arg.As<HttpRequestInStartStruct>().HttpContext.Request;
            string host = request.Host.Value;
            string httpMethod = request.Method?.ToUpperInvariant() ?? "UNKNOWN";
            string url = GetUrl(request);

            string absolutePath = request.Path.Value;

            if (request.PathBase.HasValue)
            {
                absolutePath = request.PathBase.Value + absolutePath;
            }

            string resourceUrl = UriHelpers.GetRelativeUrl(absolutePath, tryRemoveIds: true)
                .ToLowerInvariant();

            string resourceName = $"{httpMethod} {resourceUrl}";

            SpanContext propagatedContext = ExtractPropagatedContext(request);
            var tagsFromHeaders = ExtractHeaderTags(request, tracer);

            var tags = new AspNetCoreTags();
            var scope = tracer.StartActiveWithTags(HttpRequestInOperationName, propagatedContext, tags: tags);

            scope.Span.DecorateWebServerSpan(resourceName, httpMethod, host, url, tags, tagsFromHeaders);

            tags.SetAnalyticsSampleRate(IntegrationName, tracer.Settings, enabledWithGlobalSetting: true);
        }

        [DuckCopy]
        public struct HttpRequestInStartStruct
        {
            [Duck(BindingFlags = DuckAttribute.DefaultFlags | BindingFlags.IgnoreCase)]
            public HttpContext HttpContext;
        }

        private readonly struct HeadersCollectionAdapter : IHeadersCollection
        {
            private readonly IHeaderDictionary _headers;

            public HeadersCollectionAdapter(IHeaderDictionary headers)
            {
                _headers = headers;
            }

            public IEnumerable<string> GetValues(string name)
            {
                if (_headers.TryGetValue(name, out var values))
                {
                    return values.ToArray();
                }

                return Enumerable.Empty<string>();
            }

            public void Set(string name, string value)
            {
                throw new NotImplementedException();
            }

            public void Add(string name, string value)
            {
                throw new NotImplementedException();
            }

            public void Remove(string name)
            {
                throw new NotImplementedException();
            }
        }
    }
}
#endif
