using System;
using System.Diagnostics;

namespace Mercury.Logging
{
    /// <summary>
    /// Represents an object through which messages may be logged.
    /// </summary>
    public abstract class Logger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Logger"/> class.
        /// </summary>
        protected Logger()
        {
            if (Logger.DefaultFilterType != null)
                this.Filter = Activator.CreateInstance(Logger.DefaultFilterType, true) as LogFilter;
        }

        private bool _formatMessageArguments = true;
        private LogSeverity _severityThreshold = LogSeverity.Debug;
        private LogFormatter _formatter = new DefaultLogFormatter();
        private static Type _defaultFilterType = typeof(PassFilter);

        /// <summary>
        /// Gets or sets the name of this logger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a value indicating whether to message strings will be formatted when arguments are provided.  The default value is true.
        /// </summary>
        public bool FormatMessageArguments
        {
            get { return this._formatMessageArguments; }
            set { this._formatMessageArguments = value; }
        }

        /// <summary>
        /// Gets or sets the type of the default filter.  If null, no filters will be set.
        /// </summary>
        public static Type DefaultFilterType
        {
            get { return _defaultFilterType; }
            set { _defaultFilterType = value; }
        }

        /// <summary>
        /// Gets or sets the formatter to use for this logger.
        /// </summary>
        public LogFormatter Formatter 
        {
            get { return this._formatter; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "A logger's formatter cannot be a null value.");
                this._formatter = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter for this logger.
        /// </summary>
        public LogFilter Filter { get; set; }

        /// <summary>
        /// Gets or sets the lowest log severity that will be handled by this logger.  By 
        /// default this is set to Debug, which includes messages of all severity levels.
        /// </summary>
        public LogSeverity SeverityThreshold
        {
            get { return this._severityThreshold; }
            set { this._severityThreshold = value; }
        }

        /// <summary>
        /// Returns a logger in a filter context for the specified data.
        /// </summary>
        /// <param name="filterData">The filter data to provide.</param>
        /// <returns>A logger in a filter context for the specified data.</returns>
        public Logger WithFilter(object filterData)
        {
            return new FilterContext(this, filterData);
        }

        /// <summary>
        /// Writes the specified debug message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public void Debug(Func<string> message)
        {
            this.Log(LogSeverity.Debug, message);
        }

        /// <summary>
        /// Writes the specified debug message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Debug(int eventId, Func<string> message)
        {
            this.Log(LogSeverity.Debug, eventId, message);
        }

        /// <summary>
        /// Writes the specified debug message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Debug(string message)
        {
            this.Log(LogSeverity.Debug, message);
        }

        /// <summary>
        /// Writes the specified debug message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">The message to write.</param>
        public void Debug(int eventId, string message)
        {
            this.Log(LogSeverity.Debug, eventId, message);
        }

        /// <summary>
        /// Writes the debug message using the specified argument array and format.
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Debug(string format, params object[] args)
        {
            this.Log(LogSeverity.Debug, format, args);
        }

        /// <summary>
        /// Writes the debug message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Debug(int eventId, string format, params object[] args)
        {
            this.Log(LogSeverity.Debug, eventId, format, args);
        }

        /// <summary>
        /// Writes the specified information message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public void Info(Func<string> message)
        {
            this.Log(LogSeverity.Info, message);
        }

        /// <summary>
        /// Writes the specified information message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Info(int eventId, Func<string> message)
        {
            this.Log(LogSeverity.Info, eventId, message);
        }

        /// <summary>
        /// Writes the specified information message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Info(string message)
        {
            this.Log(LogSeverity.Info, message);
        }

        /// <summary>
        /// Writes the specified information message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">The message to write.</param>
        public void Info(int eventId, string message)
        {
            this.Log(LogSeverity.Info, eventId, message);
        }

        /// <summary>
        /// Writes the information message using the specified argument array and format.
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Info(string format, params object[] args)
        {
            this.Log(LogSeverity.Info, format, args);
        }

        /// <summary>
        /// Writes the information message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Info(int eventId, string format, params object[] args)
        {
            this.Log(LogSeverity.Info, eventId, format, args);
        }

        /// <summary>
        /// Writes the specified warning message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public void Warn(Func<string> message)
        {
            this.Log(LogSeverity.Warning, message);
        }

        /// <summary>
        /// Writes the specified warning message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Warn(int eventId, Func<string> message)
        {
            this.Log(LogSeverity.Warning, eventId, message);
        }

        /// <summary>
        /// Writes the specified warning message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Warn(string message)
        {
            this.Log(LogSeverity.Warning, message);
        }

        /// <summary>
        /// Writes the specified warning message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">The message to write.</param>
        public void Warn(int eventId, string message)
        {
            this.Log(LogSeverity.Warning, eventId, message);
        }

        /// <summary>
        /// Writes the warning message using the specified argument array and format.
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Warn(string format, params object[] args)
        {
            this.Log(LogSeverity.Warning, format, args);
        }

        /// <summary>
        /// Writes the warning message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Warn(int eventId, string format, params object[] args)
        {
            this.Log(LogSeverity.Warning, eventId, format, args);
        }

        /// <summary>
        /// Writes the specified error message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public void Error(Func<string> message)
        {
            this.Log(LogSeverity.Error, message);
        }

        /// <summary>
        /// Writes the specified error message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Error(int eventId, Func<string> message)
        {
            this.Log(LogSeverity.Error, eventId, message);
        }

        /// <summary>
        /// Writes the specified error message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Error(string message)
        {
            this.Log(LogSeverity.Error, message);
        }

        /// <summary>
        /// Writes the specified error message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">The message to write.</param>
        public void Error(int eventId, string message)
        {
            this.Log(LogSeverity.Error, eventId, message);
        }

        /// <summary>
        /// Writes the error message using the specified argument array and format.
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Error(string format, params object[] args)
        {
            this.Log(LogSeverity.Error, format, args);
        }

        /// <summary>
        /// Writes the error message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Error(int eventId, string format, params object[] args)
        {
            this.Log(LogSeverity.Error, eventId, format, args);
        }

        /// <summary>
        /// Writes the specified critical error message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public void Critical(Func<string> message)
        {
            this.Log(LogSeverity.Critical, message);
        }

        /// <summary>
        /// Writes the specified critical error message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Critical(int eventId, Func<string> message)
        {
            this.Log(LogSeverity.Critical, eventId, message);
        }

        /// <summary>
        /// Writes the specified critical error message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Critical(string message)
        {
            this.Log(LogSeverity.Critical, message);
        }

        /// <summary>
        /// Writes the specified critical error message with the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">The message to write.</param>
        public void Critical(int eventId, string message)
        {
            this.Log(LogSeverity.Critical, eventId, message);
        }

        /// <summary>
        /// Writes the critical error message using the specified argument array and format.
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Critical(string format, params object[] args)
        {
            this.Log(LogSeverity.Critical, format, args);
        }

        /// <summary>
        /// Writes the critical error message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Critical(int eventId, string format, params object[] args)
        {
            this.Log(LogSeverity.Critical, eventId, format, args);
        }

        /// <summary>
        /// Logs the message with the specified severity level.
        /// </summary>
        /// <param name="severity">The severity level of the message to log.</param>
        /// <param name="message">The message to log.</param>
        public void Log(LogSeverity severity, string message)
        {
            var entry = this.GetEntry(this.Name, this.Formatter.Options, severity, 0, null, message, null, this._formatMessageArguments, false);
            if (this.AllowEntry(entry))
                this.DoLogEntry(entry);
        }

        /// <summary>
        /// Logs a message with the specified severity level, argument array, and format.
        /// </summary>
        /// <param name="severity">The severity level of the message to log.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Log(LogSeverity severity, string format, params object[] args)
        {
            var entry = this.GetEntry(this.Name, this.Formatter.Options, severity, 0, null, format, args, this._formatMessageArguments, false);
            if (this.AllowEntry(entry))
                this.DoLogEntry(entry);
        }

        /// <summary>
        /// Logs a message with the specified severity level, event identifier, argument array, and format.
        /// </summary>
        /// <param name="severity">The severity level of the message to log.</param>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public void Log(LogSeverity severity, int eventId, string format, params object[] args)
        {
            var entry = this.GetEntry(this.Name, this.Formatter.Options, severity, eventId, null, format, args, this._formatMessageArguments, false);
            if (this.AllowEntry(entry))
                this.DoLogEntry(entry);
        }

        /// <summary>
        /// Logs a message with the specified severity level.
        /// </summary>
        /// <param name="severity">The severity level of the message to log.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Log(LogSeverity severity, Func<string> message)
        {
            var entry = this.GetEntry(this.Name, this.Formatter.Options, severity, 0, message, null, null, this._formatMessageArguments, false);
            if (this.AllowEntry(entry))
                this.DoLogEntry(entry);
        }

        /// <summary>
        /// Logs a message with the specified severity level and event identifier.
        /// </summary>
        /// <param name="severity">The severity level of the message to log.</param>
        /// <param name="eventId">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public void Log(LogSeverity severity, int eventId, Func<string> message)
        {
            var entry = this.GetEntry(this.Name, this.Formatter.Options, severity, eventId, message, null, null, this._formatMessageArguments, false);
            if (this.AllowEntry(entry))
                this.DoLogEntry(entry);
        }

        /// <summary>
        /// Logs the specified <see cref="Mercury.Logging.LogEntry"/>.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        public bool Log(LogEntry entry)
        {
            return this.DoLogEntry(entry);
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Writes the specified output directly to the log source.
        /// </summary>
        /// <param name="output">The output text to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        public bool Write(string output)
        {
            return this.WriteLog(output);
        }

        /// <summary>
        /// Writes the specified output and appends a newline character to the end of it.
        /// </summary>
        /// <param name="output">The output text to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        public bool WriteLine(string output)
        {
            return this.Write(string.Format("{0}{1}", output, Environment.NewLine));
        }

        /// <summary>
        /// Logs the specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected virtual bool DoLogEntry(LogEntry entry)
        {
            if (entry == null)
                return false;
            return this.WriteLog(this.Formatter.Format(entry));
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected abstract bool WriteLog(string message);

        /// <summary>
        /// Determines whether the specified log entry can pass filtering.
        /// </summary>
        /// <param name="entry">The log entry to test.</param>
        /// <returns>True if the log entry passes filtering; otherwise, false.</returns>
        protected virtual bool AllowEntry(LogEntry entry)
        {
            if (entry.Severity < this.SeverityThreshold)
                return false;
            if (this.Filter == null)
                return true;
            return this.Filter.Allow(entry);
        }

        /// <summary>
        /// Gets a log entry containing the specified values.
        /// </summary>
        /// <param name="loggerName">The name of the logger that created this entry.</param>
        /// <param name="options">The log output options.</param>
        /// <param name="severity">The severity of this entry.</param>
        /// <param name="eventId">The event identifier of this entry.</param>
        /// <param name="messageFunction">A function generating the message to write.</param>
        /// <param name="rawMessage">The unformatted, raw message string.</param>
        /// <param name="args">The argument array used to format the raw message.</param>
        /// <param name="formatMessageArguments">A value indicating whether to format the message string with any provided arguments.</param>
        /// <param name="writeDirect">A value indicating whether this entry should bypass log formatting and write directly to the output.</param>
        /// <returns>A log entry containing the specified values.</returns>
        public LogEntry GetEntry(string loggerName, LogOptions options, LogSeverity severity, int eventId,
            Func<string> messageFunction, string rawMessage, object[] args, bool formatMessageArguments, bool writeDirect)
        {
            var ts = Stopwatch.GetTimestamp();
            var dt = DateTime.UtcNow;
            string cs = null;
            if ((options & LogOptions.Callstack) == LogOptions.Callstack)
                cs = Logger.__CS();
            return ConstructEntry(loggerName, severity, eventId, dt, ts, cs, messageFunction, rawMessage, args, formatMessageArguments, writeDirect);
        }

        /// <summary>
        /// Gets a <see cref="Mercury.Logging.LogEntry"/> constructed from the specified values.
        /// </summary>
        /// <param name="loggerName">The name of the logger that created this entry.</param>
        /// <param name="severity">The severity of this entry.</param>
        /// <param name="eventId">The event identifier of this entry.</param>
        /// <param name="loggedDateTime">The date and time at which this entry was logged.</param>
        /// <param name="timestamp">The timestamp at which this entry was logged.</param>
        /// <param name="callStack">A snapshot of the call stack at the point in which this entry was logged.</param>
        /// <param name="messageFunction">A function generating the message to write.</param>
        /// <param name="rawMessage">The unformatted, raw message string.</param>
        /// <param name="args">The argument array used to format the raw message.</param>
        /// <param name="formatMessageArguments">A value indicating whether to format the message string with any provided arguments.</param>
        /// <param name="writeDirect">A value indicating whether this entry should bypass log formatting and write directly to the output.</param>
        /// <returns>A <see cref="Mercury.Logging.LogEntry"/> constructed from the specified values.</returns>
        protected virtual LogEntry ConstructEntry(string loggerName, LogSeverity severity, int eventId, DateTime loggedDateTime, long timestamp,
            string callStack, Func<string> messageFunction, string rawMessage, object[] args, bool formatMessageArguments, bool writeDirect)
        {
            return new LogEntry(loggerName, severity, eventId, loggedDateTime, timestamp, callStack, messageFunction, rawMessage, args, formatMessageArguments, writeDirect);
        }

        internal static string __CS()
        {
            var current = Environment.StackTrace;
            var methodIdx = current.IndexOf("Logger.__CS()");
            var nlIdx = current.IndexOf(Environment.NewLine, methodIdx);
            return current.Substring(nlIdx + 1, current.Length - (nlIdx + 1));
        }
    }
}
