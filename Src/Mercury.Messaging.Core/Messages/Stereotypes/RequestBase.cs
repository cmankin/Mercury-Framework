using System;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents a base implementation for all requests.
    /// </summary>
    /// <typeparam name="T">The type of the message body contained in this request.</typeparam>
    public class RequestBase<T> : 
        MessageBase<T>, 
        Request<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the RequestBase(Of T) class.
        /// </summary>
        public RequestBase()
        {
        }

        /// <summary>
        /// Initializes a default instance of the RequestBase(Of T) class with the specified values.
        /// </summary>
        /// <param name="responseChannel">The channel on which to respond.</param>
        /// <param name="message">The message request to send.</param>
        public RequestBase(IUntypedChannel responseChannel, T message)
            : this(responseChannel, message, Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initializes a default instance of the RequestBase(Of T) class with the specified values.
        /// </summary>
        /// <param name="responseChannel">The channel on which to respond.</param>
        /// <param name="message">The message request to send.</param>
        /// <param name="requestId">The unique identifier of the request.</param>
        public RequestBase(IUntypedChannel responseChannel, T message, string requestId)
            : this(responseChannel, message, requestId, null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the RequestBase(Of T) class with the specified values.
        /// </summary>
        /// <param name="responseChannel">The channel on which to respond.</param>
        /// <param name="message">The message request to send.</param>
        /// <param name="headers">A dictionary of message headers.</param>
        public RequestBase(IUntypedChannel responseChannel, T message, IDictionary<string, string> headers)
            : this(responseChannel, message, Guid.NewGuid().ToString(), headers)
        {
        }

        /// <summary>
        /// Initializes a default instance of the RequestBase(Of T) class with the specified values.
        /// </summary>
        /// <param name="responseChannel">The channel on which to respond.</param>
        /// <param name="message">The message request to send.</param>
        /// <param name="requestId">The unique identifier of the request.</param>
        /// <param name="headers">A dictionary of message headers.</param>
        public RequestBase(IUntypedChannel responseChannel, T message, string requestId, IDictionary<string, string> headers)
            : this(responseChannel, message, requestId, null, null, headers)
        {
        }

        /// <summary>
        /// Initializes a default instance of the RequestBase(Of T) class with the specified values.
        /// </summary>
        /// <param name="responseChannel">The channel on which to respond.</param>
        /// <param name="message">The message request to send.</param>
        /// <param name="requestId">The unique identifier of the request.</param>
        /// <param name="responseAddress">The receive address for a response.</param>
        /// <param name="responseEndPoint">The IP end point for a remote response.</param>
        /// <param name="headers">A dictionary of message headers.</param>
        public RequestBase(IUntypedChannel responseChannel, T message, string requestId, Uri responseAddress,
            IPEndPoint responseEndPoint, IDictionary<string, string> headers)
            : base(message, headers)
        {
            this.ResponseChannel = responseChannel;
            this.RequestId = requestId;
            this.ResponseAddress = responseAddress;
            this.ResponseEndPoint = responseEndPoint;
        }

        #endregion

        #region Request

        /// <summary>
        /// Gets or sets the response channel.
        /// </summary>
        public Channels.IUntypedChannel ResponseChannel { get; private set; }

        /// <summary>
        /// Gets or sets the unique identifier for the request.
        /// </summary>
        public string RequestId
        {
            get { return base.Headers[HeaderKey.RequestId]; }
            set { base.Headers[HeaderKey.RequestId] = value; }
        }

        /// <summary>
        /// Gets or sets the address for a response.
        /// </summary>
        public Uri ResponseAddress
        {
            get { return base.Headers.GetUri(HeaderKey.ResponseAddress); }
            set { base.Headers.SetUri(HeaderKey.ResponseAddress, value); }
        }

        /// <summary>
        /// Gets or sets the IP end point for a response.
        /// </summary>
        public IPEndPoint ResponseEndPoint
        {
            get { return base.Headers.GetEndPoint(HeaderKey.ResponseEndPoint); }
            set { base.Headers.SetEndPoint(HeaderKey.ResponseEndPoint, value); }
        }

        /// <summary>
        /// Constructs a response channel on the specified runtime environment.
        /// </summary>
        /// <param name="environment">The runtime environment on which to constructo the channel.</param>
        public void ConstructResponseChannel(Runtime.RuntimeEnvironment environment)
        {
            if (this.ResponseEndPoint != null)
                this.ResponseChannel = environment.GetRef(this.ResponseAddress.ToString(), this.ResponseEndPoint, 
                    RuntimeEnvironment.DEFAULT_REMOTE_TIMEOUT);
        }

        #endregion

        #region Override
        /// <summary>
        /// Deserializes the response from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == HeaderKey.RequestId)
                    {
                        this.RequestId = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == HeaderKey.ResponseAddress)
                    {
                        string address = reader.ReadString();
                        if (!string.IsNullOrEmpty(address))
                            this.ResponseAddress = new Uri(address);
                        reader.Read();
                    }
                    if (reader.LocalName == HeaderKey.ResponseEndPoint)
                    {
                        string endPoint = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(endPoint))
                            this.ResponseEndPoint = RemotingUtil.ParseRemoteFormat(endPoint);
                    }
                    if (reader.LocalName == "_base")
                    {
                        base.ReadXml(reader);
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Serializes the response to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            // Write
            if (!string.IsNullOrEmpty(this.RequestId))
                writer.WriteElementString(HeaderKey.RequestId, this.RequestId);
            if (this.ResponseAddress != null)
                writer.WriteElementString(HeaderKey.ResponseAddress, this.ResponseAddress.ToString());

            if (this.ResponseEndPoint != null)
                writer.WriteElementString(HeaderKey.ResponseEndPoint, this.Headers[HeaderKey.ResponseEndPoint]);
                
            // Write base message values
            writer.WriteStartElement("_base");
            base.WriteXml(writer);
            writer.WriteEndElement();
        }
        #endregion
    }
}