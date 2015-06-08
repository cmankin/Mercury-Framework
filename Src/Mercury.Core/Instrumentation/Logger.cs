using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Mercury.Instrumentation
{
    /// <summary>
    /// Represents a logger.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Initializes a default instance of the Logger class.
        /// </summary>
        public Logger()
        {
            this._traces = new TraceCollection();
        }

        private TraceCollection _traces;

        public TraceCollection Traces
        {
            get { return this._traces; }
        }

        public virtual void Trace(TraceEventType eventType, int id, string message)
        {
            this.Trace(eventType, id, message, null);
        }

        public virtual void Trace(TraceEventType eventType, int id, string format, params object[] args)
        {
            foreach (TraceSource src in this.Traces)
                src.TraceEvent(eventType, id, format, args);
            
        }

        #region Static
        private static Logger _current;

        /// <summary>
        /// Gets the current logger.
        /// </summary>
        public static Logger Current
        {
            get { return _current; }
        }

        /// <summary>
        /// Sets the current logger.
        /// </summary>
        /// <param name="value">The value to set.</param>
        protected internal static void SetCurrent(Logger value)
        {
            _current = value;
        }
        #endregion
    }
}
