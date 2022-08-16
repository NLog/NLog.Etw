#if !NETSTANDARD

using NLog;
using NLog.Targets;
using System;
using System.Diagnostics.Eventing;
using NLog.Common;

namespace Nlog.Etw
{
    /// <summary>
    /// A NLog target with support for basic ETW tracing. When using this provider you should specify providerId in NLog configuration 
    /// and use this GUID for further event collection, eg.:
    /// 
    /// &lt;target name="etw" type="contribetw.EventTracing" providerId="ff1d574a-58a1-45f1-ae5e-040cf8d3fae2" layout="${longdate}|${uppercase:${level}}|${message}${onexception:|Exception occurred\:${exception:format=tostring}}" /&gt;
    /// 
    /// Sample configuration and usage sample can be found also on my blog: http://lowleveldesign.wordpress.com/2014/04/18/etw-providers-for-nlog/
    /// </summary>
    [Target("EventTracing")]
    public sealed class NLogEtwTarget : TargetWithLayout
    {
        private EventProvider provider;
        private Guid providerId = Guid.NewGuid();

        /// <summary>
        /// A provider guid that will be used in ETW tracing.
        /// </summary>
        public string ProviderId
        {
            get => providerId.ToString();
            set => providerId = Guid.Parse(value);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            // we will create an EventProvider for ETW
            try
            {
                provider = new EventProvider(providerId);
            }
            catch (PlatformNotSupportedException ex)
            {
                InternalLogger.Error("InitializeTarget failed: " + ex);
            }
        }

        /// <summary>
        /// Write to event to ETW.
        /// </summary>
        /// <param name="logEvent">event to be written.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (provider == null || !provider.IsEnabled())
            {
                return;
            }
            byte t;
            if (logEvent.Level == LogLevel.Debug || logEvent.Level == LogLevel.Trace)
            {
                t = 5;
            }
            else if (logEvent.Level == LogLevel.Info)
            {
                t = 4;
            }
            else if (logEvent.Level == LogLevel.Warn)
            {
                t = 3;
            }
            else if (logEvent.Level == LogLevel.Error)
            {
                t = 2;
            }
            else //if (logEvent.Level == LogLevel.Fatal)
            {
                t = 1;
            }

            var message = RenderLogEvent(Layout, logEvent);
            provider.WriteMessageEvent(message, t, 0);
        }

        /// <summary>
        /// Close and dispose.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            provider.Dispose();
        }
    }
}

#endif