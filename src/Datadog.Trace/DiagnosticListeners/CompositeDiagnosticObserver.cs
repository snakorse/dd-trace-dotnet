#if !NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.Logging;

namespace Datadog.Trace.DiagnosticListeners
{
    internal abstract class CompositeDiagnosticObserver : DiagnosticObserver
    {
        public const string IntegrationName = "AspNetCore";

        protected static readonly Vendors.Serilog.ILogger Log = DatadogLogging.For<AspNetCoreDiagnosticObserver>();
        protected static readonly int PrefixLength = "Microsoft.AspNetCore.".Length;

        protected CompositeDiagnosticObserver(Tracer tracer)
        {
            Tracer = tracer;
        }

        protected Tracer Tracer { get; }
    }
}
#endif
