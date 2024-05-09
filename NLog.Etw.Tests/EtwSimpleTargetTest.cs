#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using NLog.Config;
using Nlog.Etw;
using NLog.Layouts;
using Xunit;

namespace NLog.Etw.Tests
{
    public class EtwSimpleTargetTest
    {
        private readonly NLogEtwTarget etwTarget;
        private readonly Guid providerId = Guid.NewGuid();

        public EtwSimpleTargetTest()
        {
            // setup NLog configuration
            var loggingConfiguration = new LoggingConfiguration();
            this.etwTarget = new NLogEtwTarget() { ProviderId = providerId.ToString(), Layout = Layout.FromString("${uppercase:${level}}|${logger}|${message}") };

            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, this.etwTarget));
            loggingConfiguration.AddTarget("etw", this.etwTarget);
            LogManager.Configuration = loggingConfiguration;
        }

        [Fact]
        public void CheckProviderId()
        {
            Assert.Equal(this.etwTarget.ProviderId, providerId.ToString());
        }

        [Fact]
        public void Writing_Message_To_Etw()
        {
            var resetEvent = new ManualResetEvent(false);
            var fpath = Path.Combine(Path.GetTempPath(), "_etwnlogtest.etl");
            using (var session = new TraceEventSession("SimpleMonitorSession", fpath))
            {
                Thread.Sleep(1000);

                try
                {
                    session.EnableProvider(providerId);
                    Thread.Sleep(1000);
                }
                catch
                {
                    Thread.Sleep(1000);
                    session.EnableProvider(providerId);
                }

                Thread.Sleep(1000);

                // send events to session
                var logger = LogManager.GetLogger("A");
                logger.Debug("test-debug");
                logger.Info("test-info");
                logger.Warn("test-warn");
                logger.Error("test-error");
                logger.Fatal("test-fatal");

                try
                {
                    Thread.Sleep(1000);
                    session.DisableProvider(providerId);
                }
                catch
                {
                    Thread.Sleep(1000);
                    session.DisableProvider(providerId);
                }

                Thread.Sleep(1000);

                logger.Fatal("don't log this one");
            }

            var collectedEvents = new List<NLogEtwEventData>(5);
            using (var source = new ETWTraceEventSource(fpath))
            {
                source.UnhandledEvents += delegate (TraceEvent data)
                {
                    if (data.Level == TraceEventLevel.Always)
                        return;   // Not ours

                    collectedEvents.Add(new NLogEtwEventData(data)
                    {
                        Message = data.FormattedMessage,
                    });

                    if (collectedEvents.Count == 5)
                    {
                        resetEvent.Set();
                    }
                };
                source.Process();
            }
            File.Delete(fpath);

            var providerName = $"Provider({providerId.ToString()})";

            // assert collected events
            var expectedEvents = new NLogEtwEventData[] {
                new NLogEtwEventData { ProviderName = providerName, TaskName = "EventWriteString", Level = TraceEventLevel.Verbose, Message = "DEBUG|A|test-debug" },
                new NLogEtwEventData { ProviderName = providerName, TaskName = "EventWriteString", Level = TraceEventLevel.Informational, Message = "INFO|A|test-info" },
                new NLogEtwEventData { ProviderName = providerName, TaskName = "EventWriteString", Level = TraceEventLevel.Warning, Message = "WARN|A|test-warn" },
                new NLogEtwEventData { ProviderName = providerName, TaskName = "EventWriteString", Level = TraceEventLevel.Error, Message = "ERROR|A|test-error" },
                new NLogEtwEventData { ProviderName = providerName, TaskName = "EventWriteString", Level = TraceEventLevel.Critical, Message = "FATAL|A|test-fatal" }
            };
            resetEvent.WaitOne(20000);
            Assert.Equal(expectedEvents.Length, collectedEvents.Count);
            Assert.Equal(expectedEvents, collectedEvents);
        }
    }
}

#endif