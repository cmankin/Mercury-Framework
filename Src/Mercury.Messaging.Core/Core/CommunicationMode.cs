using System;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Describes the communication delivery pattern.
    /// </summary>
    public enum CommunicationMode
    {
        /// <summary>
        /// Defines an asynchronous communication.
        /// </summary>
        Asynchronous,

        /// <summary>
        /// Defines a synchronous communication, in which the call 
        /// returns after the message has been processed.
        /// </summary>
        Synchronous,

        /// <summary>
        /// Defines a synchronous communication that wraps a future.
        /// </summary>
        SynchronousReturn
    }
}
