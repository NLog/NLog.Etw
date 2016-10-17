using NLog.Targets;
using Microsoft.Diagnostics.Tracing;

namespace NLog.Etw
{
    /// <summary>
    /// A NLog target with support for channel-enabled ETW tracing. When using perfview or wpr to record the events use *NLog-LogEvents
    /// to enable the NLog provider.
    /// 
    /// Channel alignment based on best practices documented here: https://blogs.msdn.microsoft.com/vancem/2012/08/14/etw-in-c-controlling-which-events-get-logged-in-an-system-diagnostics-tracing-eventsource/ 
    /// </summary>
    [Target("ExtendedEventTracing")]
    public sealed class NLogEtwExtendedTarget : TargetWithLayout
    {
        [EventSource(Name = "NLog-LogEvents")]
        private sealed class EtwLogger : EventSource
        {
            [Event(1, Level = EventLevel.Verbose, Message = "{0}: {1}", Channel = EventChannel.Debug)]
            public void Verbose(string LoggerName, string Message)
            {
                WriteEvent(1, LoggerName, Message);
            }

            [Event(2, Level = EventLevel.Informational, Message = "{0}: {1}", Channel = EventChannel.Operational)]
            public void Info(string LoggerName, string Message)
            {
                WriteEvent(2, LoggerName, Message);
            }

            [Event(3, Level = EventLevel.Warning, Message = "{0}: {1}", Channel = EventChannel.Admin)]
            public void Warn(string LoggerName, string Message)
            {
                WriteEvent(3, LoggerName, Message);
            }

            [Event(4, Level = EventLevel.Error, Message = "{0}: {1}", Channel = EventChannel.Admin)]
            public void Error(string LoggerName, string Message)
            {
                WriteEvent(4, LoggerName, Message);
            }

            [Event(5, Level = EventLevel.Critical, Message="{0}: {1}", Channel = EventChannel.Admin)]
            public void Critical(string LoggerName, string Message)
            {
                WriteEvent(5, LoggerName, Message);
            }

            internal readonly static EtwLogger Log = new EtwLogger();
        }

        /// <summary>
        /// Write to event to ETW.
        /// </summary>
        /// <param name="logEvent">event to be written.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (!EtwLogger.Log.IsEnabled())
            {
                return;
            }

            var message = Layout.Render(logEvent);
            if (logEvent.Level == LogLevel.Debug || logEvent.Level == LogLevel.Trace)
            {
                EtwLogger.Log.Verbose(logEvent.LoggerName, message);
            }
            else if (logEvent.Level == LogLevel.Info)
            {
                EtwLogger.Log.Info(logEvent.LoggerName, message);
            }
            else if (logEvent.Level == LogLevel.Warn)
            {
                EtwLogger.Log.Warn(logEvent.LoggerName, message);
            }
            else if (logEvent.Level == LogLevel.Error)
            {
                EtwLogger.Log.Error(logEvent.LoggerName, message);
            }
            else //if (logEvent.Level == LogLevel.Fatal)
            {
                EtwLogger.Log.Critical(logEvent.LoggerName, message);
            }
        }
    }
}