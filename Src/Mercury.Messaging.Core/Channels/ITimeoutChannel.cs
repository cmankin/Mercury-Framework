using System;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// A channel that can represent a timeout state.
    /// </summary>
    public interface ITimeoutChannel : LocalRef
    {
        /// <summary>
        /// Gets the time span to wait before a condition expires.
        /// </summary>
        TimeSpan Timeout { get; set; }
    }
}
