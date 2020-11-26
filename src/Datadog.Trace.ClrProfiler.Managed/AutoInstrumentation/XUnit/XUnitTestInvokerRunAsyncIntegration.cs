using System;
using System.Collections.Generic;
using System.Reflection;
using Datadog.Trace.Ci;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.DuckTyping;
using Datadog.Trace.ExtensionMethods;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.XUnit
{
    /// <summary>
    /// Xunit.Sdk.TestInvoker`1.RunAsync calltarget instrumentation
    /// </summary>
    [InstrumentMethod(
        Assemblies = new[] { "xunit.execution.dotnet", "xunit.execution.desktop" },
        Type = "Xunit.Sdk.TestInvoker`1",
        Method = "RunAsync",
        ReturnTypeName = "System.Threading.Tasks.Task`1<System.Decimal>",
        ParametersTypesNames = new string[0],
        MinimumVersion = "2.2.0",
        MaximumVersion = "2.*.*",
        IntegrationName = IntegrationName)]
    public class XUnitTestInvokerRunAsyncIntegration
    {
        private const string IntegrationName = "XUnit";
        private static readonly FrameworkDescription _runtimeDescription;

        static XUnitTestInvokerRunAsyncIntegration()
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
            if (!Tracer.Instance.Settings.IsIntegrationEnabled(IntegrationName))
            {
                return CallTargetState.GetDefault();
            }

            TestInvokerStruct invokerInstance = instance.As<TestInvokerStruct>();

            string testSuite = invokerInstance.TestClass.ToString();
            string testName = invokerInstance.TestMethod.Name;
            List<KeyValuePair<string, string>> testArguments = null;
            List<KeyValuePair<string, string>> testTraits = null;

            // Get test parameters
            object[] testMethodArguments = invokerInstance.TestMethodArguments;
            ParameterInfo[] methodParameters = invokerInstance.TestMethod.GetParameters();
            if (methodParameters?.Length > 0 && testMethodArguments?.Length > 0)
            {
                testArguments = new List<KeyValuePair<string, string>>();

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    if (i < testMethodArguments.Length)
                    {
                        testArguments.Add(new KeyValuePair<string, string>($"{TestTags.Arguments}.{methodParameters[i].Name}", testMethodArguments[i]?.ToString() ?? "(null)"));
                    }
                    else
                    {
                        testArguments.Add(new KeyValuePair<string, string>($"{TestTags.Arguments}.{methodParameters[i].Name}", "(default)"));
                    }
                }
            }

            // Get traits
            Dictionary<string, List<string>> traits = invokerInstance.TestCase.Traits;
            if (traits.Count > 0)
            {
                testTraits = new List<KeyValuePair<string, string>>();

                foreach (KeyValuePair<string, List<string>> traitValue in traits)
                {
                    testTraits.Add(new KeyValuePair<string, string>($"{TestTags.Traits}.{traitValue.Key}", string.Join(", ", traitValue.Value) ?? "(null)"));
                }
            }

            AssemblyName testInvokerAssemblyName = instance.GetType().Assembly.GetName();

            Tracer tracer = Tracer.Instance;
            string testFramework = "xUnit " + testInvokerAssemblyName.Version.ToString();

            Scope scope = tracer.StartActive("xunit.test");
            Span span = scope.Span;

            span.Type = SpanTypes.Test;
            span.SetMetric(Tags.Analytics, 1.0d);
            span.SetTraceSamplingPriority(SamplingPriority.AutoKeep);
            span.ResourceName = $"{testSuite}.{testName}";
            span.SetTag(TestTags.Suite, testSuite);
            span.SetTag(TestTags.Name, testName);
            span.SetTag(TestTags.Framework, testFramework);
            span.SetTag(TestTags.Type, TestTags.TypeTest);
            CIEnvironmentValues.DecorateSpan(span);

            span.SetTag(CommonTags.RuntimeName, _runtimeDescription.Name);
            span.SetTag(CommonTags.RuntimeOSArchitecture, _runtimeDescription.OSArchitecture);
            span.SetTag(CommonTags.RuntimeOSPlatform, _runtimeDescription.OSPlatform);
            span.SetTag(CommonTags.RuntimeProcessArchitecture, _runtimeDescription.ProcessArchitecture);
            span.SetTag(CommonTags.RuntimeVersion, _runtimeDescription.ProductVersion);

            if (testArguments != null)
            {
                foreach (KeyValuePair<string, string> argument in testArguments)
                {
                    span.SetTag(argument.Key, argument.Value);
                }
            }

            if (testTraits != null)
            {
                foreach (KeyValuePair<string, string> trait in testTraits)
                {
                    span.SetTag(trait.Key, trait.Value);
                }
            }

            return new CallTargetState(scope);
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
            Scope scope = (Scope)state.State;
            if (scope != null)
            {
                TestInvokerStruct invokerInstance = instance.As<TestInvokerStruct>();
                exception ??= invokerInstance.Aggregator.ToException();

                if (exception != null)
                {
                    if (exception.GetType().Name == "SkipException")
                    {
                        scope.Span.SetTag(TestTags.Status, TestTags.StatusSkip);
                        scope.Span.SetTag(TestTags.SkipReason, exception.Message);
                    }
                    else
                    {
                        scope.Span.SetException(exception);
                        scope.Span.SetTag(TestTags.Status, TestTags.StatusFail);
                    }
                }
                else
                {
                    scope.Span.SetTag(TestTags.Status, TestTags.StatusPass);
                }

                scope.Dispose();
            }

            return returnValue;
        }
    }
}
