using System;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a message notifying an agent that a linked agent has shutdown.
    /// </summary>
    public class Exit
    {
        /// <summary>
        /// Initializes a default instance of the Exit class.
        /// </summary>
        public Exit()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the Exit class with the specified agent ID.
        /// </summary>
        /// <param name="id">The ID of the exited agent.</param>
        public Exit(string id)
        {
            this.InstanceId = id;
        }

        /// <summary>
        /// Gets the ID of the exited agent instance.
        /// </summary>
        public string InstanceId { get; set; }
    }
}
