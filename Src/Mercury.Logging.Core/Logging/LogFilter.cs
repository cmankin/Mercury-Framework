using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// Represents a filter on log entries.
    /// </summary>
    public abstract class LogFilter
    {
        /// <summary>
        /// Returns a value indicating whether the specified log entry can pass the filter.
        /// </summary>
        /// <param name="entry">The log entry to test.</param>
        /// <returns>True if the specified log entry can pass the filter; otherwise, false.</returns>
        public abstract bool Allow(LogEntry entry);
    }
}
