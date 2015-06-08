using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Globalization;
using Microsoft.Ccr.Core;
using Microsoft.Ccr.Core.Arbiters;
using Microsoft.Ccr.Adapters.IO;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Instrumentation;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a message queue for processing messages on an Agent.
    /// </summary>
    public class AgentPort : 
        InternalResource, 
        IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the AgentPort 
        /// class with the specified runtime environment.
        /// </summary>
        /// <param name="environment">The RuntimeEnvironment on which this AgentPort is managed.</param>
        public AgentPort(RuntimeEnvironment environment)
        {
            // Set environment
            this.SetEnvironmentIfNull(environment);
            this.AgentQueue = new Port<IRoutingContext>();
            this.SyncAgentQueue = new Port<IRoutingContext>();
            this.InterruptQueue = new Port<Interrupt>();

            // Initialize first agent queue receiver
            Arbiter.Activate(this.Environment.AgentTasks,
                Arbiter.Receive(false, this.AgentQueue, PostHandler));

            // Initialize receivers dictionary
            this._receivers = new Dictionary<Type, ReceiveHandler<object>>();

            // Initialize default receivers
            this.Receive<Fault>((fault) => 
                {
                    this.Shutdown(new Fault(null, fault, typeof(Fault)));
                });

            this.Receive<Stop>((msg) => 
                {
                    this.Shutdown(null);
                });
        }
        #endregion

        #region Agent Properties

        private Agent _agentHandle;

        /// <summary>
        /// Gets the associated agent.
        /// </summary>
        protected internal Agent AgentHandle 
        {
            get { return this._agentHandle; }
            set
            {
                if (this._agentHandle == null)
                    this._agentHandle = value;
            }
        }

        /// <summary>
        /// Gets the type of the agent assigned to this port.
        /// </summary>
        public Type AgentType
        {
            get { return this.AgentHandle.GetType(); }
        }

        private bool _isSynchronous = false;

        /// <summary>
        /// Gets or sets a value indicating whether this port processes messages synchronously.
        /// </summary>
        public bool IsSynchronous
        {
            get { return this._isSynchronous; }
            set { this._isSynchronous = value; }
        }

        #endregion

        #region Agent Queue

        /// <summary>
        /// The default agent queue.
        /// </summary>
        protected internal Port<IRoutingContext> AgentQueue { get; private set; }
        /// <summary>
        /// The synchronous processing agent queue.
        /// </summary>
        protected internal Port<IRoutingContext> SyncAgentQueue { get; private set; }
        /// <summary>
        /// The interruptible agent queue.
        /// </summary>
        protected internal Port<Interrupt> InterruptQueue { get; private set; }
 
        /// <summary>
        /// Updates the last access date and time.
        /// </summary>
        protected void UpdateAccessDateTime()
        {
            this.LastAccess = DateTime.UtcNow;
        }

        /// <summary>
        /// Posts the routing context to the internal port.
        /// </summary>
        /// <param name="context">The IRoutingContext to post.</param>
        protected override internal void Post(IRoutingContext context)
        {
            // Message access time
            UpdateAccessDateTime();

            // If interrupt...
            Interrupt interruptContext = context.Message as Interrupt;
            if (interruptContext != null)
            {
                this.InterruptQueue.Post(interruptContext);
                return;
            }

            // If synchronous port, post to synchronous queue.
            if (this.IsSynchronous || context.IsSynchronous)
            {
                this._synchronousExecute = true;
                using (var syncArbiter = new SynchronousArbiter(this.Environment.SyncAgentTasks))
                {
                    // Post to synchronous port
                    this.SyncAgentQueue.Post(context);
                    // Handle synchronous receive
                    bool flag = syncArbiter.Receive(this.SyncAgentQueue, PostHandler);
                    if (!flag)
                    {
                        // handle fault
                        this.Shutdown(new Fault(new ArgumentException("Synchronous receive failed!"), 
                            null, context.MessageType));
                    }
                }
            }
            else   // Post to asynchronous agent queue.
            {
                this.AgentQueue.Post(context);
            }
        }

        /// <summary>
        /// Indicates whether current execution is synchronous.
        /// </summary>
        private bool _synchronousExecute;

        /// <summary>
        /// Handles a routing context received from the internal port.
        /// </summary>
        protected virtual void PostHandler(IRoutingContext context)
        {
            try
            {
                // Trace
                Channels.ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), context, this, this.Environment, "Entering agent post handler method.");

                // Resolve to registered receivers
                if (this.Receivers.ContainsKey(context.MessageType))
                {
                    try
                    {

                        ReceiveHandler<object> rec = this.Receivers[context.MessageType];
                        if (rec != null)
                            rec.Invoke(context.Message);
                    }
                    catch (Exception ex)
                    {
                        Fault fault = context.Message as Fault;
                        if (fault != null)
                        {
                            this.Shutdown(new Fault(ex, fault, context.MessageType));
                        }
                        else
                        {
                            this.Shutdown(new Fault(ex, null, context.MessageType));
                        }
                    }

                    // Notify task completion => decrement task count?
                }

                // Register next receive
                if (!this.IsShuttingDown && !HandleInterrupt())
                {
                    if (!this._synchronousExecute)
                        Arbiter.Activate(this.Environment.AgentTasks,
                            Arbiter.Receive(false, this.AgentQueue, PostHandler));
                    this._synchronousExecute = false;
                }
            }
            catch (Exception ex)
            {
                if (context != null)
                    this.Shutdown(new Fault(ex, null, context.MessageType));
                else
                    this.Shutdown(new Fault(ex));
            }
            finally
            {
                // Trace
                Channels.ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), context, this, this.Environment, "Exiting agent post handler method.");
            }
        }

        private int _insideInterrupt = 0;

        /// <summary>
        /// Determines whether an interrupt item is waiting to be handled and attempts to handle it.
        /// </summary>
        /// <returns>True if an interrupt was waiting to be handled 
        /// and has been dispatched to handle; otherwise, false.</returns>
        protected virtual bool HandleInterrupt()
        {
            if (this.InterruptQueue.ItemCount > 0 && (Interlocked.CompareExchange(ref this._insideInterrupt, 1, 0) == 0))
            {
                Interrupt item = this.InterruptQueue.Test() as Interrupt;
                if (item != null)
                {
                    PostHandler(item.Context);
                    return true;
                }
            }
            Interlocked.Exchange(ref this._insideInterrupt, 0);
            return false;
        }

        #endregion

        #region Receivers

        /// <summary>
        /// Registers a receiver for a message of the specified type.  
        /// Registering the same receiver multiple times overwrites 
        /// the previous instance.
        /// </summary>
        /// <typeparam name="T">The type of the message to receive.</typeparam>
        /// <param name="handler">The handler delegate for the received message.</param>
        public void Receive<T>(ReceiveHandler<T> handler) where T : class
        {
            // Convert generic handler to persisted handler type
            ReceiveHandler<object> persistHandler = new ReceiveHandler<object>(obj => handler((T)obj));
            this.AddReceiver(typeof(T), persistHandler);
        }

        /// <summary>
        /// Registers a receiver with the specified derived type and generic handler.
        /// </summary>
        /// <typeparam name="T">The type handled by the receive handler.</typeparam>
        /// <param name="derivedType">The type to register.</param>
        /// <param name="handler">The receive handler delegate to associate.</param>
        public void AddReceiver<T>(Type derivedType, ReceiveHandler<T> handler) where T : class
        {
            // Verify that the type is a derived type.
            bool isDerived = typeof(T).IsDerivedFrom(derivedType);
            if (!isDerived)
                throw new ArgumentException("The type must derived from this method's generic type parameter.", "derivedType");

            // Convert generic handler to persisted handler type
            ReceiveHandler<object> persistHandler = new ReceiveHandler<object>(obj => handler((T)obj));
            this.AddReceiver(derivedType, persistHandler);
        }

        /// <summary>
        /// Registers a receiver type and handler.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="handler">The handler delegate to associate.</param>
        public void AddReceiver(Type type, ReceiveHandler<object> handler)
        {
            // Overwrite existing receiver for this type
            if (this.Receivers.ContainsKey(type))
                this.Receivers.Remove(type);

            this.Receivers.Add(type, handler);
        }

        private Dictionary<Type, ReceiveHandler<object>> _receivers;

        /// <summary>
        /// Gets a dictionary of registered receive handlers.
        /// </summary>
        protected Dictionary<Type, ReceiveHandler<object>> Receivers
        {
            get
            {
                return this._receivers;
            }
        }

        #endregion

        #region Shutdown

        /// <summary>
        /// Forces this port to stop processing messages and shutdown.
        /// </summary>
        /// <param name="fault">The Fault message to send to any linked agents. 
        /// If fault is null, an Exit message will be sent to all linked 
        /// agents; otherwise, a Fault message will be sent specifying the 
        /// error information.</param>
        public virtual void Shutdown(Fault fault)
        {
            try
            {
                // Trace
                Channels.ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, this, this.Environment, "Entering agent shutdown method.");

                if (!this.IsShuttingDown)
                {
                    if (fault != null)
                    {
                        fault.AgentType = this.AgentType;
                        fault.AgentId = this.Id;

                        // Trace fault
                        ContextInfo ctxInfo = MessagingCoreInstrumentation.TryGetContext(MethodBase.GetCurrentMethod(), null, this, this.Environment, null, null, null);
                        MessagingCoreInstrumentation.TraceError((int)MessagingCoreEventId.AgentError, fault, ctxInfo, "Fault initiating the shutdown sequence...");

                        // Register
                        this.Environment.RegisterFaultWithRuntime(fault);
                        // Send
                        SendToLinkedPorts<Fault>(fault);
                    }
                    else
                    {
                        // Send
                        SendToLinkedPorts<Exit>(new Exit(this.Id));
                    }

                    //_isShuttingDown = true;
                    base.Shutdown();
                    this.Environment.Kill(this.Id);
                }
            }
            finally
            {
                // Trace
                Channels.ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, this, this.Environment, "Exiting agent shutdown method.");
            }
        }

        #endregion

        #region Links

        private object _linkLock = new object();
        private Dictionary<string, AgentPort> _linkedPorts = new Dictionary<string, AgentPort>();
        internal IDictionary<string, AgentPort> LinkedPorts
        {
            get { return this._linkedPorts; }
        }

        internal void AddLink(AgentPort port)
        {
            if (port == null)
                return;

            if (!this.LinkedPorts.ContainsKey(port.Id))
            {
                lock (this._linkLock)
                {
                    if (!this.LinkedPorts.ContainsKey(port.Id))
                        this.LinkedPorts.Add(port.Id, port);
                }
            }
        }

        internal void RemoveLink(AgentPort port)
        {
            if (port == null)
                return;

            if (this.LinkedPorts.ContainsKey(port.Id))
            {
                lock (this._linkLock)
                {
                    if (this.LinkedPorts.ContainsKey(port.Id))
                        this.LinkedPorts.Remove(port.Id);
                }
            }
        }

        internal bool HasLink(AgentPort port)
        {
            return this.LinkedPorts.ContainsKey(port.Id);
        }

        /// <summary>
        /// Sends the specified message to all linked ports.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        protected void SendToLinkedPorts<T>(T message)
        {
            lock (this._linkLock)
            {
                foreach (string instance in this.LinkedPorts.Keys)
                {
                    this.Environment.RoutingEngine.Send<T>(instance, message);
                }
            }
        }

        internal string GetLinkedPortsAsString()
        {
            var culture = CultureInfo.InvariantCulture;
            StringBuilder builder = new StringBuilder();
            lock (this._linkLock)
            {
                IEnumerator<string> enumerator = this.LinkedPorts.Keys.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                    builder.Append(enumerator.Current);
                while (enumerator.MoveNext())
                {
                    builder.Append(string.Format(culture, ",{0}{1}", System.Environment.NewLine, enumerator.Current));
                }
            }
            return builder.ToString();
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
                    this.LinkedPorts.Clear();
                    this.AgentHandle = null;
                    this.AgentQueue.Clear();
                }
            }
            this.disposedValue = true;
        }

        #endregion
    }
}
