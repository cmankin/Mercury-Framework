using System;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// Describes a fault on a packet.
    /// </summary>
    public enum PacketFaultCode
    {
        /// <summary>
        /// The received message was formatted incorrectly.
        /// </summary>
        InvalidMessageFormat,

        /// <summary>
        /// The received message did not contain the expected "end message" record.
        /// </summary>
        UnexpectedEndOfMessage,

        /// <summary>
        /// An error occurred during processing of the received message.
        /// </summary>
        ProcessError,

        /// <summary>
        /// Encountered a message that was too large to process.
        /// </summary>
        MessageSizeOverflow
    }
}
