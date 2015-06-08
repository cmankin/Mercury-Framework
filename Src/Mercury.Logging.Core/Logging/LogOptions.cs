using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// Specifies various data options that can be written to log output.
    /// </summary>
    [Flags]
    public enum LogOptions
    {
        /// <summary>
        /// Do not write any elements.
        /// </summary>
        None = 0,

        /// <summary>
        /// Write the date and time at which the log occurred as Coordinated Universal Time (UTC).
        /// </summary>
        DateTime = 1,

        /// <summary>
        /// Writes the timestamp, which is the return value of the Stopwatch.GetTimestamp method.
        /// </summary>
        Timestamp = 2,

        /// <summary>
        /// Write the process identity, which is the return value of the ProcessId property.
        /// </summary>
        ProcessId = 4,

        /// <summary>
        /// Write the thread identity, which is the return value of the Thread.ManagedThreadId property of the logging thread.
        /// </summary>
        ThreadId = 8,

        /// <summary>
        /// Write the call stack, which is represented by the return value of the Environment.StackTrace property.
        /// </summary>
        Callstack = 16
    }
}
