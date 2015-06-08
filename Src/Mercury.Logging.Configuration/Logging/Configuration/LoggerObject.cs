using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents a <see cref="Mercury.Logging.Logger"/> as defined in configuration.
    /// </summary>
    public class LoggerObject
        : FrameworkObject
    {
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;

        static LoggerObject()
        {
            LoggerObject.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            FrameworkObject.MergeProperties(LoggerObject.ConfigurationPropertyCollection);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return LoggerObject.ConfigurationPropertyCollection; }
        }
    }
}
