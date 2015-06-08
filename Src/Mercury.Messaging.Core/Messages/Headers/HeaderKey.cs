using System;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Provides a set of header keys to be used when setting or retrieving headers on a message.
    /// </summary>
    public static class HeaderKey
    {
        /// <summary>
        /// The body type field.
        /// </summary>
        public static readonly string BodyType = "BodyType";
        /// <summary>
        /// The correlation ID field.
        /// </summary>
        public static readonly string CorrelationId = "CorrelationId";
        /// <summary>
        /// The destination address field.
        /// </summary>
        public static readonly string DestinationAddress = "DestinationAddress";
        /// <summary>
        /// The fault address field.
        /// </summary>
        public static readonly string FaultAddress = "FaultAddress";
        /// <summary>
        /// The message ID field.
        /// </summary>
        public static readonly string MessageId = "MessageId";
        /// <summary>
        /// The request ID field.
        /// </summary>
        public static readonly string RequestId = "RequestId";
        /// <summary>
        /// The response address field.
        /// </summary>
        public static readonly string ResponseAddress = "ResponseAddress";
        /// <summary>
        /// The response end point field.
        /// </summary>
        public static readonly string ResponseEndPoint = "ResponseEndPoint";
        /// <summary>
        /// The sender address field.
        /// </summary>
        public static readonly string SenderAddress = "SenderAddress";
        /// <summary>
        /// The method field.
        /// </summary>
        public static readonly string Method = "Method";
    }
}
