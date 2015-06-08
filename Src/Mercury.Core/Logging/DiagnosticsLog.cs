using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Mercury.Logging
{
    /// <summary>
    /// A log that utilizes System.Diagnostics TraceSource objects for logging.
    /// </summary>
    public class DiagnosticsLog
        : ILog, ILog<DiagnosticsLog>
    {
        /// <summary>
        /// Initializes a default instance of the Mercury.Logging.DiagnosticsLog class.
        /// </summary>
        public DiagnosticsLog()
        {
        }

        private object m_lock = new object();
        private TraceSource _trace;

        /// <summary>
        /// Gets the TraceSource used to write data.
        /// </summary>
        public TraceSource Trace
        {
            get { return this._trace; }
            set { this._trace = value; }
        }

        #region ILog
        /// <summary>
        /// Gets the name of this log.
        /// </summary>
        public virtual string Name
        {
            get { return this._trace != null ? this._trace.Name : null; }
        }

        /// <summary>
        /// Initializes this log with the specified log name.
        /// </summary>
        /// <param name="logName">The name of this log.</param>
        public virtual void Initialize(string logName)
        {
            if (!string.IsNullOrEmpty(logName))
            {
                TraceSource ts = new TraceSource(logName);
                InitializeTraceSource(ts);
                this.Trace = ts;
            }
        }

        /// <summary>
        /// Initializes the TraceSource for this log.
        /// </summary>
        /// <param name="ts">The TraceSource to initialize.</param>
        protected virtual void InitializeTraceSource(TraceSource ts)
        {
        }

        /// <summary>
        /// Writes the specified debug message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Debug(Func<string> message)
        {
            this.Debug(0, message);
        }

        /// <summary>
        /// Writes the specified debug message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Debug(int id, Func<string> message)
        {
            this.Debug(id, message.Invoke(), null);
        }

        /// <summary>
        /// Writes the debug message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Debug(string format, params object[] args)
        {
            this.Debug(0, format, args);
        }

        /// <summary>
        /// Writes the debug message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Debug(int id, string format, params object[] args)
        {
            EnsureArgsNotNull(ref args);
            if (this.Trace != null)
                this.Trace.TraceEvent(TraceEventType.Verbose, id, format, args);
            InternalFlushListeners();
        }

        /// <summary>
        /// Writes the specified information message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Info(Func<string> message)
        {
            this.Info(0, message);
        }

        /// <summary>
        /// Writes the specified information message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Info(int id, Func<string> message)
        {
            this.Info(id, message.Invoke(), null);
        }

        /// <summary>
        /// Writes the information message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Info(string format, params object[] args)
        {
            this.Info(0, format, args);
        }

        /// <summary>
        /// Writes the information message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Info(int id, string format, params object[] args)
        {
            EnsureArgsNotNull(ref args);
            if (this.Trace != null)
                this.Trace.TraceEvent(TraceEventType.Information, id, format, args);
            InternalFlushListeners();
        }

        /// <summary>
        /// Writes the specified warning message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Warn(Func<string> message)
        {
            this.Warn(0, message);
        }

        /// <summary>
        /// Writes the specified warning message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Warn(int id, Func<string> message)
        {
            this.Warn(id, message.Invoke(), null);
        }

        /// <summary>
        /// Writes the warning message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Warn(string format, params object[] args)
        {
            this.Warn(0, format, args);
        }

        /// <summary>
        /// Writes the warning message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Warn(int id, string format, params object[] args)
        {
            EnsureArgsNotNull(ref args);
            if (this.Trace != null)
                this.Trace.TraceEvent(TraceEventType.Warning, id, format, args);
            InternalFlushListeners();
        }

        /// <summary>
        /// Writes the specified error message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Error(Func<string> message)
        {
            this.Error(0, message);
        }

        /// <summary>
        /// Writes the specified error message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Error(int id, Func<string> message)
        {
            this.Error(id, message.Invoke(), null);
        }

        /// <summary>
        /// Writes the error message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Error(string format, params object[] args)
        {
            this.Error(0, format, args);
        }

        /// <summary>
        /// Writes the error message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Error(int id, string format, params object[] args)
        {
            EnsureArgsNotNull(ref args);
            if (this.Trace != null)
                this.Trace.TraceEvent(TraceEventType.Error, id, format, args);
            InternalFlushListeners();
        }

        /// <summary>
        /// Writes the specified critical error message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Critical(Func<string> message)
        {
            this.Critical(0, message);
        }

        /// <summary>
        /// Writes the specified critical error message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        public virtual void Critical(int id, Func<string> message)
        {
            this.Critical(id, message.Invoke(), null);
        }

        /// <summary>
        /// Writes the critical error message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Critical(string format, params object[] args)
        {
            this.Critical(0, format, args);
        }

        /// <summary>
        /// Writes the critical error message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        public virtual void Critical(int id, string format, params object[] args)
        {
            EnsureArgsNotNull(ref args);
            if (this.Trace != null)
                this.Trace.TraceEvent(TraceEventType.Critical, id, format, args);
            InternalFlushListeners();
        }

        /// <summary>
        /// Flushes all trace listeners on internal trace source.
        /// </summary>
        protected void InternalFlushListeners()
        {
            lock (this.m_lock)
            {
                if (!System.Diagnostics.Trace.AutoFlush && this.Trace != null)
                    this.Trace.Flush();
                this.Trace.Close();
            }
        }

        /// <summary>
        /// Ensures that the specified argument array is not null.
        /// </summary>
        /// <param name="args">The argument array to ensure.</param>
        internal void EnsureArgsNotNull(ref object[] args)
        {
            if (args == null)
                args = new object[] { };
        }

        #endregion
    }
}
