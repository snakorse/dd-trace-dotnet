using System;

namespace Datadog.Trace
{
    /// <summary>
    /// Interface for managing a scope.
    /// </summary>
    internal interface IScopeManager
    {
        event EventHandler<SpanEventArgs> SpanOpened;

        event EventHandler<SpanEventArgs> SpanActivated;

        event EventHandler<SpanEventArgs> SpanDeactivated;

        event EventHandler<SpanEventArgs> SpanClosed;

        event EventHandler<SpanEventArgs> TraceEnded;

        IScope Active { get; }

        IScope Activate(ISpan span, bool finishOnClose);

        void Close(IScope scope);
    }
}
