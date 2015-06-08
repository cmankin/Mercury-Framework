using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents a parameter or property of a logging framework object.
    /// </summary>
    public class Parameter
        : ConfigurationElement
    {
        private const string REF_BINDING_TAG = "ref=";
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;
        private static ConfigurationProperty NameProperty;
        private static ConfigurationProperty ValueProperty;

        private FrameworkObject cachedRef;

        static Parameter()
        {
            Parameter.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            Parameter.NameProperty = new ConfigurationProperty("name", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
            Parameter.ValueProperty = new ConfigurationProperty("value", typeof(string), "");

            Parameter.ConfigurationPropertyCollection.Add(Parameter.NameProperty);
            Parameter.ConfigurationPropertyCollection.Add(Parameter.ValueProperty);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return Parameter.ConfigurationPropertyCollection; }
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name
        {
            get { return (string)this[Parameter.NameProperty]; }
        }

        /// <summary>
        /// Gets the string value of the parameter.
        /// </summary>
        public string Value
        {
            get { return (string)this[Parameter.ValueProperty]; }
        }

        internal FrameworkObject ReferencedObject
        {
            get { return this.cachedRef; }
        }

        internal void Resolve()
        {
            var strVal = this.Value;
            if (!string.IsNullOrEmpty(strVal) && strVal[0] == '{' && strVal[strVal.Length - 1] == '}')
            {
                var start = strVal.IndexOf(REF_BINDING_TAG, 1);
                if (start > -1)
                {
                    start += REF_BINDING_TAG.Length;
                    var endIdx = strVal.Length - 1;
                    var refId = strVal.Substring(start, endIdx - start);
                    this.cachedRef = FrameworkObject.GetCachedObject(refId);
                }
            }
        }
    }
}
