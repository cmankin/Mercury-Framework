using System;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// An untyped channel that references a local resource.
    /// </summary>
    public class LocalRefChannel : 
        ChannelBase,
        LocalRef
    {
        /// <summary>
        /// Initializes a default instance of the LocalRefChannel with the specified resource.
        /// </summary>
        /// <param name="resource">The local resource to reference.</param>
        public LocalRefChannel(InternalResource resource)
            : base(resource)
        {
            if (resource!=null)
                this._resId = resource.Id;
        }

        /// <summary>
        /// The ID of the referenced resource.
        /// </summary>
        [CLSCompliant(false)]
        protected string _resId;

        /// <summary>
        /// Gets the ID of the local resource to which messages on this channel are sent.
        /// </summary>
        public string ResId
        {
            get { return this._resId; }
        }
    }
}
