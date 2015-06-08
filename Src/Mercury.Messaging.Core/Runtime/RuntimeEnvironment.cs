using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Instrumentation;
using Microsoft.Ccr.Core;

namespace Mercury.Messaging.Runtime
{
    /// <summary>
    /// Represents a runtime environment for managing a distinct pool of agents as addressable resources.
    /// </summary>
    public class RuntimeEnvironment
        : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the RuntimeEnvironment class with the specified name.
        /// </summary>
        /// <param name="name">The name of the runtime environment.</param>
        public RuntimeEnvironment(string name)
            : this(name, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuntimeEnvironment class with the specified name and number of threads per CPU.
        /// </summary>
        /// <param name="name">The name of the runtime environment.</param>
        /// <param name="threadsPerCpu">The number of threads to run on each CPU.</param>
        public RuntimeEnvironment(string name, int threadsPerCpu)
            : this(name, Processors * threadsPerCpu, Processors * threadsPerCpu)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuntimeEnvironment class with the specified name and thread counts.
        /// </summary>
        /// <param name="name">The name of the runtime environment.</param>
        /// <param name="agentThreadCount">The number of threads on which to run agents or 0 to use defaults.</param>
        /// <param name="timerThreadCount">The number of threads on which to run time scheduled tasks or 0 to use defaults.</param>
        public RuntimeEnvironment(string name, int agentThreadCount, int timerThreadCount)
        {
            this._runtimeAddress = RuntimeUri.Create(name);
            this._resources = new ResourcePool(this.RuntimeAddress.ToString());
            this._routingEngine = new RuntimeRoutingEngine(this);


            // Setup dispatchers
            this.dispatcher = new Dispatcher(agentThreadCount, "runtime-dispatcher");
            this.timerDispatcher = new Dispatcher(timerThreadCount, "runtime-timer-dispatcher");
            this.AgentTasks = new DispatcherQueue("agent-tasks", this.dispatcher);
            this.SyncAgentTasks = new DispatcherQueue("synchronous-tasks", this.dispatcher);
            this.TimerScheduledTasks = new DispatcherQueue("scheduled-tasks", this.timerDispatcher);
            this.Tasks = new DispatcherQueue();

            // Initialize instrumentation
            MessagingCoreInstrumentation.Initialize(name, this.dispatcher);
            ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, null, this, "Runtime environment constructed.  Ready for tasks...");
        }
        #endregion

        #region Resource Management

        private ResourcePool _resources;
        /// <summary>
        /// The internal resource pool.
        /// </summary>
        protected ResourcePool Resources
        {
            get { return this._resources; }
        }

        internal string AddResource(IResource resource)
        {
            if (string.IsNullOrEmpty(resource.Id) || (!this._resources.Contains(resource.Id)))
                return this.Resources.Add(resource);
            return null;
        }

        internal void AddResource(IResource resource, string resourceId)
        {
            this.Resources.Store(resource, resourceId);
        }

        internal static readonly TimeSpan DEFAULT_REMOTE_TIMEOUT = 30.Seconds();

        /// <summary>
        /// Gets the number of resources running in this environment.
        /// </summary>
        public int ResourceCount
        {
            get { return this.Resources.Count; }
        }

        /// <summary>
        /// Returns a reference to a resource with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the resource to reference.</param>
        /// <returns>A reference to a resource with the specified ID or null.</returns>
        public LocalRef GetRef(string id)
        {
            return GetRef(id, false);
        }

        /// <summary>
        /// Returns a reference to a resource with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the resource to reference.</param>
        /// <param name="isSynchronous">A value indicating whether messages on this resource 
        /// should be handled synchronously.  Only applies to an agent reference.</param>
        /// <returns>A reference to a resource with the specified ID or null.</returns>
        public LocalRef GetRef(string id, bool isSynchronous)
        {
            return this.RoutingEngine.GetUntypedChannel(id, isSynchronous) as LocalRef;
        }

