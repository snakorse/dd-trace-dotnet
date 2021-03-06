#if NETFRAMEWORK
using Datadog.Trace.RuntimeMetrics;
using Datadog.Trace.Vendors.StatsdClient;
using Moq;
using Xunit;

namespace Datadog.Trace.Tests.RuntimeMetrics
{
    public class PerformanceCountersListenerTests
    {
        [Fact]
        public void PushEvents()
        {
            var statsd = new Mock<IDogStatsd>();

            using var listener = new PerformanceCountersListener(statsd.Object);

            listener.Refresh();

            statsd.Verify(s => s.Gauge(MetricsNames.Gen0HeapSize, It.IsAny<long?>(), 1, null), Times.Once);
            statsd.Verify(s => s.Gauge(MetricsNames.Gen1HeapSize, It.IsAny<long?>(), 1, null), Times.Once);
            statsd.Verify(s => s.Gauge(MetricsNames.Gen2HeapSize, It.IsAny<long?>(), 1, null), Times.Once);
            statsd.Verify(s => s.Gauge(MetricsNames.LohSize, It.IsAny<long?>(), 1, null), Times.Once);

            statsd.VerifyNoOtherCalls();
            statsd.ResetCalls();

            listener.Refresh();

            statsd.Verify(s => s.Gauge(MetricsNames.Gen0HeapSize, It.IsAny<long?>(), 1, null), Times.Once);
            statsd.Verify(s => s.Gauge(MetricsNames.Gen1HeapSize, It.IsAny<long?>(), 1, null), Times.Once);
            statsd.Verify(s => s.Gauge(MetricsNames.Gen2HeapSize, It.IsAny<long?>(), 1, null), Times.Once);
            statsd.Verify(s => s.Gauge(MetricsNames.LohSize, It.IsAny<long?>(), 1, null), Times.Once);

            // Those metrics aren't pushed the first time (differential count)
            statsd.Verify(s => s.Increment(MetricsNames.Gen0CollectionsCount, It.IsAny<int>(), 1, null), Times.Once);
            statsd.Verify(s => s.Increment(MetricsNames.Gen1CollectionsCount, It.IsAny<int>(), 1, null), Times.Once);
            statsd.Verify(s => s.Increment(MetricsNames.Gen2CollectionsCount, It.IsAny<int>(), 1, null), Times.Once);
            statsd.Verify(s => s.Counter(MetricsNames.ContentionCount, It.IsAny<long?>(), 1, null), Times.Once);

            statsd.VerifyNoOtherCalls();
        }
    }
}


#endif
