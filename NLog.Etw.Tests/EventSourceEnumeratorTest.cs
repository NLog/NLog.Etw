using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Xunit;

namespace NLog.Etw.Tests
{
    public class EventSourceEnumeratorTest
    {
        [Fact]
        public void EtwEventSourceTargetTest()
        {
            // setup NLog configuration
            var loggingConfiguration = new LoggingConfiguration();
            var etwTarget = new EtwEventSourceTarget() { ProviderName = nameof(EventSourceEnumeratorTest), Layout = Layout.FromString("${uppercase:${level}}|${message}") };
            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, etwTarget));
            loggingConfiguration.AddTarget("etw", etwTarget);
            LogManager.Configuration = loggingConfiguration;

            using (var eventSourceEnumerator = new EventSourceEnumerator())
            {
                // send events to session
                var logger = LogManager.GetLogger("A");
                logger.Debug("test-debug");
                logger.Info("test-info");
                logger.Warn("test-warn");
                logger.Error("test-error");
                logger.Fatal("test-fatal");

                Assert.NotEmpty(eventSourceEnumerator.Events);
                Assert.True(eventSourceEnumerator.Events.ContainsKey(nameof(EventSourceEnumeratorTest)));
                Assert.NotEmpty(eventSourceEnumerator.Events[nameof(EventSourceEnumeratorTest)]);
                Assert.Contains("test-fatal", eventSourceEnumerator.Events[nameof(EventSourceEnumeratorTest)].LastOrDefault());
            }
        }

        [Fact]
        public void EtwEventCounterTargetTest()
        {
            // setup NLog configuration
            var loggingConfiguration = new LoggingConfiguration();
            var etwTarget = new EtwEventCounterTarget() { ProviderName = nameof(EventSourceEnumeratorTest), CounterName = nameof(EtwEventCounterTargetTest) };
            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, etwTarget));
            loggingConfiguration.AddTarget("etw", etwTarget);
            LogManager.Configuration = loggingConfiguration;

            using (var eventSourceEnumerator = new EventSourceEnumerator())
            {
                // send events to session
                var logger = LogManager.GetLogger("A");
                logger.Debug("test-debug");
                logger.Info("test-info");
                logger.Warn("test-warn");
                logger.Error("test-error");
                logger.Fatal("test-fatal");

                System.Threading.Thread.Sleep(10000);

                Assert.NotEmpty(eventSourceEnumerator.Events);
                Assert.True(eventSourceEnumerator.Events.ContainsKey(nameof(EventSourceEnumeratorTest)));
                Assert.NotEmpty(eventSourceEnumerator.Events[nameof(EventSourceEnumeratorTest)]);
            }
        }
    }
}
