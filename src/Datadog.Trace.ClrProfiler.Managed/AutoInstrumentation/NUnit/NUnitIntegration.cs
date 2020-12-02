using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Datadog.Trace.Ci;
using Datadog.Trace.ExtensionMethods;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.NUnit
{
    internal static class NUnitIntegration
    {
        private static readonly FrameworkDescription _runtimeDescription;

        static NUnitIntegration()
        {
            // Preload environment variables.
            CIEnvironmentValues.DecorateSpan(null);

            _runtimeDescription = FrameworkDescription.Create();
        }

        internal static Scope CreateScope<TContext>(TContext executionContext, Type targetType)
            where TContext : ITestExecutionContext
        {
            ITest currentTest = executionContext.CurrentTest;
            MethodInfo testMethod = currentTest.Method.MethodInfo;
            object[] testMethodArguments = currentTest.Arguments;
            IPropertyBag testMethodProperties = currentTest.Properties;

            if (testMethod == null)
            {
                return null;
            }

            string testFramework = "NUnit " + targetType?.Assembly?.GetName().Version;
            string testSuite = testMethod.DeclaringType?.FullName;
            string testName = testMethod.Name;
            string skipReason = null;
            List<KeyValuePair<string, string>> testArguments = null;
            List<KeyValuePair<string, string>> testTraits = null;

            // Get test parameters
            ParameterInfo[] methodParameters = testMethod.GetParameters();
            if (methodParameters?.Length > 0)
            {
                testArguments = new List<KeyValuePair<string, string>>();

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    if (testMethodArguments != null && i < testMethodArguments.Length)
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
            if (testMethodProperties != null)
            {
                skipReason = (string)testMethodProperties.Get("_SKIPREASON");
                testTraits = new List<KeyValuePair<string, string>>();

                foreach (var key in testMethodProperties.Keys)
                {
                    if (key == "_SKIPREASON")
                    {
                        continue;
                    }

                    IList value = testMethodProperties[key];
                    IEnumerable<string> values = Enumerable.Empty<string>();
                    if (value != null)
                    {
                        List<string> lstValues = new List<string>();
                        foreach (object valObj in value)
                        {
                            if (valObj is null)
                            {
                                continue;
                            }

                            lstValues.Add(valObj.ToString());
                        }

                        values = lstValues;
                    }

                    testTraits.Add(new KeyValuePair<string, string>($"{TestTags.Traits}.{key}", string.Join(", ", values) ?? "(null)"));
                }
            }

            Tracer tracer = Tracer.Instance;
            Scope scope = tracer.StartActive("nunit.test");
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

            if (skipReason != null)
            {
                span.SetTag(TestTags.Status, TestTags.StatusSkip);
                span.SetTag(TestTags.SkipReason, skipReason);
                span.Finish(TimeSpan.Zero);
                scope.Dispose();
                scope = null;
            }

            span.ResetStartTime();
            return scope;
        }

        internal static void FinishScope(Scope scope, Exception ex)
        {
            // unwrap the generic NUnitException
            if (ex != null && ex.GetType().FullName == "NUnit.Framework.Internal.NUnitException")
            {
                ex = ex.InnerException;
            }

            switch (ex?.GetType().FullName)
            {
                case "NUnit.Framework.SuccessException":
                    scope.Span.SetTag(TestTags.Status, TestTags.StatusPass);
                    break;
                case "NUnit.Framework.IgnoreException":
                    scope.Span.SetTag(TestTags.Status, TestTags.StatusSkip);
                    break;
                default:
                    scope.Span.SetException(ex);
                    scope.Span.SetTag(TestTags.Status, TestTags.StatusFail);
                    break;
            }
        }
    }
}
