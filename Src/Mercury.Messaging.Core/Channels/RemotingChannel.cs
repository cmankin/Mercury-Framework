using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using Mercury;
using Mercury.Net;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Messages;
using Mercury.Messaging.ServiceModel;
using Mercury.Messaging.Serialization;
using Mercury.Messaging.Instrumentation;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// A channel used to send a message across application boundaries.
    /// </summary>
    public class RemotingChannel : 
        LocalRefChannel,
        RemoteRef,
        IXmlSerializable, IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the RemotingChannel class.
        /// </summary>
        public RemotingChannel()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the RemotingChannel class with the specified values.
        /// </summary>
        /// <param name="endPoint">The IP end point of the destination environment.</param>
        /// <param name="forwardingId">The ID of the destination resource.</param>
        /// <param name="destinationType">The System.Type of the destination agent.</param>
        /// <param name="timeout">The time span to wait for a reply from the remote 
        /// environment before this channel throws a message delivery exception.</param>
        /// <param name="environment">The runtime environment on which this channel exists.</param>
        internal RemotingChannel(IPEndPoint endPoint, string forwardingId, Type destinationType, TimeSpan timeout, RuntimeEnvironment environment)
            : base(null)
        {
            this._endPoint = endPoint;
            this._resId = forwardingId;
            this.DestinationType = destinationType;
            this.Timeout = timeout;
            this.SetEnvironmentIfNull(environment);
            this._currentNode = environment != null ? environment.RuntimeAddress.ToString() : null;
        }
        #endregion

        #region Properties
        private string _currentNode;

        /// <summary>
        /// The System.Type of the destination agent.
        /// </summary>
        protected internal Type DestinationType;

        private IPEndPoint _endPoint;

        /// <summary>
        /// Gets the IP end point of the runtime environment on which the destination agent resides.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return this._endPoint; }
        }

        private TimeSpan _timeout = 15.Seconds();

        /// <summary>
        /// Gets or sets the timeout value for this channel.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return this._timeout; }
            set { this._timeout = value; }
        }

        /// <summary>
        /// Gets the identifier of the last send operation.
        /// </summary>
        public int LastOperationId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to post message delivery errors to this channel's error handler.
        /// </summary>
        public bool PostErrors { get; set; }

        /// <summary>
        /// Gets or sets a delegate to a method that can handle message delivery errors that occur on send operations.
        /// </summary>
        public Action<int, Exception> ErrorHandler { get; set; }
        #endregion

        #region Send

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public override void Send<T>(T message)
        {   
            // Get type
            Type type = typeof(T);

            // Create message
            SerialMessage serial = new SerialMessage(type, message, this.ResId, false, this.DestinationType, this._currentNode, this.Environment.EndPoint);

            // Send
            SendSerialMessage(serial, false);
        }

        /// <summary>
        /// Sends the message with the specified type.
        /// </summary>
        /// <param name="type">The type of the message to send.</param>
        /// <param name="message">The message to send.</param>
        public void Send(Type type, object message)
        {
            // Create message
            SerialMessage serial = new SerialMessage(type, message, this.ResId, false, this.DestinationType, this._currentNode, this.Environment.EndPoint);

            // Send
            SendSerialMessage(serial, false);
        }

        /// <summary>
        /// Sends a message synchronously to the receiver on this channel.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>An identifier for the send operation.</returns>
        public int SendSync<T>(T message)
        {
            // Send
            return SendSync(typeof(T), message);
        }

        /// <summary>
        /// Sends a message synchronously to the receiver on this channel.
        /// </summary>
        /// <param name="type">The type of the message to send.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>An identifier for the send operation.</returns>
        public int SendSync(Type type, object message)
        {
            try
            {
                // Persist
                ChannelFactory.Persist(this.Environment, this);

                // Create message
                SerialMessage serial = new SerialMessage(type, message, this.ResId, true,
                    this.DestinationType, this.Id, this.Environment.EndPoint);

                // Send
                return SendSerialMessage(serial, true);
            }
            finally
            {
                // expire
                ChannelFactory.Expire(this.Environment, this);
            }
        }

        /// <summary>
        /// A value indicating whether send access is locked by a synchronous operation.
        /// </summary>
        private int _locked = 0;

        /// <summary>
        /// Signals the end of a synchronous send operation.
        /// </summary>
        private ManualResetEvent _signal = new ManualResetEvent(false);

        /// <summary>
        /// Sends a serial message on the channel and returns an identifier for the send operation.
        /// </summary>
        /// <param name="message">The serial message to send.</param>
        /// <param name="waitSynchronous">A value indicating whether the send should wait for a synchronous return.</param>
        /// <returns>An identifier for the send operation.</returns>
        protected virtual int SendSerialMessage(SerialMessage message, bool waitSynchronous)
        {
            int operationId = RemotingManager.GetOperationId();
            if (this.PostErrors)
                RemotingManager.RegisterOperation(operationId, this, this.Timeout);

            // If locked, throw
            if (this._locked != 0)
                throw new RTConstraintException(Properties.Strings.RTConstraintException_Remote_Sync);

            // Get xml
            string xml = RuntimeSerializer.Serialize(message);
            // Get packet
            byte[] packet = RuntimePacketProtocol.GetPacket(xml, operationId);

            // Get cached end point
            var info = RemotingManager.TryResolve(this.EndPoint);
            if (info == null)
            {
                info = new RemotingInfo(this.ResId != null ? this.ResId : string.Empty, this.EndPoint);
                RemotingManager.Cache(info);
            }

            try
            {
                // Verify socket...
                RemotingInfo.TryEnsureSocket(info);

                // Send
                try
                {
                    // Trace
                    ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), message as IRoutingContext, this, this.Environment, this.EndPoint, this.ResId, "Sending message to remote resource...");
                    RemotingManager.Write(info.Socket, packet, operationId);
                }
                catch (SocketException ex)
                {
                    if (info.Socket != null && ex.SocketErrorCode != SocketError.Success)
                    {
                        info.Socket.Shutdown(SocketShutdown.Both);
                        info.Socket.Close();
                    }
                    info.Socket = TcpSockets.Connect(info.EndPoint);
                    RemotingManager.Write(info.Socket, packet, operationId);
                }
            }
            catch (Exception ex)
            {
                // Trace exception
                ContextInfo ci = MessagingCoreInstrumentation.TryGetContext(MethodBase.GetCurrentMethod(), null, this, this.Environment, this.EndPoint, this.ResId, "Error occurred while attempting to deliver a message to a remote environment");
                MessagingCoreInstrumentation.TraceError((int)MessagingCoreEventId.MessageDeliveryError, new Fault(ex), ci, "Error occurred while attempting to deliver a message to a remote environment.");
                throw;
            }

            // If synchronous wait
            if (waitSynchronous)
            {
                Interlocked.Exchange(ref this._locked, 1);
                this._signal.Reset();
                this._signal.WaitOne(this.Timeout);
                Interlocked.Exchange(ref this._locked, 0);
            }
            this.LastOperationId = operationId;
            return operationId;
        }

        /// <summary>
        /// Overrides the default post behavior.
        /// </summary>
        /// <param name="context">The routing context posted.</param>
        protected internal override void Post(IRoutingContext context)
        {
            // If continue message received, stop waiting.
            if (context.MessageType == typeof(OpContinue) && !this._signal.SafeWaitHandle.IsClosed)
                this._signal.Set();
            else    // Send 
                this.Send(context.MessageType, context.Message);
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
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var culture = CultureInfo.InvariantCulture;
            if (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == "EndPoint")
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
                                this._endPoint = new IPEndPoint(ip, Convert.ToInt32(port, culture));
                            }
                        }
                        reader.Read();
                    }
                    if (reader.LocalName == "ResourceId")
                    {
                        this._resId = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "DestinationType")
                    {
                        string typeXml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(typeXml))
                            this.DestinationType = RuntimeSerializer.DeserializeType(typeXml);
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Serializes the response to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Write
            writer.WriteStartElement("EndPoint");
            writer.WriteAttributeString("Address", this.EndPoint.Address.ToString());
            writer.WriteAttributeString("Port", this.EndPoint.Port.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(this.ResId))
                writer.WriteElementString("ResourceId", this.ResId);

            if (this.DestinationType != null)
            {
                writer.WriteStartElement("DestinationType");
                RuntimeSerializer.SerializeType(this.DestinationType);
                writer.WriteEndElement();
            }
        }
        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposedValue;

        /// <summary>
        /// Performs the actual dispose.
        /// </summary>
        /// <param name="disposing">A value indicating whether the object is currently being disposed.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    OnManagedDispose();
                }

                if (this._signal != null)
                    this._signal.Close();
                OnUnmanagedDispose();
            }
            this.disposedValue = true;
        }

        /// <summary>
        /// Disposes managed resources.
        /// </summary>
        protected virtual void OnManagedDispose()
        {
        }

        /// <summary>
        /// Disposes unmanaged resources.
        /// </summary>
        protected virtual void OnUnmanagedDispose()
        {
        }
        #endregion
    }
}