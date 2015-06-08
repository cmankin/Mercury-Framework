using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;
using Mercury.Logging;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Provides methods for altering aspects of the logging framework.
    /// </summary>
    public static class LogFramework
    {
        private const string DEFAULT_SECTION_NAME = "mercuryLogging";
        private static string LogSectionName = DEFAULT_SECTION_NAME;
        private static System.Configuration.Configuration defaultConfiguration;
        private static object s_lock = new object();
        private static LoggingSection logSection;

        static LogFramework()
        {
            defaultConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="type">The System.Type from which to derive the logger name.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return GetLogger(type.FullName);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(string name)
        {
            return BuildLoggerFromConfiguration(name);
        }

        /// <summary>
        /// Opens the logging configuration from the specified file using the specified section name.
        /// </summary>
        /// <param name="exePath">The full path to the executable (exe) file whose configuration should be loaded.</param>
        /// <param name="sectionName">The name of the logging section element.</param>
        public static void OpenConfiguration(string exePath, string sectionName)
        {
            lock (s_lock)
            {
                LogFramework.LogSectionName = sectionName;
                LogFramework.defaultConfiguration = ConfigurationManager.OpenExeConfiguration(exePath);
            }
        }

        private static void EnsureConfiguration()
        {
            if (logSection == null)
            {
                lock (s_lock)
                {
                    if (logSection == null)
                    {
                        logSection = defaultConfiguration.GetSection(LogFramework.LogSectionName) as LoggingSection;
                    }
                }
            }
        }

        private static Logger BuildLoggerFromConfiguration(string name)
        {
            EnsureConfiguration();
            if (logSection != null)
            {
                var root = logSection.Root;
                if (root != null)
                    return root.GetInstance(name);
            }
            return null;
        }
    }
}
