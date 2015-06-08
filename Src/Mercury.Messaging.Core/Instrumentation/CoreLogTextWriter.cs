using System;
using System.Globalization;
using Mercury.Logging;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// An object representing a text file writer formatted especially for the Mercury.Messaging.Core environment.
    /// </summary>
    public class CoreLogTextWriter
        : TextWriterTraceListenerEx
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Messaging.Instrumentation.CoreLogTextWriter"/> class.
        /// </summary>
        public CoreLogTextWriter()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Messaging.Instrumentation.CoreLogTextWriter"/> class with the specified file path.
        /// </summary>
        /// <param name="path">The file path of the file to write to.</param>
        public CoreLogTextWriter(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Messaging.Instrumentation.CoreLogTextWriter"/> class with the specified path and name.
        /// </summary>
        /// <param name="path">The file path of the file to write to.</param>
        /// <param name="name">The name of the new instance.</param>
        public CoreLogTextWriter(string path, string name)
            : base(path, name)
        {
        }

        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, object data)
        {
            this.WriteLine(string.Empty);
            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Writes trace information, an array of data objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An array of objects to emit as data.</param>
        public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, params object[] data)
        {
            this.WriteLine(string.Empty);
            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string message)
        {
            this.WriteLine(string.Empty);
            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
        {
            this.WriteLine(string.Empty);
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }
    }
}
