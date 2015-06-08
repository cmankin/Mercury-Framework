using System;
using System.Collections.Generic;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// A general collection of headers.
    /// </summary>
    public interface IHeaders : IEnumerable<KeyValuePair<string,string>>
    {
        /// <summary>
        /// Gets the string value associated with the specified header key.
        /// </summary>
        /// <param name="key">The header key to locate.</param>
        /// <returns>The string value associated with the specified header key.</returns>
        string this[string key] { get; set; }

        /// <summary>
        /// Returns the internal header dictionary.
        /// </summary>
        /// <returns>The internal header dictionary.</returns>
        IDictionary<string, string> GetDictionary();
    }
}
