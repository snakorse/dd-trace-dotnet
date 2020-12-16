using System;

namespace Datadog.Trace
{
    internal interface ISpan
    {
        string ServiceName { get; set; }

        string OperationName { get; set; }

        string ResourceName { get; set; }

        string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this span represents an error.
        /// </summary>
        bool Error { get; set; }

        ISpan SetTag(string key, string value);

        string GetTag(string key);

        ISpan SetMetric(string key, double? value);

        double? GetMetric(string key);

        void SetException(Exception exception);
    }
}