        /// <summary>
        /// Returns a reference to the first agent resource with the specified type.
        /// </summary>
        /// <param name="type">The type of the resource to reference.</param>
        /// <returns>A reference to the first agent resource with the specified type or null.</returns>
        public LocalRef GetRef(Type type)
        {
            return GetRef(type, false);
        }

        /// <summary>
        /// Returns a reference to the first agent resource with the specified type.
        /// </summary>
        /// <param name="type">The type of the resource to reference.</param>
        /// <param name="isSynchronous">A value indicating whether messages on this resource 
        /// should be handled synchronously.  Only applies to an agent reference.</param>
        /// <returns>A reference to the first agent resource with the specified type or null.</returns>
        public LocalRef GetRef(Type type, bool isSynchronous)
        {
            IEnumerable<IResource> resources = this.GetAllInternalResources();
            if (resources != null)
            {
                foreach (IResource res in resources)
                {
                    AgentPort port = res as AgentPort;
                    if (port != null && port.AgentHandle.GetType() == type)
                        return GetRef(port.Id, isSynchronous);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a reference to a remote resource with the specified ID and end point.
        /// </summary>
        /// <param name="id">The ID of the resource to reference.</param>
        /// <param name="endPoint">The IP end point of the remote runtime.</param>
        /// <param name="timeout">The time to wait for an acknowledgement from the remote runtime 
        /// or a reply before the channel shuts down communication.</param>
        /// <returns>A reference to a remote resource with the specified ID and end point.</returns>
        public RemoteRef GetRef(string id, IPEndPoint endPoint, TimeSpan timeout)
        {
            return ((IRemoteRouting)this.RoutingEngine).GetRemoteChannel(id, endPoint, null, timeout) as RemoteRef;
        }

        /// <summary>
        /// Returns a reference to a remote resource with the specified ID and end point.
        /// </summary>
        /// <param name="agentType">The type of the agent to reference. This will send 
        /// a message to each agent with the specified type on the remote runtime.</param>
        /// <param name="endPoint">The IP end point of the remote runtime.</param>
        /// <param name="timeout">The time to wait for an acknowledgement from the remote runtime 
        /// or a reply before the channel shuts down communication.</param>
        /// <returns>A reference to a remote resource with the specified ID and end point.</returns>
        public RemoteRef GetRef(Type agentType, IPEndPoint endPoint, TimeSpan timeout)
        {
            return ((IRemoteRouting)this.RoutingEngine).GetRemoteChannel(string.Empty, endPoint, agentType, timeout) as RemoteRef;
        }

        /// <summary>
        /// Gets an enumerable collection of references to agents with the specified type.
        /// </summary>
        /// <param name="searchType">The type of agent to locate.</param>
        /// <returns>An enumerable collection of references to agents with the specified type.</returns>
        public IEnumerable<LocalRef> GetAgentRefsByType(Type searchType)
        {
            foreach (IResource res in this.Resources._resources.References)
            {
                AgentPort port = res as AgentPort;
                if (port != null && port.AgentHandle.GetType() == searchType)
                    yield return this.GetRef(port.Id);
            }
            yield break;
        }

        /// <summary>
        /// Gets a future for the agent at the specified ID returning the specified type result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result to return.</typeparam>
        /// <param name="id">The ID of the agent on which to obtain a future.</param>
        /// <returns>A future for the agent at the specified ID returning the specified type result.</returns>
        public Future<TResult> GetFuture<TResult>(string id)
        {
            AgentPort port = FindAgentPort(id);
            if (port != null)
                return new FutureChannel<TResult>(port);
            return null;
        }

        /// <summary>
        /// Gets a future channel that waits on all of the specified futures to complete.
        /// </summary>
        /// <typeparam name="TResult">The type of the result to return.</typeparam>
        /// <param name="timeout">A timeout value for each future to wait.</param>
        /// <param name="futures">An array of futures on which to wait.</param>
        /// <returns>A future channel that waits on all of the specified futures to complete.</returns>
        public Future<TResult> GetFuture<TResult>(TimeSpan timeout, params Future<TResult>[] futures)
        {
            return new FutureMulticastChannel<TResult>(this, timeout, futures);
        }

        /// <summary>
        /// Spawns an agent and returns an identifier for the newly spawned agent's port.
        /// </summary>
        /// <param name="type">The type of the agent to spawn.</param>
        /// <returns>An identifier for the newly spawned agent's port.</returns>
        public string Spawn(Type type)
        {
            return this.Spawn(type, null);
        }

        /// <summary>
        /// Spawns an agent and returns an identifier for the newly spawned agent's port.
        /// </summary>
        /// <param name="type">The type of the agent to spawn.</param>
        /// <param name="args">Additional arguments to pass to the agent's constructor.</param>
        /// <returns>An identifier for the newly spawned agent's port.</returns>
        public string Spawn(Type type, params object[] args)
        {
            AgentPort port = AgentFactory.Create(this, type, args);
            return port.Id;
        }

        /// <summary>
        /// Spawns an agent and links on the specified link ID, returning 
        /// an identifier for the newly spawned agent's port.
        /// </summary>
        /// <param name="type">The type of the agent to spawn.</param>
        /// <param name="linkId">The agent ID on which to link the newly spawned agent.</param>
        /// <param name="args">Additional arguments to pass to the agent's constructor.</param>
        /// <returns>An identifier for the newly spawned agent's port.</returns>
        public string SpawnLink(Type type, string linkId, params object[] args)
        {
            AgentPort port = AgentFactory.Create(this, type, args);
            this.Link(linkId, port.Id);
            return port.Id;
        }

        /// <summary>
        /// Terminates the agent at the specified ID.
        /// </summary>
        /// <param name="id">The identifier of the agent to terminate.</param>
        public void Kill(string id)
        {
            this.Resources.Delete(id);
        }

        /// <summary>
        /// Returns the resource with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the resource to locate.</param>
        /// <returns>The resource associated with the specified ID.</returns>
        public IResource FindResource(string id)
        {
            return this.Resources.Get(id);
        }

        /// <summary>
        /// Returns the AgentPort associated with the specified port ID.
        /// </summary>
        /// <param name="id">The instance identifier of the AgentPort to locate.</param>
        /// <returns>The AgentPort associated with the specified port ID.</returns>
        public AgentPort FindAgentPort(string id)
        {
            return this.Resources.Get(id) as AgentPort;
        }

        /// <summary>
        /// Returns the first AgentPort whose agent type matches the specified type.
        /// </summary>
        /// <param name="type">The type of the agent to locate.</param>
        /// <returns>The first AgentPort whose agent type matches the specified type or null.</returns>
        internal AgentPort FindAgentPort(Type type)
        {
            foreach (IResource res in this.Resources._resources.References)
            {
                AgentPort port = res as AgentPort;
                if (port != null && port.AgentType == type)
                    return port;
            }
            return null;
        }

        /// <summary>
        /// Returns the first AgentPort whose agent type matches the specified type.
        /// </summary>
        /// <param name="type">The type of the agent to locate.</param>
        /// <returns>The first AgentPort whose agent type matches the specified type or null.</returns>
        internal IEnumerable<AgentPort> FindAgentPorts(Type type)
        {
            foreach (IResource res in this.Resources._resources.References)
            {
                AgentPort port = res as AgentPort;
                if (port != null && port.AgentType == type)
                    yield return port;
            }
        }

        /// <summary>
        /// Gets an enumerable collection of all internal resources.
        /// </summary>
        /// <returns>An enumerable collection of all internal resources.</returns>
        internal IEnumerable<IResource> GetAllInternalResources()
        {
            return this._resources._resources.References as IEnumerable<IResource>;
        }

        /// <summary>
        /// Returns a new agent port for this runtime environment.
        /// </summary>
        /// <returns>A new agent port for this runtime environment.</returns>
        public AgentPort NewPort()
        {
            return new AgentPort(this);
        }

        #endregion

        #region Dispatcher Queues

        private Dispatcher dispatcher;
        private Dispatcher timerDispatcher;

        /// <summary>
        /// Gets a dispatcher queue for asynchronous agent tasks.
        /// </summary>
        internal DispatcherQueue AgentTasks { get; private set; }

        /// <summary>
        /// Gets a dispatcher queue for synchronous agent tasks.
        /// </summary>
        internal DispatcherQueue SyncAgentTasks { get; private set; }

        /// <summary>
        /// Gets a dispatcher queue for scheduled tasks.
        /// </summary>
        internal DispatcherQueue TimerScheduledTasks { get; private set; }

        /// <summary>
        /// Gets a dispatcher queue for general tasks that uses the CLR threadpool.
        /// </summary>
        internal DispatcherQueue Tasks { get; private set; }

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
                    this.Shutdown();
                }
            }
            this.disposedValue = true;
        }

