using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Mercury.Logging
{
    /// <summary>
    /// Enables formatting of log entry messages before output.
    /// </summary>
    public abstract class LogFormatter
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.LogFormatter"/> class.
        /// </summary>
        protected LogFormatter()
        {
        }

        private string _dateTimeFormat = "o";

        /// <summary>
        /// Gets or sets the log output options.
        /// </summary>
        public LogOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the format according to a vaild DateTime format character or pattern.
        /// </summary>
        public string DateTimeFormat
        {
            get { return this._dateTimeFormat; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this._dateTimeFormat = "o";
                else
                    this._dateTimeFormat = value;
            }
        }

        /// <summary>
        /// Returns a formatted string that represents the specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to format.</param>
        /// <returns>A formatted string that represents the specified log entry.</returns>
        public abstract string Format(LogEntry entry);

        /// <summary>
        /// Gets a formatted string representation of the specified date and time.
        /// </summary>
        /// <param name="dt">The date and time value to format.</param>
        /// <returns></returns>
        public string GetFormattedDateTime(DateTime dt)
        {
            return dt.ToString(this.DateTimeFormat);
        }

        /// <summary>
        /// Builds a formatted string representing additional log options for the specified entry.
        /// </summary>
        protected virtual void GetFormattedOptions(LogEntry entry, StringBuilder sb)
        {
            if ((this.Options & LogOptions.DateTime) == LogOptions.DateTime)
                sb.AppendFormat("{0}\tDateTime:{1}", Environment.NewLine, this.GetFormattedDateTime(entry.LoggedDateTime));
            if ((this.Options & LogOptions.Timestamp) == LogOptions.Timestamp)
                sb.AppendFormat("{0}\tTimestamp:{1}", Environment.NewLine, entry.Timestamp);
            if ((this.Options & LogOptions.ProcessId) == LogOptions.ProcessId)
                sb.AppendFormat("{0}\tProcessId:{1}", Environment.NewLine, entry.ProcessId);
            if ((this.Options & LogOptions.ThreadId) == LogOptions.ThreadId)
                sb.AppendFormat("{0}\tThreadId:{1}", Environment.NewLine, entry.ThreadId);
            if ((this.Options & LogOptions.Callstack) == LogOptions.Callstack)
                sb.AppendFormat("{0}\tCallstack:{1}", Environment.NewLine, entry.Callstack);
        }
    }
}
