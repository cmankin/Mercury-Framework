using System;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// Represents a channel over which a message may be sent.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Gets a value indicating whether this channel has been persisted as a resource.
        /// </summary>
        bool IsPersisted { get; }

        /// <summary>
        /// Gets the resource ID of this channel if it has been persisted.
        /// </summary>
        string Id { get; }
    }

    /// <summary>
    /// Represents a channel over which a message may be sent.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    public interface IChannel<in T> : IChannel
    {
        /// <summary>
        /// Sends a message to the receiver on this channel.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(T message);
    }
}
