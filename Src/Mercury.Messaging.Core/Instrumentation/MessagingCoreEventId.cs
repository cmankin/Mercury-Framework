using System;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Describes the event identifiers used by the messaging core.
    /// </summary>
    public enum MessagingCoreEventId
    {
        /// <summary>
        /// A general error event
        /// </summary>
        Error = 7000,

        /// <summary>
        /// A critical error event.
        /// </summary>
        CriticalError = 7001,

        /// <summary>
        /// An runtime error occurred.
        /// </summary>
        RuntimeError = 7002,

        /// <summary>
        /// An error in agent processing occurred.
        /// </summary>
        AgentError = 7003,

        /// <summary>
        /// An error occurred during message delivery.
        /// </summary>
        MessageDeliveryError = 7004
    }
}
