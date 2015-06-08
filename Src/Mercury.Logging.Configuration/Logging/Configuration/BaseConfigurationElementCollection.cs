using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// A base class for extending the <see cref="System.Configuration.ConfigurationElementCollection"/>.
    /// </summary>
    /// <typeparam name="TConfigurationElement">The System.Type of the ConfigurationElement contained in this collection.</typeparam>
    public abstract class BaseConfigurationElementCollection<TConfigurationElement>
        : ConfigurationElementCollection, IList<TConfigurationElement> where TConfigurationElement : ConfigurationElement, new()
    {
        /// <summary>
        /// The default property collection.
        /// </summary>
        protected static ConfigurationPropertyCollection DefaultConfigurationPropertyCollection = new ConfigurationPropertyCollection();

        /// <summary>
        /// Gets the type of this ConfigurationElementCollection.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Gets a collection of properties on this element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return BaseConfigurationElementCollection<TConfigurationElement>.DefaultConfigurationPropertyCollection; }
        }

        /// <summary>
        /// Creates a new <see cref="System.Configuration.ConfigurationElement"/> of the type of element contained in this collection.
        /// </summary>
        /// <returns>A new <see cref="System.Configuration.ConfigurationElement"/> of the type of element contained in this collection.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new TConfigurationElement();
        }

        /// <summary>
        /// Gets the XML element name used in configuration for the elements in this collection.
        /// </summary>
        protected override string ElementName
        {
            get { return "element"; }
        }

        /// <summary>
        /// Gets the key that identifies the specified element in this collection.
        /// </summary>
        /// <param name="element">The element from which to extract the key.</param>
        /// <returns>The key that identifies the specified element in this collection.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        /// <summary>
        /// Gets the index of the specified element.
        /// </summary>
        /// <param name="item">The element to find.</param>
        /// <returns>The index of the specified element.</returns>
        public int IndexOf(TConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        /// <summary>
        /// Inserts the specified element at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the element.</param>
        /// <param name="item">The element to insert.</param>
        public void Insert(int index, TConfigurationElement item)
        {
            this.BaseAdd(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            this.BaseRemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
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

        /// <summary>
        /// Adds the element to the collection.
        /// </summary>
        /// <param name="item">The element to add.</param>
        public void Add(TConfigurationElement item)
        {
            this.BaseAdd(item, true);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            this.BaseClear();
        }

        /// <summary>
        /// Gets a value indicating whether the specified element exists in this collection.
        /// </summary>
        /// <param name="item">The element to test for existence.</param>
        /// <returns>True if the element exists in this collection; otherwise, false.</returns>
        public bool Contains(TConfigurationElement item)
        {
            return (this.IndexOf(item) > -1);
        }

        /// <summary>
        /// Copies the elements of this collection into the specified array starting at the specified index within the array.
        /// </summary>
        /// <param name="array">The array into which the elements of this collection will be copied.</param>
        /// <param name="arrayIndex">The index in the array at which to begin copying elements.</param>
        public void CopyTo(TConfigurationElement[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets a value indicating whether this collection is a read-only collection.
        /// </summary>
        public new bool IsReadOnly
        {
            get { return this.IsReadOnly(); }
        }

        /// <summary>
        /// Removes the specified element from this collection.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>A value indicating whether the specified element could be removed.</returns>
        public bool Remove(TConfigurationElement item)
        {
            this.BaseRemove(this.GetElementKey(item));
            return true;
        }

        /// <summary>
        /// Gets an object used to enumerate elements in this collection.
        /// </summary>
        /// <returns>An object used to enumerate elements in this collection.</returns>
        public new IEnumerator<TConfigurationElement> GetEnumerator()
        {
            foreach (TConfigurationElement item in this)
            {
                yield return item;
            }
        }
    }
}
