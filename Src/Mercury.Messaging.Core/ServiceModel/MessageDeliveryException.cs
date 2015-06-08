using System;
using System.Runtime.Serialization;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// Thrown when a message cannot be verified to have had a successful delivery.
    /// </summary>
    [Serializable]
    public class MessageDeliveryException :
        Exception
    {
        /// <summary>
        /// Initializes a default instance of the MessageDeliveryException class.
        /// </summary>
        public MessageDeliveryException()
            : this(Properties.Strings.Message_Delivery_Exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MessageDeliveryException class with the specified fault code.
        /// </summary>
        /// <param name="faultCode">The integer value of the packet fault code that occurred.</param>
        public MessageDeliveryException(int faultCode)
            : this(faultCode, MessageDeliveryException._GetMessageByCode(faultCode), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MessageDeliveryException 
        /// class with the specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public MessageDeliveryException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MessageDeliveryException 
        /// class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the 
        /// current exception, or null if no exception is specified.</param>
        public MessageDeliveryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MessageDeliveryException class with 
        /// the specified fault code, error message, and inner exception.
        /// </summary>
        /// <param name="faultCode">The integer value of the packet fault code that occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the 
        /// current exception, or null if no exception is specified.</param>
        public MessageDeliveryException(int faultCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.RawCode = faultCode;
        }

        /// <summary>
        /// Initializes a new instance of the MessageDeliveryException class with serialized data. 
        /// </summary>
        /// <param name="info">The Syste.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The Syste.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected MessageDeliveryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Gets the raw integer code for this fault.
        /// </summary>
        public int RawCode { get; protected set; }

        private PacketFaultCode _faultCode;

        /// <summary>
        /// Gets the fault code encountered on this exception.
        /// </summary>
        public PacketFaultCode FaultCode
        {
            get
            {
                if (Enum.IsDefined(typeof(PacketFaultCode), this.RawCode))
                    this._faultCode = (PacketFaultCode)this.RawCode;
                return this._faultCode;
            }
        }

        private static string _GetMessageByCode(int faultCode)
        {
            if (Enum.IsDefined(typeof(PacketFaultCode), faultCode))
            {
                PacketFaultCode code = (PacketFaultCode)faultCode;
                switch (code)
                {
                    case PacketFaultCode.InvalidMessageFormat:
                        return Properties.Strings.Invalid_Message_Exception;
                    case PacketFaultCode.UnexpectedEndOfMessage:
                        return Properties.Strings.Partial_Message_Exception;
                    case PacketFaultCode.ProcessError:
                        return Properties.Strings.Remote_Process_Failure;
                    case PacketFaultCode.MessageSizeOverflow:
                        return Properties.Strings.Message_Size_Overflow;
                }
            }
            return Properties.Strings.Message_Delivery_Exception;
        }
    }
}
