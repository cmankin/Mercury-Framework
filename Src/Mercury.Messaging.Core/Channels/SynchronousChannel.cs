using System;
using System.Reflection;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// Represents a channel over which messages may be sent synchronously.
    /// </summary>
    public class SynchronousChannel : 
        LocalRefChannel
    {
        /// <summary>
        /// Initializes a default instance of the SynchronousChannel class with the specified resource.
        /// </summary>
        /// <param name="resource">The resource to reference.</param>
        public SynchronousChannel(InternalResource resource)
            : base(resource)
        {
        }

        /// <summary>
        /// Sends a message to the referenced resource.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public override void Send<T>(T message)
        {
            IRoutingContext ctx = new RoutingContext<T, SynchronousChannel>(message, this, true);
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), ctx, this, this.Environment, null, this.ResId, null);
            this.Resource.Post(ctx);
        }
    }

    /// <summary>
    /// Represents a channel over which messages may be sent synchronously.
    /// </summary>
    /// <typeparam name="T">The type of the message to send.</typeparam>
    public class SynchronousChannel<T> : 
        ChannelBase<T>
    {
        /// <summary>
        /// Initializes a default instance of the SynchronousChannel class with the specified resource.
        /// </summary>
        /// <param name="resource">The resource to reference.</param>
        public SynchronousChannel(InternalResource resource)
            : base(resource)
        {
            this._resourceId = resource.Id;
        }

        /// <summary>
        /// The ID of the referenced resource.
        /// </summary>
        [CLSCompliant(false)]
        protected string _resourceId;

        /// <summary>
        /// Gets the ID of the local resource to which messages on this channel are sent.
        /// </summary>
        public string ResourceId
        {
            get { return this._resourceId; }
        }

        /// <summary>
        /// Sends a message to the referenced resource.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public override void Send(T message)
        {
            IRoutingContext ctx = new RoutingContext<T, SynchronousChannel<T>>(message, this, true);
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), ctx, this, this.Environment, null, this.ResourceId, null);
            this.Resource.Post(ctx);
        }
    }
}
