using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Behaviors.Channels
{
    /// <summary>
    /// Represents an untyped channel to a named resource.
    /// </summary>
    public interface INamedChannel : IUntypedChannel
    {
        /// <summary>
        /// Gets the address of the named resource.
        /// </summary>
        string Address { get; }
    }
}
