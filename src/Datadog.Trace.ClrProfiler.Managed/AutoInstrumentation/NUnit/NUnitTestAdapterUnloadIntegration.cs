using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.NUnit
{
    /// <summary>
    /// NUnit.VisualStudio.TestAdapter.NUnitTestAdapter.Unload() calltarget instrumentation
    /// </summary>
    [InstrumentMethod(
        Assembly = "NUnit3.TestAdapter",
        Type = "NUnit.VisualStudio.TestAdapter.NUnitTestAdapter",
        Method = "Unload",
        ReturnTypeName = ClrNames.Void,
        ParametersTypesNames = new string[0],
        MinimumVersion = "3.0.0",
        MaximumVersion = "3.*.*",
        IntegrationName = IntegrationName)]
    public class NUnitTestAdapterUnloadIntegration
    {
        private const string IntegrationName = "NUnit";

    }
}
