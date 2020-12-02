using System.Collections.Generic;
using Datadog.Trace.DuckTyping;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.MSTestV2
{
    /// <summary>
    /// TestCategoryAttribute ducktype struct
    /// </summary>
    [DuckCopy]
    public struct TestCategoryAttributeStruct
    {
        /// <summary>
        /// Gets the test categories
        /// </summary>
        public IList<string> TestCategories;
    }
}
