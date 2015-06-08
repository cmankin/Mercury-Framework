using System;
using Mercury;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Behaviors;
using System.Collections.Concurrent;

namespace Mercury.Messaging.Behaviors.IPC2
{
    /// <summary>
    /// Represents a generic server agent.
    /// </summary>
    public abstract class GenericServer : 
        Agent
    {
        #region Constructors

        /// <summary>
        /// Initializes a default instance of the GenericServer class with the specified values.
        /// </summary>
        /// <param name="port">The agent port on which to receive.</param>
        /// <param name="retrySpec">A retry specification for lost messages.</param>
        public GenericServer(AgentPort port, RetrySpecification retrySpec)
        {
            // Set internal members
            this.RetrySpecification = retrySpec;
            this.Port = port;
            this.Environment = port.Environment;

            // Initialize new receivers dictionary
            this._receivers = new Dictionary<Type, ReceiveHandler<object>>();
        }

        #endregion

        #region Receivers

        private Dictionary<Type, ReceiveHandler<object>> _receivers;

        /// <summary>
        /// Gets a dictionary of registered receive handlers.
        /// </summary>
        protected Dictionary<Type, ReceiveHandler<object>> Receivers
        {
            get
            {
                return this._receivers;
            }
        }

        /// <summary>
        /// Captures and handles the internal receive.
        /// </summary>
        protected virtual void InternalReceive<T>(Request<T> request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            // Server reply
            request.ResponseChannel.Send<Response<ServerReply>>(
                new ResponseBase<ServerReply>(new ServerReply(), request.RequestId));

            // Get internal message
            T message = request.Body;
            if (message != null)
            {
                // Attempt receive
                Type keyType = typeof(T);
                if (this.Receivers.ContainsKey(keyType))
                    this.Receivers[keyType].Invoke(message);
            }
        }

        /// <summary>
        /// Sends a server reply response on the specified response channel with the specified request ID.
        /// </summary>
        /// <param name="responseChannel">The response channel on which to send the reply.</param>
        /// <param name="requestId">The unique ID associated with the request.</param>
        protected virtual void Reply(IUntypedChannel responseChannel, string requestId)
        {
            responseChannel.Send<Response<ServerReply>>(new ResponseBase<ServerReply>(new ServerReply(), requestId));
        }

        /// <summary>
        /// Sends a request of the specified message and type to the agent or server 
        /// with the specified ID.  When sending a server request, a server reply is 
        /// expected to be returned.  Failure to reply could cause the message to be 
        /// resent, or eventually, abandoned.
        /// </summary>
        /// <typeparam name="T">The type of the message request to send.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="agentId">The agent ID of the receiver.</param>
        /// <returns>True if the request was sent successfully; otherwise, false.</returns>
        public virtual bool SendServerRequest<T>(T message, string agentId)
        {
            if (message != null && !string.IsNullOrEmpty(agentId))
            {
                // Get receiver agent
                LocalRef agent = this.Environment.GetRef(agentId);

                int attempts = 1;
                bool trySend = true;
                while (trySend)
                {
                    Future<Response<ServerReply>> channel = agent.SendFuture<Response<ServerReply>, T>(message);
                    channel.WaitUntilCompleted(this.RetrySpecification.WaitInterval);

                    // Get future
                    Response<ServerReply> reply = channel.Get;
                    if (reply != null)
                        return true;
                    
                    // Retry
                    attempts++;
                    if (this.RetrySpecification.Attempts > -1 && 
                        attempts > this.RetrySpecification.Attempts)
                        trySend = false;
                }
            }
            return false;
        }

        /// <summary>
        /// Registers a receive handler for a message of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the message to receive.</typeparam>
        /// <param name="handler">The receive handler to use.</param>
        public void Receive<T>(ReceiveHandler<T> handler) where T : class
        {
            // Overwrite if receiver already exists on type
            if (this.Receivers.ContainsKey(typeof(T)))
                this.Receivers.Remove(typeof(T));

            ReceiveHandler<object> persistHandler = new ReceiveHandler<object>(obj => handler((T)obj));
            this.Receivers.Add(typeof(T), persistHandler);
            this.Port.Receive<Request<T>>(InternalReceive);
        }

        #endregion

        #region Data
        /// <summary>
        /// The retry specification.
        /// </summary>
        protected readonly RetrySpecification RetrySpecification;

        /// <summary>
        /// The runtime environment.
        /// </summary>
        protected RuntimeEnvironment Environment;

        /// <summary>
        /// The agent port.
        /// </summary>
        protected AgentPort Port;
        #endregion
    }
}
