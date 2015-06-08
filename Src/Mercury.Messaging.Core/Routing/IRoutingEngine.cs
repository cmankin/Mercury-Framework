using System;
using System.Collections.Generic;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Routing
{
    /// <summary>
    /// Describes a typical routing engine to handle message delivery.
    /// </summary>
    public interface IRoutingEngine
    {
        /// <summary>
        /// Gets a multicast channel for the specified reference channels.
        /// </summary>
        /// <param name="channels">An enumerable of reference channels to include.</param>
        /// <returns>A multicast channel for the specified reference channels.</returns>
        IUntypedChannel GetMulticastChannel(IEnumerable<LocalRef> channels);

        /// <summary>
        /// Gets an untyped channel that references the specified resource.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <param name="isSynchronous">A value indicating whether a synchronous channel should be created.</param>
        /// <returns>An untyped channel that references the specified resource.</returns>
        IUntypedChannel GetUntypedChannel(string resourceId, bool isSynchronous);

        /// <summary>
        /// Returns a value indicating whether the specified 
        /// resource ID corresponds to an agent.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to check.</param>
        /// <returns>True if the specified resource ID corresponds to an agent; otherwise, false.</returns>
        bool IsAgent(string resourceId);

        /// <summary>
        /// Sends a message of the specified type to the specified agent.
        /// </summary>
        /// <typeparam name="T">The type of the message to send over the channel.</typeparam>
        /// <param name="agentId">The ID of the agent on which to retrieve a channel.</param>
        /// <param name="message">The message to send.</param>
        void Send<T>(string agentId, T message);

        /// <summary>
        /// Sends a message of the specified type to all local agents.
        /// </summary>
        /// <typeparam name="T">The type of the message to send over the channel.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="includeNonAgents">A value indicating whether to include persisted, 
        /// non agent resources, such as channels, in the send operation.</param>
        void Send<T>(T message, bool includeNonAgents = false);

        /// <summary>
        /// Sends a message of the specified type to all agents of the specified type on this environment.  
        /// If needing to execute remotely, a remote runtime environment can be specified in the 
        /// appropriate overload.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent for which a channel will be retrieved.</typeparam>
        /// <typeparam name="T">The type of the message to send over the channel.</typeparam>
        /// <param name="message">The message to send.</param>
        void Send<TAgent, T>(T message) 
            where TAgent : class, Agent;

        /// <summary>
        /// Sends a message of the specified type to all agents of the specified type on the specified runtime environment.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent for which a channel will be retrieved.</typeparam>
        /// <typeparam name="T">The type of the message to send over the channel.</typeparam>
        /// <param name="runtimeAddress">The Uri address of the runtime environment on which this message should be handled.</param>
        /// <param name="message">The message to send.</param>
        void Send<TAgent, T>(Uri runtimeAddress, T message) 
            where TAgent : class, Agent;
    }
}
