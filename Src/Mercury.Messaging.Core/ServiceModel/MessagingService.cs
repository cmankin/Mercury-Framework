using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// A runtime messaging service.
    /// </summary>
    public class MessagingService : 
        RuntimeServiceBase
    {
        /// <summary>
        /// Initializes a default instance of the MessagingService class 
        /// with the specified IP end point and runtime environment.
        /// </summary>
        /// <param name="endPoint">The IP end point on which to listen.</param>
        /// <param name="environment">The runtime environment on which this service runs.</param>
        public MessagingService(IPEndPoint endPoint, RuntimeEnvironment environment)
            : base(endPoint)
        {
            this._environment = environment;
            this._environment.EndPoint = endPoint;
        }

        private RuntimeEnvironment _environment;

        /// <summary>
        /// Gets the runtime environment on which this service runs.
        /// </summary>
        public RuntimeEnvironment Environment
        {
            get { return this._environment; }
        }

        private string _server;

        /// <summary>
        /// Gets the server dedicated thread ID.
        /// </summary>
        public string Server
        {
            get { return this._server; }
        }

        private object _lockHandle = new object();
        internal static RuntimeListener _hostedListener;
        private RuntimeListener _listener;
        private Thread _serverThread;

        /// <summary>
        /// Gets the server thread.
        /// </summary>
        public Thread ServerThread
        {
            get
            {
                if (this._serverThread == null)
                {
                    lock (this._lockHandle)
                    {
                        if (this._serverThread == null)
                        {
                            this._serverThread = new Thread(new ThreadStart(this.StartInternal));
                            this._serverThread.IsBackground = true;
                            this._server = this._serverThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }
                return this._serverThread;
            }
            set { this._serverThread = value; }
        }

        /// <summary>
        /// Gets or sets the exception handler to use on the runtime listener.
        /// </summary>
        public Action<Exception> ListenerExceptionHandler { get; set; }

        /// <summary>
        /// Starts the service.
        /// </summary>
        public override void Start()
        {
            // Start thread
            if (!this.IsStarted && this.ServerThread != null)
            {
                // If server is not null && thread is stopped.
                if (this.ServerThread.ThreadState == ThreadState.Stopped)
                    throw new ObjectDisposedException("ServerThread");
                this.ServerThread.Start();
            }
        }

        /// <summary>
        /// Starts the internal server on the dedicated thread.
        /// </summary>
        protected void StartInternal()
        {
            // Stop listener
            this.StopListener();

            // Create listener and start.
            this._listener = GetListener(this.Environment);
            MessagingService._hostedListener = this._listener;
            this.IsStarted = true;
            Exception ex = this._listener.Start(this.EndPoint);

            // Log exception
        }

        /// <summary>
        /// Gets a new runtime listener.
        /// </summary>
        /// <param name="environment">The runtime environment on which to listen.</param>
        /// <returns>A runtime listener.</returns>
        public virtual RuntimeListener GetListener(RuntimeEnvironment environment)
        {
            return new RuntimeListener(environment) { ExceptionHandler = this.ListenerExceptionHandler };
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public override void Stop()
        {
            StopListener();
            this._serverThread.Join();
            base.Stop();
        }

        /// <summary>
        /// Stops the internal listener if one exists.
        /// </summary>
        protected void StopListener()
        {
            if (this._listener != null)
                this._listener.Stop();
        }
    }
}
