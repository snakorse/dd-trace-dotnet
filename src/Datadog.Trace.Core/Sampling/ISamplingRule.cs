namespace Datadog.Trace.Sampling
{
    internal interface ISamplingRule
    {
        /// <summary>
        /// Gets the rule name.
        /// Used for debugging purposes mostly.
        /// </summary>
        string RuleName { get; }

        /// <summary>
        /// Gets the priority.
        /// Higher number means higher priority.
        /// </summary>
        int Priority { get; }

        bool IsMatch(ISpan span);

        float GetSamplingRate(ISpan span);
    }
}
