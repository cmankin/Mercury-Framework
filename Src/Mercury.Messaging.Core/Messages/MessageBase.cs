using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Mercury.Messaging.Core;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a base implementation of a generic IMessage.
    /// </summary>
    /// <typeparam name="T">The type of the message body.</typeparam>
    public class MessageBase<T> : 
        IMessage<T>, 
        IMessageHeader,
        IXmlSerializable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the MessageBase(Of T) class.
        /// </summary>
        public MessageBase()
            : this(default(T))
        {
        }

        /// <summary>
        /// Initializes a default instance of the MessageBase(Of T) class with the specified message body.
        /// </summary>
        /// <param name="message">The message body.</param>
        public MessageBase(T message)
        {
            this.Body = message;
            this._headers = new DictionaryHeaders();

            this._headers[HeaderKey.BodyType] = typeof(T).FullName;
        }

        /// <summary>
        /// Initializes a default instance of the MessageBase(Of T) class with the specified message body and headers.
        /// </summary>
        /// <param name="message">The message body.</param>
        /// <param name="headers">A dictionary of headers information.</param>
        public MessageBase(T message, IDictionary<string, string> headers)
        {
            this.Body = message;
            this._headers = new DictionaryHeaders(headers);

            this._headers[HeaderKey.BodyType] = typeof(T).FullName;
        }

        #endregion

        #region IMessageHeader
        /// <summary>
        /// Gets or sets the sender address URI.
        /// </summary>
        public Uri SenderAddress
        {
            get { return this._headers.GetUri(HeaderKey.SenderAddress); }
            set { this._headers.SetUri(HeaderKey.SenderAddress, value); }
        }

        /// <summary>
        /// Gets or sets the destination address URI.
        /// </summary>
        public Uri DestinationAddress
        {
            get { return this._headers.GetUri(HeaderKey.DestinationAddress); }
            set { this._headers.SetUri(HeaderKey.DestinationAddress, value); }
        }

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        public string CorrelationId
        {
            get { return this._headers[HeaderKey.CorrelationId]; }
            set { this._headers[HeaderKey.CorrelationId] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier for this message.
        /// </summary>
        public string MessageId
        {
            get { return this._headers[HeaderKey.MessageId]; }
            set { this._headers[HeaderKey.MessageId] = value; }
        }

        /// <summary>
        /// Gets or sets the fault address URI.
        /// </summary>
        public Uri FaultAddress
        {
            get { return this._headers.GetUri(HeaderKey.FaultAddress); }
            set { this._headers.SetUri(HeaderKey.FaultAddress, value); }
        }

        private IHeaders _headers;

        /// <summary>
        /// Gets the message header dictionary.
        /// </summary>
        public IHeaders Headers
        {
            get { return this._headers; }
        }

        #endregion

        #region IMessage<T>
        /// <summary>
        /// Gets the message body.
        /// </summary>
        public T Body { get; private set; }
        #endregion

        #region IXmlSerializable
        /// <summary>
        /// Returns the XML schema.
        /// </summary>
        /// <returns>Default value is null.</returns>
        public virtual System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Deserializes the message from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == "Body")
                    {
                        string innerXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(innerXml))
                        {
                            T messageBody = RuntimeSerializer.Deserialize<T>(innerXml);
                            if (messageBody != null)
                                this.Body = messageBody;
                        }
                    }
                    if (reader.LocalName == HeaderKey.CorrelationId)
                    {
                        this.CorrelationId = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == HeaderKey.DestinationAddress)
                    {
                        string destination = reader.ReadString();
                        if (!string.IsNullOrEmpty(destination))
                            this.DestinationAddress = new Uri(destination);
                        reader.Read();
                    }
                    if (reader.LocalName == HeaderKey.FaultAddress)
                    {
                        string fault = reader.ReadString();
                        if (!string.IsNullOrEmpty(fault))
                            this.FaultAddress = new Uri(fault);
                        reader.Read();
                    }
                    if (reader.LocalName == HeaderKey.MessageId)
                    {
                        this.MessageId = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == HeaderKey.SenderAddress)
                    {
                        string sender = reader.ReadString();
                        if (!string.IsNullOrEmpty(sender))
                            this.SenderAddress = new Uri(sender);
                        reader.Read();
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Serializes the message to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            // Write
            if (this.Body != null && this.Body is IXmlSerializable)
            {
                writer.WriteStartElement("Body");
                RuntimeSerializer.Serialize(this.Body, writer);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrEmpty(this.CorrelationId))
                writer.WriteElementString(HeaderKey.CorrelationId, this.CorrelationId);

            if (this.DestinationAddress != null)
                writer.WriteElementString(HeaderKey.DestinationAddress, this.DestinationAddress.ToString());

            if (this.FaultAddress != null)
                writer.WriteElementString(HeaderKey.FaultAddress, this.FaultAddress.ToString());

            if (!string.IsNullOrEmpty(this.MessageId))
                writer.WriteElementString(HeaderKey.MessageId, this.MessageId);

            if (this.SenderAddress != null)
                writer.WriteElementString(HeaderKey.SenderAddress, this.SenderAddress.ToString());
        }
        #endregion
    }
}
