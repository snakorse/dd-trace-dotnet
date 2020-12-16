using System;

namespace Datadog.Trace
{
    internal interface IScope : IDisposable
    {
        ISpan Span { get; }

        IScope Parent { get; }
    }
}
