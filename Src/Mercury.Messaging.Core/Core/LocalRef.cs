using System;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a reference to a resource.
    /// </summary>
    public interface LocalRef : IUntypedChannel
    {
        /// <summary>
        /// The ID of the referenced resource.
        /// </summary>
        string ResId { get; }
    }
}
