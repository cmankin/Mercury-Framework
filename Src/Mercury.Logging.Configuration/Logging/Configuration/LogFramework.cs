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

        private static object s_lock = new object();
        private static int cacheKey;
        private static LoggingSection logSection;

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="type">The System.Type from which to derive the logger name.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(Type type)
        {
            return GetLogger(type, null, null);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="type">The System.Type from which to derive the logger name.</param>
        /// <param name="provider">The configuration provider to use.  Null specifies the default provider.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(Type type, ConfigurationProvider provider)
        {
            return GetLogger(type, provider, null);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="type">The System.Type from which to derive the logger name.</param>
        /// <param name="provider">The configuration provider to use.  Null specifies the default provider.</param>
        /// <param name="sectionName">The name of the configuration section to load.  Null specifies the default configuration section.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(Type type, ConfigurationProvider provider, string sectionName)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return GetLogger(type.FullName, provider, sectionName);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(string name)
        {
            return GetLogger(name, null, null);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <param name="provider">The configuration provider to use.  Null specifies the default provider.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(string name, ConfigurationProvider provider)
        {
            return GetLogger(name, provider, null);
        }

        /// <summary>
        /// Gets a logger constructed from configuration.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <param name="provider">The configuration provider to use.  Null specifies the default provider.</param>
        /// <param name="sectionName">The name of the configuration section to load.  Null specifies the default configuration section.</param>
        /// <returns>A logger constructed from configuration.</returns>
        public static Logger GetLogger(string name, ConfigurationProvider provider, string sectionName)
        {
            return BuildLoggerFromConfiguration(name, provider, sectionName);
        }

        private static void EnsureConfiguration(ConfigurationProvider provider, string sectionName)
        {
            int test = GetHashCode(provider, sectionName);
            if (test != cacheKey || logSection == null)
            {
                lock (s_lock)
                {
                    if (test != cacheKey || logSection == null)
                    {
                        cacheKey = test;
                        FrameworkObject.ClearCache();
                        logSection = provider.LoadConfiguration().GetSection(sectionName) as LoggingSection;
                    }
                }
            }
        }

        private static int GetHashCode(ConfigurationProvider provider, string sectionName)
        {
            return provider.GetHashCode() ^ sectionName.GetHashCode();
        }

        private static Logger BuildLoggerFromConfiguration(string name, ConfigurationProvider provider, string sectionName)
        {
            if (provider == null)
                provider = new ClientConfigurationProvider(ConfigurationUserLevel.None);
            if (string.IsNullOrEmpty(sectionName))
                sectionName = DEFAULT_SECTION_NAME;

            EnsureConfiguration(provider, sectionName);
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
