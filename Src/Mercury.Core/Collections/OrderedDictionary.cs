using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Collections
{
    /// <summary>
    /// Represents a generic collection of key/value pairs that are accessible by either key or index.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class OrderedDictionary<TKey,TValue> : IOrderedDictionary<TKey,TValue>
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the OrderedDictionary class.
        /// </summary>
        public OrderedDictionary()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a default instance of the OrderedDictionary 
        /// class with the specified element capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of elements the dictionary can contain.</param>
        public OrderedDictionary(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the OrderedDictionary 
        /// class with the specified capacity and equality comparer.
        /// </summary>
        /// <param name="capacity">The maximum number of elements the dictionary can contain.</param>
        /// <param name="comparer">An equality comparer.</param>
        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            this._initialCapacity = capacity;
            this._comparer = comparer;
        }

        /// <summary>
        /// Initializes a default instance of the OrderedDictionary class with the specified ordered dictionary.
        /// </summary>
        /// <param name="dictionary">The ordered dictionary to use.  Creates a read-only copy.</param>
        public OrderedDictionary(OrderedDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            this._isReadOnly = true;
            this._objectList = dictionary._objectList;
            this._objectTable = dictionary._objectTable;
            this._comparer = dictionary._comparer;
            this._initialCapacity = dictionary._initialCapacity;
        }
        #endregion

        #region OrderedDictionaryKeyValueCollection
        /// <summary>
        /// A private class for a dictionary key/value collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class OrderedDictionaryKeyValueCollection<T> :
            IEnumerable<T>,
            ICollection<T>
        {
            private List<KeyValuePair<TKey, TValue>> _objects;
            private bool _isKeys;

            public OrderedDictionaryKeyValueCollection(List<KeyValuePair<TKey, TValue>> objects, bool isKeys)
            {
                this._objects = objects;
                this._isKeys = isKeys;
            }

            public void Add(T item)
            {
                throw new NotImplementedException(Properties.Strings.OrderedDictionaryKeyValueCollection_ReadOnly);
            }

            public void Clear()
            {
                throw new NotImplementedException(Properties.Strings.OrderedDictionaryKeyValueCollection_ReadOnly);
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException(Properties.Strings.OrderedDictionaryKeyValueCollection_ReadOnly);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException("arrayIndex");

                // Get type
                int isKeyArray = 0;
                if (array is KeyValuePair<TKey, TValue>)
                    isKeyArray = -1;
                if (arrayIndex is TKey)
                    isKeyArray = 1;

                // Cycle array
                for (int i = 0; i < this._objects.Count; i++)
                {
                    KeyValuePair<TKey, TValue> current = this._objects[i];
                    if (isKeyArray == -1)
                        array.SetValue(current, arrayIndex);
                    if (isKeyArray == 0)
                        array.SetValue(current.Value, arrayIndex);
                    if (isKeyArray == 1)
                        array.SetValue(current.Key, arrayIndex);
                    arrayIndex += 1;
                }
            }

            public int Count
            {
                get { return this._objects.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException(Properties.Strings.OrderedDictionaryKeyValueCollection_ReadOnly);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return GetInternalEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetInternalEnumerator();
            }

            protected IEnumerator<T> GetInternalEnumerator()
            {
                return new OrderedDictionaryKeyValueEnumerator<T>(this._objects, this._isKeys ? 1 : 2);
            }

        }
        #endregion

        #region OrderedDictionaryKeyValueEnumerator
        /// <summary>
        /// A private enumerator for the ordered dictionary key value collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class OrderedDictionaryKeyValueEnumerator<T> :
            IEnumerator<T>
        {
            internal const int Keys = 1;
            internal const int Values = 2;
            internal const int KeyValuePair = 3;

            private int iPos = -1;
            private int _objectReturnType;
            private List<KeyValuePair<TKey, TValue>> listEnumerator;

            public OrderedDictionaryKeyValueEnumerator(List<KeyValuePair<TKey, TValue>> list, int objectReturnType)
            {
                this.listEnumerator = list;
                this._objectReturnType = objectReturnType;
            }

            public T Current
            {
                get { return (T)this.GetCurrent(); }
            }
            
            object System.Collections.IEnumerator.Current
            {
                get { return this.GetCurrent(); }
            }

            public KeyValuePair<TKey, TValue> Entry
            {
                get { return this.listEnumerator[this.iPos]; }
            }

            private object GetCurrent()
            {
                KeyValuePair<TKey, TValue> kv = this.Entry;
                if (this._objectReturnType == 1)
                    return kv.Key;
                if (this._objectReturnType == 2)
                    return kv.Value;
                return kv;
            }

            public bool MoveNext()
            {
                if (this.iPos < this.listEnumerator.Count - 1)
                {
                    this.iPos++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.iPos = -1;
            }

            private bool disposedValue;

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue)
                {
                    if (disposing)
                    {
                        // Managed dispose
                    }
                }
                this.disposedValue = true;
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        #endregion

        #region IEnumerable<T>
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that iterates through a collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return GetInternalEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that iterates through a collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetInternalEnumerator();
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetInternalEnumerator()
        {
            return new OrderedDictionaryKeyValueEnumerator<KeyValuePair<TKey, TValue>>(this._objectList, 3);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of items in the dictionary.
        /// </summary>
        public int Count
        {
            get { return this._objectList.Count; }
        }

        private bool _isReadOnly;

        /// <summary>
        /// Gets a value indicating whether this dictionary is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return this._isReadOnly; }
        }
        #endregion

        #region Items
        private int _initialCapacity;
        private IEqualityComparer<TKey> _comparer;

        private List<KeyValuePair<TKey, TValue>> _objectList = new List<KeyValuePair<TKey, TValue>>();

        private Dictionary<TKey, TValue> _objectTable = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The index position of the element to retrieve.</param>
        /// <returns>The element at the specified index.</returns>
        public TValue this[int index]
        {
            get { return this._objectList[index].Value; }
            set
            {
                if (this.IsReadOnly)
                    throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
                if (index < 0 || index >= this._objectList.Count)
                    throw new ArgumentOutOfRangeException("index");
                
                // Update
                TKey key = this._objectList[index].Key;
                this._objectList[index] = new KeyValuePair<TKey, TValue>(key, value);
                this._objectTable[key] = value;
            }
        }

        /// <summary>
        /// Gets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to retrieve.</param>
        /// <returns>The element with the specified key.</returns>
        public TValue this[TKey key]
        {
            get { return this._objectTable[key]; }
            set
            {
                if (this.IsReadOnly)
                    throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
                if (this._objectTable.ContainsKey(key))
                {
                    this._objectTable[key] = value;
                    this._objectList[this.IndexOfKey(key)] = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }
                this.Add(key, value);
            }
        }

        /// <summary>
        /// Gets an ICollection(Of T) containing the keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return new OrderedDictionaryKeyValueCollection<TKey>(this._objectList, true); }
        }

        /// <summary>
        /// Gets an ICollection(Of T) containing the values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return new OrderedDictionaryKeyValueCollection<TValue>(this._objectList, false); }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(TKey key, TValue value)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
            this._objectTable.Add(key, value);
            this._objectList.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The object to add to the dictionary.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
            this._objectTable.Add(item.Key, item.Value);
            this._objectList.Add(item);
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            if (this.IsReadOnly)
                throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
            this._objectTable.Clear();
            this._objectList.Clear();
        }

        /// <summary>
        /// Returns a read-only copy of the current dictionary.
        /// </summary>
        /// <returns>A read-only copy of the current dictionary.</returns>
        public OrderedDictionary<TKey, TValue> AsReadOnly()
        {
            return new OrderedDictionary<TKey, TValue>(this);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the dictionary.</param>
        /// <returns>True if the item is found in the dictionary; otherwise, false.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this._objectList.Contains(item);
        }

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>True if the dictionary contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            return this._objectTable.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the dictionary to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the 
        /// elements copied from the dictionary. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this._objectList.CopyTo(array, arrayIndex);
        }

        private int IndexOfKey(TKey key)
        {
            for (int i = 0; i < this._objectList.Count; i++)
            {
                TKey key2 = this._objectList[i].Key;
                if (this._comparer != null)
                {
                    if (this._comparer.Equals(key2, key))
                        return i;
                }
                else
                {
                    if (key2.Equals(key))
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Inserts an element into the dictionary at the specified index.
        /// </summary>
        /// <param name="index">The index position at which to insert the element.</param>
        /// <param name="key">The key of the element to insert.</param>
        /// <param name="value">The value of the element to insert.</param>
        public void Insert(int index, TKey key, TValue value)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
            if (index < 0 || index >= this.Count)
                throw new ArgumentOutOfRangeException("index");
            this._objectTable.Add(key, value);
            this._objectList.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes the element at the specified index from the dictionary.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
            if (index < 0 || index >= this.Count)
                throw new ArgumentOutOfRangeException("index");
            TKey key = this._objectList[index].Key;
            this._objectList.RemoveAt(index);
            this._objectTable.Remove(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>True if the element is successfully removed; otherwise, false.  
        /// This method also returns false if key was not found in the dictionary.</returns>
        public bool Remove(TKey key)
        {
            if (this.IsReadOnly)
                throw new NotSupportedException(Properties.Strings.OrderedDictionary_ReadOnly);
            if (key == null)
                throw new ArgumentNullException("key");
            int index = this.IndexOfKey(key);
            if (index < 0)
                return false;
            this._objectTable.Remove(key);
            this._objectList.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes the specified element from the dictionary.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>True if the element is successfully removed; otherwise, false.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type of the value parameter.  
        /// This parameter is passed uninitialized.</param>
        /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            if (!this._objectTable.ContainsKey(key))
                return false;
            value = this._objectTable[key];
            return true;
        }
        #endregion
    }
}
