using System;
using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents a referenced logging framework object.
    /// </summary>
    public class ObjectRef
        : ConfigurationElement
    {
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;
        private static ConfigurationProperty RefProperty;

        private FrameworkObject cachedRef;

        static ObjectRef()
        {
            ObjectRef.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            ObjectRef.RefProperty = new ConfigurationProperty("ref", typeof(string), "", ConfigurationPropertyOptions.IsRequired);

            ObjectRef.ConfigurationPropertyCollection.Add(ObjectRef.RefProperty);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return ObjectRef.ConfigurationPropertyCollection; }
        }

        /// <summary>
        /// Gets the original reference string.
        /// </summary>
        internal string ReferenceString
        {
            get { return (string)this[ObjectRef.RefProperty]; }
        }

        /// <summary>
        /// Gets an instance of the referenced object.
        /// </summary>
        public Type ReferenceType
        {
            get { return this.cachedRef != null ? this.cachedRef.Type : null; }
        }

        internal FrameworkObject ReferenceObject
        {
            get { return this.cachedRef; }
        }

        internal void Resolve()
        {
            this.cachedRef = FrameworkObject.GetCachedObject(this.ReferenceString);
        }

        /// <summary>
        /// Gets an instance of the referenced object.
        /// </summary>
        /// <returns>An instance of the referenced object.</returns>
        public object GetInstance()
        {
            if (this.cachedRef != null)
                return this.cachedRef.GetInstance();
            return null;
        }
    }
}
