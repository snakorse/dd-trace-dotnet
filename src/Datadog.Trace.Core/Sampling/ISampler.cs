using System.Collections.Generic;

namespace Datadog.Trace.Sampling
{
    internal interface ISampler
    {
        void SetDefaultSampleRates(IEnumerable<KeyValuePair<string, float>> sampleRates);

        SamplingPriority GetSamplingPriority(ISpan span);

        void RegisterRule(ISamplingRule rule);
    }
}
