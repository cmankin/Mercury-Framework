using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Mercury.Logging
{
    /// <summary>
    /// Extensions for the core logging classes.
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// Logger dictionary.
        /// </summary>
        private static readonly Lazy<ConcurrentDictionary<string, ILog>> _loggers = new Lazy<ConcurrentDictionary<string, ILog>>();

        /// <summary>
        /// Returns the logger with the specified log name.
        /// </summary>
        /// <typeparam name="T">The type of the instance being logged.</typeparam>
        /// <param name="type">The instance being logged.</param>
        /// <param name="logName">The name of the logger to use.</param>
        /// <returns>The logger with the specified log name.</returns>
        public static ILog Log<T>(this T type, string logName)
        {
            //string objectName = typeof(T).FullName;
            return Log(logName);
        }

        /// <summary>
        /// Returns the logger with the specified log name.
        /// </summary>
        /// <param name="logName">The name of the logger to use.</param>
        /// <returns>The logger with the specified log name.</returns>
        public static ILog Log(this string logName)
        {
            ILog logger = null;
            _loggers.Value.TryGetValue(logName, out logger);
            if (logger == null)
            {
                logger = Mercury.Logging.Log.GetLogger(logName);
                if (logger != null)
                    _loggers.Value.TryAdd(logName, logger);
            }

            return logger;
        }
    }
}
