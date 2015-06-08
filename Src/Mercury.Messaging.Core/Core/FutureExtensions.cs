using System;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Core extensions to Mercury.Messaging.Core.Future object.
    /// </summary>
    public static class FutureExtensions
    {
        /// <summary>
        /// Creates a future by sending a message on a resource channel.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <typeparam name="TMessage">The type of the message to send.</typeparam>
        /// <param name="reference">The resource channel on which the message will be sent.</param>
        /// <param name="message">The message to be sent. This message 
        /// must be received as a Request(Of TMessage).</param>
        /// <returns>A future constructed from a message sent on a resource channel.</returns>
        public static Future<TResult> SendFuture<TResult, TMessage>(this LocalRefChannel reference, TMessage message)
        {
            Future<TResult> fut = new FutureChannel<TResult>(reference.Resource);
            fut.Send<TMessage>(message);
            return fut;
        }

        /// <summary>
        /// Creates and returns a future for a result of the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <param name="resource">The resource on which to get a future.</param>
        /// <returns>A future for a result of the specified type.</returns>
        public static Future<TResult> GetFuture<TResult>(this InternalResource resource)
        {
            return new FutureChannel<TResult>(resource);
        }

        /// <summary>
        /// Creates a future by sending a message on a resource channel.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <typeparam name="TMessage">The type of the message to send.</typeparam>
        /// <param name="reference">A reference to the resource to which the message will be sent.</param>
        /// <param name="message">The message to be sent. This message 
        /// must be received as a Request(Of TMessage).</param>
        /// <returns>A future constructed from a message sent on a resource channel.</returns>
        public static Future<TResult> SendFuture<TResult, TMessage>(this LocalRef reference, TMessage message)
        {
            LocalRefChannel refChannel = reference as LocalRefChannel;
            if (refChannel != null)
                return refChannel.SendFuture<TResult, TMessage>(message);
            return null;
        }
    }
}
