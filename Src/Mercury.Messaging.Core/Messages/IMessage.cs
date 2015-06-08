using System;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a basic interface for all messages.
    /// </summary>
    public interface IMessage
    {
    }

    /// <summary>
    /// Represents a generic interface for messages.
    /// </summary>
    /// <typeparam name="T">The type of the message body.</typeparam>
    public interface IMessage<T> : IMessage
    {
        /// <summary>
        /// Gets the message body.
        /// </summary>
        T Body { get; }
    }
}
