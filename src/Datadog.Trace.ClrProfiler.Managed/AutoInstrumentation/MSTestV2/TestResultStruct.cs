using System;
using Datadog.Trace.DuckTyping;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.MSTestV2
{
    /// <summary>
    /// TestResult ducktype struct
    /// </summary>
    [DuckCopy]
    public struct TestResultStruct
    {
        /// <summary>
        /// Test failure exception
        /// </summary>
        public Exception TestFailureException;
    }
}
