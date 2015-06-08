using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Messages;

namespace Mercury.Messaging.Behaviors.Channels
{
    /// <summary>
    /// Represents a channel to a named resource.
    /// </summary>
    public class NamedChannel : IUntypedChannel, INamedChannel
    {
        /// <summary>
        /// Initializes a default instance of the NamedChannel class with the specified values.
        /// </summary>
        /// <param name="hosts">The host routing table to use.</param>
        /// <param name="address">The routing address for the agent.</param>
        public NamedChannel(HostRoutingTable hosts, string address)
        {
            this._address = address;
            this.Hosts = hosts;
        }

        /// <summary>
        /// The internal host routing table.
        /// </summary>
        protected readonly HostRoutingTable Hosts;

        /// <summary>
        /// The internal address.
        /// </summary>
        protected readonly string _address;

        /// <summary>
        /// Gets the routing address for the agent.
        /// </summary>
        public string Address
        {
            get { return this._address; }
        }

        /// <summary>
        /// Sends the specified message to the agent.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public virtual void Send<T>(T message)
        {
            string hostAddr = this.GetHostAddress();
            LocalRef host = this.Hosts.TryFind(hostAddr);
            if (host == null)
                throw new ArgumentException("Cannot find host from specified address on named channel.");

            host.Send<T>(message);
        }

        /// <summary>
        /// Gets the parent host address.
        /// </summary>
        /// <returns>The parent host address.</returns>
        protected string GetHostAddress()
        {
            return HostRoutingTable.GetHostAddress(this.Address);
        }

        /// <summary>
        /// Gets a value indicating whether this channel has been persisted as a resource.
        /// </summary>
        public bool IsPersisted
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the resource ID of this channel if it has been persisted.
        /// </summary>
        public string Id
        {
            get { return null; }
        }
    }
}
