using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// A set of extensions to core System.Net classes.
    /// </summary>
    public static class NetExtensions
    {
        /// <summary>
        /// Retrieves an address from a host entry that satisfies the specified condition.
        /// </summary>
        /// <param name="host">The host entry on which to search.</param>
        /// <param name="condition">The predicate function to use.</param>
        /// <returns>An IP address that satisfies the specified condition or null.</returns>
        public static IPAddress GetAddress(this IPHostEntry host, Func<IPAddress, bool> condition)
        {
            return host.AddressList.FirstOrDefault<IPAddress>(condition);
        }
    }
}
