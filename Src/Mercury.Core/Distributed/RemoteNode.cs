using System;
using System.Net;
using System.Text;
using System.Globalization;

namespace Mercury.Distributed
{
    /// <summary>
    /// Represents a remote caching node.
    /// </summary>
    public sealed class RemoteNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Distributed.RemoteNode"/> 
        /// class with the specified name and IP end point.
        /// </summary>
        /// <param name="name">The name used to initialize the remote runtime environment.</param>
        /// <param name="endPoint">The IP end point of the remote node.</param>
        public RemoteNode(string name, IPEndPoint endPoint)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("The remote node must be named.");
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");
            this.Name = name;
            this.NodeAddress = RemoteNode.CreateAddress(this.Name);
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// The default host name prefix used to create a node address.
        /// </summary>
        public const string DEFAULT_HOSTNAME_PREFIX = "http://";

        /// <summary>
        /// Gets the name used to initialize the remote runtime environment.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the runtime address of the remote node.
        /// </summary>
        public string NodeAddress { get; private set; }

        /// <summary>
        /// Gets the IP end point of the remote node.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Returns a runtime-formatted address string created from the specified name.
        /// </summary>
        /// <param name="name">The name of the runtime from which to create an address.</param>
        /// <returns>A runtime-formatted address string created from the specified name.</returns>
        public static string CreateAddress(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", RemoteNode.DEFAULT_HOSTNAME_PREFIX, name);
        }
    }
}
