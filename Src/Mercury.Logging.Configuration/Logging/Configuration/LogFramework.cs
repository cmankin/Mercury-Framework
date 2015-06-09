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
        /// <summary>
        /// The default section name within a configuration file from which the logging framework will load configuration data.
        /// </summary>
        public const string DEFAULT_SECTION_NAME = "mercuryLogging";

        private static string LogSectionName = DEFAULT_SECTION_NAME;
        private static ConfigurationProvider activeConfigurationProvider = new ClientConfigurationProvider(ConfigurationUserLevel.None);
        private static object s_lock = new object();
        private static LoggingSection logSection;

        /// <summary>
        /// Gets the currently active configuration provider.
        /// </summary>
        public static ConfigurationProvider ConfigurationProvider
        {
            get { return activeConfigurationProvider; }
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
        /// Sets the section name that will be referenced in the configuration file.
        /// </summary>
        /// <param name="sectionName">The section name to use.</param>
        public static void SetConfigurationSectionName(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
                sectionName = DEFAULT_SECTION_NAME;
            lock (s_lock)
            {
                LogSectionName = sectionName;
                Reset();
            }
        }

        /// <summary>
        /// Sets the provider that will be used to create a configuration object to access the configuration file.
        /// </summary>
        /// <param name="provider">The configuration provider to set.</param>
        public static void SetConfigurationProvider(ConfigurationProvider provider)
        {
            // Default if null
            if (provider == null)
                provider = new ClientConfigurationProvider(ConfigurationUserLevel.None);
            lock (s_lock)
            {
                activeConfigurationProvider = provider;
                Reset();
            }
        }

        private static void Reset()
        {
            logSection = null;
            FrameworkObject.ClearCache();
        }

        private static void EnsureConfiguration()
        {
            if (logSection == null)
            {
                lock (s_lock)
                {
                    if (logSection == null)
                    {
                        logSection = activeConfigurationProvider.LoadConfiguration().GetSection(LogFramework.LogSectionName) as LoggingSection;
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
