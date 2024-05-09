#if NETFRAMEWORK
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace NLog.Etw.Tests
{
    internal sealed class EventSourceEnumerator : EventListener
    {
        public ConcurrentDictionary<string, List<string>> Events { get; } = new ConcurrentDictionary<string, List<string>>();

        public EventSourceEnumerator()
        {
            EventSourceCreated += OnEventSourceCreated;
            EventWritten += OnEventWritten;
        }

#if NETFRAMEWORK
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            OnEventWritten(this, eventData);
        }
#endif

        private void OnEventWritten(object sender, EventWrittenEventArgs e)
        {
            var events = Events[e.EventSource.Name];
            if (events != null)
            {
                var message = e.Message;
                if (string.IsNullOrEmpty(message) && e.Payload?.Count > 0)
                {
                    for (int i = 0; i < e.Payload?.Count; ++i)
                    {
                        if (!string.IsNullOrEmpty(message))
                            message += "|";

                        if (e.Payload[i] is IEnumerable complexPayload && !(complexPayload is string))
                        {
                            foreach (var item in complexPayload)
                                message += item.ToString() + " ,";
                        }
                        else
                        {
                            message += e.Payload[i].ToString();
                        }
                    }
                }
                events.Add(message);
            }
        }

        private void OnEventSourceCreated(object sender, EventSourceCreatedEventArgs e)
        {
            if (e.EventSource is null)
                return;

            if (!Events.ContainsKey(e.EventSource.Name))
            {
                var args = new Dictionary<string, string> { ["EventCounterIntervalSec"] = "1" };
                EnableEvents(e.EventSource, EventLevel.LogAlways, EventKeywords.All, args);
                Events.TryAdd(e.EventSource.Name, new List<string>());
            }
        }
    }
}
