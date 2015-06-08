using System;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// An object representing a log destination component.
    /// </summary>
    public class LogSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Messaging.Instrumentation.LogSource"/> 
        /// class with the specified values.
        /// </summary>
        /// <param name="outputSource">The type of the destination source.</param>
        /// <param name="connection">The connection string for the log source.</param>
        /// <param name="args">An array of arguments to use with the connection string.</param>
        public LogSource(TraceOutputSource outputSource, string connection, params string[] args)
        {
            this.OutputSource = outputSource;
            this.Connection = connection;
            this.Args = args;
        }

        /// <summary>
        /// Gets the type of the destination source.
        /// </summary>
        public TraceOutputSource OutputSource { get; private set; }

        /// <summary>
        /// Gets the connection string for the log source.
        /// </summary>
        public string Connection { get; private set; }

        /// <summary>
        /// Gets an array of arguments to use with the connection string.
        /// </summary>
        public string[] Args { get; private set; }
    }
}
