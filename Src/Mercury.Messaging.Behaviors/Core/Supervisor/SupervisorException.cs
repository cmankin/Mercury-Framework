using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Thrown when an operation on a supervisor fails.
    /// </summary>
    [Serializable]
    class SupervisorException : Exception
    {
        /// <summary>
        /// Initializes a default instance of the SupervisorException class with the specified values.
        /// </summary>
        public SupervisorException()
            : this("The supervisor operation resulted in an exception.")
        {
        }

        /// <summary>
        /// Initializes a default instance of the SupervisorException class with the specified values.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SupervisorException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SupervisorException class with the specified values.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current 
        /// exception or a null reference if no exception is specified.</param>
        public SupervisorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

    }
}
