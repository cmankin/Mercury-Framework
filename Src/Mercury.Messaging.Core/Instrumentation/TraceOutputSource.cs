using System;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Describes the destination of trace output.
    /// </summary>
    public enum TraceOutputSource
    {
        /// <summary>
        /// Outputs trace information to the Windows Event Log.
        /// </summary>
        WindowsEventLog,

        /// <summary>
        /// Outputs trace information to a text file.
        /// </summary>
        File,

        /// <summary>
        /// Outputs trace information to a SQL database.
        /// </summary>
        Sql
    }
}
