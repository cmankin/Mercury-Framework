using System;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// The base framework object class used in configuration.
    /// </summary>
    public abstract class FrameworkObject
        : ConfigurationElement
    {
        private static Dictionary<string, FrameworkObject> cached = new Dictionary<string, FrameworkObject>();
        private static ConfigurationProperty ParametersProperty = new ConfigurationProperty("", typeof(ParameterCollection), new ParameterCollection(), ConfigurationPropertyOptions.IsDefaultCollection);
        private static ConfigurationProperty ChildrenProperty = new ConfigurationProperty("children", typeof(ObjectRefCollection), new ObjectRefCollection());
        private static ConfigurationProperty AssemblyProperty = new ConfigurationProperty("assembly", typeof(string), "");
        private static ConfigurationProperty TypeProperty = new ConfigurationProperty("type", typeof(string), "");
        private static ConfigurationProperty RefProperty = new ConfigurationProperty("ref", typeof(string), "");
        private static ConfigurationProperty IdProperty = new ConfigurationProperty("id", typeof(string), "", ConfigurationPropertyOptions.IsRequired);

        private Type cachedType;
        private FrameworkObject cachedRef;

        /// <summary>
        /// Adds the properties on this base class to the specified property collection.
        /// </summary>
        /// <param name="properties">The property collection on which to add the base class properties.</param>
        protected static void MergeProperties(ConfigurationPropertyCollection properties)
        {
            if (properties != null)
            {
                properties.Add(FrameworkObject.ParametersProperty);
                properties.Add(FrameworkObject.ChildrenProperty);
                properties.Add(FrameworkObject.AssemblyProperty);
                properties.Add(FrameworkObject.TypeProperty);
                properties.Add(FrameworkObject.RefProperty);
                properties.Add(FrameworkObject.IdProperty);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="Mercury.Logging.Configuration.Parameter"/> objects.
        /// </summary>
        public ParameterCollection Parameters
        {
            get { return (ParameterCollection)this[FrameworkObject.ParametersProperty]; }
        }

        /// <summary>
        /// Gets a collection of object references with a child relationship to this object.
        /// </summary>
        public ObjectRefCollection Children
        {
            get { return (ObjectRefCollection)this[FrameworkObject.ChildrenProperty]; }
        }

        /// <summary>
        /// Gets the System.Type of this framework object.
        /// </summary>
        public Type Type
        {
            get { return this.cachedType; }
        }

        /// <summary>
        /// Gets the assembly string for this instance.
        /// </summary>
        protected string AssemblyString
        {
            get { return (string)this[FrameworkObject.AssemblyProperty]; }
        }

        /// <summary>
        /// Gets the type string for this instance.
        /// </summary>
        protected string TypeString
        {
            get { return (string)this[FrameworkObject.TypeProperty]; }
        }

        /// <summary>
        /// Gets the reference string for this instance.
        /// </summary>
        public string RefString
        {
            get { return (string)this[FrameworkObject.RefProperty]; }
        }

        /// <summary>
        /// Gets a unique identifier for this framework object.
        /// </summary>
        public string Id
        {
            get { return (string)this[FrameworkObject.IdProperty]; }
        }

        /// <summary>
        /// Generates an instance of this framework object.
        /// </summary>
        /// <returns></returns>
        public object GetInstance()
        {
            if (this.cachedRef != null && this.cachedRef.Type != null)
            {
                var instance = Activator.CreateInstance(this.cachedRef.Type);
                if (instance != null)
                    this.cachedRef.PrepareInstance(instance);
                return instance;
            }
            else if (this.Type != null)
            {
                var instance = Activator.CreateInstance(this.Type);
                if (instance != null)
                    this.PrepareInstance(instance);
                return instance;
            }
            return null;
        }

        /// <summary>
        /// Configures the specified instance according to the parameters of this element.
        /// </summary>
        /// <param name="instance">The instance to configure.</param>
        protected void PrepareInstance(object instance)
        {
            if (instance != null)
            {
                var type = instance.GetType();

                // Set parameters
                ParameterCollection pColl = this.Parameters;
                if (pColl != null && pColl.Count > 0)
                {
                    PropertyInfo property;
                    Parameter param;
                    for (int i = 0; i < pColl.Count; i++)
                    {
                        param = pColl[i];
                        property = type.GetProperty(param.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                        if (property != null)
                            FrameworkObject.SetProperty(property, param, instance);
                    }
                }

                // Set children
                ObjectRefCollection rColl = this.Children;
                if (rColl != null && rColl.Count > 0)
                {
                    var itf = instance as IAddChild;
                    if (itf == null)
                        throw new InvalidOperationException("Cannot add child objects to an instance which does not support it.");

                    ObjectRef obj;
                    for (int i = 0; i < rColl.Count; i++)
                    {
                        obj = rColl[i];
                        itf.AddChild(obj.GetInstance());
                    }
                }

                var init = instance as IInitializable;
                if (init != null)
                    init.Initialize();
            }
        }

        internal static void SetProperty(PropertyInfo property, Parameter param, object instance)
        {
            var converter = TypeDescriptor.GetConverter(property.PropertyType);
            object value = null;
            if (param.ReferencedObject != null)
            {
                value = param.ReferencedObject.GetInstance();
            }
            else
            {
                string strVal = param.Value;
                if (property.PropertyType.IsEnum)
                    strVal = strVal.Replace('|', ',');
                value = converter.ConvertFromString(strVal);
            }
            property.SetValue(instance, value, null);
        }

        /// <summary>
        /// Called after deserialization
        /// </summary>
        protected override void PostDeserialize()
        {
            FrameworkObject.Cache(this);
            this.ResolveFrameworkObjects();
            base.PostDeserialize();
        }

        internal static void Cache(FrameworkObject value)
        {
            if (value.Id == null)
                throw new ArgumentException("Cannot cache a framework object whose 'Id' is null.");
            if (FrameworkObject.cached.ContainsKey(value.Id))
                throw new ArgumentException(string.Format("The specified key '{0}' already exists in the framework object cache.", value.Id));
            FrameworkObject.cached.Add(value.Id, value);
        }

        internal static FrameworkObject GetCachedObject(string referenceId)
        {
            FrameworkObject obj = null;
            if (referenceId != null)
                FrameworkObject.cached.TryGetValue(referenceId, out obj);
            return obj;
        }
        
        internal static Type ResolveType(string typeName, string assemblyName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            Type t = GetTypeFromLoadedAssemblies(typeName, assemblyName);
            if (t == null && !string.IsNullOrEmpty(assemblyName))
            {
                var asm = Assembly.LoadFrom(assemblyName);
                if (asm != null)
                    t = asm.GetType(typeName);
            }
            if (t == null)
                t = Type.GetType(typeName);
            return t;
        }

        private static Type GetTypeFromLoadedAssemblies(string typeName, string assemblyName)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (loadedAssemblies != null && loadedAssemblies.Length > 0)
            {
                Type t = null;
                bool searchType = string.IsNullOrEmpty(assemblyName) ? true : false;
                Assembly asm;
                for (int i = 0; i < loadedAssemblies.Length; i++)
                {
                    asm = loadedAssemblies[i];
                    if (!searchType)
                    {
                        if (asm.GetName().Name == assemblyName)
                            return asm.GetType(typeName);
                    }
                    else
                    {
                        t = asm.GetType(typeName);
                        if (t != null)
                            return t;
                    }
                }
            }
            return null;
        }

        private void ResolveFrameworkObjects()
        {
            // resolve any type info
            this.cachedType = ResolveType(this.TypeString, this.AssemblyString);

            // resolve reference
            this.cachedRef = GetCachedObject(this.RefString);
            if (this.cachedRef != null && this.cachedType == null)
                this.cachedType = this.cachedRef.Type;

            // resolve parameters
            if (this.Parameters != null)
            {
                Parameter param;
                for (int i = 0; i < this.Parameters.Count; i++)
                {
                    param = this.Parameters[i];
                    param.Resolve();
                }
            }

            // resolve child references
            if (this.Children != null)
            {
                ObjectRef obj;
                for (int i = 0; i < this.Children.Count; i++)
                {
                    obj = this.Children[i];
                    obj.Resolve();
                }
            }
        }
    }
}
