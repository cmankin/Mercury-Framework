using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using System.Collections.Concurrent;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a routing table for host names and paths.
    /// </summary>
    public class HostRoutingTable : IHostRouting
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the HostRoutingTable class.
        /// </summary>
        public HostRoutingTable()
        {
            this._hosts = new ConcurrentDictionary<string, LocalRef>();
        }
        #endregion

        #region Implementation
        private ConcurrentDictionary<string, LocalRef> _hosts;

        /// <summary>
        /// Gets the host routing dictionary.
        /// </summary>
        public ConcurrentDictionary<string, LocalRef> Hosts
        {
            get { return this._hosts; }
        }

        /// <summary>
        /// Attempts to add or update the host at the specified 
        /// address with the specified agent reference.
        /// </summary>
        /// <param name="address">The address of the host to add or update.</param>
        /// <param name="reference">The agent reference to add or update.</param>
        /// <returns>True if the add or update was successful; otherwise, false.</returns>
        public bool TryAddOrUpdate(string address, LocalRef reference)
        {
            Exception fault;
            return TryAddOrUpdate(address, reference, out fault);
        }

        /// <summary>
        /// Attempts to add or update the host at the specified 
        /// address with the specified agent reference.
        /// </summary>
        /// <param name="address">The address of the host to add or update.</param>
        /// <param name="reference">The agent reference to add or update.</param>
        /// <param name="fault">Out. The exception that caused the operation to fail or null.</param>
        /// <returns>True if the add or update was successful; otherwise, false.</returns>
        public bool TryAddOrUpdate(string address, LocalRef reference, out Exception fault)
        {
            fault = null;
            try
            {
                this.Hosts.AddOrUpdate(address, reference, (key, existingValue) =>
                    {
                        if (existingValue == reference)
                            return existingValue;
                        else
                            return reference;
                    });
            }
            catch (Exception ex)
            {
                fault = ex;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to remove the host with the specified address from the routing table.
        /// </summary>
        /// <param name="address">The address of the host to remove.</param>
        /// <returns>True if the remove operation was successful; otherwise, false.</returns>
        public bool TryRemove(string address)
        {
            LocalRef outValue;
            return this.Hosts.TryRemove(address, out outValue);
        }
        #endregion

        #region IHostRouting

        /// <summary>
        /// Attempts to find and return the agent reference associated with the specified address.
        /// </summary>
        /// <param name="address">The address of the agent reference to find.</param>
        /// <returns>The agent reference associated with the specified address.</returns>
        public LocalRef TryFind(string address)
        {
            LocalRef outValue = null;
            this.Hosts.TryGetValue(address, out outValue);
            return outValue;
        }

        /// <summary>
        /// Attempts to find the address associated with the specified agent ID.
        /// </summary>
        /// <param name="agentId">The agent ID whose address to find.</param>
        /// <param name="address">Out. The associated address or null.</param>
        /// <returns>True if the address is found; otherwise, false.</returns>
        public bool TryFind(string agentId, out string address)
        {
            foreach (KeyValuePair<string, LocalRef> kv in this.Hosts)
            {
                if (kv.Value != null && kv.Value.ResId == agentId)
                {
                    address = kv.Key;
                    return true;
                }
            }

            address = null;
            return false;
        }

        #endregion

        #region Static

        /// <summary>
        /// Returns the address of the parent host from the specified address.
        /// </summary>
        /// <param name="address">The address whose parent host address to parse.</param>
        /// <returns>The address of the parent host.</returns>
        public static string GetHostAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");

            Uri uriAddress = new Uri(address);
            string builder = uriAddress.Host;
            for (int i = 0; i < uriAddress.Segments.Length - 1; i++)
            {
                builder += uriAddress.Segments[i];
            }
            return builder;
        }

        //public static string CreateChildAddress(Uri hostUri, string childName)
        //{
        //    return CreateChildAddress(hostUri.ToString(), childName);
        //}

        ///// <summary>
        ///// Returns 
        ///// </summary>
        ///// <param name="hostAddress"></param>
        ///// <param name="childName"></param>
        ///// <returns></returns>
        //public static string CreateChildAddress(string hostAddress, string childName)
        //{
        //    return string.Format("{0}{1}", hostAddress, childName);
        //}

        #endregion
    }
}
