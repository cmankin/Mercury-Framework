using System;
using System.Runtime.Serialization;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Thrown when a condition or constraint is violated on the runtime environment.
    /// </summary>
    [Serializable]
    public class RTConstraintException : Exception
    {
        /// <summary>
        /// Initializes a default instance of the RTConstraintException class.
        /// </summary>
        public RTConstraintException()
            : this(Properties.Strings.RTConstraintException_General)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RTConstraintException 
        /// class with the specified message.
        /// </summary>
        /// <param name="message">The string to display when the exception is thrown.</param>
        public RTConstraintException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RTConstraintException 
        /// class with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The string to display when the exception is thrown.</param>
        /// <param name="innerException">The exception instance that caused the current exception.</param>
        public RTConstraintException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RTConstraintException class with serialized data. 
        /// </summary>
        /// <param name="info">The Syste.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The Syste.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected RTConstraintException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
