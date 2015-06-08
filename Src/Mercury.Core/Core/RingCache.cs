using System;
using System.Collections.Concurrent;
using System.Text;

namespace Mercury
{
    /// <summary>
    /// Implementation of a circular cache.
    /// </summary>
    /// <typeparam name="T">The type of the data to cache.</typeparam>
    public sealed class RingCache<T>
    {
        private object cacheLock = new object();
        private InternalCacheItem[] _buffer;
        private ConcurrentDictionary<int, InternalCacheItem> _cache;
        private int _bufferSize;
        private Action<T> _disposeHandler;
        private int insertionHead;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.RingCache{T}"/> class with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">The maximum number of elements that can be added to the cache.</param>
        public RingCache(int bufferSize)
            : this(bufferSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.RingCache{T}"/> class with the specified buffer size and dispose handler.
        /// </summary>
        /// <param name="bufferSize">The maximum number of elements that can be added to the cache.</param>
        /// <param name="disposeHandler">A delegate to a method that can dispose of elements that are overwritten.</param>
        public RingCache(int bufferSize, Action<T> disposeHandler)
        {
            this._bufferSize = bufferSize;
            this._buffer = new InternalCacheItem[bufferSize];
            this._cache = new ConcurrentDictionary<int, InternalCacheItem>(RingCache<T>.DefaultConcurrencyLevel, bufferSize);
            this._disposeHandler = disposeHandler;
            if (disposeHandler == null)
                this._disposeHandler = new Action<T>(_DisposeCacheItem);
        }

        /// <summary>
        /// Gets the maximum number of elements that can be added to the cache.
        /// </summary>
        public int BufferSize
        {
            get { return this._bufferSize; }
        }

        /// <summary>
        /// Gets the number of elements in the cache.
        /// </summary>
        public int Count
        {
            get { return this._cache.Count; }
        }

        private static void _DisposeCacheItem(T item)
        {
            IDisposable test = item as IDisposable;
            if (test != null)
                test.Dispose();
        }

        private static int DefaultConcurrencyLevel
        {
            get { return 4 * Environment.ProcessorCount; }
        }

        /// <summary>
        /// Removes all elements from the cache.
        /// </summary>
        /// <param name="withDispose">A value indicating whether each element should be disposed before being removed.</param>
        public void Clear(bool withDispose)
        {
            lock (this.cacheLock)
            {
                if (withDispose)
                {
                    foreach (InternalCacheItem item in this._buffer)
                        this._HandleDisposableItem(item);
                }
                this._cache.Clear();
                Array.Clear(this._buffer, 0, this._buffer.Length);
            }
        }

        /// <summary>
        /// Attempts to add the specified item with the associated key to the cache.
        /// </summary>
        /// <param name="key">The key associated with the item to add.</param>
        /// <param name="item">The item value to add.</param>
        /// <returns>True if key does not already exist in the cache; otherwise, false.</returns>
        public bool Add(string key, T item)
        {
            InternalCacheItem c_item = new InternalCacheItem(key, item);
            InternalCacheItem test = null;
            return this.AddInternal(c_item, out test);
        }

        /// <summary>
        /// Attempts to add the specified item with the associated key to the cache.
        /// </summary>
        /// <param name="key">The key associated with the item to add.</param>
        /// <param name="item">The item value to add.</param>
        /// <param name="overwrite">Out. The value that was overwritten by the added item or null.</param>
        /// <returns>True if key does not already exist in the cache; otherwise, false.</returns>
        public bool Add(string key, T item, out T overwrite)
        {
            overwrite = default(T);
            InternalCacheItem c_item = new InternalCacheItem(key, item);
            InternalCacheItem test = null;
            bool flag = this.AddInternal(c_item, out test);
            if (flag && test != null && test.Item != null)
                overwrite = (T)test.Item;
            return flag;
        }

        private bool AddInternal(InternalCacheItem item, out InternalCacheItem overwrite)
        {
            overwrite = null;
            if (this._cache.ContainsKey(item.Hash))
                return false;
            lock (this.cacheLock)
            {
                if (this._cache.ContainsKey(item.Hash))
                    return false;
                this._VolatileAdd(item, out overwrite);
                return true;
            }
        }

        private void _VolatileAdd(InternalCacheItem item, out InternalCacheItem overwrite)
        {
            overwrite = null;
            if (!(this.insertionHead < this.BufferSize))
                this._ResertInsertionHead();

            var current = this._buffer[this.insertionHead];
            if (current != null)
            {
                this._HandleDisposableItem(current);
                if (this._cache.ContainsKey(current.Hash))
                    this._cache.TryRemove(current.Hash, out overwrite);
            }

            this._buffer[this.insertionHead] = item;
            this._cache.TryAdd(item.Hash, item);
            this.insertionHead++;
        }

        private void _ResertInsertionHead()
        {
            this.insertionHead = 0;
        }

        /// <summary>
        /// Removes the item associated with the specified key from the cache.  This 
        /// is an expensive operation; try to avoid it as much as possible.
        /// </summary>
        /// <param name="key">The key associated with the item to remove.</param>
        /// <returns>True if the item was removed from the cache; otherwise, false.</returns>
        public bool Remove(string key)
        {
            InternalCacheItem nKey = new InternalCacheItem(key, null);
            InternalCacheItem removed = null;
            return this.RemoveInternal(nKey, out removed);
        }

        /// <summary>
        /// Removes the item associated with the specified key from the cache.  This 
        /// is an expensive operation; try to avoid it as much as possible.
        /// </summary>
        /// <param name="key">The key associated with the item to remove.</param>
        /// <param name="value">Out. The item removed from the cache.</param>
        /// <returns>True if the item was removed from the cache; otherwise, false.</returns>
        public bool Remove(string key, out T value)
        {
            value = default(T);
            InternalCacheItem nKey = new InternalCacheItem(key, null);
            InternalCacheItem removed = null;
            bool flag = this.RemoveInternal(nKey, out removed);
            if (flag && removed != null && removed.Item != null)
                value = (T)removed.Item;
            return flag;
        }

        internal bool RemoveInternal(InternalCacheItem item, out InternalCacheItem removed)
        {
            removed = null;
            if (!this._cache.ContainsKey(item.Hash))
                return false;
            lock (this.cacheLock)
            {
                if (!this._cache.ContainsKey(item.Hash))
                    return false;
                this._VolatileRemove(item, out removed);
                return true;
            }
        }

        private void _VolatileRemove(InternalCacheItem item, out InternalCacheItem removed)
        {
            this._cache.TryRemove(item.Hash, out removed);
            for (int i = 0; i < this._buffer.Length; i++)
            {
                removed = this._buffer[i];
                if (removed.Hash == item.Hash)
                {
                    this._DeleteIndexFromBuffer(i);
                    break;
                }
            }
        }

        private void _DeleteIndexFromBuffer(int index)
        {
            if (index > -1 && index < this.BufferSize)
            {
                this._buffer[index] = null;

                // Calculate compaction
                var endIndex = Math.Max(this.insertionHead, this.BufferSize - 1);
                var lastItemIndex = endIndex - 1;
                var copyLength = lastItemIndex - index;

                if (copyLength > 0)
                {
                    var temp = new InternalCacheItem[copyLength];
                    Array.Copy(this._buffer, index + 1, temp, 0, copyLength);
                    Array.Copy(temp, 0, this._buffer, index, temp.Length);
                    this._buffer[lastItemIndex] = null;
                }
                if (this.insertionHead > index)
                    this.insertionHead--;
            }
        }

        /// <summary>
        /// Returns the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value associated with the specified key.</returns>
        public T GetValue(string key)
        {
            InternalCacheItem cKey = new InternalCacheItem(key, null);
            var result = this.GetValueInternal(cKey);
            if (result != null && result.Item != null)
                return (T)result.Item;
            return default(T);
        }

        private InternalCacheItem GetValueInternal(InternalCacheItem cacheKey)
        {
            InternalCacheItem result = null;
            this._cache.TryGetValue(cacheKey.Hash, out result);
            return result;
        }

        private void _HandleDisposableItem(InternalCacheItem item)
        {
            if (this._disposeHandler != null && item != null && item.Item is T)
                this._disposeHandler.Invoke((T)item.Item);
        }
    }
}
