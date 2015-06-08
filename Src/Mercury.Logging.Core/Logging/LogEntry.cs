using System;
using System.Threading;
using System.Diagnostics;

namespace Mercury.Logging
{
    /// <summary>
    /// Represents a message entry to a log.
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.LogEntry"/> class with the specified values.
        /// </summary>
        /// <param name="loggerName">The name of the logger that created this entry.</param>
        /// <param name="severity">The severity of this entry.</param>
        /// <param name="eventId">The event identifier of this entry.</param>
        /// <param name="loggedDateTime">The date and time at which this entry was logged.</param>
        /// <param name="timestamp">The timestamp at which this entry was logged.</param>
        /// <param name="callStack">A snapshot of the call stack at the point in which this entry was logged.</param>
        /// <param name="messageFunc">A function generating the message to write.</param>
        /// <param name="rawMessage">The unformatted, raw message string.</param>
        /// <param name="args">The argument array used to format the raw message.</param>
        /// <param name="formatMessageArguments">A value indicating whether to format the message string with any provided arguments.</param>
        /// <param name="writeDirect">A value indicating whether this entry should bypass log formatting and write directly to the output.</param>
        public LogEntry(string loggerName, LogSeverity severity, int eventId, DateTime loggedDateTime, long timestamp, string callStack,
            Func<string> messageFunc, string rawMessage, object[] args, bool formatMessageArguments, bool writeDirect)
        {
            this.LoggerName = loggerName;
            this.Severity = severity;
            this.EventId = eventId;
            this.LoggedDateTime = loggedDateTime;
            this.Timestamp = timestamp;
            this.Callstack = callStack;
            this._messageFunction = messageFunc;
            this.RawMessage = rawMessage;
            this.Args = args;
            this.FormatMessageArguments = formatMessageArguments;
            this.WriteDirect = writeDirect;
            this.ProcessId = Process.GetCurrentProcess().Id;
            this.ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private string _cachedMessage;
        private Func<string> _messageFunction;

        /// <summary>
        /// Gets the name of the logger that created this entry.
        /// </summary>
        public string LoggerName { get; internal set; }

        /// <summary>
        /// Gets the severity of this entry.
        /// </summary>
        public LogSeverity Severity { get; private set; }
        
        /// <summary>
        /// Gets the event identifier of this entry.
        /// </summary>
        public int EventId { get; private set; }

        /// <summary>
        /// Gets the identifier of the thread on which this entry was logged.
        /// </summary>
        public int ThreadId { get; protected set; }

        /// <summary>
        /// Gets the identifier of the process on which this entry was logged.
        /// </summary>
        public int ProcessId { get; protected set; }

        /// <summary>
        /// Gets the date and time at which this entry was logged.
        /// </summary>
        public DateTime LoggedDateTime { get; private set; }

        /// <summary>
        /// Gets the timestamp at which this entry was logged.
        /// </summary>
        public long Timestamp { get; private set; }

        /// <summary>
        /// Gets the call stack at which this entry was logged.
        /// </summary>
        public string Callstack { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to format the message string with any provided arguments.
        /// </summary>
        public bool FormatMessageArguments { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this entry should bypass log formatting and write directly to the output.
        /// </summary>
        public bool WriteDirect { get; private set; }

        /// <summary>
        /// Gets the message to log.
        /// </summary>
        public string Message
        {
            get { return this.GetFinalMessage(); }
        }

        /// <summary>
        /// Gets the message text to be logged.
        /// </summary>
        /// <returns>The message text to be logged.</returns>
        protected virtual string GetFinalMessage()
        {
            this.EnsureMessage();
            return this._cachedMessage;
        }

        /// <summary>
        /// Gets the filter data to apply to this entry.
        /// </summary>
        internal object FilterData { get; set; }

        /// <summary>
        /// Gets the unformatted, raw message string.
        /// </summary>
        public string RawMessage { get; private set; }

        /// <summary>
        /// Gets any arguments that should be 
        /// </summary>
        public object[] Args { get; private set; }

        /// <summary>
        /// Returns a string that represents the current log entry.
        /// </summary>
        /// <returns>A string that represents the current log entry.</returns>
        public override string ToString()
        {
            return this.RawMessage;
        }

        internal void EnsureMessage()
        {
            if (string.IsNullOrEmpty(this._cachedMessage))
            {
                if (this._messageFunction != null)
                    this.RawMessage = this._messageFunction.Invoke();
                this._cachedMessage = this.RawMessage;
                if (this.FormatMessageArguments && this.Args != null && this.Args.Length > 0)
                    this._cachedMessage = string.Format(this.RawMessage, this.Args);
            }
        }
    }
}
