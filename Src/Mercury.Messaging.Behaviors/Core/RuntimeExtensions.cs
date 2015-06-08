using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Behaviors.Channels;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Core behaviors extensions to the RuntimeEnvironment.
    /// </summary>
    public static class RuntimeExtensions
    {
        /// <summary>
        /// Spawns and returns a reference to a supervisor created with the specified configuration.
        /// </summary>
        /// <typeparam name="TSupervisor">The type of the supervisor to spawn.</typeparam>
        /// <param name="environment">The runtime environment on which to spawn the supervisor.</param>
        /// <param name="strategy">The restart strategy for the supervisor.</param>
        /// <param name="specs">Any static child specifications.</param>
        /// <returns>A reference to a supervisor created with the specified configuration.</returns>
        public static SupervisorRef SpawnSupervisor<TSupervisor>(this RuntimeEnvironment environment, 
            RestartStrategy strategy, params ChildSpecification[] specs) where TSupervisor : Supervisor
        {
            LocalRef agent = environment.Spawn<TSupervisor>(strategy, specs);
            if (agent != null)
            {
                AgentPort port = environment.FindAgentPort(agent.ResId);
                if (port != null)
                    return new SupervisorRefChannel(port);
            }
            return null;
        }

        /// <summary>
        /// Returns a reference to a supervisor with the specified ID.
        /// </summary>
        /// <param name="environment">The runtime environment of the supervisor.</param>
        /// <param name="id">The ID of the supervisor to find.</param>
        /// <returns>A reference to a supervisor with the specified ID.</returns>
        public static SupervisorRef GetSupervisor(this RuntimeEnvironment environment, string id)
        {
            LocalRef agent = environment.GetRef(id);
            if (agent != null)
            {
                AgentPort port = environment.FindAgentPort(agent.ResId);
                if (port != null && port.AgentType.IsDerivedFrom(typeof(Supervisor)))
                    return new SupervisorRefChannel(port);
            }
            return null;
        }
    }
}
