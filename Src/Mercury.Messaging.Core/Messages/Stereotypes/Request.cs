using System;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a message stereotype designating a message to which a response is expected.
    /// </summary>
    /// <typeparam name="T">The type of the message body.</typeparam>
    public interface Request<T> : 
        IMessage<T>, 
        IRequestHeader
    {
        /// <summary>
        /// Gets the channel on which the response should be sent.
        /// </summary>
        IUntypedChannel ResponseChannel { get; }
    }
}
