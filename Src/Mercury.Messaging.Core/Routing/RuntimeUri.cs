using System;
using System.Globalization;
using Mercury.Distributed;

namespace Mercury.Messaging.Routing
{
    /// <summary>
    /// A static runtime URI utility.
    /// </summary>
    public static class RuntimeUri
    {
        /// <summary>
        /// Creates a runtime URI from the specified name.
        /// </summary>
        /// <param name="name">The name string from which to create a URI.</param>
        /// <returns>A runtime URI constructed from the specified name.</returns>
        public static Uri Create(string name)
        {
            return new Uri(RemoteNode.CreateAddress(name));
        }
    }
}
