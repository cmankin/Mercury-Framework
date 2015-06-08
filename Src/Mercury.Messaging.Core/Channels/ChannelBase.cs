using System;
using System.Net;
using System.Reflection;
using System.Globalization;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Instrumentation;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// Represents the base implementation of an untyped channel.
    /// </summary>
    public abstract class ChannelBase :
        InternalResource,
        IUntypedChannel
    {
        /// <summary>
        /// Initializes a default instance of the ChannelBase class with the specified resource.
        /// </summary>
        /// <param name="resource">The resource port to reference.</param>
        protected ChannelBase(InternalResource resource)
        {
            this.Resource = resource;
        }

        /// <summary>
        /// The resource to which messages will be sent.
        /// </summary>
        protected internal InternalResource Resource { get; private set; }

        /// <summary>
        /// Sends a message to the referenced resource.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public virtual void Send<T>(T message)
        {
            IRoutingContext ctx = new RoutingContext<T, IChannel>(message, this, false);
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), ctx, this, this.Environment, null, ChannelBase.GetResourceId(this.Resource), null);
            
            // Send
            this.Resource.Post(ctx);
        }

        /// <summary>
        /// Gets a value indicating whether this channel has been persisted as a resource.
        /// </summary>
        public virtual bool IsPersisted
        {
            get { return (!string.IsNullOrEmpty(this.Id)); }
        }

        /// <summary>
        /// Posts a routing context which is relayed to this channel's receiver.
        /// </summary>
        /// <param name="context">The routing context to post.</param>
        protected internal override void Post(IRoutingContext context)
        {
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), context, this, this.Environment, null, ChannelBase.GetResourceId(this.Resource), null);
            this.Resource.Post(context);
        }

        #region Internal Methods
        /// <summary>
        /// Gets the resource identifier from the specified resource.
        /// </summary>
        /// <param name="resource">The resource from which to retrieve a resource identifier.</param>
        /// <returns>The resource identifier or an empty string.</returns>
        internal static string GetResourceId(InternalResource resource)
        {
            return (resource != null ? resource.Id : string.Empty);
        }

        internal static void TraceContext(MethodBase methodContext, IRoutingContext routeContext, IResource resource, RuntimeEnvironment environment, string message)
        {
            ChannelBase.TraceContext(methodContext, routeContext, resource, environment, null, null, message);
        }

        internal static void TraceContext(MethodBase methodContext, IRoutingContext routeContext, IResource resource, RuntimeEnvironment environment, IPEndPoint receiverEP, string receiverId, string message)
        {
            MessagingCoreInstrumentation.TraceIfEnabled(
                MessagingCoreInstrumentation.TryGetContext(methodContext, routeContext, resource, 
                environment, receiverEP, receiverId, message));
        }
        #endregion
    }

    /// <summary>
    /// Represents a generic base implementation of a typed channel.
    /// </summary>
    /// <typeparam name="T">The type of the messages that may be sent on this channel.</typeparam>
    public class ChannelBase<T> : 
        ChannelBase,
        IChannel<T>
    {
        /// <summary>
        /// Initializes a default instance of the ChannelBase(Of T) class with the specified resource.
        /// </summary>
        /// <param name="resource">The agent port to reference.</param>
        protected ChannelBase(InternalResource resource)
            : base(resource)
        {
        }

        /// <summary>
        /// Sends a message to the referenced resource.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public virtual void Send(T message)
        {
            IRoutingContext ctx = new RoutingContext<T, IChannel<T>>(message, this, false);
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), ctx, this, this.Environment, null, ChannelBase.GetResourceId(this.Resource), null);

            // Send
            this.Resource.Post(ctx);
        }

        /// <summary>
        /// Posts a routing context which is relayed to this channel's receiver.
        /// </summary>
        /// <param name="context">The routing context to post.</param>
        protected internal override void Post(IRoutingContext context)
        {
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), context, this, this.Environment, null, ChannelBase.GetResourceId(this.Resource), null);
            this.Resource.Post(context);
        }
    }
}
