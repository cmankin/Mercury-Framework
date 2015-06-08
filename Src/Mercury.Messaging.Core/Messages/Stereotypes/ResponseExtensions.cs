using System;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Core response extensions to the Mercury.Messaging.Messages.Request(Of T) interface.
    /// </summary>
    public static class ResponseExtensions
    {
        /// <summary>
        /// Sends a response to a request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to respond to.</param>
        /// <param name="response">The message response to the request, 
        /// delivered as a Response(Of TRequest,TResponse).</param>
        public static void Respond<TRequest, TResponse>(this Request<TRequest> request, TResponse response)
        {
            var msg = request.Create<TRequest, TResponse>(response);
            request.ResponseChannel.Send<Response<TRequest, TResponse>>(msg);
        }

        /// <summary>
        /// Sends a response to a request.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response to send.</typeparam>
        /// <param name="request">The request to respond to.</param>
        /// <param name="response">The message response to the request.</param>
        public static void Respond<TResponse>(this Request<TResponse> request, TResponse response)
        {
            var msg = request.Create<TResponse>(response);
            request.ResponseChannel.Send<Response<TResponse>>(msg);
        }

        /// <summary>
        /// Create a response message with the specified response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to respond to.</param>
        /// <param name="response">The response message.</param>
        /// <returns>A response message with the specified response.</returns>
        public static Response<TResponse> Create<TResponse>(this Request<TResponse> request, TResponse response)
        {
            return new ResponseBase<TResponse>(response, request.RequestId);
        }

        /// <summary>
        /// Create a response message with the specified response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request for which to respond.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to respond to.</param>
        /// <param name="response">The response message.</param>
        /// <returns>A response message with the specified response.</returns>
        public static Response<TRequest, TResponse> Create<TRequest, TResponse>(this Request<TRequest> request, TResponse response)
        {
            return new ResponseBase<TRequest, TResponse>(request, response);
        }
    }
}
