using System;
using System.Net;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Routing
{
    /// <summary>
    /// Represents a routing engine for a RuntimeEnvironment.
    /// </summary>
    public class RuntimeRoutingEngine : 
        IRoutingEngine,
        IRemoteRouting
    {
        #region Constructors

        /// <summary>
        /// Initializes a default instance of the RuntimeRoutingEngine 
        /// class with the specified RuntimeEnvironment.
        /// </summary>
        /// <param name="environment">The RuntimeEnvironment to use.</param>
        public RuntimeRoutingEngine(RuntimeEnvironment environment)
        {
            this.Environment = environment;
        }

        #endregion

        #region IRoutingEngine

        /// <summary>
        /// Gets the local runtime environment on which routing occurs.
        /// </summary>
        protected RuntimeEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets a multicast channel for the specified reference channels.
        /// </summary>
        /// <param name="channels">An enumerable of reference channels to include.</param>
        /// <returns>A multicast channel for the specified reference channels.</returns>
        public IUntypedChannel GetMulticastChannel(IEnumerable<LocalRef> channels)
        {
            if (channels != null)
                return new MultiCastChannel(channels) as IUntypedChannel;
            return null;
        }

        /// <summary>
        /// Gets an untyped channel that references the specified resource.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <param name="isSynchronous">A value indicating whether a synchronous channel should be created.</param>
        /// <returns>An untyped channel that references the specified resource.</returns>
        public IUntypedChannel GetUntypedChannel(string resourceId, bool isSynchronous)
        {
            IPEndPoint remoteEndPoint = this.ResolveNode(resourceId);
            if (remoteEndPoint != null)
                return GetRemoteChannel(resourceId, remoteEndPoint, null);
            if (isSynchronous)
                return GetSynchronousChannel(resourceId);
            return GetLocalAsyncChannel(resourceId);
        }

        /// <summary>
        /// Returns a synchronous channel for the local resource.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <returns>A synchronous channel referencing the local resource.</returns>
        protected IUntypedChannel GetSynchronousChannel(string resourceId)
        {
            return ChannelFactory.Create<SynchronousChannel>(this.Environment, resourceId);
        }

        /// <summary>
        /// Returns an asynchronous channel for the local resource.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <returns>An asynchronous channel referencing the local resource.</returns>
        protected IUntypedChannel GetLocalAsyncChannel(string resourceId)
        {
            return ChannelFactory.Create<LocalRefChannel>(this.Environment, resourceId);
        }

        /// <summary>
        /// Returns a value indicating whether the specified 
        /// resource ID corresponds to an agent.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to check.</param>
        /// <returns>True if the specified resource ID corresponds to an agent; otherwise, false.</returns>
        public bool IsAgent(string resourceId)
        {
            AgentPort port = this.Environment.FindAgentPort(resourceId);
            if (port != null)
                return true;
            return false;
        }

        /// <summary>
        /// Sends the message to the specified resource.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="resourceId">The ID of the resource to send to.</param>
        /// <param name="message">The message to send.</param>
        public void Send<T>(string resourceId, T message)
        {
            IUntypedChannel channel = this.GetUntypedChannel(resourceId, false);
            if (channel != null)
                channel.Send<T>(message);
        }

        /// <summary>
        /// Sends a message of the specified type to all local agents.
        /// </summary>
        /// <typeparam name="T">The type of the message to send over the channel.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="includeNonAgents">A value indicating whether to include persisted, 
        /// non agent resources, such as channels, in the send operation.</param>
        public void Send<T>(T message, bool includeNonAgents = false)
        {
            IEnumerable<IResource> resources = this.Environment.GetAllInternalResources();
            if (resources != null)
            {
                IUntypedChannel local;
                foreach (IResource res in resources)
                {
                    local = GetUntypedChannel(res.Id, false);
                    if (local != null)
                    {
                        if (includeNonAgents)
                        {
                            local.Send<T>(message);
                        }
                        else
                        {
                            if (res is AgentPort)
                                local.Send<T>(message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends the message to all agents of the specified type.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to send to.</typeparam>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public void Send<TAgent, T>(T message) where TAgent : class, Agent
        {
            IEnumerable<LocalRef> chs = this.Environment.GetAgentRefsByType(typeof(TAgent));
            if (chs != null)
            {
                LocalRef channel = new MultiCastChannel(chs) as LocalRef;
                channel.Send<T>(message);
            }
        }

        /// <summary>
        /// Sends the message to the specified agent.
        /// </summary>
        /// <typeparam name="TAgent">The type of the agent to send to.</typeparam>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="runtimeAddress">The URI address of the agent to send to.</param>
        /// <param name="message">The message to send.</param>
        public void Send<TAgent, T>(Uri runtimeAddress, T message) where TAgent : class, Agent
        {
            // Get remote end point
            IPEndPoint ep = this.ResolveNode(runtimeAddress.ToString());
            
            // Get channel
            IUntypedChannel channel = this.GetRemoteChannel(runtimeAddress.ToString(), ep, null);
            if (channel != null && ep != null)
                channel.Send<T>(message);
        }

        #endregion

        #region IRemoteRouting
        /// <summary>
        /// Attempts to expire any posted, remote operations that are waiting to receive message delivery failure notifications.
        /// </summary>
        public void ExpirePostedOperations()
        {
            RemotingManager.ExpireOperations();
        }

        /// <summary>
        /// Returns a value indicating whether the specified node has been registered.
        /// </summary>
        /// <param name="nodeId">The address identifier of the node to check.</param>
        /// <returns>True if the specified node has been registered; otherwise, false.</returns>
        public bool IsRegistered(string nodeId)
        {
            return RemotingManager.ContainsNode(nodeId);
        }

        /// <summary>
        /// Returns a remote channel constructed from the specified values.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <param name="remoteEndPoint">The IP end point of the runtime environment on which the resource exists.</param>
        /// <returns>A remote channel constructed from the specified values.</returns>
        public IUntypedChannel GetRemoteChannel(string resourceId, IPEndPoint remoteEndPoint)
        {
            return this.GetRemoteChannel(resourceId, remoteEndPoint, (Type)null);
        }

        /// <summary>
        /// Returns a remote channel constructed from the specified values.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <param name="remoteEndPoint">The IP end point of the runtime environment on which the resource exists.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        /// <returns>A remote channel constructed from the specified values.</returns>
        public IUntypedChannel GetRemoteChannel(string resourceId, IPEndPoint remoteEndPoint, Type destinationType)
        {
            return this.GetRemoteChannel(resourceId, remoteEndPoint, destinationType, RuntimeEnvironment.DEFAULT_REMOTE_TIMEOUT);
        }

        /// <summary>
        /// Returns a remote channel constructed from the specified values.
        /// </summary>
        /// <param name="resourceId">The ID of the resource to reference.</param>
        /// <param name="remoteEndPoint">The IP end point of the runtime environment on which the resource exists.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        /// <param name="timeout">A timeout value for a message received reply from the remote environment.</param>
        /// <returns>A remote channel constructed from the specified values.</returns>
        public IUntypedChannel GetRemoteChannel(string resourceId, IPEndPoint remoteEndPoint, Type destinationType, TimeSpan timeout)
        {
            return ChannelFactory.Create<RemotingChannel>(remoteEndPoint, resourceId, destinationType, timeout, this.Environment) as IUntypedChannel;
        }

        /// <summary>
        /// Resolves the IP end point registered with the specified node ID.
        /// </summary>
        /// <param name="nodeId">The identifier of the node to resolve.</param>
        /// <returns>The IP end point registered with the specified node ID.</returns>
        public IPEndPoint ResolveNode(string nodeId)
        {
            var info = this.ResolveNodeInternal(nodeId);
            return info != null ? info.EndPoint : null;
        }

        internal RemotingInfo ResolveNodeInternal(string resourceOrNodeId)
        {
            // Check node
            if (string.IsNullOrEmpty(resourceOrNodeId))
                return null;

            // Get authority
            string checkId = RemotingInfo.GetNodeId(resourceOrNodeId);
            return RemotingManager.TryResolve(checkId);
        }

        /// <summary>
        /// Registers the specified node ID with the specified IP end point.
        /// </summary>
        /// <param name="nodeId">The identifier of the node to register.</param>
        /// <param name="endPoint">The IP end point to register.</param>
        public void RegisterNode(string nodeId, IPEndPoint endPoint)
        {
            RemotingManager.Register(nodeId, endPoint);
        }

        /// <summary>
        /// Unregisters the specified node ID.
        /// </summary>
        /// <param name="nodeId">The identifier of the node to unregister.</param>
        public void UnregisterNode(string nodeId)
        {
            RemotingManager.Unregister(nodeId);
        }
        #endregion
    }
}
