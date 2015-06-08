using System;
using System.Net;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Serialization
{
    /// <summary>
    /// A message that can be serialized and passed across application boundaries.
    /// </summary>
    public sealed class SerialMessage : 
        ITypeReceive,
        IRoutingContext,
        IXmlSerializable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the SerialMessage class.
        /// </summary>
        public SerialMessage()
            : this(null, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SerialMessage class with the specified values.
        /// </summary>
        /// <param name="message">The message to receive.</param>
        /// <param name="destination">The destination agent ID.</param>
        public SerialMessage(object message, string destination)
            : this(message, destination, false, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SerialMessage class with the specified values.
        /// </summary>
        /// <param name="message">The message to receive.</param>
        /// <param name="destination">The destination agent ID.</param>
        /// <param name="isSynchronous">A value indicating whether the received 
        /// message should be processed synchronously.</param>
        public SerialMessage(object message, string destination, bool isSynchronous)
            : this(message, destination, isSynchronous, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SerialMessage class with the specified values.
        /// </summary>
        /// <param name="message">The message to receive.</param>
        /// <param name="destination">The destination agent ID.</param>
        /// <param name="isSynchronous">A value indicating whether the received 
        /// message should be processed synchronously.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        public SerialMessage(object message, string destination, bool isSynchronous, Type destinationType)
            : this((message != null) ? message.GetType() : null, message, destination, isSynchronous, destinationType)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SerialMessage class with the specified values.
        /// </summary>
        /// <param name="type">The type of the message to receive.</param>
        /// <param name="message">The message to receive.</param>
        /// <param name="destination">The destination agent ID.</param>
        /// <param name="isSynchronous">A value indicating whether the received 
        /// message should be processed synchronously.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        public SerialMessage(Type type, object message, string destination, bool isSynchronous, Type destinationType)
            : this(type, message, destination, isSynchronous, destinationType, string.Empty, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SerialMessage class with the specified values.
        /// </summary>
        /// <param name="type">The type of the message to receive.</param>
        /// <param name="message">The message to receive.</param>
        /// <param name="destination">The destination agent ID.</param>
        /// <param name="isSynchronous">A value indicating whether the received 
        /// message should be processed synchronously.</param>
        /// <param name="destinationType">The type of the destination agent.</param>
        /// <param name="returnId">The ID of the resource on which a signal 
        /// ending a blocking request should be returned.</param>
        /// <param name="returnEndPoint">The IP end point at which a 
        /// signal ending a blocking request should be returned.</param>
        internal SerialMessage(Type type, object message, string destination, bool isSynchronous,
            Type destinationType, string returnId, IPEndPoint returnEndPoint)
        {
            this._type = type;
            this.Message = message;
            this._destination = destination;
            this._isSynchronous = isSynchronous;
            this._destinationType = destinationType;
            this._returnId = returnId;
            this._returnEndPoint = returnEndPoint;
            this._routeId = Guid.NewGuid().ToString();
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
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == "Type")
                    {
                        string typeXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(typeXml))
                            this._type = RuntimeSerializer.DeserializeType(typeXml);
                    }
                    if (reader.LocalName == "BaseType")
                    {
                        string typeXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(typeXml))
                        {
                            Type msgType = RuntimeSerializer.DeserializeType(typeXml);
                            if (reader.LocalName == "Message")
                            {
                                string messageXml = reader.ReadInnerXml();
                                this.Message = RuntimeSerializer.Deserialize(messageXml, msgType);
                            }
                        }
                    }
                    if (reader.LocalName == "Destination")
                    {
                        this._destination = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "DestinationType")
                    {
                        string typeXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(typeXml))
                            this._destinationType = RuntimeSerializer.DeserializeType(typeXml);
                    }
                    if (reader.LocalName == "IsSynchronous")
                    {
                        this._isSynchronous = bool.Parse(reader.ReadString());
                        reader.Read();
                    }
                    if (reader.LocalName == "ReturnId")
                    {
                        this._returnId = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "ReturnEndPoint")
                    {
                        if (reader.HasAttributes)
                        {
                            string address;
                            reader.MoveToAttribute("Address");
                            address = reader.Value;

                            string port;
                            reader.MoveToAttribute("Port");
                            port = reader.Value;

                            if (!string.IsNullOrEmpty(address) && (!string.IsNullOrEmpty(port)))
                            {
                                IPAddress ip = IPAddress.Parse(address);
                                this._returnEndPoint = new IPEndPoint(ip, Convert.ToInt32(port, CultureInfo.InvariantCulture));
                            }
                        }
                        reader.Read();
                    }
                    if (reader.LocalName == "RouteId")
                    {
                        this._routeId = reader.ReadString();
                        reader.Read();
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
            var culture = CultureInfo.InvariantCulture;
            // Write
            writer.WriteStartElement("Type");
            RuntimeSerializer.SerializeType(this.Type, writer);
            writer.WriteEndElement();

            writer.WriteStartElement("BaseType");
            RuntimeSerializer.SerializeType(this.Message.GetType(), writer);
            writer.WriteEndElement();

            writer.WriteStartElement("Message");
            RuntimeSerializer.Serialize(this.Message, writer);
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(this.Destination))
                writer.WriteElementString("Destination", this.Destination);

            if (this.DestinationType != null)
            {
                writer.WriteStartElement("DestinationType");
                RuntimeSerializer.SerializeType(this.DestinationType, writer);
                writer.WriteEndElement();
            }

            if (this.IsSynchronous)
                writer.WriteElementString("IsSynchronous", this.IsSynchronous.ToString(culture));

            if (!string.IsNullOrEmpty(this.ReturnId))
                writer.WriteElementString("ReturnId", this.ReturnId);

            if (this.ReturnEndPoint != null)
            {
                writer.WriteStartElement("ReturnEndPoint");
                writer.WriteAttributeString("Address", this.ReturnEndPoint.Address.ToString());
                writer.WriteAttributeString("Port", this.ReturnEndPoint.Port.ToString(culture));
                writer.WriteEndElement();
            }

            if (!string.IsNullOrEmpty(this.RouteId))
                writer.WriteElementString("RouteId", this.RouteId);
        }
        #endregion

        #region IRoutingContext
        /// <summary>
        /// Gets the channel on which this message will be sent.
        /// </summary>
        public IChannel Channel
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the expected message type.
        /// </summary>
        public Type MessageType
        {
            get { return this.Type; }
        }

        private string _routeId;

        /// <summary>
        /// Gets the route identifier.
        /// </summary>
        public string RouteId
        {
            get { return this._routeId; }
        }
        #endregion

        #region ITypeReceive

        private Type _type;

        /// <summary>
        /// Gets the type to receive.
        /// </summary>
        public Type Type
        {
            get { return this._type; }
        }

        /// <summary>
        /// Gets the base type to receive.  Must be identical to or implement the specified Type.
        /// </summary>
        public Type BaseType
        {
            get
            {
                if (this.Message != null)
                    return this.Message.GetType();
                return null;
            }
        }
        
        /// <summary>
        /// Gets the instance of the type to receive.
        /// </summary>
        public object Instance
        {
            get { return this.Message; }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the message.
        /// </summary>
        public object Message { get; private set; }

        private string _destination;

        /// <summary>
        /// Gets the destination URI.
        /// </summary>
        public string Destination
        {
            get { return this._destination; }
        }

        private Type _destinationType;

        /// <summary>
        /// Gets the type of the destination agent.
        /// </summary>
        public Type DestinationType
        {
            get { return this._destinationType; }
        }

        private bool _isSynchronous;

        /// <summary>
        /// Gets a value indicating whether this message should be processed synchronously.
        /// </summary>
        public bool IsSynchronous
        {
            get { return this._isSynchronous; }
        }

        private string _returnId;

        /// <summary>
        /// Gets the ID of the resource to which a signal 
        /// ending a blocking request should be returned.
        /// </summary>
        internal string ReturnId
        {
            get { return this._returnId; }
        }

        private IPEndPoint _returnEndPoint;

        /// <summary>
        /// Gets the IP end point at which a signal ending 
        /// a blocking request should be returned.
        /// </summary>
        internal IPEndPoint ReturnEndPoint
        {
            get { return this._returnEndPoint; }
        }
        #endregion 
    }
}
