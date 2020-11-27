using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.NUnit
{
    /// <summary>
    /// NUnit.Framework.Internal.Commands.SkipCommand.Execute() calltarget instrumentation
    /// </summary>
    [InstrumentMethod(
        Assembly = "nunit.framework",
        Type = "NUnit.Framework.Internal.Commands.SkipCommand",
        Method = "Execute",
        ReturnTypeName = "NUnit.Framework.Internal.TestResult",
        ParametersTypesNames = new[] { "NUnit.Framework.Internal.TestExecutionContext" },
        MinimumVersion = "3.0.0",
        MaximumVersion = "3.*.*",
        IntegrationName = IntegrationName)]
    public class NUnitSkipCommandExecuteIntegration
    {
        private const string IntegrationName = "NUnit";

    }
}
