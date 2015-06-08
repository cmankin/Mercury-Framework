using System;
using System.Net;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// A reference to a remove agent.
    /// </summary>
    public interface RemoteRef : LocalRef
    {
        /// <summary>
        /// Gets the IP end point of the remote resource.
        /// </summary>
        IPEndPoint EndPoint { get; }
        
        /// <summary>
        /// Gets the identifier of the last send operation.
        /// </summary>
        int LastOperationId { get; }

        /// <summary>
        /// Sends a message synchronously to the receiver on this channel.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>An identifier for the send operation.</returns>
        int SendSync<T>(T message);
    }
}
