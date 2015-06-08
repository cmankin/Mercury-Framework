using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a cross-referenced list of elements which may be accessed by key or value.
    /// </summary>
    /// <typeparam name="T">The type of the value reference.</typeparam>
    public class Catalog<T> where T:class
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the Catalog(Of T) class.
        /// </summary>
        public Catalog()
        {
            this._internalReferences = new ConcurrentDictionary<T, string>();
            this._internalKeys = new ConcurrentDictionary<string, T>();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Adds the specified key and reference to the catalog.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="reference">The reference of the element to add.</param>
        public void Add(string key, T reference)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (reference == null)
                throw new ArgumentNullException("reference");

            if (!this.ContainsKey(key) && !this.ContainsReference(reference))
            {
                lock (this.dataLock)
                {
                    if (!this.ContainsKey(key) && !this.ContainsReference(reference))
                    {
                        this.InternalKeys.Add(key, reference);
                        this.InternalReferences.Add(reference, key);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all elements from the catalog.
        /// </summary>
        public void Clear()
        {
            lock (this.dataLock)
            {
                this.InternalKeys.Clear();
                this.InternalReferences.Clear();
            }
        }

        /// <summary>
        /// Removes the element associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (this.ContainsKey(key))
            {
                lock (this.dataLock)
                {
                    if (this.ContainsKey(key))
                    {
                        T reference = this.InternalKeys[key];
                        if (reference != null)
                        {
                            this.InternalKeys.Remove(key);
                            this.InternalReferences.Remove(reference);
                        }
                    }
                }
            }
        }

        #endregion

        #region Get Methods
        /// <summary>
        /// Returns the element associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the catalog.</param>
        /// <returns>The element associated with the specified key or the default value of T.</returns>
        public T GetReference(string key)
        {
            T item;
            this.InternalKeys.TryGetValue(key, out item);
            return item;            
        }

        /// <summary>
        /// Returns the key of the specified element.
        /// </summary>
        /// <param name="reference">The element to locate in the catalog.</param>
        /// <returns>The key of the specified element or null.</returns>
        public string GetKey(T reference)
        {
            string key;
            this.InternalReferences.TryGetValue(reference, out key);
            return key;
        }

        /// <summary>
        /// Gets an IEnumerable(Of T) containing references deriving from the specified type.
        /// </summary>
        /// <param name="type">The type to locate in the catalog.</param>
        /// <returns>An IEnumerable(Of T) containing references deriving from the specified type.</returns>
        public IEnumerable<T> GetByType(Type type)
        {
            foreach (T rt in this.InternalReferences.Keys)
            {
                if (type.IsInstanceOfType(rt))
                    yield return rt;
            }
        }

        /// <summary>
        /// Gets the first reference located that derives from the specified type.
        /// </summary>
        /// <param name="type">The type to locate in the catalog.</param>
        /// <returns>The first reference located that derives from the specified type.</returns>
        public T GetFirst(Type type)
        {
            return GetByType(type).First();
        }

        /// <summary>
        /// Returns a value indicating whether the catalog contains 
        /// an element associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>True if the catalog contains the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return this.InternalKeys.ContainsKey(key);
        }

        /// <summary>
        /// Returns a value indicating whether the catalog contains the specified element. 
        /// </summary>
        /// <param name="reference">The element to locate.</param>
        /// <returns>True if the catalog contains the specified element; otherwise, false.</returns>
        public bool ContainsReference(T reference)
        {
            return this.InternalReferences.ContainsKey(reference);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of elements in the catalog.
        /// </summary>
        public int Count
        {
            get { return this.InternalReferences.Count; }
        }

        /// <summary>
        /// Gets a collection of keys contained in the catalog.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return this.InternalKeys.Keys;
            }
        }

        /// <summary>
        /// Gets a collection of references contained in the catalog.
        /// </summary>
        public ICollection<T> References
        {
            get
            {
                return this.InternalReferences.Keys;
            }
        }

        #endregion

        #region Data

        private object dataLock = new object();
        private ConcurrentDictionary<T, string> _internalReferences;

        /// <summary>
        /// The internal reference dictionary.
        /// </summary>
        protected IDictionary<T, string> InternalReferences
        {
            get { return this._internalReferences; }
        }

        private ConcurrentDictionary<string, T> _internalKeys;

        /// <summary>
        /// The internal key dictionary.
        /// </summary>
        protected IDictionary<string, T> InternalKeys
        {
            get { return this._internalKeys; }
        }

        #endregion
    }
}
