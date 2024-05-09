#if NETFRAMEWORK
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using NLog.Config;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace NLog.Etw.Tests
{
    public class EtwExtendedTargetTest
    {
        private readonly NLogEtwExtendedTarget etwTarget;

        public EtwExtendedTargetTest()
        {
            // setup NLog configuration
            var loggingConfiguration = new LoggingConfiguration();
            this.etwTarget = new NLogEtwExtendedTarget() { Layout = Layout.FromString("${uppercase:${level}}|${logger}|${message}") };

            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, this.etwTarget));
            loggingConfiguration.AddTarget("etw", this.etwTarget);
            LogManager.Configuration = loggingConfiguration;
        }

        [Fact]
        public void Writing_Message_To_Etw()
        {
            var providerName = "LowLevelDesign-NLogEtwSource";
            var resetEvent = new ManualResetEvent(false);
            var fpath = Path.Combine(Path.GetTempPath(), "_etwnlogtest.etl");
            using (var session = new TraceEventSession("SimpleMonitorSession", fpath))
            {
                Thread.Sleep(1000);

                var eventSourceGuid = TraceEventProviders.GetEventSourceGuidFromName(providerName);

                try
                {
                    session.EnableProvider(eventSourceGuid);
                    Thread.Sleep(1000);
                }
                catch
                {
                    Thread.Sleep(1000);
                    eventSourceGuid = TraceEventProviders.GetEventSourceGuidFromName(providerName);
                    session.EnableProvider(eventSourceGuid);
                }

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
                    session.DisableProvider(eventSourceGuid);
                }
                catch
                {
                    Thread.Sleep(1000);
                    session.DisableProvider(eventSourceGuid);
                }

                Thread.Sleep(1000);

                logger.Fatal("don't log this one");
            }

            var collectedEvents = new List<NLogEtwEventData>(5);
            using (var source = new ETWTraceEventSource(fpath))
            {
                var parser = new DynamicTraceEventParser(source);
                parser.All += delegate (TraceEvent data)
                {
                    if (data.Level == TraceEventLevel.Always)
                        return;   // Not ours

                    collectedEvents.Add(new NLogEtwEventData(data)
                    {
                        LoggerName = (string)data.PayloadByName("LoggerName"),
                        Message = (string)data.PayloadByName("Message")
                    });

                    if (collectedEvents.Count == 5)
                    {
                        resetEvent.Set();
                    }
                };
                source.Process();
            }
            File.Delete(fpath);

            // assert collected events
            var expectedEvents = new NLogEtwEventData[] {
                new NLogEtwEventData { EventId = 1, ProviderName = providerName, TaskName = "Verbose", LoggerName = "A", Level = TraceEventLevel.Verbose, Message = "DEBUG|A|test-debug" },
                new NLogEtwEventData { EventId = 2, ProviderName = providerName, TaskName = "Info", LoggerName = "A", Level = TraceEventLevel.Informational, Message = "INFO|A|test-info" },
                new NLogEtwEventData { EventId = 3, ProviderName = providerName, TaskName = "Warn", LoggerName = "A", Level = TraceEventLevel.Warning, Message = "WARN|A|test-warn" },
                new NLogEtwEventData { EventId = 4, ProviderName = providerName, TaskName = "Error", LoggerName = "A", Level = TraceEventLevel.Error, Message = "ERROR|A|test-error" },
                new NLogEtwEventData { EventId = 5, ProviderName = providerName, TaskName = "Critical", LoggerName = "A", Level = TraceEventLevel.Critical, Message = "FATAL|A|test-fatal" }
            };
            resetEvent.WaitOne(20000);
            Assert.Equal(collectedEvents, expectedEvents);
        }
    }
}

#endif