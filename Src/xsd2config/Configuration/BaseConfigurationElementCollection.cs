using System.Collections.Generic;
using System.Configuration;

namespace Mercury.Configuration
{
    /// <summary>
    /// A base class for extending the <see cref="System.Configuration.ConfigurationElementCollection"/>.
    /// </summary>
    /// <typeparam name="TConfigurationElement">The type of the ConfigurationElement contained in this collection.</typeparam>
    public abstract class BaseConfigurationElementCollection<TConfigurationElement>
        : ConfigurationElementCollection, IList<TConfigurationElement> where TConfigurationElement : ConfigurationElement, new()
    {
        /// <summary>
        /// The default property collection.
        /// </summary>
        protected static ConfigurationPropertyCollection DefaultConfigurationPropertyCollection = new ConfigurationPropertyCollection();

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return BaseConfigurationElementCollection<TConfigurationElement>.DefaultConfigurationPropertyCollection; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TConfigurationElement();
        }

        protected override string ElementName
        {
            get { return "element"; }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        public int IndexOf(TConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        public void Insert(int index, TConfigurationElement item)
        {
            this.BaseAdd(index, item);
        }

        public void RemoveAt(int index)
        {
            this.BaseRemoveAt(index);
        }

        public TConfigurationElement this[int index]
        {
            get { return (TConfigurationElement)this.BaseGet(index); }
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public void Add(TConfigurationElement item)
        {
            this.BaseAdd(item, true);
        }

        public void Clear()
        {
            this.BaseClear();
        }

        public bool Contains(TConfigurationElement item)
        {
            return (this.IndexOf(item) > -1);
        }

        public void CopyTo(TConfigurationElement[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public new bool IsReadOnly
        {
            get { return this.IsReadOnly(); }
        }

        public bool Remove(TConfigurationElement item)
        {
            this.BaseRemove(this.GetElementKey(item));
            return true;
        }

        public new IEnumerator<TConfigurationElement> GetEnumerator()
        {
            foreach (TConfigurationElement item in this)
            {
                yield return item;
            }
        }
    }
}
