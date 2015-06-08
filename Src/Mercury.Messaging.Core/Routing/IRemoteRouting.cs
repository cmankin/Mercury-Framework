using System;
using System.Net;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Routing
{
    /// <summary>
    /// Describes an interface to a set of remote routing procedures.
    /// </summary>
    public interface IRemoteRouting
    {
        /// <summary>
        /// Attempts to expire any posted, remote operations that are waiting to receive message delivery failure notifications.
        /// </summary>
        void ExpirePostedOperations();

        /// <summary>
        /// Returns a value indicating whether the specified node has been registered.
        /// </summary>
        /// <param name="nodeId">The address identifier of the node to check.</param>
        /// <returns>True if the specified node has been registered; otherwise, false.</returns>
        bool IsRegistered(string nodeId);

        /// <summary>
        /// Returns a channel to the specified remote agent at the specified end point.
        /// </summary>
        /// <param name="agentId">The ID of the remote agent.</param>
        /// <param name="remoteEndPoint">The IP end point of the node on which the agent is located.</param>
        /// <returns>A channel to the specified remote agent at the specified end point.</returns>
        IUntypedChannel GetRemoteChannel(string agentId, IPEndPoint remoteEndPoint);

        /// <summary>
        /// Returns a channel to the specified remote agent at the specified end point.
        /// </summary>
        /// <param name="agentId">The ID of the remote agent.</param>
        /// <param name="remoteEndPoint">The IP end point of the node on which the agent is located.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        /// <returns>A channel to the specified remote agent at the specified end point.</returns>
        IUntypedChannel GetRemoteChannel(string agentId, IPEndPoint remoteEndPoint, Type destinationType);

        /// <summary>
        /// Returns a channel to the specified remote agent at the specified end point.
        /// </summary>
        /// <param name="agentId">The ID of the remote agent.</param>
        /// <param name="remoteEndPoint">The IP end point of the node on which the agent is located.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        /// <param name="timeout">The time span to wait for the remote channel to receive a message acknowledgement.</param>
        /// <returns>A channel to the specified remote agent at the specified end point.</returns>
        IUntypedChannel GetRemoteChannel(string agentId, IPEndPoint remoteEndPoint, Type destinationType, TimeSpan timeout);

        /// <summary>
        /// Resolves the specified node name to an IP end point.
        /// </summary>
        /// <param name="nodeId">The node name to resolve.</param>
        /// <returns>An IP end point associated with the specified node name.</returns>
        IPEndPoint ResolveNode(string nodeId);

        /// <summary>
        /// Registers the specified node name with the specified IP end point.
        /// </summary>
        /// <param name="nodeId">The node name to register.</param>
        /// <param name="endPoint">The IP end point to associate.</param>
        void RegisterNode(string nodeId, IPEndPoint endPoint);

        /// <summary>
        /// Unregisters the specified node name.
        /// </summary>
        /// <param name="nodeId">The node name to unregister.</param>
        void UnregisterNode(string nodeId);
    }
}
