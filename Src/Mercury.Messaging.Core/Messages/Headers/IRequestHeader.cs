using System;
using System.Net;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents header information for a request.
    /// </summary>
    public interface IRequestHeader
        : IMessageHeader
    {
        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        string RequestId { get; }

        /// <summary>
        /// Gets the address on which to route the response.
        /// </summary>
        Uri ResponseAddress { get; set; }

        /// <summary>
        /// Gets the end point at which to respond.
        /// </summary>
        IPEndPoint ResponseEndPoint { get; set; }

        /// <summary>
        /// Constructs a response channel using the specified environment.
        /// </summary>
        /// <param name="environment">The environment from which to send a response.</param>
        void ConstructResponseChannel(RuntimeEnvironment environment);
    }
}
