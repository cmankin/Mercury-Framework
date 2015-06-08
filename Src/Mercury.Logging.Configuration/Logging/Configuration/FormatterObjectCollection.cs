using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// A collection of <see cref="Mercury.Logging.Configuration.FormatterObject"/> objects.
    /// </summary>
    public class FormatterObjectCollection
        : BaseConfigurationElementCollection<FormatterObject>
    {
        /// <summary>
        /// Gets the XML element name used in configuration for the elements in this collection.
        /// </summary>
        protected override string ElementName
        {
            get { return "formatter"; }
        }

        /// <summary>
        /// Gets the key that identifies the specified element in this collection.
        /// </summary>
        /// <param name="element">The element from which to extract the key.</param>
        /// <returns>The key that identifies the specified element in this collection.</returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            var instance = (FormatterObject)element;
            return instance.Id;
        }
    }
}
