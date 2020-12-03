using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.Configuration;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.RabbitMQ
{
    /// <summary>
    /// RabbitMQ.Client BasicGet calltarget instrumentation
    /// </summary>
    [InstrumentMethod(
        Assembly = "RabbitMQ.Client",
        Type = "RabbitMQ.Client.Impl.ModelBase",
        Method = "BasicGet",
        ReturnTypeName = "RabbitMQ.Client.BasicGetResult",
        ParametersTypesNames = new[] { ClrNames.String, ClrNames.Bool },
        MinimumVersion = "3.6.9",
        MaximumVersion = "6.*.*",
        IntegrationName = RabbitMQConstants.IntegrationName)]
    public class BasicGetIntegration
    {
        private const string Command = "basic.get";

        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(BasicGetIntegration));

        /// <summary>
        /// OnMethodBegin callback
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="instance">Instance value, aka `this` of the instrumented method.</param>
        /// <param name="queue">The queue name of the message</param>
        /// <param name="autoAck">The original autoAck argument</param>
        /// <returns>Calltarget state value</returns>
        public static CallTargetState OnMethodBegin<TTarget>(TTarget instance, string queue, bool autoAck)
        {
            Scope scope = null;
            RabbitMQTags tags = null;

            scope = ScopeFactory.CreateScope(Tracer.Instance, out tags, Command, queue: queue);
            tags?.SetSpanKind(SpanKinds.Consumer);

            if (scope != null)
            {
                string queueDisplayName = string.IsNullOrEmpty(queue) || !queue.StartsWith("amq.gen-") ? queue : "<generated>";
                scope.Span.ResourceName = $"{Command} {queueDisplayName}";
            }

            return new CallTargetState(new IntegrationState(scope, tags));
        }

        /// <summary>
        /// OnMethodEnd callback
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <typeparam name="TResult">Type of the BasicGetResult</typeparam>
        /// <param name="instance">Instance value, aka `this` of the instrumented method.</param>
        /// <param name="basicGetResult">BasicGetResult instance</param>
        /// <param name="exception">Exception instance in case the original code threw an exception.</param>
        /// <param name="state">Calltarget state value</param>
        /// <returns>A default CallTargetReturn to satisfy the CallTarget contract</returns>
        public static CallTargetReturn<TResult> OnMethodEnd<TTarget, TResult>(TTarget instance, TResult basicGetResult, Exception exception, CallTargetState state)
            where TResult : IBasicGetResult
        {
            IntegrationState integrationState = (IntegrationState)state.State;
            if (integrationState.Scope is null)
            {
                return new CallTargetReturn<TResult>(basicGetResult);
            }

            try
            {
                if (basicGetResult != null)
                {
                    var tags = integrationState.Tags;
                    tags.MessageSize = basicGetResult.Body.Length.ToString();
                }

                if (exception != null)
                {
                    integrationState.Scope.Span.SetException(exception);
                }
            }
            finally
            {
                integrationState.Scope.Dispose();
            }

            return new CallTargetReturn<TResult>(basicGetResult);
        }

        private readonly struct IntegrationState
        {
            public readonly Scope Scope;
            public readonly RabbitMQTags Tags;

            public IntegrationState(Scope scope, RabbitMQTags tags)
            {
                Scope = scope;
                Tags = tags;
            }
        }
    }
}
