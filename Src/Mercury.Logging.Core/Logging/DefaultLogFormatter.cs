using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// A basic formatter for log data.
    /// </summary>
    public class DefaultLogFormatter
        : LogFormatter
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.DefaultLogFormatter"/> class.
        /// </summary>
        public DefaultLogFormatter()
        {
        }

        /// <summary>
        /// Returns a formatted string that represents the specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to format.</param>
        /// <returns>A formatted string that represents the specified log entry.</returns>
        public override string Format(LogEntry entry)
        {
            if (entry.WriteDirect)
                return entry.Message;
            if (this.Options != LogOptions.None)
            {
                var sb = new StringBuilder();
                sb.Append(this.GetFormattedText(entry));
                this.GetFormattedOptions(entry, sb);
                return sb.ToString();
            }
            return this.GetFormattedText(entry);
        }

        /// <summary>
        /// Returns a string representing the specified log entry.  This string representation 
        /// does not include any additional options that may be enabled.
        /// </summary>
        /// <param name="entry">The log entry to format.</param>
        /// <returns>A string representing the specified log entry and not including additional logging options.</returns>
        protected string GetFormattedText(LogEntry entry)
        {
            if (entry == null)
                return "";
            return string.Format("{4}{0} {1} [{2}] : {3}", entry.LoggerName, entry.Severity.ToString(),
                entry.EventId.ToString(), entry.Message, Environment.NewLine);
        }
    }
}