        #endregion

        #region Routing

        private Uri _runtimeAddress;

        /// <summary>
        /// Gets the Uri address of this runtime environment.
        /// </summary>
        /// <returns>The Uri address of this runtime environment.</returns>
        public Uri RuntimeAddress
        {
            get { return this._runtimeAddress; }
        }

        /// <summary>
        /// Gets the IP end point for this runtime environment.
        /// </summary>
        public IPEndPoint EndPoint { get; internal set; }

        private IRoutingEngine _routingEngine;

        /// <summary>
        /// Gets the routing engine used by this runtime environment.
        /// </summary>
        public IRoutingEngine RoutingEngine
        {
            get { return this._routingEngine; }
        }

        /// <summary>
        /// Creates a link between the agents referenced with the specified agent IDs.
        /// </summary>
        /// <param name="pid1">The first agent ID to link.</param>
        /// <param name="pid2">The second agent ID to link.</param>
        public void Link(string pid1, string pid2)
        {
            Link(FindAgentPort(pid1), FindAgentPort(pid2));
        }

        /// <summary>
        /// Removes a link between the agents referenced with the specified agent IDs.
        /// </summary>
        /// <param name="pid1">The first agent ID to unlink.</param>
        /// <param name="pid2">The second agent ID to unlink.</param>
        public void Unlink(string pid1, string pid2)
        {
            Unlink(FindAgentPort(pid1), FindAgentPort(pid2));
        }

