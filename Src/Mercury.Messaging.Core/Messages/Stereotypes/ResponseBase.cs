using System;
using System.Collections.Generic;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// The base class for all response stereotype implementations.
    /// </summary>
    /// <typeparam name="TResponse">The type of message body for this response.</typeparam>
    public class ResponseBase<TResponse> : 
        MessageBase<TResponse>, 
        Response<TResponse>
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the ResponseBase(Of TResponse) class.
        /// </summary>
        public ResponseBase()
        {
        }

        /// <summary>
        /// Initializes a default instance of the ResponseBase(Of TResponse) class with the specified values.
        /// </summary>
        /// <param name="message">The message response.</param>
        /// <param name="requestId">The request ID.</param>
        public ResponseBase(TResponse message, string requestId = null)
            : base(message)
        {
            this.RequestId = requestId;
        }

        /// <summary>
        /// Initializes a default instance of the ResponseBase(Of TResponse) class with the specified values.
        /// </summary>
        /// <param name="message">The message response.</param>
        /// <param name="headers">A headers dictionary.</param>
        public ResponseBase(TResponse message, IDictionary<string, string> headers)
            : base(message, headers)
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        public string RequestId
        {
            get { return base.Headers[HeaderKey.RequestId]; }
            set { base.Headers[HeaderKey.RequestId] = value; }
        }

        #region Override
        /// <summary>
        /// Deserializes the response from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == HeaderKey.RequestId)
                    {
                        this.RequestId = reader.ReadString();
                        reader.Read();
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

            // Write base message values
            writer.WriteStartElement("_base");
            base.WriteXml(writer);
            writer.WriteEndElement();
        }
        #endregion
    }

    /// <summary>
    /// The base class for a response stereotype implementation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request triggering this response.</typeparam>
    /// <typeparam name="TResponse">The type of the message body of this response.</typeparam>
    public class ResponseBase<TRequest, TResponse> : 
        ResponseBase<TResponse>, 
        Response<TRequest, TResponse>
    {
        /// <summary>
        /// Initializes a default instance of the ResponseBase(Of TRequest, TResponse) class.
        /// </summary>
        public ResponseBase()
        {
        }

        /// <summary>
        /// Initializes a default instance of the ResponseBase(Of TRequest, TResponse) class with the specified values.
        /// </summary>
        /// <param name="request">The request to respond to.</param>
        /// <param name="message">The message response.</param>
        public ResponseBase(Request<TRequest> request, TResponse message)
            : base(message, (request != null) ? request.RequestId : null)
        {
            if (request != null)
                this.Request = request.Body;
        }

        /// <summary>
        /// Gets or sets the associated request.
        /// </summary>
        public TRequest Request { get; private set; }

        #region Override
        /// <summary>
        /// Deserializes the response from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == "Request")
                    {
                        string requestXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(requestXml))
                            this.Request = RuntimeSerializer.Deserialize<TRequest>(requestXml);
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
            if (this.Request != null)
            {
                writer.WriteStartElement("Request");
                RuntimeSerializer.Serialize(this.Request, writer);
                writer.WriteEndElement();
            }

            // Write base message values
            writer.WriteStartElement("_base");
            base.WriteXml(writer);
            writer.WriteEndElement();
        }
        #endregion
    }
}
