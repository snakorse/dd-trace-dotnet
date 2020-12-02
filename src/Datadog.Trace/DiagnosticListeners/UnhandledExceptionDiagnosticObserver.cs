#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Reflection;
using Datadog.Trace.DuckTyping;

namespace Datadog.Trace.DiagnosticListeners
{
    internal class UnhandledExceptionDiagnosticObserver : CompositeDiagnosticObserver
    {
        private string _hostingUnhandledExceptionEventKey;
        private string _diagnosticsUnhandledExceptionEventKey;

        public UnhandledExceptionDiagnosticObserver(Tracer tracer)
            : base(tracer)
        {
        }

        protected override string ListenerName => nameof(UnhandledExceptionDiagnosticObserver);

        protected override void OnNext(string eventName, object arg)
        {
            if (eventName[^1] == 'n')
            {
                if (ReferenceEquals(eventName, _hostingUnhandledExceptionEventKey) ||
                    ReferenceEquals(eventName, _diagnosticsUnhandledExceptionEventKey))
                {
                    OnHostingUnhandledException(arg);
                    return;
                }

                var suffix = eventName.AsSpan().Slice(PrefixLength);

                if (suffix.SequenceEqual("Hosting.UnhandledException"))
                {
                    OnHostingUnhandledException(arg);
                    return;
                }

                if (suffix.SequenceEqual("Diagnostics.UnhandledException"))
                {
                    OnHostingUnhandledException(arg);
                    return;
                }
            }

            return;
        }

        protected override bool IsEventEnabled(string eventName)
        {
            if (eventName[^1] == 'n')
            {
                if (ReferenceEquals(eventName, _hostingUnhandledExceptionEventKey) ||
                    ReferenceEquals(eventName, _diagnosticsUnhandledExceptionEventKey))
                {
                    return true;
                }

                var suffix = eventName.AsSpan().Slice(PrefixLength);

                if (suffix.SequenceEqual("Hosting.UnhandledException"))
                {
                    _hostingUnhandledExceptionEventKey = eventName;
                    return true;
                }

                if (suffix.SequenceEqual("Diagnostics.UnhandledException"))
                {
                    _diagnosticsUnhandledExceptionEventKey = eventName;
                    return true;
                }

                return false;
            }

            return false;
        }

        private void OnHostingUnhandledException(object arg)
        {
            var tracer = Tracer ?? Tracer.Instance;

            if (!tracer.Settings.IsIntegrationEnabled(IntegrationName))
            {
                return;
            }

            var span = tracer.ActiveScope?.Span;

            if (span != null)
            {
                span.SetException(arg.As<UnhandledExceptionStruct>().Exception);
            }
        }

        [DuckCopy]
        public struct UnhandledExceptionStruct
        {
            [Duck(BindingFlags = DuckAttribute.DefaultFlags | BindingFlags.IgnoreCase)]
            public Exception Exception;
        }
    }
}
#endif
