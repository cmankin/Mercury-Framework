using System;
using Mercury;
using Mercury.Messaging.Core;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Routing
{
    /// <summary>
    /// Describes a Message carried by a Channel.
    /// </summary>
    public interface IRoutingContext
    { 
        /// <summary>
        /// Gets the message for this context.
        /// </summary>
        object Message { get; }

        /// <summary>
        /// Gets the channel for this context.
        /// </summary>
        IChannel Channel { get; }

        /// <summary>
        /// Gets the expected message type.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// Gets a value indicating whether the message 
        /// should be processed synchronously.
        /// </summary>
        bool IsSynchronous { get; }

        /// <summary>
        /// Gets the route identifier.
        /// </summary>
        string RouteId { get; }
    }

    /// <summary>
    /// Describes a message carried by a channel.
    /// </summary>
    public class RoutingContext : IRoutingContext
    {
        /// <summary>
        /// Initializes a default instance of the RoutingContext class with the specified values.
        /// </summary>
        /// <param name="message">The message being sent.</param>
        /// <param name="channel">The channel over which the message was sent.</param>
        /// <param name="isSynchronous">A value indicating whether the message should be processed synchronously.</param>
        public RoutingContext(object message, IChannel channel, bool isSynchronous)
        {
            this._message = message;
            this._channel = channel;
            this._isSynchronous = isSynchronous;

            // Get message type
            var type = message.GetType();
            var iType = type.GetGenericInterfaceFromTypeDefinition(typeof(Request<>));
            if (iType != null)
            {
                type = iType;
            }
            else
            {
                iType = type.GetGenericInterfaceFromTypeDefinition(typeof(Response<>));
            }
            this._messageType = type;
            this._routeId = Guid.NewGuid().ToString();
        }

        private object _message;

        /// <summary>
        /// Gets the message for this context.
        /// </summary>
        public object Message
        {
            get { return this._message; }
        }

        private IChannel _channel;

        /// <summary>
        /// Gets the channel for this context.
        /// </summary>
        public IChannel Channel
        {
            get { return this._channel; }
        }

        private Type _messageType;

        /// <summary>
        /// Gets the expected message type.
        /// </summary>
        public Type MessageType
        {
            get { return this._messageType; }
        }

        private bool _isSynchronous;

        /// <summary>
        /// Gets a value indicating whether the message described 
        /// in this context should be processed synchronously.
        /// </summary>
        public bool IsSynchronous
        {
            get { return this._isSynchronous; }
        }

        private string _routeId;

        /// <summary>
        /// Gets the route identifier.
        /// </summary>
        public string RouteId
        {
            get { return this._routeId; }
        }
    }

    /// <summary>
    /// Describes a message carried by a channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message in this context.</typeparam>
    /// <typeparam name="TChannel">The type of the channel in this context.</typeparam>
    public class RoutingContext<TMessage, TChannel> : 
        IRoutingContext
        where TChannel : IChannel
    {
        /// <summary>
        /// Initializes a default instance of the RoutingContext(Of TMessage, TChannel) 
        /// class with the specified message and channel.
        /// </summary>
        /// <param name="message">The message to route.</param>
        /// <param name="channel">The channel on which the message will be sent.</param>
        /// <param name="isSynchronous">A value indicating whether the message should be processed synchronously.</param>
        public RoutingContext(TMessage message, TChannel channel, bool isSynchronous)
        {
            this.Message = message;
            this.Channel = channel;
            this._messageType = typeof(TMessage);
            if (typeof(TMessage) == typeof(object))
                this._messageType = message.GetType();
            this._isSynchronous = isSynchronous;
            this._routeId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the generically typed message.
        /// </summary>
        public TMessage Message { get; private set; }

        /// <summary>
        /// Gets the generically typed channel.
        /// </summary>
        public TChannel Channel { get; private set; }

        /// <summary>
        /// Gets the message in this context.
        /// </summary>
        object IRoutingContext.Message
        {
            get { return this.Message; }
        }

        /// <summary>
        /// Gets the channel in this context.
        /// </summary>
        IChannel IRoutingContext.Channel
        {
            get { return this.Channel; }
        }

        private Type _messageType;

        /// <summary>
        /// Gets the type of the message in this context.
        /// </summary>
        public Type MessageType
        {
            get { return this._messageType; }
        }

        private bool _isSynchronous;

        /// <summary>
        /// Gets a value indicating whether the message described 
        /// in this context should be processed synchronously.
        /// </summary>
        public bool IsSynchronous
        {
            get { return this._isSynchronous; }
        }

        private string _routeId;

        /// <summary>
        /// Gets the route identifier.
        /// </summary>
        public string RouteId
        {
            get { return this._routeId; }
        }
    }
}
