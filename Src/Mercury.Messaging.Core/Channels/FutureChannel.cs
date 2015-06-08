using System;
using System.Reflection;
using Mercury.Messaging.Instrumentation;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Core;
using Microsoft.Ccr.Core;
using System.Threading;
using Mercury;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// A channel to an agent that can return a value asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    public class FutureChannel<TResult> : 
        LocalRefChannel,
        Future<TResult>, IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the FutureChannel class with the specified agent port.
        /// </summary>
        /// <param name="resource">The agent port to reference.</param>
        public FutureChannel(InternalResource resource)
            : base(resource)
        {
            // Set environment
            this.SetEnvironmentIfNull(resource.Environment);
            
            // Persist channel
            ChannelFactory.Persist(resource.Environment, this);

            //this._receiverPort = port;
            this.InternalPort = new Port<IRoutingContext>();
            this.WaitPort = new Port<IRoutingContext>();
            this.ResultPort = new Port<object>();
            
            // Set defaults
            this.Timeout = 60.Seconds();
        }
        #endregion

        #region Send

        /// <summary>
        /// Sends a message to the referenced agent port.  After the first message, subsequent message 
        /// delivery will block and messages will be accumulated while this channel waits for a return 
        /// value from the agent port.  Upon return, all accumulated messages will be released to the 
        /// agent port and send behavior will revert to that of a simple, asynchronous channel.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public override void Send<T>(T message)
        {
            if (this._locked == 0)
            {
                // Lock for future values
                Interlocked.Add(ref this._locked, 1);

                // Create routing context
                IUntypedChannel responseChannel = ChannelFactory.Create<LocalRefChannel>(this.Environment, this.Id);
                Request<T> request = new RequestBase<T>(responseChannel, message,
                    Guid.NewGuid().ToString(), new Uri(this.Id), this.Environment.EndPoint, null);
                IRoutingContext context = new RoutingContext<Request<T>, IChannel>(request, this, false);

                // If port does not exist or is in shutdown.
                if (this.Resource == null || this.Resource.IsShuttingDown)
                {
                    if (this._signal != null)
                    {
                        this._signal.Set();
                        this.Dispose();
                    }
                    return;
                }

                // Post
                this.WaitPort.Post(context);
                Arbiter.Activate(this.Resource.Environment.Tasks,
                    this.WaitPort.Join(this.ResultPort, (ctx, res) =>
                    {
                        if (Interlocked.CompareExchange<object>(ref this._result, res, null) == null)
                        {
                            // Trace
                            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), ctx, this, this.Environment, null, ChannelBase.GetResourceId(this.Resource), "Received reply message.");

                            // Release accumulated messages
                            InitializeAccumulatorMessagePost();
                            if (!this._signal.SafeWaitHandle.IsClosed)
                                this._signal.Set();
                        }
                    }));

                ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), context, this, this.Environment, null, ChannelBase.GetResourceId(this.Resource), null);
                this.Resource.Post(context);
            }
            else
            {
                this.InternalPort.Post(new RoutingContext<T, IChannel>(message, this, false));
            }
        }

        /// <summary>
        /// Posts the routing context to the internal result port.
        /// </summary>
        /// <param name="context">The routing context to post.</param>
        protected internal override void Post(IRoutingContext context)
        {
            // Remove persisted channel from resources
            ExpireChannelResource();

            if (context != null)
                this.ResultPort.Post(context.Message);
            else
                this.ResultPort.Post(null);
        }

        /// <summary>
        /// Expires this channel's resource allocation.
        /// </summary>
        private void ExpireChannelResource()
        {
            if (this.IsPersisted)
                ChannelFactory.Expire(this.Environment, this);
        }

        #endregion

        #region Data

        /// <summary>
        /// The internal timeout.
        /// </summary>
        protected TimeSpan Timeout { get; set; }

        /// <summary>
        /// The internal message accumulator port.
        /// </summary>
        protected Port<IRoutingContext> InternalPort { get; private set; }
        /// <summary>
        /// The internal message wait port.
        /// </summary>
        protected Port<IRoutingContext> WaitPort { get; private set; }
        /// <summary>
        /// The internal result port.
        /// </summary>
        protected Port<object> ResultPort { get; private set; }

        // Lock state
        private int _locked = 0;

        // Event signal
        private ManualResetEvent _signal = new ManualResetEvent(false);

        private object _result;
        #endregion

        #region Get
        /// <summary>
        /// Gets the result of the future.
        /// </summary>
        public TResult Get
        {
            get
            {
                if (this._result == null)
                    this.TryFirstResult();
                if (this._result == null)
                    return default(TResult);
                return (TResult)this._result;
            }
        }

        /// <summary>
        /// Attempts to get the first result message sent to this channel.  This method blocks 
        /// until a message is received on this channel or the default timeout elapses.
        /// </summary>
        /// <returns>The result value.</returns>
        protected object TryFirstResult()
        {
            // Wait for signal
            this.WaitUntilCompleted(this.Timeout);

            // Return result
            return this._result;
        }

        /// <summary>
        /// Blocks the current thread until this channel receives a result or the timeout elapses.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait.</param>
        /// <returns>True if the wait operation did not expire; otherwise, false.</returns>
        public bool WaitUntilCompleted(int timeout)
        {
            return WaitUntilCompleted(timeout.Milliseconds());
        }

        /// <summary>
        /// Blocks the current thread until this channel receives a result or the timeout elapses.
        /// </summary>
        /// <param name="timeout">The time span that represents the number of milliseconds to wait.</param>
        /// <returns>True if the wait operation did not expire; otherwise, false.</returns>
        public bool WaitUntilCompleted(TimeSpan timeout)
        {
            if (this._signal.SafeWaitHandle.IsClosed || !this.IsPersisted)
                return true;

            // Wait
            this._signal.Reset();
            bool flag = this._signal.WaitOne(timeout);
            this.Dispose();

            // Remove persisted channel from resources
            ExpireChannelResource();
            // Return wait values
            return flag;
        }

        private void InitializeAccumulatorMessagePost()
        {
            // Release all waiting messages
            if (this.Resource != null && !this.Resource.IsShuttingDown)
            {
                Arbiter.Activate(this.Resource.Environment.AgentTasks,
                    Arbiter.Receive(true, this.InternalPort, (ctx) =>
                    {
                        this.Resource.Post(ctx);
                    }));
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
