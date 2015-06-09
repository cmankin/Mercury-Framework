using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// A configuration section for defining logging framework objects.
    /// </summary>
    public class LoggingSection
        : ConfigurationSection
    {
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;
        private static ConfigurationProperty FormattersProperty;
        private static ConfigurationProperty FiltersProperty;
        private static ConfigurationProperty LoggersProperty;
        private static ConfigurationProperty RootProperty;

        static LoggingSection()
        {
            LoggingSection.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            LoggingSection.FormattersProperty = new ConfigurationProperty("formatters", typeof(FormatterObjectCollection));
            LoggingSection.FiltersProperty = new ConfigurationProperty("filters", typeof(FilterObjectCollection));
            LoggingSection.LoggersProperty = new ConfigurationProperty("loggers", typeof(LoggerObjectCollection));
            LoggingSection.RootProperty = new ConfigurationProperty("root", typeof(LogRoot), null, ConfigurationPropertyOptions.IsRequired);

            LoggingSection.ConfigurationPropertyCollection.Add(LoggingSection.FiltersProperty);
            LoggingSection.ConfigurationPropertyCollection.Add(LoggingSection.FormattersProperty);
            LoggingSection.ConfigurationPropertyCollection.Add(LoggingSection.LoggersProperty);
            LoggingSection.ConfigurationPropertyCollection.Add(LoggingSection.RootProperty);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return LoggingSection.ConfigurationPropertyCollection; }
        }

        /// <summary>
        /// Gets the root logging element.
        /// </summary>
        public LogRoot Root
        {
            get { return (LogRoot)this[LoggingSection.RootProperty]; }
        }

        /// <summary>
        /// Gets a collection of defined filter objects.
        /// </summary>
        public FilterObjectCollection Filters
        {
            get { return (FilterObjectCollection)this[LoggingSection.FiltersProperty]; }
        }

        /// <summary>
        /// Gets a collection of defined formatter objects.
        /// </summary>
        public FormatterObjectCollection Formatters
        {
            get { return (FormatterObjectCollection)this[LoggingSection.FormattersProperty]; }
        }

        /// <summary>
        /// Gets a collection of defined loggers.
        /// </summary>
        public LoggerObjectCollection Loggers
        {
            get { return (LoggerObjectCollection)this[LoggingSection.LoggersProperty]; }
        }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>True when an unknown attribute is encountered during deserialization; otherwise, false.</returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            // Ignore unrecognized attributes.
            return true;
        }
    }
}
