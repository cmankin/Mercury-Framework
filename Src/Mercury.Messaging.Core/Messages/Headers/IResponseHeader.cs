using System;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a set of message header information for a response.
    /// </summary>
    public interface IResponseHeader : 
        IMessageHeader
    {
        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        string RequestId { get; }
    }
}
