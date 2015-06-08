using System;
using System.Collections.Generic;
using System.Reflection;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// A channel that can send a message to multiple resources.
    /// </summary>
    public class MultiCastChannel : 
        ChannelBase,
        IUntypedChannel
    {
        /// <summary>
        /// Initializes a new instance of the MultiCastChannel class with the specified resources.
        /// </summary>
        /// <param name="resources">The resource channels to reference.</param>
        public MultiCastChannel(IEnumerable<LocalRef> resources)
            : base(null)
        {
            this._resources = resources;
        }

        private IEnumerable<LocalRef> _resources;

        /// <summary>
        /// Sends a message to the resources on this channel.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public override void Send<T>(T message)
        {
            foreach (LocalRef channel in this._resources)
                channel.Send<T>(message);
        }

        /// <summary>
        /// Posts a routing context which is relayed to the designated receivers.
        /// </summary>
        /// <param name="context">The routing context to post.</param>
        protected internal override void Post(Routing.IRoutingContext context)
        {
            // NEVER post to multicast channel
            MethodInfo method = typeof(MultiCastChannel).GetMethod("Send");
            if (method != null)
            {
                MethodInfo generic = method.MakeGenericMethod(context.MessageType);
                generic.Invoke(this, new object[] { context.Message });
            }
        }
    }
}
