using System;
using System.Net;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Core request extensions to the Mercury.Messaging.Channels.IUntypedChannel object.
    /// </summary>
    public static class RequestExtensions
    {
        /// <summary>
        /// Sends a request with the specified response channel.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to send.</typeparam>
        /// <param name="channel">The channel over which the request will be sent.</param>
        /// <param name="req">The message request to send, delivered as a Request(Of TRequest) to the receiver.</param>
        /// <param name="responseChannel">The channel on which to respond to a request.</param>
        /// <returns>The request message sent.</returns>
        public static Request<TRequest> Request<TRequest>(this IUntypedChannel channel, TRequest req, IUntypedChannel responseChannel)
        {
            var msg = new RequestBase<TRequest>(responseChannel, req);
            channel.Send<Request<TRequest>>(msg);
            return msg;
        }

        /// <summary>
        /// Sends a request with the specified response channel.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to send.</typeparam>
        /// <param name="channel">The channel over which the request will be sent.</param>
        /// <param name="req">The message request to send, delivered as a Request(Of TRequest) to the receiver.</param>
        /// <param name="responseChannel">The channel on which to respond to a request.</param>
        /// <param name="requestId">The unique identifier for this message.</param>
        /// <returns>The request message sent.</returns>
        public static Request<TRequest> Request<TRequest>(this IUntypedChannel channel, TRequest req, 
            IUntypedChannel responseChannel, string requestId)
        {
            var msg = new RequestBase<TRequest>(responseChannel, req, requestId);
            channel.Send<Request<TRequest>>(msg);
            return msg;
        }

        /// <summary>
        /// Sends a request with the specified response channel.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to send.</typeparam>
        /// <param name="channel">The channel over which the request will be sent.</param>
        /// <param name="req">The message request to send, delivered as a Request(Of TRequest) to the receiver.</param>
        /// <param name="responseChannel">The channel on which to respond to a request.</param>
        /// <param name="requestId">The unique identifier for this message.</param>
        /// <param name="responseAddress">The address of the response.</param>
        /// <param name="responseEndPoint">The IP end point for the response.</param>
        /// <returns>The request message sent.</returns>
        public static Request<TRequest> Request<TRequest>(this IUntypedChannel channel, TRequest req,
            IUntypedChannel responseChannel, string requestId, Uri responseAddress, IPEndPoint responseEndPoint)
        {
            var msg = new RequestBase<TRequest>(responseChannel, req, requestId, responseAddress, responseEndPoint, null);
            channel.Send<Request<TRequest>>(msg);
            return msg;
        }
    }
}
