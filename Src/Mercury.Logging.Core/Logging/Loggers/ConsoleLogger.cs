using System;

namespace Mercury.Logging.Loggers
{
    /// <summary>
    /// A logger that writes to the console.
    /// </summary>
    public class ConsoleLogger
        : Logger
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.Loggers.ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger()
        {
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            if (message == null)
                return false;
            Console.Write(message);
            return true;
        }
    }
}
