using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Represents the root logging objects in configuration.
    /// </summary>
    public class LogRoot
        : ConfigurationElement
    {
        private static ConfigurationPropertyCollection ConfigurationPropertyCollection;
        private static ConfigurationProperty ParametersProperty;
        private static ConfigurationProperty ChildrenProperty;

        static LogRoot()
        {
            LogRoot.ConfigurationPropertyCollection = new ConfigurationPropertyCollection();
            LogRoot.ParametersProperty = new ConfigurationProperty("parameters", typeof(ParameterCollection));
            LogRoot.ChildrenProperty = new ConfigurationProperty("", typeof(LoggerObjectCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

            LogRoot.ConfigurationPropertyCollection.Add(LogRoot.ParametersProperty);
            LogRoot.ConfigurationPropertyCollection.Add(LogRoot.ChildrenProperty);
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return LogRoot.ConfigurationPropertyCollection; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Mercury.Logging.Configuration.Parameter"/> objects.
        /// </summary>
        public ParameterCollection Parameters
        {
            get { return (ParameterCollection)this[LogRoot.ParametersProperty]; }
        }

        /// <summary>
        /// Gets a collection of logger objects associated with this root.
        /// </summary>
        public LoggerObjectCollection Children
        {
            get { return (LoggerObjectCollection)this[LogRoot.ChildrenProperty]; }
        }

        /// <summary>
        /// Gets an instance of the root logger.
        /// </summary>
        /// <param name="requestedName">The requested name for the logger.</param>
        /// <returns>An instance of the root logger.</returns>
        public Logger GetInstance(string requestedName)
        {
            var collection = this.Children;
            if (collection != null && collection.Count > 0)
            {
                if (collection.Count == 1)
                {
                    var singleInstance = collection[0].GetInstance() as Logger;
                    this.PrepareInstance(singleInstance, requestedName);
                    return singleInstance;
                }

                CompositeLogger composite = new CompositeLogger();
                this.PrepareInstance(composite, requestedName);
                
                Logger child;
                for (int i = 0; i < collection.Count; i++)
                {
                    child = collection[i].GetInstance() as Logger;
                    if (child != null)
                        composite.Add(child);
                }
                return composite;
            }
            return null;
        }

        /// <summary>
        /// Configures the specified instance according to the parameters of this element.
        /// </summary>
        /// <param name="instance">The instance to configure.</param>
        /// <param name="requestedName">The requested name for the specified <see cref="Mercury.Logging.Logger"/>.</param>
        protected void PrepareInstance(Logger instance, string requestedName)
        {
            var collection = this.Parameters;
            if (collection != null && collection.Count > 0)
            {
                Type instanceType = instance.GetType();
                PropertyInfo property;
                Parameter param;
                for (int i = 0; i < collection.Count; i++)
                {
                    param = collection[i];
                    property = instanceType.GetProperty(param.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (property != null)
                        FrameworkObject.SetProperty(property, param, instance);
                }
            }
            if (!string.IsNullOrEmpty(requestedName))
                instance.Name = requestedName;
        }

        /// <summary>
        /// Called after deserialization.
        /// </summary>
        protected override void PostDeserialize()
        {
            this.ResolveRoot();
            base.PostDeserialize();
        }

        private void ResolveRoot()
        {
            var paramColl = this.Parameters;
            if (paramColl != null && paramColl.Count > 0)
            {
                Parameter param;
                for (int i = 0; i < paramColl.Count; i++)
                {
                    param = paramColl[i];
                    param.Resolve();
                }
            }
        }
    }
}
