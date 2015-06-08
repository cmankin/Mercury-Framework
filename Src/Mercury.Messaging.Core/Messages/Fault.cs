using System;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Xml.Serialization;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a fault which occurs during an agent task.
    /// </summary>
    public sealed class Fault
        : IXmlSerializable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the Fault class.
        /// </summary>
        public Fault()
        {
        }

        /// <summary>
        /// Initializes a default instance of the Fault class with the specified values.
        /// </summary>
        /// <param name="exception">The exception which caused this fault.</param>
        public Fault(Exception exception)
            : this(exception, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the Fault class with the specified values.
        /// </summary>
        /// <param name="exception">The exception which caused this fault.</param>
        /// <param name="innerFault">A prior fault which caused this fault.</param>
        public Fault(Exception exception, Fault innerFault)
            : this(exception, innerFault, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the Fault class with the specified values.
        /// </summary>
        /// <param name="exception">The exception which caused this fault.</param>
        /// <param name="innerFault">A prior fault which caused this fault.</param>
        /// <param name="receivedMessageType">The type of the last message received before the fault.</param>
        public Fault(Exception exception, Fault innerFault, Type receivedMessageType)
        {
            this.Exception = SerialException.Create(exception);
            this.InnerFault = innerFault;
            this.ReceivedMessageType = receivedMessageType;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the exception that generated the fault.
        /// </summary>
        public SerialException Exception { get; set; }

        /// <summary>
        /// Gets the type of the message received that caused the fault.
        /// </summary>
        public Type ReceivedMessageType { get; set; }

        /// <summary>
        /// Gets the type of the agent on which the fault occurred.
        /// </summary>
        public Type AgentType { get; set; }

        /// <summary>
        /// Gets the instance address of the agent on which the fault occurred.
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        /// Gets the fault that caused this fault.
        /// </summary>
        public Fault InnerFault { get; set; }

        /// <summary>
        /// Gets a trace of the calls through which this fault was 
        /// propagated along with any exception stack traces.
        /// </summary>
        public string CallTrace
        {
            get { return GetCallTrace(this); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a string representation of the current Fault.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetCallTrace(this);
        }

        internal static string GetCallTrace(Fault fault)
        {
            var culture = CultureInfo.InvariantCulture;
            string strFault = "=> Fault :";

            if (fault.AgentType != null)
                strFault = string.Format(culture, "{0} {1}", strFault, fault.AgentType);

            if (!string.IsNullOrEmpty(fault.AgentId))
                strFault = string.Format(culture, "{0} [{1}]", strFault, fault.AgentId);

            if (fault.ReceivedMessageType != null)
                strFault = string.Format(culture, "{0} {1}{2}", strFault, fault.ReceivedMessageType, '\n');

            if (fault.Exception != null)
            {
                strFault = string.Format(culture, "{0}=>Exception: {1}{2}{3}{4}", strFault,
                    fault.Exception.Message, '\n', fault.Exception.ToString(), '\n');
            }

            if (fault.InnerFault != null)
                strFault = string.Format(culture, "{0}{1}{2}", strFault, '\n', GetCallTrace(fault.InnerFault));

            return strFault;
        }
        #endregion

        #region IXmlSerializable
        /// <summary>
        /// Returns the XML schema.
        /// </summary>
        /// <returns>Default value is null.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Deserializes the response from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == "Exception")
                    {
                        string exceptionXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(exceptionXml))
                        {
                            SerialException exception = SerialException.Create(exceptionXml);
                            if (exception != null)
                                this.Exception = exception;
                        }
                    }

                    if (reader.LocalName == "ReceivedMessageType")
                    {
                        string typeXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(typeXml))
                            this.ReceivedMessageType = RuntimeSerializer.Deserialize<Type>(typeXml);
                    }

                    if (reader.LocalName == "AgentType")
                    {
                        string typeXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(typeXml))
                            this.AgentType = RuntimeSerializer.Deserialize<Type>(typeXml);
                    }

                    if (reader.LocalName == "AgentId")
                    {
                        this.AgentId = reader.ReadString();
                        reader.Read();
                    }

                    if (reader.LocalName == "InnerFault")
                    {
                        string innerXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(innerXml))
                            this.InnerFault = RuntimeSerializer.Deserialize<Fault>(innerXml);
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Serializes the response to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public void WriteXml(XmlWriter writer)
        {
            Encoding encoding = RuntimeSerializer.CurrentEncoding;

            // Write exception
            if (this.Exception != null)
            {;
                writer.WriteStartElement("Exception");
                writer.WriteStartElement(SerialException.SerialExceptionElement);
                this.Exception.WriteXml(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            // Write settings
            if (this.ReceivedMessageType != null)
                writer.WriteElementString("ReceivedMessageType",
                    RuntimeSerializer.SerializeType(this.ReceivedMessageType, encoding));
            if (this.AgentType != null)
                writer.WriteElementString("AgentType",
                    RuntimeSerializer.SerializeType(this.AgentType, encoding));
            if (!string.IsNullOrEmpty(this.AgentId))
                writer.WriteElementString("AgentId", this.AgentId);

            // Write inner fault
            if (this.InnerFault != null)
            {
                writer.WriteStartElement("InnerFault");
                writer.WriteStartElement("Fault");
                this.InnerFault.WriteXml(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        
        #endregion
    }
}