        /// <summary>
        /// Returns a value indicating whether a link exists between 
        /// the agents referenced with the specified agent IDs.
        /// </summary>
        /// <param name="pid1">The first agent ID to check.</param>
        /// <param name="pid2">The second agent ID to check.</param>
        /// <returns>True if a link exists; otherwise, false.</returns>
        public bool HasLink(string pid1, string pid2)
        {
            AgentPort port1 = FindAgentPort(pid1);
            AgentPort port2 = FindAgentPort(pid2);
            if (port1 != null && port2 != null)
                return HasLink(port1, port2);
            return false;
        }

        internal static void Link(AgentPort port1, AgentPort port2)
        {
            if (port1 != null && port2 != null)
            {
                port1.AddLink(port2);
                port2.AddLink(port1);
            }
        }

        internal static void Unlink(AgentPort port1, AgentPort port2)
        {
            if (port1 != null)
                port1.RemoveLink(port2);
            if (port2 != null)
                port2.RemoveLink(port1);
        }

        internal static bool HasLink(AgentPort port1, AgentPort port2)
        {
            return (port1.HasLink(port2) && port2.HasLink(port1));
        }

        /// <summary>
        /// Post the specified message to the specified port.
        /// </summary>
        /// <typeparam name="T">The type of the message to post.</typeparam>
        /// <param name="port">The agent port on which to post.</param>
        /// <param name="message">The message to post.</param>
        protected internal static void PostToPort<T>(AgentPort port, T message)
        {
            PostToPort<T>(port, message, false);
        }

        /// <summary>
        /// Post the specified message to the specified port.
        /// </summary>
        /// <typeparam name="T">The type of the message to post.</typeparam>
        /// <param name="port">The agent port on which to post.</param>
        /// <param name="message">The message to post.</param>
        /// <param name="isSynchronous">A value indicating whether the message should be processed synchronously.</param>
        protected internal static void PostToPort<T>(AgentPort port, T message, bool isSynchronous)
        {
            IRoutingContext context = new RoutingContext<T, IChannel>(message, null, isSynchronous);
            PostToPort(port, context);
        }

