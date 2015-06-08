using System;
using System.Net;
using System.Reflection;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Routing;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// Represents a factory for creating channel instances.
    /// </summary>
    public static class ChannelFactory
    {
        /// <summary>
        /// Persists the specified channel as a resource on the specified environment.
        /// </summary>
        /// <param name="environment">The environment on which to persist.</param>
        /// <param name="channel">The channel to persist.</param>
        /// <returns>The resource ID of the persisted channel.</returns>
        public static string Persist(RuntimeEnvironment environment, IChannel channel)
        {
            IResource resource = channel as IResource;
            if (resource != null)
            {
                var resId = environment.AddResource(resource);
                ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, resource, environment, "Channel resource peristed.");
                return resId;
            }
            return null;
        }

        /// <summary>
        /// Removes the specified channel resource from the specified environment.
        /// </summary>
        /// <param name="environment">The environment from which to remove the resource.</param>
        /// <param name="channel">The resource channel to remove.</param>
        public static void Expire(RuntimeEnvironment environment, IChannel channel)
        {
            environment.Kill(channel.Id);
            InternalResource resource = channel as InternalResource;
            if (resource != null)
                resource.Id = null;
        }

        /// <summary>
        /// Creates an untyped channel of the specified type with the specified constraints.
        /// </summary>
        /// <typeparam name="TChannel">The type of the channel to create.</typeparam>
        /// <param name="environment">The runtime environment on which the resource exists.</param>
        /// <param name="id">The ID of the resource being referenced.</param>
        /// <returns>An untyped channel of the specified type with the specified constraints.</returns>
        public static IUntypedChannel Create<TChannel>(RuntimeEnvironment environment, string id)
            where TChannel : class, IUntypedChannel
        {
            InternalResource port = GetResource(environment, id);
            if (port != null)
                return CoreInstanceProvider.Create<IUntypedChannel, TChannel>(port);
            return null;
        }

        /// <summary>
        /// Creates an untyped channel of the specified type with the specified constraints.
        /// </summary>
        /// <typeparam name="TChannel">The type of the channel to create.</typeparam>
        /// <param name="environment">The runtime environment on which the agent exists.</param>
        /// <param name="agentType">The type of the agent referenced.</param>
        /// <returns>An untyped channel of the specified type with the specified constraints.</returns>
        public static IUntypedChannel Create<TChannel>(RuntimeEnvironment environment, Type agentType)
            where TChannel : class, IUntypedChannel
        {
            AgentPort port = GetAgentPort(environment, agentType);
            if (port != null)
                return CoreInstanceProvider.Create<IUntypedChannel, TChannel>(port);
            return null;
        }

        /// <summary>
        /// Creates a channel of the specified type with the specified constraints.
        /// </summary>
        /// <typeparam name="TChannel">The type of the channel to create.</typeparam>
        /// <param name="args">Constructor arguments for creating the channel.</param>
        /// <returns>A channel of the specified type with the specified constraints.</returns>
        public static IChannel Create<TChannel>(params object[] args)
            where TChannel : class, IChannel
        {
            return CoreInstanceProvider.Create<IChannel, TChannel>(args);
        }

        /// <summary>
        /// Creates a channel of the specified type with the specified constraints.
        /// </summary>
        /// <typeparam name="TChannel">The type of the channel to create.</typeparam>
        /// <typeparam name="TMessage">The type of messages to send.</typeparam>
        /// <param name="environment">The runtime environment on which the resource exists.</param>
        /// <param name="id">The ID of the resource being referenced.</param>
        /// <returns>A channel of the specified type with the specified constraints.</returns>
        public static IChannel<TMessage> Create<TChannel, TMessage>(RuntimeEnvironment environment, string id)
            where TChannel : class, IChannel<TMessage>
        {
            InternalResource port = GetResource(environment, id);
            if (port!=null)
                return CoreInstanceProvider.Create<IChannel<TMessage>, TChannel>(port);
            return null;
        }

        /// <summary>
        /// Creates a channel of the specified type with the specified constraints.
        /// </summary>
        /// <typeparam name="TChannel">The type of the channel to create.</typeparam>
        /// <typeparam name="TAgent">The type of the agent referenced.</typeparam>
        /// <typeparam name="TMessage">The type of messages to send.</typeparam>
        /// <param name="environment">The runtime environment on which the agent exists.</param>
        /// <returns>A channel of the specified type with the specified constraints.</returns>
        public static IChannel<TMessage> Create<TChannel, TAgent, TMessage>(RuntimeEnvironment environment)
            where TChannel : class, IChannel<TMessage>
            where TAgent : class, Agent
        {
            AgentPort port = GetAgentPort(environment, typeof(TAgent));
            if (port!=null)
                return CoreInstanceProvider.Create<IChannel<TMessage>, TChannel>(port);
            return null;
        }

        internal static InternalResource GetResource(RuntimeEnvironment environment, string id)
        {
            return environment.FindResource(id) as InternalResource;
        }

        internal static AgentPort GetAgentPort(RuntimeEnvironment environment, Type agentType)
        {
            return environment.FindAgentPort(agentType);
        }
    }
}
