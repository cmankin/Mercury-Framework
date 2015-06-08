using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using System.Collections.Concurrent;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a supervisor on a collection of child agents.
    /// </summary>
    public class Supervisor : Agent
    {
        #region Constructors

        /// <summary>
        /// Initializes a default instance of the Supervisor class with the specified values.
        /// </summary>
        /// <param name="port">The agent port for this supervisor.</param>
        /// <param name="strategy">The restart strategy for this supervisor.</param>
        /// <param name="children">Initial child specifications for this supervisor.</param>
        public Supervisor(AgentPort port, RestartStrategy strategy, ChildSpecification[] children)
        {
            // Set initial values
            this.RoutingTable = new HostRoutingTable();
            this.Specifications = new ConcurrentDictionary<string, ChildSpecification>();
            this.Strategy = strategy;
            this.Environment = port.Environment;
            this.Port = port;

            // Start registered children
            if (children != null && children.Length > 0)
            {
                foreach (ChildSpecification cs in children)
                {
                    this.Specifications.TryAdd(cs.Name, cs);
                    StartupChildAgent(cs, cs.Name);
                }
            }

            // Handle add child
            port.Receive<StartChild>((msg) => 
                {
                    this.StartChildFromSpec(msg.Specification);
                });

            // Handle exit
            port.Receive<Exit>(HandleChildTerminationSignal);

            // Handle fault
            port.Receive<Fault>(HandleChildTerminationSignal);

            // Handle request child handle
            port.Receive<Request<SendChildId>>((msg) => 
                {
                    if (msg != null && msg.Body != null && !string.IsNullOrEmpty(msg.Body.Name))
                    {
                        LocalRef reference = this.RoutingTable.TryFind(msg.Body.Name);
                        if (reference != null)
                            msg.Respond<SendChildId>(new SendChildId(msg.Body.Name, reference.ResId));
                        else    // Send empty string 
                            msg.Respond<SendChildId>(new SendChildId(msg.Body.Name, string.Empty));
                        return;
                    }
                    // Fault
                    if (msg != null)
                        msg.ResponseChannel.Send<Fault>(
                            new Fault(new ArgumentException("Request body improperly formatted.  Cannot process child ID request."), 
                                null, typeof(Request<SendChildId>)));
                });

            // Handle stop child
            port.Receive<StopChild>((msg) => 
                {
                    if (msg != null)
                        TerminateChild(msg.ChildName);
                });

            // Handle delete child spec
            port.Receive<DeleteChild>((msg) => 
                {
                    if (msg != null)
                        DeleteChild(msg.ChildName);
                });

            // Handle restart child
            port.Receive<RestartChild>((msg) => 
                {
                    if (msg != null)
                    {
                        ChildSpecification spec;
                        this.Specifications.TryGetValue(msg.ChildName, out spec);
                        if (spec != null)
                            StartupChildAgent(spec, msg.ChildName);
                    }
                });

            // Handle request children
            port.Receive<Request<GetChildren>>((msg) => 
                {
                    if (msg != null)
                    {
                        // Add info to list
                        List<ChildInfo> infoList = new List<ChildInfo>();
                        foreach (ChildSpecification spec in this.Specifications.Values)
                        {
                            LocalRef agent = this.RoutingTable.TryFind(spec.Name);
                            infoList.Add(new ChildInfo(spec, agent.ResId));
                        }

                        // Return list
                        msg.ResponseChannel.Send(new SendChildren(infoList.ToArray()));
                    }
                });
        }
        #endregion

        #region Manage Children

        /// <summary>
        /// Adds and starts the child according to the provided child specification.
        /// </summary>
        /// <param name="spec">The child specification to start.</param>
        protected virtual void StartChildFromSpec(ChildSpecification spec)
        {
            try
            {
                if (this.Specifications.TryAdd(spec.Name, spec))
                    StartupChildAgent(spec, spec.Name);
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
        }

        /// <summary>
        /// Invokes the startup function on the specified child specification and adds/updates the routing table.
        /// </summary>
        /// <param name="spec">The child specification to startup.</param>
        /// <param name="internalName">The internal identifier for the specified child.</param>
        protected virtual void StartupChildAgent(ChildSpecification spec, string internalName)
        {
            try
            {
                // Construct child
                object[] args = spec.ConstructorArgs;
                string agentId = spec.Startup.Invoke(this.Environment, this.Port.Id, spec.Type, args);
                if (string.IsNullOrEmpty(agentId))
                    throw new ArgumentException("Startup function on a child agent must return a valid agent ID.");

                // Add routing address
                LocalRef agent = this.Environment.GetRef(agentId);
                this.RoutingTable.TryAddOrUpdate(internalName, agent);
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
        }

        /// <summary>
        /// Terminates the child with the specified name by sending a stop message 
        /// and resorting to an Environment.Kill() if the child fails to shutdown.
        /// </summary>
        /// <param name="internalName">The internal identifier of the child to terminate.</param>
        protected virtual void TerminateChild(string internalName)
        {
            try
            {
                // Get specifications
                ChildSpecification spec;
                this.Specifications.TryGetValue(internalName, out spec);
                if (spec != null)
                {
                    // Set to terminate instance
                    spec.Terminate = true;

                    // Automatically remove temporary agent specifications
                    if (spec.Restart == RestartMode.Temporary)
                        this.Specifications.TryRemove(internalName, out spec);
                }

                // Get agent instance
                LocalRef child = this.RoutingTable.TryFind(internalName);
                if (child == null)
                    throw new ArgumentException("Cannot terminate the specified agent.  Agent not found.", "internalId");

                // Send stop message to agent
                child.Send<Stop>(new Stop());
                this.Signal.WaitOne(spec.Shutdown);

                // If agent not stopped, kill and remove
                child = this.RoutingTable.TryFind(internalName);
                if (child != null)
                {
                    this.Environment.Kill(child.ResId);
                    this.RoutingTable.TryRemove(internalName);
                }
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
        }

        /// <summary>
        /// Deletes the specification of the child with the specified name.
        /// </summary>
        /// <param name="internalId">The internal identifier of the child specification to delete.</param>
        protected virtual void DeleteChild(string internalId)
        {
            try
            {
                LocalRef reference = this.RoutingTable.TryFind(internalId);
                if (reference != null)
                    throw new ArgumentException("The specified child agent has not been stopped.  Cannot delete specifications on active agents.");

                ChildSpecification spec;
                this.Specifications.TryRemove(internalId, out spec);
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
        }

        /// <summary>
        /// Handles the termination signal sent by a child on shutdown.
        /// </summary>
        /// <param name="message">The termination message sent by the child.</param>
        protected virtual void HandleChildTerminationSignal(object message)
        {
            try
            {
                // Set timestamp
                if (DateTime.Now.Subtract(this.TimeStamp) > this.Strategy.RestartInterval)
                {
                    this.NumRestarts = 0;
                    this.TimeStamp = DateTime.Now;
                }

                // Get agent id
                string agentId = string.Empty;

                // Attempt to get  ID from fault
                Fault fault = message as Fault;
                if (fault != null)
                {
                    agentId = fault.AgentId;
                    fault = new Fault(null, fault, typeof(Fault));
                }
                else    // Get ID exit signal
                {
                    Exit msg = message as Exit;
                    if (msg != null)
                        agentId = msg.InstanceId;
                }

                // If still alive, kill
                LocalRef reference = this.Environment.GetRef(agentId);
                if (reference != null)
                    this.Environment.Kill(reference.ResId);

                // Handle restarts
                string internalId;
                this.RoutingTable.TryFind(agentId, out internalId);
                if (!string.IsNullOrEmpty(internalId))
                    HandleRestartStrategy(internalId, fault);
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
        }

        #endregion

        #region Handle Restart

        private void HandleRestartStrategy(string internalId, Fault fault)
        {
            try
            {
                // Set normal exit flag
                bool normalExit = (fault == null);

                // Get specs
                ChildSpecification spec;
                this.Specifications.TryGetValue(internalId, out spec);
                if (spec != null)
                {
                    // If restarts exceeded
                    if (this.NumRestarts > this.Strategy.Restarts)
                    {
                        this.Shutdown(fault);
                    }
                    else    // Can restart
                    {
                        if (this.Strategy.RestartMode == SupervisorRestartMode.OneForAll)
                        {
                            ShutdownChildren();
                            RestartChildren(normalExit);
                        }
                        else if (this.Strategy.RestartMode == SupervisorRestartMode.OneForOne)
                        {
                            TryRestartChild(internalId, spec, normalExit);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
        }

        private bool TryRestartChild(string internalId, ChildSpecification spec, bool normalExit)
        {
            try
            {
                // Remove from routing table
                this.RoutingTable.TryRemove(internalId);

                // If terminated by supervisor
                if (spec.Terminate)
                    return false;

                if (spec.Restart == RestartMode.Permanent)
                {
                    StartupChildAgent(spec, internalId);
                    return true;
                }

                // If exited abnormally...
                if (!normalExit)
                {
                    if (spec.Restart == RestartMode.Transient)
                    {
                        StartupChildAgent(spec, internalId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowSupervisorFaultAndShutdown(new Fault(ex));
            }
            return false;
        }

        /// <summary>
        /// Attempts to restart all children from the child specification table.
        /// </summary>
        /// <param name="normalExit">A value indicating whether a clean exit triggered the restart.</param>
        protected void RestartChildren(bool normalExit)
        {
            foreach (KeyValuePair<string, ChildSpecification> kv in this.Specifications)
            {
                TryRestartChild(kv.Key, kv.Value, normalExit);
            }
        }

        #endregion

        #region Shutdown

        /// <summary>
        /// Shuts down the supervisor with the specified fault.
        /// </summary>
        /// <param name="fault">The fault which caused the shutdown or null if no fault is specified.</param>
        protected virtual void Shutdown(Fault fault)
        {
            if (this.Signal != null)
                this.Signal.Dispose();
            this.ShutdownChildren();
            this.Port.Shutdown(fault);
        }

        /// <summary>
        /// Performs an Environment.Kill() on all children and removes them from the routing table.
        /// </summary>
        protected void ShutdownChildren()
        {
            foreach (KeyValuePair<string, LocalRef> kv in this.RoutingTable.Hosts)
            {
                this.Environment.Kill(kv.Value.ResId);
                this.RoutingTable.TryRemove(kv.Key);
            }
        }

        /// <summary>
        /// Throws a fault on this supervisor and shuts down.
        /// </summary>
        /// <param name="fault">The fault which caused the shutdown or null if no fault is specified.</param>
        protected void ThrowSupervisorFaultAndShutdown(Fault fault)
        {
            this.Shutdown(fault);
        }

        #endregion

        #region Data
        /// <summary>
        /// The child specification table.
        /// </summary>
        internal protected readonly ConcurrentDictionary<string, ChildSpecification> Specifications;

        /// <summary>
        /// The runtime environment.
        /// </summary>
        internal protected readonly RuntimeEnvironment Environment;

        /// <summary>
        /// The internal child routing table.
        /// </summary>
        internal protected readonly HostRoutingTable RoutingTable;

        /// <summary>
        /// The child restart strategy.
        /// </summary>
        protected readonly RestartStrategy Strategy;

        /// <summary>
        /// The internal agent port.
        /// </summary>
        protected readonly AgentPort Port;

        private DateTime TimeStamp;
        private int NumRestarts;
        private AutoResetEvent Signal = new AutoResetEvent(false);
        #endregion
    }
}
