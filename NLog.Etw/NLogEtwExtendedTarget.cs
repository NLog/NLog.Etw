#if NET45
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Etw
{
    /// <summary>
    /// A NLog target with support for channel-enabled ETW tracing. When using perfview or wpr to record the events use *LowLevelDesign-NLogEtwSource
    /// to enable the NLog provider.
    /// 
    /// Sample configuration and usage sample can be found on my blog: http://lowleveldesign.wordpress.com/2014/04/18/etw-providers-for-nlog/
    /// 
    /// Channel alignment based on best practices documented here: https://blogs.msdn.microsoft.com/vancem/2012/08/14/etw-in-c-controlling-which-events-get-logged-in-an-system-diagnostics-tracing-eventsource/ 
    /// </summary>
    [Target("ExtendedEventTracing")]
    public sealed class NLogEtwExtendedTarget : TargetWithLayout
    {
        [EventSource(Name = "LowLevelDesign-NLogEtwSource")]
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
            if (EtwLogger.Log.IsEnabled())
            {
                if (logEvent.Level == LogLevel.Debug || logEvent.Level == LogLevel.Trace)
                {
                    if (EtwLogger.Log.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                    {
                        var message = Layout.Render(logEvent);
                        EtwLogger.Log.Verbose(logEvent.LoggerName, message);
                    }
                }
                else if (logEvent.Level == LogLevel.Info)
                {
                    if (EtwLogger.Log.IsEnabled(EventLevel.Informational, EventKeywords.None))
                    {
                        var message = Layout.Render(logEvent);
                        EtwLogger.Log.Info(logEvent.LoggerName, message);
                    }
                }
                else if (logEvent.Level == LogLevel.Warn)
                {
                    if (EtwLogger.Log.IsEnabled(EventLevel.Warning, EventKeywords.None))
                    {
                        var message = Layout.Render(logEvent);
                        EtwLogger.Log.Warn(logEvent.LoggerName, message);
                    }
                }
                else if (logEvent.Level == LogLevel.Error)
                {
                    if (EtwLogger.Log.IsEnabled(EventLevel.Error, EventKeywords.None))
                    {
                        var message = Layout.Render(logEvent);
                        EtwLogger.Log.Error(logEvent.LoggerName, message);
                    }
                }
                else //if (logEvent.Level == LogLevel.Fatal)
                {
                    var message = Layout.Render(logEvent);
                    EtwLogger.Log.Critical(logEvent.LoggerName, message);
                }
            }
        }
    }
}