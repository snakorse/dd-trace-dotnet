using System;
using Datadog.Trace.Ci;
using Datadog.Trace.ClrProfiler.CallTarget;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.MSTestV2
{
    /// <summary>
    /// Microsoft.VisualStudio.TestPlatform.TestFramework.Execute calltarget instrumentation
    /// </summary>
    [InstrumentMethod(
        Assembly = "Microsoft.VisualStudio.TestPlatform.TestFramework",
        Type = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
        Method = "Execute",
        ReturnTypeName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestResult",
        ParametersTypesNames = new[] { "Microsoft.VisualStudio.TestTools.UnitTesting.ITestMethod" },
        MinimumVersion = "14.0.0",
        MaximumVersion = "14.*.*",
        IntegrationName = IntegrationName)]
    public class TestMethodAttributeExecuteIntegration
    {
        private const string IntegrationName = "MSTestV2";
        private static readonly FrameworkDescription _runtimeDescription;

        static TestMethodAttributeExecuteIntegration()
        {
            // Preload environment variables.
            CIEnvironmentValues.DecorateSpan(null);

            _runtimeDescription = FrameworkDescription.Create();
        }

        /// <summary>
        /// OnMethodBegin callback
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <typeparam name="TTestMethod">Type of the ITestMethod</typeparam>
        /// <param name="instance">Instance value, aka `this` of the instrumented method.</param>
        /// <param name="testMethod">Test method instance</param>
        /// <returns>Calltarget state value</returns>
        public static CallTargetState OnMethodBegin<TTarget, TTestMethod>(TTarget instance, TTestMethod testMethod)
        {
            if (!Tracer.Instance.Settings.IsIntegrationEnabled(IntegrationName))
            {
                return CallTargetState.GetDefault();
            }

            return CallTargetState.GetDefault();
        }

        /// <summary>
        /// OnAsyncMethodEnd callback
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <typeparam name="TReturn">Type of the return value</typeparam>
        /// <param name="instance">Instance value, aka `this` of the instrumented method.</param>
        /// <param name="returnValue">Return value</param>
        /// <param name="exception">Exception instance in case the original code threw an exception.</param>
        /// <param name="state">Calltarget state value</param>
        /// <returns>A response value, in an async scenario will be T of Task of T</returns>
        public static CallTargetReturn<TReturn> OnMethodEnd<TTarget, TReturn>(TTarget instance, TReturn returnValue, Exception exception, CallTargetState state)
        {
            return new CallTargetReturn<TReturn>(returnValue);
        }
    }
}
