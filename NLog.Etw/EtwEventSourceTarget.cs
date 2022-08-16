using System;
using System.Collections.Concurrent;
#if NET45
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif
using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// NLog Target to ETW EventSource. Supporting dynamic EventSource name
    /// </summary>
    [Target("EtwEventSource")]
    public class EtwEventSourceTarget : TargetWithLayout
    {
        private static readonly ConcurrentDictionary<string, EventSource> EventSources = new ConcurrentDictionary<string, EventSource>(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Name used for the EventSouce contructor
        /// </summary>
        [RequiredParameter]
        public Layout ProviderName { get; set; }

        /// <summary>
        /// Context TaskName for EventData
        /// </summary>
        public Layout EventTaskName { get; set; } = "${level}";

        private EventSource _eventSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwEventSourceTarget"/> class.
        /// </summary>
        public EtwEventSourceTarget()
        {
            Layout = "${message}";  // Timestamp + LogLevel is included in the ETW-EventData
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            var providerName = RenderLogEvent(ProviderName, LogEventInfo.CreateNullEvent())?.Trim();
            if (string.IsNullOrEmpty(providerName))
            {
                throw new NLogConfigurationException("EtwEventSourceTarget - ProviderName must be configured");
            }

            if (!EventSources.TryGetValue(providerName, out _eventSource))
            {
                _eventSource = new EventSource(providerName, EventSourceSettings.EtwSelfDescribingEventFormat);
                if (_eventSource.ConstructionException != null)
                {
                    NLog.Common.InternalLogger.Error("EtwEventSourceTarget(Name={0}): EventSource({1}) constructor threw exception: {2}", Name, providerName, _eventSource.ConstructionException);
                }
                if (!EventSources.TryAdd(providerName, _eventSource))
                {
                    _eventSource = EventSources[providerName];
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
            if (_eventSource?.IsEnabled() == true)
            {
                if (logEvent.Level == LogLevel.Debug || logEvent.Level == LogLevel.Trace)
                {
                    WriteEvent(logEvent, EventLevel.Verbose);
                }
                else if (logEvent.Level == LogLevel.Info)
                {
                    WriteEvent(logEvent, EventLevel.Informational);
                }
                else if (logEvent.Level == LogLevel.Warn)
                {
                    WriteEvent(logEvent, EventLevel.Warning);
                }
                else if (logEvent.Level == LogLevel.Error)
                {
                    WriteEvent(logEvent, EventLevel.Error);
                }
                else //if (logEvent.Level == LogLevel.Fatal)
                {
                    WriteEvent(logEvent, EventLevel.Critical);
                }
            }
        }

        private void WriteEvent(LogEventInfo logEvent, EventLevel level)
        {
            if (_eventSource.IsEnabled(level, EventKeywords.None))
            {
                var message = RenderLogEvent(Layout, logEvent);
                var taskName = RenderLogEvent(EventTaskName, logEvent) ?? string.Empty;

                var sourceOptions = new EventSourceOptions
                {
                    Level = level,
                    Opcode = EventOpcode.Info
                };
                if (logEvent.Exception != null)
                {
                    var eventData = new NLogEventDataException
                    {
                        LoggerName = logEvent.LoggerName,
                        Message = message,
                        Exception = logEvent.Exception.ToString()
                    };
                    _eventSource.Write(taskName, ref sourceOptions, ref eventData);
                }
                else
                {
                    var eventData = new NLogEventData
                    {
                        LoggerName = logEvent.LoggerName,
                        Message = message
                    };
                    _eventSource.Write(taskName, ref sourceOptions, ref eventData);
                }
            }
        }

        [EventData]
        private struct NLogEventData
        {
            public string LoggerName { get; set; }
            public string Message { get; set; }
        }

        [EventData]
        private struct NLogEventDataException
        {
            public string LoggerName { get; set; }
            public string Message { get; set; }
            public string Exception { get; set; }
        }
    }
}
