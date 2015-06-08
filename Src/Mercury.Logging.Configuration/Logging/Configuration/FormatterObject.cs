using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents a logging framework formatter as defined in configuration.
    /// </summary>
    public class FormatterObject
        : FrameworkObject
    {
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;

        static FormatterObject()
        {
            FormatterObject.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            FrameworkObject.MergeProperties(FormatterObject.ConfigurationPropertyCollection);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return FormatterObject.ConfigurationPropertyCollection; }
        }
    }
}
