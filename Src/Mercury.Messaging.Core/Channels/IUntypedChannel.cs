using System;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// Represents a non-generic channel over which a message may be sent.
    /// </summary>
    public interface IUntypedChannel
        : IChannel
    {
        /// <summary>
        /// Sends a message to the receiver on this channel.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        void Send<T>(T message);
    }
}
