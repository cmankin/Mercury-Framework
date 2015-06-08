using System;
using System.Runtime.Serialization;

namespace Mercury.Messaging.Runtime
{
    /// <summary>
    /// Thrown when a resource pool attempts to allocate a resource beyonds its limit.
    /// </summary>
    [Serializable]
    public class ResourceLimitException : Exception
    {
        /// <summary>
        /// Initializes a default instance of the ResourceLimitException class.
        /// </summary>
        public ResourceLimitException()
            : this("Resource limit reached.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceLimitException 
        /// class with the specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ResourceLimitException(string message)
            : this(message,null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceLimitException class 
        /// with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the 
        /// current exception, or null if no exception is specified.</param>
        public ResourceLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceLimitException class with serialized data. 
        /// </summary>
        /// <param name="info">The Syste.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The Syste.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected ResourceLimitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
