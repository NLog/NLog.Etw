#if NETFRAMEWORK
using System;
using Microsoft.Diagnostics.Tracing;

namespace NLog.Etw.Tests
{
    public class NLogEtwEventData
    {
        public NLogEtwEventData()
        {
        }

        public NLogEtwEventData(TraceEvent traceEvent)
        {
            EventId = (int)traceEvent.ID;
            Level = traceEvent.Level;
            ProviderName = traceEvent.ProviderName;
            TaskName = traceEvent.TaskName;
        }

        public TraceEventLevel Level { get; set; }

        public int EventId { get; set; }

        public string ProviderName { get; set; }

        public string TaskName { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj == this)
                return true;
            var ev = obj as NLogEtwEventData;
            if (ev == null)
                return false;
            return ev.Level == this.Level
                && string.Equals(ev.ProviderName, this.ProviderName, StringComparison.Ordinal)
                && (string.Equals(ev.TaskName, this.TaskName, StringComparison.Ordinal) || ev.TaskName?.IndexOf(this.TaskName) >= 0 || this.TaskName?.IndexOf(ev.TaskName) >= 0)
                && string.Equals(ev.Message, this.Message, StringComparison.Ordinal)
                && string.Equals(ev.LoggerName, this.LoggerName, StringComparison.Ordinal)
                && ev.EventId == this.EventId;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }

        public override string ToString()
        {
            return $"ProviderName={ProviderName} Level={Level} TaskName={TaskName} EventId={EventId} Logger={LoggerName} Message={Message}";
        }
    }
}

#endif