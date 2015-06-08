using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Behaviors;
using Mercury.Messaging.Behaviors.IPC2;
using Mercury.Messaging.Test.Behaviors.Mock;

namespace Mercury.Messaging.Test.Behaviors
{
    /// <summary>
    /// Summary description for IPC2_Protocols
    /// </summary>
    [TestClass]
    public class IPC2_Behaviors_specs
    {
        public IPC2_Behaviors_specs()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Create_generic_server_and_ping()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            RetrySpecification spec = new RetrySpecification(5, 2.Seconds());
            LocalRef genServer = env.Spawn<GenServ>(spec);
            
            // Attempt send
            Future<Response<ServerReply>> future = genServer.SendFuture<Response<ServerReply>, ServerPing>(new ServerPing());
            future.WaitUntilCompleted(spec.WaitInterval);
            var response = future.Get;
            
            // Assert 
            Assert.IsNotNull(response);
            Assert.IsInstanceOfType(response, typeof(Response<ServerReply>));
            env.Shutdown();
        }

        [TestMethod]
        public void Create_supervisor_with_static_children()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 10, 30.Seconds()),
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                    {
                        return re.SpawnLink(agentType, supId, args);
                    }, RestartMode.Permanent, 0, typeof(AgentSink)),
                new ChildSpecification("worker2", (re, supId, agentType, args) => 
                    {
                        return re.SpawnLink(agentType, supId, args);
                    }, RestartMode.Transient, 2000, typeof(AgentSink)),
                new ChildSpecification("worker3", (re, supId, agentType, args) =>
                    {
                        return re.SpawnLink(agentType, supId, args);
                    }, RestartMode.Transient, 2000, typeof(AgentSink)));

            // Assert
            AgentPort port = env.FindAgentPort(sup.ResId);
            Assert.IsNotNull(port);
            Assert.IsInstanceOfType(port.AgentHandle, typeof(Supervisor));

            Supervisor instance = port.AgentHandle as Supervisor;
            Assert.IsNotNull(instance);
            env.Shutdown();
        }

        [TestMethod]
        public void Create_supervisor_with_static_and_dynamic_children()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 10, 30.Seconds()),
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, 0, typeof(AgentSink)),
                new ChildSpecification("worker2", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Transient, 2000, typeof(AgentSink)),
                new ChildSpecification("worker3", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Transient, 2000, typeof(AgentSink)));


            // Create dynamic child supervisor
            string supName = "supervisor2";
            sup.StartChild(new ChildSpecification(supName,
                (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, -1, typeof(Supervisor), 
                new RestartStrategy(SupervisorRestartMode.OneForOne, 10, 60.Seconds()), null));

            // Assert
            AgentPort port = env.FindAgentPort(sup.ResId);
            Assert.IsNotNull(port);
            Assert.IsInstanceOfType(port.AgentHandle, typeof(Supervisor));

            // Check supervisor children
            Supervisor castSuper = port.AgentHandle as Supervisor;
            Assert.IsTrue(castSuper.RoutingTable.Hosts.Count == 4);
            env.Shutdown();
        }

        [TestMethod]
        public void Worker_restart_strategy_specs()
        {
            // Setup
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 30, 30.Seconds()),
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, 0, typeof(AgentSink)),
                new ChildSpecification("worker2", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Temporary, 2000, typeof(AgentSink)),
                new ChildSpecification("worker3", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Transient, 2000, typeof(AgentSink)));

            // Get worker IDs
            string worker1Id = sup.GetChildId("worker1");
            string worker2Id = sup.GetChildId("worker2");
            string worker3Id = sup.GetChildId("worker3");

            Assert.IsFalse(string.IsNullOrEmpty(worker1Id));
            Assert.IsFalse(string.IsNullOrEmpty(worker2Id));
            Assert.IsFalse(string.IsNullOrEmpty(worker3Id));

            // Test worker 1 restart - always restarts
            LocalRef worker1 = env.GetRef(worker1Id);
            worker1.Send<Fault>(new Fault(new ArgumentException()));

            Thread.Sleep(2000);

            worker1Id = sup.GetChildId("worker1");
            worker1 = env.GetRef(worker1Id);
            Assert.IsNotNull(worker1);

            worker1.Send<Stop>(new Stop());

            Thread.Sleep(2000);

            worker1Id = sup.GetChildId("worker1");
            worker1 = env.GetRef(worker1Id);
            Assert.IsNotNull(worker1);

            // Test worker 2 restart - never restarts
            LocalRef worker2 = env.GetRef(worker2Id);
            worker2.Send<Stop>(new Stop());

            Thread.Sleep(2000);

            worker2Id = sup.GetChildId("worker2");
            Assert.IsTrue(string.IsNullOrEmpty(worker2Id));

            // Test worker 3 restart - restarts on fault; otherwise, no restart
            LocalRef worker3 = env.GetRef(worker3Id);
            worker3.Send<Fault>(new Fault(new ArgumentException()));

            Thread.Sleep(2000);
            try
            {
                worker3Id = sup.GetChildId("worker3");
                worker3 = env.GetRef(worker3Id);
                Assert.IsNotNull(worker3);
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                }
            }
            worker3.Send<Stop>(new Stop());

            Thread.Sleep(2000);

            worker3Id = sup.GetChildId("worker3");
            worker3 = env.GetRef(worker3Id);
            Assert.IsNull(worker3);
            env.Shutdown();
        }

        [TestMethod]
        public void Dynamic_add_child_to_supervisor()
        {
            // Setup
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 30, 30.Seconds()));

            // Dynamic add
            sup.StartChild(
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, 0, typeof(AgentSink)));

            // Get id
            string id = sup.GetChildId("worker1");
            LocalRef child = env.GetRef(id);

            // Assert
            Assert.IsNotNull(child);
            env.Shutdown();
        }

        [TestMethod]
        public void Delete_child_on_supervisor()
        {
            // Setup
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 30, 30.Seconds()));

            // Dynamic add
            sup.StartChild(
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, 0, typeof(AgentSink)));

            // Get id
            string id = sup.GetChildId("worker1");
            LocalRef child = env.GetRef(id);

            // Assert child exists
            Assert.IsNotNull(child);

            // Stop and delete
            sup.StopChild("worker1");
            sup.DeleteChild("worker1");

            // Asssert child no longer exists
            string id2 = sup.GetChildId("worker1");
            LocalRef child2 = env.GetRef(id);
            Assert.IsTrue(string.IsNullOrEmpty(id2));
            Assert.IsNull(child2);
            env.Shutdown();
        }

        [TestMethod]
        public void Stop_and_restart_child_on_supervisor()
        {
            // Setup
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 30, 30.Seconds()));

            // Dynamic add
            sup.StartChild(
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, 0, typeof(AgentSink)));

            // Get id
            string id = sup.GetChildId("worker1");
            LocalRef child = env.GetRef(id);

            // Assert child exists
            Assert.IsNotNull(child);

            // Stop child
            sup.StopChild("worker1");

            // Asssert child agent is no longer running
            string id2 = sup.GetChildId("worker1");
            LocalRef child2 = env.GetRef(id);
            Assert.IsTrue(string.IsNullOrEmpty(id2));
            Assert.IsNull(child2);

            // Restart child
            sup.RestartChild("worker1");
            string id3 = sup.GetChildId("worker1");
            LocalRef child3 = env.GetRef(id3);
            Assert.IsFalse(string.IsNullOrEmpty(id3));
            Assert.IsNotNull(child3);
            env.Shutdown();
        }

        [TestMethod]
        public void Get_all_children_on_supervisor()
        {
            // Setup
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            SupervisorRef sup = env.SpawnSupervisor<Supervisor>(
                new RestartStrategy(SupervisorRestartMode.OneForOne, 30, 30.Seconds()),
                new ChildSpecification("worker1", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Permanent, 0, typeof(AgentSink)),
                new ChildSpecification("worker2", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Temporary, 2000, typeof(AgentSink)),
                new ChildSpecification("worker3", (re, supId, agentType, args) =>
                {
                    return re.SpawnLink(agentType, supId, args);
                }, RestartMode.Transient, 2000, typeof(AgentSink)));

            // Get all children
            ChildInfo[] children = sup.GetAllChildren();
            
            // Assert
            Assert.IsNotNull(children);
            Assert.IsTrue(children.Length == 3);
            env.Shutdown();
        }
    }
}
