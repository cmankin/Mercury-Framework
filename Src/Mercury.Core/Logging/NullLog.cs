using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// A default logger that does not publish data.
    /// </summary>
    public class NullLog
        : ILog, ILog<NullLog>
    {
        /// <summary>
        /// Null implementation.
        /// </summary>
        public string Name
        {
            get { return null; }
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="loggerName">Unused.</param>
        public void Initialize(string loggerName)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        public void Debug(Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        public void Debug(int id, Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Debug(string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Debug(int id, string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        public void Info(Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        public void Info(int id, Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Info(string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Info(int id, string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        public void Warn(Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        public void Warn(int id, Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Warn(string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Warn(int id, string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        public void Error(Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        public void Error(int id, Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Error(string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Error(int id, string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        public void Critical(Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        public void Critical(int id, Func<string> message)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Critical(string message, params object[] format)
        {
        }

        /// <summary>
        /// Null implementation.  Does no processing.
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="message">Unused.</param>
        /// <param name="format">Unused.</param>
        public void Critical(int id, string message, params object[] format)
        {
        }
    }
}
