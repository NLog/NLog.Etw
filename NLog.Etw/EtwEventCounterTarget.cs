#if !NET46
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Etw
{
    /// <summary>
    /// NLog Target to ETW EventCounter. Supporting dynamic EventSource name
    /// </summary>
    [Target("EtwEventCounter")]
    public class EtwEventCounterTarget : Target
    {
        private static readonly ConcurrentDictionary<string, EventCounter> EventCounters = new ConcurrentDictionary<string, EventCounter>(StringComparer.OrdinalIgnoreCase);

        private EventCounter? _eventCounter;

        /// <summary>
        /// Name used for the <see cref="EventSource" />-contructor
        /// </summary>
        public Layout ProviderName { get; set; } = Layout.Empty;

        /// <summary>
        /// Name used for the <see cref="EventCounter" />-contructor
        /// </summary>
        public Layout CounterName { get; set; } = Layout.Empty;

        /// <summary>
        /// The value by which to increment the counter.
        /// </summary>
        public Layout<int> MetricValue { get; set; } = 1;

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            var providerName = RenderLogEvent(ProviderName, LogEventInfo.CreateNullEvent())?.Trim();
            if (providerName is null || string.IsNullOrEmpty(providerName))
            {
                throw new NLogConfigurationException("EtwEventCounterTarget - ProviderName must be configured");
            }

            var counterName = RenderLogEvent(CounterName, LogEventInfo.CreateNullEvent())?.Trim();
            if (counterName is null || string.IsNullOrEmpty(counterName))
            {
                throw new NLogConfigurationException("EtwEventCounterTarget - CounterName must be configured");
            }

            if (!EtwEventSourceTarget.EventSources.TryGetValue(providerName, out var eventSource))
            {
                eventSource = new EventSource(providerName, EventSourceSettings.EtwSelfDescribingEventFormat);
                if (eventSource.ConstructionException != null)
                {
                    NLog.Common.InternalLogger.Error("EtwEventCounterTarget(Name={0}): EventSource({1}) constructor threw exception: {2}", Name, providerName, eventSource.ConstructionException);
                }
                if (!EtwEventSourceTarget.EventSources.TryAdd(providerName, eventSource))
                {
                    eventSource = EtwEventSourceTarget.EventSources[providerName];
                }
            }

            if (!EventCounters.TryGetValue(counterName, out _eventCounter))
            {
                _eventCounter = new EventCounter(counterName, eventSource);
                if (!EventCounters.TryAdd(counterName, _eventCounter))
                {
                    _eventCounter = EventCounters[counterName];
                }
            }

            base.InitializeTarget();
        }

        /// <summary>
        /// Write LogEvent to ETW.
        /// </summary>
        /// <param name="logEvent">event to be written.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            int metricValue = RenderLogEvent(MetricValue, logEvent);
            _eventCounter?.WriteMetric(metricValue);
        }
    }
}

#endif