using System;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a future on the results of a given agent.
    /// </summary>
    public interface Future<TResult> : IUntypedChannel
    {
        /// <summary>
        /// Gets the result of the future.  If the result 
        /// has not yet been sent, this call will block 
        /// until the future is resolved or the default 
        /// timeout expires.
        /// </summary>
        TResult Get { get; }

        /// <summary>
        /// Blocks the current thread until the future is resolved or the timeout expires.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait, or -1 to wait indefinitely.</param>
        /// <returns>True if the future is resolved; otherwise, false.</returns>
        bool WaitUntilCompleted(int timeout);

        /// <summary>
        /// Blocks the current thread until the future is resolved or the timeout expires.
        /// </summary>
        /// <param name="timeout">A time span that represents the number of milliseconds to wait.  
        /// Use a time span representing -1 milliseconds to wait indefinitely.</param>
        /// <returns>True if the future is resolved; otherwise, false.</returns>
        bool WaitUntilCompleted(TimeSpan timeout);
    }
}
