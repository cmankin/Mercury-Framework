using System;
using System.Reflection;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// A factory for agent creation.
    /// </summary>
    public static class AgentFactory
    {
        /// <summary>
        /// Creates an agent port with the specified values.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to create.</typeparam>
        /// <param name="environment">The runtime environment on which the agent is registered.</param>
        /// <returns>An agent port instance constructed from the specified values or null.</returns>
        public static AgentPort Create<TAgent>(RuntimeEnvironment environment) where TAgent : Agent
        {
            return Create<TAgent>(environment, null);
        }

        /// <summary>
        /// Creates an agent port with the specified values.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to create.</typeparam>
        /// <param name="environment">The runtime environment on which the agent is registered.</param>
        /// <param name="args">Constructor arguments for agent construction.</param>
        /// <returns>An agent port instance constructed from the specified values or null.</returns>
        public static AgentPort Create<TAgent>(RuntimeEnvironment environment, params object[] args) where TAgent : Agent
        {
            AgentPort port = environment.NewPort();
            environment.AddResource(port);
            port.AgentHandle = CreateHandlerInstance<TAgent>(port, args);
            return port;
        }

        /// <summary>
        /// Creates an agent port with the specified values.
        /// </summary>
        /// <param name="environment">The runtime environment on which the agent is registered.</param>
        /// <param name="agentType">The type of the agent to create.</param>
        /// <returns>An agent port instance constructed from the specified values or null.</returns>
        public static AgentPort Create(RuntimeEnvironment environment, Type agentType)
        {
            return Create(environment, agentType, null);
        }

        /// <summary>
        /// Creates an agent port with the specified values.
        /// </summary>
        /// <param name="environment">The runtime environment on which the agent is registered.</param>
        /// <param name="agentType">The type of the agent to create.</param>
        /// <param name="args">Constructor arguments for agent construction.</param>
        /// <returns>An agent port instance constructed from the specified values or null.</returns>
        public static AgentPort Create(RuntimeEnvironment environment, Type agentType, params object[] args)
        {
            return Create(environment, null, agentType, args);
        }

        /// <summary>
        /// Creates an agent port with the specified resource ID.
        /// </summary>
        /// <param name="environment">The runtime environment on which the agent is registered.</param>
        /// <param name="id">The resource ID to use.</param>
        /// <param name="agentType">The type of the agent to create.</param>
        /// <param name="args">Constructor arguments for agent construction.</param>
        /// <returns>An agent port instance constructed from the specified values or null. 
        /// If the specified resource ID is taken, an argument exception will be thrown.</returns>
        public static AgentPort Create(RuntimeEnvironment environment, string id, Type agentType, params object[] args)
        {
            AgentPort port = environment.NewPort();
            if (string.IsNullOrEmpty(id))
                environment.AddResource(port);
            else
                environment.AddResource(port, id);
            port.AgentHandle = CreateHandlerInstance(agentType, port, args);

            // Trace & return
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, port, environment, "Agent port created.");
            return port;
        }

        internal static Agent CreateHandlerInstance<TAgent>(AgentPort port, params object[] args) where TAgent : Agent
        {
            return CreateHandlerInstance(typeof(TAgent), port, args) as Agent;
        }

        internal static Agent CreateHandlerInstance(Type agentType, AgentPort port, params object[] args)
        {
            object[] instanceArgs = null;
            if (args != null && args.Length > 0)
            {
                instanceArgs = new object[args.Length + 1];
                instanceArgs[0] = port;
                Array.Copy(args, 0, instanceArgs, 1, args.Length);
            }
            else
            {
                instanceArgs = new object[1] { port };
            }
            return CoreInstanceProvider.Create(agentType, instanceArgs) as Agent;
        }
    }
}
