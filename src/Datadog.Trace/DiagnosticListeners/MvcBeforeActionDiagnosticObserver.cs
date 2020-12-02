#if NETCOREAPP
using System;
using System.Reflection;
using Datadog.Trace.DuckTyping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Datadog.Trace.DiagnosticListeners
{
    internal class MvcBeforeActionDiagnosticObserver : CompositeDiagnosticObserver
    {
        private string _mvcBeforeActionEventKey;

        public MvcBeforeActionDiagnosticObserver(Tracer tracer)
            : base(tracer)
        {
        }

        protected override string ListenerName => nameof(MvcBeforeActionDiagnosticObserver);

        protected override void OnNext(string eventName, object arg)
        {
            if (_mvcBeforeActionEventKey != null && ReferenceEquals(eventName, _mvcBeforeActionEventKey))
            {
                OnMvcBeforeAction(arg);
            }
            else
            {
                throw new Exception();
            }
        }

        protected override bool IsEventEnabled(string eventName)
        {
            var lastChar = eventName[^1];

            if (lastChar == 'n')
            {
                if (ReferenceEquals(eventName, _mvcBeforeActionEventKey))
                {
                    return true;
                }

                var suffix = eventName.AsSpan().Slice(PrefixLength);

                if (suffix.SequenceEqual("Mvc.BeforeAction"))
                {
                    _mvcBeforeActionEventKey = eventName;
                    return true;
                }
            }

            return false;
        }

        private void OnMvcBeforeAction(object arg)
        {
            var tracer = Tracer ?? Tracer.Instance;

            if (!tracer.Settings.IsIntegrationEnabled(IntegrationName))
            {
                return;
            }

            Span span = tracer.ActiveScope?.Span;

            if (span != null)
            {
                // NOTE: This event is the start of the action pipeline. The action has been selected, the route
                //       has been selected but no filters have run and model binding hasn't occurred.
                BeforeActionStruct typedArg = arg.As<BeforeActionStruct>();
                ActionDescriptor actionDescriptor = typedArg.ActionDescriptor;
                HttpRequest request = typedArg.HttpContext.Request;

                string httpMethod = request.Method?.ToUpperInvariant() ?? "UNKNOWN";
                string routeTemplate = actionDescriptor.AttributeRouteInfo?.Template;
                if (routeTemplate is null)
                {
                    string controllerName = actionDescriptor.RouteValues["controller"];
                    string actionName = actionDescriptor.RouteValues["action"];

                    routeTemplate = $"{controllerName}/{actionName}";
                }

                string resourceName = $"{httpMethod} {routeTemplate}";

                // override the parent's resource name with the MVC route template
                span.ResourceName = resourceName;
            }
        }

        [DuckCopy]
        public struct BeforeActionStruct
        {
            [Duck(BindingFlags = DuckAttribute.DefaultFlags | BindingFlags.IgnoreCase)]
            public HttpContext HttpContext;

            [Duck(BindingFlags = DuckAttribute.DefaultFlags | BindingFlags.IgnoreCase)]
            public ActionDescriptor ActionDescriptor;
        }
    }
}
#endif
