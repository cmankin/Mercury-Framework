using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// A set of methods for initializing the logging 
    /// framework and retrieving the context.
    /// </summary>
    public static class Log
    {
        #region Static

        /// <summary>
        /// Initializes the logging framework with the specified logger type.
        /// </summary>
        /// <typeparam name="T">The type of the logger to use.</typeparam>
        public static void InitializeWith<T>() where T : ILog, new()
        {
            _currentType = typeof(T);
        }

        /// <summary>
        /// Initializes the logging framework with the specified logger instance. If 
        /// the log instance is set all logging calls will be routed to this logger.
        /// </summary>
        /// <param name="logger">The ILog instance to use for logging.</param>
        public static void InitializeWith(ILog logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _currentType = logger.GetType();
            _current = logger;
        }

        private static Type _currentType = typeof(NullLog);
        private static ILog _current;

        /// <summary>
        /// Returns a logger initialized with the specified log name.
        /// </summary>
        /// <param name="logName">The name of the logger.</param>
        /// <returns>A logger initialized with the specified log name or the default logger instance.</returns>
        public static ILog GetLogger(string logName)
        {
            if (RegisteredLogNames.Count > 0 && !RegisteredLogNames.Contains(logName))
                return null;

            var logger = _current;

            if (_current == null)
            {
                logger = Activator.CreateInstance(_currentType) as ILog;
                if (logger != null)
                    logger.Initialize(logName);
            }
            return logger;
        }

        private static List<string> _registeredLogNames = new List<string>();

        /// <summary>
        /// Returns a list of registered log names. If any logs are registered, Mercury.Logging.Log.GetLogger() 
        /// will only create and return logs for the registered log names.
        /// </summary>
        public static IList<string> RegisteredLogNames
        {
            get { return _registeredLogNames; }
        }

        /// <summary>
        /// Returns the file path with select system path variables expanded.
        /// </summary>
        /// <param name="filePath">The file path to expand.</param>
        /// <returns>The file path with select system path variables expanded or the original file path.</returns>
        public static string GetExpandedFilePath(string filePath)
        {
            return Mercury.Instrumentation.InstrumentationUtil.GetExpandedFilePath(filePath);
        }
        #endregion
    }
}
