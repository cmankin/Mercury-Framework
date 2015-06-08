using System;
using System.Net;
using Mercury.Messaging.Core;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Runtime.Scheduler;

namespace Mercury.Messaging.Runtime
{
    /// <summary>
    /// Core extensions for a Mercury.Messaging.Runtime.RuntimeEnvironment class.
    /// </summary>
    public static class RuntimeExtensions
    {
        /// <summary>
        /// Returns a reference to a remote resource with the specified 
        /// ID at the specified remote IP address and port.
        /// </summary>
        /// <param name="environment">The runtime environment to use.</param>
        /// <param name="id">The ID of the remote resource.</param>
        /// <param name="address">The IP address of the machine hosting the remote environment.</param>
        /// <param name="port">The port number on which the remote environment is listening.</param>
        /// <returns>A reference to a remote resource with the specified 
        /// ID at the specified remote IP address and port.</returns>
        public static LocalRef GetRef(this RuntimeEnvironment environment, string id, string address, int port)
        {
            return environment.GetRef(id, new IPEndPoint(IPAddress.Parse(address), port), RuntimeEnvironment.DEFAULT_REMOTE_TIMEOUT);
        }

        /// <summary>
        /// Creates and returns a reference to a new anonymous agent.
        /// </summary>
        /// <param name="environment">The runtime environment on which to create the agent.</param>
        /// <param name="port">The action entry point for the anonymous agent.</param>
        /// <returns>A reference to a new anonymous agent.</returns>
        public static LocalRef NewAnonymous(this RuntimeEnvironment environment, Action<AgentPort> port)
        {
            return AnonymousAgent.New(environment, port);
        }

        /// <summary>
        /// Spawns and returns a reference to a new agent of the specified type.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to spawn.</typeparam>
        /// <param name="environment">The runtime environment on which to spawn the agent.</param>
        /// <returns>A reference to a new agent of the specified type.</returns>
        public static LocalRef Spawn<TAgent>(this RuntimeEnvironment environment) where TAgent : class, Agent
        {
            string id = environment.Spawn(typeof(TAgent));
            return environment.GetRef(id);
        }

        /// <summary>
        /// Spawns and returns a reference to a new agent of the specified type.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to spawn.</typeparam>
        /// <param name="environment">The runtime environment on which to spawn the agent.</param>
        /// <param name="args">Constructor args for agent construction.</param>
        /// <returns>A reference to a new agent of the specified type.</returns>
        public static LocalRef Spawn<TAgent>(this RuntimeEnvironment environment, params object[] args) 
            where TAgent : class, Agent
        {
            string id = environment.Spawn(typeof(TAgent), args);
            return environment.GetRef(id);
        }

        /// <summary>
        /// Spawns and links a new agent of the specified type to the agent with the specified agent ID.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to spawn.</typeparam>
        /// <param name="environment">The runtime environment on which to spawn the agent.</param>
        /// <param name="linkId">The agent ID of the agent with which to create a link.</param>
        /// <param name="args">Constructor args for agent construction.</param>
        /// <returns>A reference to a new agent of the specified type.</returns>
        public static LocalRef SpawnLink<TAgent>(this RuntimeEnvironment environment, string linkId, params object[] args) 
            where TAgent : class, Agent
        {
            LocalRef agent = environment.Spawn<TAgent>(args);
            if (agent != null)
                environment.Link(linkId, agent.ResId);
            return agent;
        }

        /// <summary>
        /// Kills a resource on a runtime environment.
        /// </summary>
        /// <param name="environment">The runtime environment on which the resource is running.</param>
        /// <param name="resource">A reference to the resource to kill.</param>
        public static void Kill(this RuntimeEnvironment environment, LocalRef resource)
        {
            environment.Kill(resource.ResId);
        }

        /// <summary>
        /// Gets a new timer scheduler.
        /// </summary>
        /// <param name="environment">The runtime environment on which to run the scheduler.</param>
        /// <returns>A new timer scheduler.</returns>
        public static IScheduler GetScheduler(this RuntimeEnvironment environment)
        {
            return new TimerScheduler(environment);
        }

        /// <summary>
        /// Gets a timeout channel for the specified resource. The default timeout is 30 
        /// seconds. If the timeout value is changed, the countdown will be restarted.
        /// </summary>
        /// <param name="environment">The environment on which to create the channel.</param>
        /// <param name="resource">The resource to reference.</param>
        /// <returns>A timeout channel for the specified resource.</returns>
        public static ITimeoutChannel GetTimeoutChannel(this RuntimeEnvironment environment, InternalResource resource)
        {
            return ChannelFactory.Create<ResourceTimeoutChannel>(environment, resource) as ITimeoutChannel;
        }
    }
}
