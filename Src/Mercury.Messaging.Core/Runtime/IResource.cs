using System;

namespace Mercury.Messaging.Runtime
{
    /// <summary>
    /// Represents a resource managed by a resource pool.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Gets the instance ID of this resource.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the date and time of the last message accessing this resource.
        /// </summary>
        DateTime LastAccess { get; }
    }
}
