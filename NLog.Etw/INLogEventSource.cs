#if NET45
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif

namespace NLog.Etw
{
    /// <summary>
    /// Allows custom <see cref="EventSource"/> to be used with <see cref="NLogEtwExtendedTarget"/>
    /// 
    /// Receives NLog <see cref="LogEventInfo"/> for writing to custom <see cref="EventSource"/> methods to ETW
    /// </summary>
    public interface INLogEventSource
    {
        /// <summary>
        /// Returns the real EventSource
        /// </summary>
        /// <remarks>
        /// The custom EventSource implementing this interface-property should just return this (or static instance)
        /// </remarks>
        EventSource EventSource { get; }

        /// <summary>
        /// Write NLog LogEvent to ETW 
        /// </summary>
        /// <remarks>
        /// The custom EventSource implementing this interface-method should mark the method with attribute [NonEvent]
        /// </remarks>
        /// <param name="eventLevel"></param>
        /// <param name="layoutMessage"></param>
        /// <param name="logEvent"></param>
        void Write(EventLevel eventLevel, string layoutMessage, LogEventInfo logEvent);
    }
}
