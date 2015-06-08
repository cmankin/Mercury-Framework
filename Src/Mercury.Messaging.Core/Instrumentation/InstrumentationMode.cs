using System;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Describes the state of the instrumentation.
    /// </summary>
    public enum InstrumentationMode
    {
        /// <summary>
        /// Instrumentation is inactive.
        /// </summary>
        None,

        /// <summary>
        /// Only errors are reported to the specified log source.
        /// </summary>
        ErrorOnly,

        /// <summary>
        /// Information is monitored by an external application but not logged.
        /// </summary>
        MonitorOnly,

        /// <summary>
        /// Full debug and error information is logged.
        /// </summary>
        Debug,

        /// <summary>
        /// Monitoring is active and debug information is logged.
        /// </summary>
        MonitorDebug
    }
}
