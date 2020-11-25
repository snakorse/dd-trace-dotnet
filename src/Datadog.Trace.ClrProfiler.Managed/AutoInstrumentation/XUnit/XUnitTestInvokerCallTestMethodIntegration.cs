using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.Ci;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.XUnit
{
    /// <summary>
    /// Xunit.Sdk.TestInvoker.RunAsync calltarget instrumentation
    /// </summary>
    [InstrumentMethod(
        Assembly = "xunit.execution.dotnet",
        Type = "Xunit.Sdk.TestInvoker`1",
        Method = "RunAsync",
        ReturnTypeName = "System.Threading.Tasks.Task`1<System.Decimal>",
        ParametersTypesNames = new string[0],
        MinimumVersion = "2.2.0",
        MaximumVersion = "2.*.*",
        IntegrationName = IntegrationName)]
    public class XUnitTestInvokerCallTestMethodIntegration
    {
        private const string IntegrationName = "XUnit";
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(XUnitTestInvokerCallTestMethodIntegration));
        private static readonly FrameworkDescription _runtimeDescription;

        static XUnitTestInvokerCallTestMethodIntegration()
        {
            // Preload environment variables.
            CIEnvironmentValues.DecorateSpan(null);

            _runtimeDescription = FrameworkDescription.Create();
        }

        /// <summary>
        /// OnMethodBegin callback
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="instance">Instance value, aka `this` of the instrumented method.</param>
        /// <returns>Calltarget state value</returns>
        public static CallTargetState OnMethodBegin<TTarget>(TTarget instance)
        {
            return default;
        }

        /// <summary>
        /// OnAsyncMethodEnd callback
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="instance">Instance value, aka `this` of the instrumented method.</param>
        /// <param name="returnValue">Return value</param>
        /// <param name="exception">Exception instance in case the original code threw an exception.</param>
        /// <param name="state">Calltarget state value</param>
        /// <returns>A response value, in an async scenario will be T of Task of T</returns>
        public static decimal OnAsyncMethodEnd<TTarget>(TTarget instance, decimal returnValue, Exception exception, CallTargetState state)
        {
            return returnValue;
        }
    }
}
