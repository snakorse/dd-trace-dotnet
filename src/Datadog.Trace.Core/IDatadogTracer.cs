using System;
using Datadog.Trace.Sampling;

namespace Datadog.Trace
{
    internal interface IDatadogTracer
    {
        string DefaultServiceName { get; }

        IScopeManager ScopeManager { get; }

        ISampler Sampler { get; }

        ISpan StartSpan(string operationName);

        ISpan StartSpan(string operationName, ISpanContext parent);

        ISpan StartSpan(string operationName, ISpanContext parent, string serviceName, DateTimeOffset? startTime, bool ignoreActiveScope);

        void Write(ISpan[] span);

        /// <summary>
        /// Make a span the active span and return its new scope.
        /// </summary>
        /// <param name="span">The span to activate.</param>
        /// <returns>A Scope object wrapping this span.</returns>
        IScope ActivateSpan(ISpan span);

        /// <summary>
        /// Make a span the active span and return its new scope.
        /// </summary>
        /// <param name="span">The span to activate.</param>
        /// <param name="finishOnClose">Determines whether closing the returned scope will also finish the span.</param>
        /// <returns>A Scope object wrapping this span.</returns>
        IScope ActivateSpan(ISpan span, bool finishOnClose);
    }
}
