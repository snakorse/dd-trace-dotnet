using System;
using Datadog.Trace.Configuration;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.RabbitMQ
{
    internal static class ScopeFactory
    {
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(ScopeFactory));

        internal static Scope CreateScope(Tracer tracer, out RabbitMQTags tags, string command, ISpanContext parentContext = null, string queue = null, string exchange = null, string routingKey = null)
        {
            tags = null;

            if (!tracer.Settings.IsIntegrationEnabled(RabbitMQConstants.IntegrationId))
            {
                // integration disabled, don't create a scope, skip this trace
                return null;
            }

            Scope scope = null;

            try
            {
                Span parent = tracer.ActiveScope?.Span;

                tags = new RabbitMQTags();
                scope = tracer.StartActiveWithTags(RabbitMQConstants.OperationName, tags: tags, serviceName: $"{tracer.DefaultServiceName}-{RabbitMQConstants.IntegrationName}");
                var span = scope.Span;

                span.Type = SpanTypes.MessageClient;
                span.ResourceName = command;
                tags.Command = command;

                tags.Queue = queue;
                tags.Exchange = exchange;
                tags.RoutingKey = routingKey;

                tags.InstrumentationName = RabbitMQConstants.IntegrationName;
                tags.SetAnalyticsSampleRate(RabbitMQConstants.IntegrationId, tracer.Settings, enabledWithGlobalSetting: false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating or populating scope.");
            }

            // always returns the scope, even if it's null because we couldn't create it,
            // or we couldn't populate it completely (some tags is better than no tags)
            return scope;
        }
    }
}