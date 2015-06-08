using System;

namespace Mercury.Logging
{
    /// <summary>
    /// Represents a context for filter data to be applied to a message.
    /// </summary>
    public sealed class FilterContext
        : Logger
    {
        internal FilterContext(Logger logger, object filterData)
        {
            this._logger = logger;
            this.Filter = null;
            this.FilterData = filterData;
        }

        private Logger _logger;

        /// <summary>
        /// Gets the filter data for this context.
        /// </summary>
        public object FilterData { get; private set; }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Determines whether the specified log entry can pass filtering.
        /// </summary>
        /// <param name="entry">The log entry to test.</param>
        /// <returns>True if the log entry passes filtering; otherwise, false.</returns>
        protected override bool AllowEntry(LogEntry entry)
        {
            if (entry.Severity < this.SeverityThreshold)
                return false;
            if (this._logger == null)
                return false;
            if (this._logger.Filter == null)
                return true;
            entry.FilterData = this.FilterData;
            return this._logger.Filter.Allow(entry);
        }

        /// <summary>
        /// Logs the specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool DoLogEntry(LogEntry entry)
        {
            if (entry == null)
                return false;
            return this._logger.Log(entry);
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            var entry = this._logger.GetEntry(this.Name, LogOptions.None, LogSeverity.Critical, 0, null, message, null, false, true);
            if (!this.AllowEntry(entry))
                return false;
            return this._logger.Write(message);
        }
    }
}
