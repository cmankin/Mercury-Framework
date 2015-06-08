using System;
using System.Collections.Generic;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a dictionary of message header items.
    /// </summary>
    public class DictionaryHeaders : IHeaders
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the DictionaryHeaders class with the specified dictionary.
        /// </summary>
        /// <param name="headers">A dictionary of header information.</param>
        public DictionaryHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
                headers = new Dictionary<string, string>();
            this._headers = headers;
        }

        /// <summary>
        /// Initializes a default instance of the DictionaryHeaders class.
        /// </summary>
        public DictionaryHeaders()
            : this(new Dictionary<string, string>())
        {
        }

        #endregion

        #region IHeaders

        internal readonly IDictionary<string, string> _headers;

        /// <summary>
        /// Gets the string value associated with the specified header key.
        /// </summary>
        /// <param name="key">The header key to locate.</param>
        /// <returns>The string value associated with the specified header key.</returns>
        public string this[string key]
        {
            get
            {
                string value;
                this._headers.TryGetValue(key, out value);
                return value;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this._headers.Remove(key);
                else
                    this._headers[key] = value;
            }
        }

        /// <summary>
        /// Returns the internal header dictionary.
        /// </summary>
        /// <returns>The internal header dictionary.</returns>
        public IDictionary<string, string> GetDictionary()
        {
            return this._headers;
        }

        /// <summary>
        /// Returns an enumerator to iterate over the elements of the dictionary.
        /// </summary>
        /// <returns>An enumerator to iterate over the elements of the dictionary.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this._headers.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator to iterate over the elements of the dictionary.
        /// </summary>
        /// <returns>An enumerator to iterate over the elements of the dictionary.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
