using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// A listener for the runtime environment.
    /// </summary>
    public class RuntimeListener :
        IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the RuntimeListener class with the specified runtime environment.
        /// </summary>
        /// <param name="environment">The runtime environment on which to listen.</param>
        public RuntimeListener(RuntimeEnvironment environment)
        {
            this.Environment = environment;
        }
        #endregion

        #region Start
        /// <summary>
        /// The environment on which to listen.
        /// </summary>
        public RuntimeEnvironment Environment;

        private TcpListener _listener;
        private bool _canListen = false;

        /// <summary>
        /// Gets a value indicating whether the listener can continue listening.
        /// </summary>
        public bool CanListen
        {
            get { return this._canListen; }
        }

        private bool _shutdown;

        /// <summary>
        /// Gets a value indicating whether the listener is in the process of shutting down.
        /// </summary>
        public bool Shutdown
        {
            get { return this._shutdown; }
        }

        /// <summary>
        /// Gets or sets the delegate to a method that can handle listener exceptions.
        /// </summary>
        public Action<Exception> ExceptionHandler {get; set;}

        private Exception _lastKnownException;

        /// <summary>
        /// Gets the last registered exception.
        /// </summary>
        public Exception LastKnownException
        {
            get { return this._lastKnownException; }
            protected set
            {
                this._lastKnownException = value;
                if (value != null && this.ExceptionHandler != null)
                    this.ExceptionHandler.Invoke(value);
            }
        }

        /// <summary>
        /// A thread signal.
        /// </summary>
        public ManualResetEvent Connected = new ManualResetEvent(false);

        /// <summary>
        /// Starts the listener and accepts connections.  This 
        /// method blocks until shutdown or critical error.
        /// </summary>
        /// <param name="localEP">The IP end point on which to listen.</param>
        /// <returns>An exception occurring on the listener.</returns>
        public Exception Start(IPEndPoint localEP)
        {
            try
            {
                try
                {
                    // Get listener
                    this._listener = new TcpListener(localEP);
                }
                catch (ObjectDisposedException ex)
                {
                    this.LastKnownException = ex;
                    return ex;
                }

                // Start listener...
                this._listener.Start();
                this._canListen = true;
                while (this.CanListen)
                {
                    // Set event to nonsignaled state.
                    Connected.Reset();

                    // Begin accepting connections
                    this._listener.BeginAcceptSocket(new AsyncCallback(HandleAcceptTcpSocket), this._listener);

                    // Wait for connection
                    Connected.WaitOne();
                }
            }
            catch (Exception ex)
            {
                this.LastKnownException = ex;
                return ex;
            }
            finally
            {
                this.StopListenerInternal();
            }

            return null;
        }

        /// <summary>
        /// Stops the listener.
        /// </summary>
        public void Stop()
        {
            this._canListen = false;
            if (this.Connected != null)
                this.Connected.Set();
            this.StopListenerInternal();
        }

        private void StopListenerInternal()
        {
            if (this._listener != null)
            {
                this._shutdown = true;
                this._listener.Stop();
                this._listener = null;
            }
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Accepts a socket connection asynchronously.
        /// </summary>
        /// <param name="ar">The IASyncResult containing information about the connected socket.</param>
        protected void HandleAcceptTcpSocket(IAsyncResult ar)
        {
            if (!this.Shutdown)
            {
                // Signal continue
                Connected.Set();

                // Get listener handling the request
                TcpListener listen = (TcpListener)ar.AsyncState;

                // Get client socket
                Socket handler = listen.EndAcceptSocket(ar);
                this.AssignTcpSocketHandler(handler);
            }
        }

        /// <summary>
        /// Process the network data contained in the specified packet state object.
        /// </summary>
        /// <param name="state">An object providing data state information.</param>
        /// <param name="bytesRead">The number of bytes read from the last receive operation.</param>
        /// <returns>True if the data was completely received and handled; otherwise, false.</returns>
        protected virtual bool ProcessNetworkData(PacketState state, int bytesRead)
        {
            // Get work socket
            Socket handler = state.WorkSocket;

            // Reset offset
            int offset = 0;

            // Add received bytes
            state.ReceivedBytes += bytesRead;

            // Denial-of-service check (max 40MB message size limit)
            if (state.ReceivedBytes > RuntimePacketProtocol.MaxPacketSize)
            {
                HandleMessageSizeOverflow(state);
                throw new Exception(string.Format("A message was received that exceeded the allowable message size limit of {0} bytes.", RuntimePacketProtocol.MaxPacketSize));
            }

            // Handle small notifications and headers...
            if (!state.ReceivedHeader)
            {
                // If fault packet...
                if (state.Buffer[0] == 0x0b)
                {
                    if (state.ReceivedBytes < 7)
                        return false;
                    ushort fault = BitConverter.ToUInt16(state.Buffer, 1);
                    int messageId = BitConverter.ToInt32(state.Buffer, 3);
                    RemotingManager.PostError(messageId, new MessageDeliveryException(fault));
                    return true;
                }
                else    // Try handle data packet...
                {
                    // Verify header exists
                    if (state.Buffer[0] != 0x00)
                    {
                        HandleInvalidPacket(state);
                        return true;
                    }
                    if (state.ReceivedBytes < 12)
                        return false;
                    // Set identifer and record size
                    state.MessageId = BitConverter.ToInt32(state.Buffer, 3);
                    state.RecordSize = BitConverter.ToInt32(state.Buffer, 8);
                    state.ReceivedHeader = true;
                    offset = 12;
                }
            }

            // Store
            if (state.ReceivedBytes != state.RecordSize + 13)
            {
                int recordLength = (bytesRead - (offset));
                state.Append(state.Buffer, offset, recordLength);
                // Continue receive
                return false;
            }
            else    // Packet received
            {
                // Check for valid message end
                if (state.Buffer[bytesRead - 1] != PacketState.END_RECORD)
                {
                    HandleInvalidEndPacket(state);
                    return true;
                }

                // Get final record
                int recordLength = (bytesRead - (offset + 2));
                state.Append(state.Buffer, offset, recordLength);
                string xmlContent = RuntimePacketProtocol.DefaultEncoding.GetString(state.Builder, 0, state.Builder.Length);  //packet.StringBuilder.ToString();

                // Deserialize
                SerialMessage message = null;
                try
                {
                    message = RuntimeSerializer.Deserialize<SerialMessage>(xmlContent);
                }
                catch
                {
                    HandleInvalidPacket(state);
                    return true;
                }

                // Process
                DefaultMessageProcessor(message, state);
                return true;
            }
        }

        /// <summary>
        /// Processes XML message content according to the default runtime settings.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="state">The packet state associated with this message.</param>
        protected void DefaultMessageProcessor(SerialMessage message, PacketState state)
        {
            if (message != null)
            {
                // Attempt to cache the socket...
                if (message.ReturnEndPoint != null)
                {
                    var info = new RemotingInfo(message.ReturnId, message.ReturnEndPoint, state.WorkSocket);
                    if (info.NodeId != null)
                        RemotingManager.Register(info);
                    else
                        RemotingManager.Cache(info);
                }

                // Construct response channel.
                IRequestHeader header = message.Instance as IRequestHeader;
                if (header != null)
                    header.ConstructResponseChannel(this.Environment);

                // Forward message contents...
                IUntypedChannel local = this.Environment.GetRef(message.Destination, message.IsSynchronous);
                if (local == null)
                    local = this.Environment.RoutingEngine.GetMulticastChannel(this.Environment.GetAgentRefsByType(message.DestinationType));
                if (local != null)
                    CoreMethodUtil.ExecuteNonReturnMethod(local.GetType(), local, "Send",
                        new Type[] { message.Type }, message.Message);

                // Return message for remote synchronous operation...
                if (message.IsSynchronous)
                {
                    RemoteRef remote = this.Environment.GetRef(message.ReturnId, message.ReturnEndPoint,
                        RuntimeEnvironment.DEFAULT_REMOTE_TIMEOUT);
                    if (remote != null)
                        remote.Send<OpContinue>(new OpContinue());
                }
            }
        }

        /// <summary>
        /// Creates and sends a fault on a packet that fails to contain the expected "end packet" record.
        /// </summary>
        /// <param name="state">The packet state information.</param>
        public static void HandleInvalidEndPacket(PacketState state)
        {
            try
            {
                // Fault
                byte[] fault = RuntimePacketProtocol.GetPacketFault((ushort)PacketFaultCode.UnexpectedEndOfMessage, state.MessageId);
                state.WorkSocket.Send(fault, 0, fault.Length, SocketFlags.None);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Creates and sends a fault on a packet that is formatted incorrectly.
        /// </summary>
        /// <param name="state">The packet state information.</param>
        public static void HandleInvalidPacket(PacketState state)
        {
            try
            {
                // Fault
                byte[] fault = RuntimePacketProtocol.GetPacketFault((ushort)PacketFaultCode.InvalidMessageFormat, state.MessageId);
                state.WorkSocket.Send(fault, 0, fault.Length, SocketFlags.None);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Creates and sends a fault on a packet that is larger than the supported size.
        /// </summary>
        /// <param name="state">The packet state information.</param>
        public static void HandleMessageSizeOverflow(PacketState state)
        {
            try
            {
                // Fault
                byte[] fault = RuntimePacketProtocol.GetPacketFault((ushort)PacketFaultCode.MessageSizeOverflow, state.MessageId);
                state.WorkSocket.Send(fault, 0, fault.Length, SocketFlags.None);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Creates and sends a fault on a packet.
        /// </summary>
        /// <param name="state">The packet state information.</param>
        public static void HandleProcessingError(PacketState state)
        {
            try
            {
                // Fault
                byte[] fault = RuntimePacketProtocol.GetPacketFault((ushort)PacketFaultCode.ProcessError, state.MessageId);
                state.WorkSocket.Send(fault, 0, fault.Length, SocketFlags.None);
            }
            catch
            {
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
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.Stop();
                    if (this.Connected != null)
                        this.Connected.Dispose();
                }
            }
            this.disposedValue = true;
        }

        #endregion

        internal void AssignTcpSocketHandler(Socket socket)
        {
            if (socket == null)
                return;
            var info = RuntimeListener.CacheSocket(socket);
            this.AssignReadLoop(info);
        }

        internal static RemotingInfo CacheSocket(Socket socket)
        {
            if (socket == null)
                return null;
            var ie = socket.RemoteEndPoint as IPEndPoint;
            if (ie == null)
                throw new ArgumentException("Cannot cache a non-network end point.");
            var info = RemotingManager.TryResolve(ie);
            if (info != null)
                info.Socket = socket;
            if (info == null)
            {
                info = new RemotingInfo("", ie, socket);
                RemotingManager.Cache(info);
            }
            return info;
        }

        private void AssignReadLoop(RemotingInfo info)
        {
            if (info == null || info.Socket == null)
                return;
            if (info.AssignReceive())
            {
                PacketState state = new PacketState(info.Socket);
                RuntimeListener._ReadFromState(state, this);
            }
        }

        private static void _ReadFromState(PacketState state, RuntimeListener listener)
        {
            state.WorkSocket.BeginReceive(state.Buffer, 0, PacketState.BUFFER_SIZE, SocketFlags.None,
                new AsyncCallback(listener._ReadLoop), state);
        }

        private void _ReadLoop(IAsyncResult ar)
        {
            PacketState state = (PacketState)ar.AsyncState;
            if (state == null)
                return;
            Socket handler = state.WorkSocket;

            try
            {
                // Read data from client socket.
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // If packet is fully received...
                    if (this.ProcessNetworkData(state, bytesRead))
                        RuntimeListener._ReadFromState(new PacketState(handler), this);
                    else    // continue to receive...
                        RuntimeListener._ReadFromState(state, this);
                }
                else    // Continue receive loop
                {
                    RuntimeListener._ReadFromState(new PacketState(handler), this);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // Log exception?
                    this.LastKnownException = ex;
                    // Handle error on packet
                    if (state != null && state.WorkSocket != null)
                        HandleProcessingError(state);
                    // Continue receive loop...
                    RuntimeListener._ReadFromState(new PacketState(handler), this);
                }
                catch
                {
                }
            }
        }

    }
}