        /// <summary>
        /// Posts the specified routing context to the specified agent port.
        /// </summary>
        /// <param name="port">The agent port on which to post.</param>
        /// <param name="context">The routing context to post.</param>
        protected internal static void PostToPort(AgentPort port, IRoutingContext context)
        {
            port.Post(context);
        }

        /// <summary>
        /// Registers the specified remote node with this runtime environment.
        /// </summary>
        /// <param name="nodeId">The identifier of the remote node to register.</param>
        /// <param name="endPoint">The remote IP end point to register.</param>
        public void Register(string nodeId, IPEndPoint endPoint)
        {
            ((IRemoteRouting)this.RoutingEngine).RegisterNode(nodeId, endPoint);
        }

        /// <summary>
        /// Registers the specified remote node with this runtime environment.
        /// </summary>
        /// <param name="nodeId">The identifier of the remote node to register.</param>
        /// <param name="ip">The remote IP address string to register.</param>
        /// <param name="port">The port number to register.</param>
        public void Register(string nodeId, string ip, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.Register(nodeId, endPoint);
        }

        /// <summary>
        /// Unregisters the specified node on this runtime environment.
        /// </summary>
        /// <param name="nodeId">The identifier of the remote node to unregister.</param>
        public void Unregister(string nodeId)
        {
            ((IRemoteRouting)this.RoutingEngine).UnregisterNode(nodeId);
        }

        #endregion

        #region Static

        private static Encoding _encoding = System.Text.Encoding.Unicode;

        /// <summary>
        /// Gets the default encoding used by the runtime environment.
        /// </summary>
        public static Encoding Encoding
        {
            get { return _encoding; }
        }

        internal static int Processors
        {
            get { return Environment.ProcessorCount; }
        }
        #endregion

        #region Faults

        /// <summary>
        /// The last known (registered) fault on the environment.
        /// </summary>
        private Fault _lastKnownFault;

        /// <summary>
        /// Gets the last registered, known fault in this runtime.
        /// </summary>
        public Fault LastKnownFault
        {
            get { return this._lastKnownFault; }
        }

        /// <summary>
        /// Registers a fault with the runtime environment.  By default, 
        /// this will only appear as the last known fault on the runtime.
        /// </summary>
        /// <param name="fault">The fault to register.</param>
        public virtual void RegisterFaultWithRuntime(Fault fault)
        {
            this._lastKnownFault = fault;
        }

        #endregion

        /// <summary>
        /// Shuts down the runtime environment and disposes of all current resources.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                // Trace
                ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, null, this, "Beginning runtime environment shutdown sequence...");

                // Shutdown
                if (this.AgentTasks != null)
                    this.AgentTasks.Dispose();
                if (this.SyncAgentTasks != null)
                    this.SyncAgentTasks.Dispose();
                if (this.TimerScheduledTasks != null)
                    this.TimerScheduledTasks.Dispose();
                if (this.Tasks != null)
                    this.Tasks.Dispose();
                if (this._resources != null)
                    this._resources.Dispose();
            }
            catch (Exception ex)
            {
                // Trace error
                ContextInfo ctx = MessagingCoreInstrumentation.TryGetContext(MethodBase.GetCurrentMethod(), null, null, this, null, null, null);
                MessagingCoreInstrumentation.TraceError((int)MessagingCoreEventId.RuntimeError, new Fault(ex), ctx, "Error occurred during shutdown sequence.");
            }
            finally
            {
                // Trace
                ChannelBase.TraceContext(MethodBase.GetCurrentMethod(), null, null, this, "Runtime environment shutdown completed.");
                MessagingCoreInstrumentation.Flush(TimeSpan.FromMilliseconds(500.0));

                // Dispose of dispatcher & reset instrumentation
                if (this.dispatcher != null)
                    this.dispatcher.Dispose();
                if (this.timerDispatcher != null)
                    this.timerDispatcher.Dispose();
                MessagingCoreInstrumentation.Unset();
            }
        }
    }
}
