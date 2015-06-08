using System;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a message stereotype designating a response to an accepted request.
    /// </summary>
    /// <typeparam name="T">The type of the message body.</typeparam>
    public interface Response<T> : 
        IMessage<T>,
        IResponseHeader
    {
    }
    
    /// <summary>
    /// Represents a message stereotype designating a response to an accepted request.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to which this response is given.</typeparam>
    /// <typeparam name="T">The type of the message body.</typeparam>
    public interface Response<TRequest, T> :
        Response<T>
    {
        /// <summary>
        /// The request to which this response is given.
        /// </summary>
        TRequest Request { get; }
    }
}
