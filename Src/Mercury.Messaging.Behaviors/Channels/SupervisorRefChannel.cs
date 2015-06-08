using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Behaviors.Channels
{
    /// <summary>
    /// Represents a specialized channel to a supervisor resource.
    /// </summary>
    public class SupervisorRefChannel : SynchronousChannel, SupervisorRef
    {
        /// <summary>
        /// Initializes a default instance of the SupervisorRefChannel 
        /// class with the specified agent port.
        /// </summary>
        /// <param name="port">The agent port of the supervisor to reference.</param>
        public SupervisorRefChannel(AgentPort port)
            : base(port)
        {
            if (!port.AgentType.IsDerivedFrom(typeof(Supervisor)))
                throw new ArgumentException("This reference can only be obtained on supervisor agents.");
            this._id = port.Id;
        }

        /// <summary>
        /// The agent ID.
        /// </summary>
        protected readonly string _id;

        /// <summary>
        /// Gets the agent ID for the referenced supervisor.
        /// </summary>
        new public string ResId
        {
            get { return this._id; }
        }

        /// <summary>
        /// Gets the agent port for the referenced supervisor.
        /// </summary>
        /// <returns></returns>
        internal AgentPort GetPort()
        {
            return this.Resource as AgentPort;
        }
    }
}
