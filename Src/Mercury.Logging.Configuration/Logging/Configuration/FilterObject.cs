using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents a logging framework filter as defined in configuration.
    /// </summary>
    public class FilterObject
        : FrameworkObject
    {
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;

        static FilterObject()
        {
            FilterObject.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            FrameworkObject.MergeProperties(FilterObject.ConfigurationPropertyCollection);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return FilterObject.ConfigurationPropertyCollection; }
        }
    }
}
