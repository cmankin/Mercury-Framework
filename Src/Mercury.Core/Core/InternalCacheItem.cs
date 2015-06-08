
namespace Mercury
{
    internal class InternalCacheItem
    {
        private string _key;
        private int _hash;
        private object _item;

        internal InternalCacheItem(string key, object item)
        {
            this._key = key;
            this._hash = key.GetHashCode();
            this._item = item;
        }

        internal int Hash
        {
            get { return this._hash; }
        }

        internal string Key
        {
            get { return this._key; }
        }

        internal object Item
        {
            get { return this._item; }
        }
    }
}
