using System;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// An agent, deriving from the AnonymousAgent class, which may be built and called at runtime.
    /// </summary>
    public class AnonymousAgent : Agent
    {
        private AgentPort _port;

        /// <summary>
        /// Initializes a default instance of the AnonymousAgent 
        /// class with the specified agent port.
        /// </summary>
        /// <param name="port">The agent port to assign.</param>
        public AnonymousAgent(AgentPort port)
        {
            this._port = port;
        }

        /// <summary>
        /// Creates and returns a reference to a new anonymous agent.
        /// </summary>
        /// <param name="environment">The environment on which to run the agent.</param>
        /// <param name="port">An action that accepts an agent port.</param>
        /// <returns>A reference to a new anonymous agent.</returns>
        public static LocalRef New(RuntimeEnvironment environment, Action<AgentPort> port)
        {
            LocalRef agent = environment.Spawn<AnonymousAgent>();
            if (agent != null)
                port.Invoke(environment.FindAgentPort(agent.ResId));
            return agent;
        }
    }
}
