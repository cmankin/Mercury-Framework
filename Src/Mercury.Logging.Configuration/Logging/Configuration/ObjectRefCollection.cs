using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="Mercury.Logging.Configuration.ObjectRef"/> objects.
    /// </summary>
    public class ObjectRefCollection
        : BaseConfigurationElementCollection<ObjectRef>
    {
        /// <summary>
        /// Gets the XML element name used in configuration for the elements in this collection.
        /// </summary>
        protected override string ElementName
        {
            get { return "object-ref"; }
        }

        /// <summary>
        /// Gets the key that identifies the specified element in this collection.
        /// </summary>
        /// <param name="element">The element from which to extract the key.</param>
        /// <returns>The key that identifies the specified element in this collection.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            var instance = (ObjectRef)element;
            return instance.ReferenceString;
        }
    }
}
