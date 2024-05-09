#if NETFRAMEWORK
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace NLog.Etw.Tests
{
    public class EtwEventSourceTargetTests
    {
        private readonly EtwEventSourceTarget etwTarget;

        public EtwEventSourceTargetTests()
        {
            // setup NLog configuration
            var loggingConfiguration = new LoggingConfiguration();
            this.etwTarget = new EtwEventSourceTarget() { ProviderName = nameof(EtwEventSourceTargetTests), Layout = Layout.FromString("${uppercase:${level}}|${logger}|${message}") };

            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, this.etwTarget));
            loggingConfiguration.AddTarget("etw", this.etwTarget);
            LogManager.Configuration = loggingConfiguration;
        }

        [Fact]
        public void Writing_Message_To_Etw()
        {
            var resetEvent = new ManualResetEvent(false);
            var collectedEvents = new List<NLogEtwEventData>(5);

            var providerName = etwTarget.ProviderName.Render(LogEventInfo.CreateNullEvent());

            using (var session = new TraceEventSession("SimpleMonitorSession"))
            {
                // Dynamic-Parser does not work with pure EventSource-objects
                session.Source.Registered.All += delegate (TraceEvent data)
                {
                    if (data.Level == TraceEventLevel.Always)
                        return;   // Not ours

                    collectedEvents.Add(new NLogEtwEventData(data)
                    {
                        EventId = 0, // Raw EventSource gives "random" EventId
                        LoggerName = (string)data.PayloadByName("LoggerName"),
                        Message = (string)data.PayloadByName("Message"),
                    });

                    if (collectedEvents.Count == 5)
                    {
                        resetEvent.Set();
                    }
                };
                session.Source.UnhandledEvents += delegate (TraceEvent data)
                {
                    if ((int)data.ID == 0xFFFE)
                        return; // The EventSource manifest events show up as unhanded, filter them out.
                };

                var task = System.Threading.Tasks.Task.Run(() => session.Source.Process());

                Thread.Sleep(1000);

                session.EnableProvider(providerName);

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
                    var eventSourceGuid = TraceEventProviders.GetEventSourceGuidFromName(providerName);
                    session.DisableProvider(eventSourceGuid);
                }
                catch
                {
                    Thread.Sleep(1000);
                    var eventSourceGuid = TraceEventProviders.GetEventSourceGuidFromName(providerName);
                    session.DisableProvider(eventSourceGuid);
                }

                Thread.Sleep(1000);

                logger.Fatal("don't log this one");
                resetEvent.WaitOne(20000);
            }

            // assert collected events
            var expectedEvents = new NLogEtwEventData[] {
                new NLogEtwEventData { EventId = 0, ProviderName = providerName, TaskName = "Debug", Level = TraceEventLevel.Verbose, LoggerName = "A", Message = "DEBUG|A|test-debug" },
                new NLogEtwEventData { EventId = 0, ProviderName = providerName, TaskName = "Info", Level = TraceEventLevel.Informational, LoggerName = "A", Message = "INFO|A|test-info" },
                new NLogEtwEventData { EventId = 0, ProviderName = providerName, TaskName = "Warn", Level = TraceEventLevel.Warning, LoggerName = "A", Message = "WARN|A|test-warn" },
                new NLogEtwEventData { EventId = 0, ProviderName = providerName, TaskName = "Error", Level = TraceEventLevel.Error, LoggerName = "A", Message = "ERROR|A|test-error" },
                new NLogEtwEventData { EventId = 0, ProviderName = providerName, TaskName = "Fatal", Level = TraceEventLevel.Critical,  LoggerName = "A", Message = "FATAL|A|test-fatal" }
            };
            Assert.Equal(collectedEvents, expectedEvents);
        }

        [Fact]
        public void Writing_Exception_To_Etw()
        {
            var resetEvent = new ManualResetEvent(false);
            var collectedEvents = new List<NLogEtwEventData>(5);

            var providerName = etwTarget.ProviderName.Render(LogEventInfo.CreateNullEvent());

            using (var session = new TraceEventSession("SimpleMonitorSession"))
            {
                // Dynamic-Parser does not work with pure EventSource-objects
                session.Source.Registered.All += delegate (TraceEvent data)
                {
                    if (data.Level == TraceEventLevel.Always)
                        return;   // Not ours

                    string exception = ((string)data.PayloadByName("Exception"));
                    exception = exception?.Substring(0, exception.IndexOf(Environment.NewLine)) ?? string.Empty;

                    collectedEvents.Add(new NLogEtwEventData(data)
                    {
                        EventId = 0, // Raw EventSource gives "random" EventId
                        LoggerName = (string)data.PayloadByName("LoggerName"),
                        Message = (string)data.PayloadByName("Message") + "|" + exception,
                    });

                    if (collectedEvents.Count == 1)
                    {
                        resetEvent.Set();
                    }
                };
                session.Source.UnhandledEvents += delegate (TraceEvent data)
                {
                    if ((int)data.ID == 0xFFFE)
                        return; // The EventSource manifest events show up as unhanded, filter them out.
                };

                var task = System.Threading.Tasks.Task.Run(() => session.Source.Process());

                Thread.Sleep(1000);

                session.EnableProvider(providerName);

                Thread.Sleep(1000);

                // send events to session
                var logger = LogManager.GetLogger("A");
                try
                {
                    if (!task.IsCanceled)
                        throw new InvalidProgramException("Best exception in the world");
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex, "test-fatal");
                }

                resetEvent.WaitOne(20000);
            }

            // assert collected events
            var expectedEvents = new NLogEtwEventData[] {
                new NLogEtwEventData { EventId = 0, ProviderName = providerName, TaskName = "Fatal", Level = TraceEventLevel.Critical,  LoggerName = "A", Message = "FATAL|A|test-fatal|System.InvalidProgramException: Best exception in the world" }
            };
            Assert.Equal(collectedEvents, expectedEvents);
        }
    }
}

#endif