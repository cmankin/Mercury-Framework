using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Collections
{
    /// <summary>
    /// Represents a generic, indexed collection of key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public interface IOrderedDictionary<TKey, TValue> :
        IDictionary<TKey,TValue>,
        ICollection<KeyValuePair<TKey,TValue>>,
        IEnumerable
    {
        /// <summary>
        /// Inserts the specified key and value at the specified index of the collection.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        void Insert(int index, TKey key, TValue value);

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        void RemoveAt(int index);
    }
}
