using System;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a set of header information for a message.
    /// </summary>
    public interface IMessageHeader
    {
        /// <summary>
        /// The sender address of the message.
        /// </summary>
        Uri SenderAddress { get; }

        /// <summary>
        /// The address for the message destination.
        /// </summary>
        Uri DestinationAddress { get; }

        /// <summary>
        /// The identifier correlating this message to a message exchange/conversation.
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// The identifier for this message.
        /// </summary>
        string MessageId { get; }

        /// <summary>
        /// The address where message delivery faults should be sent.
        /// </summary>
        Uri FaultAddress { get; }

        /// <summary>
        /// Gets the headers collection.
        /// </summary>
        IHeaders Headers { get; }
    }
}
