using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Test.Agents.Mock;

namespace Mercury.Messaging.Test.Routing
{
    /// <summary>
    /// Summary description for RoutingEngine_Specs
    /// </summary>
    [TestClass]
    public class RoutingEngine_Specs
    {
        public RoutingEngine_Specs()
        {
            this.environment = new RuntimeEnvironment("prometheus.org");
            this.agentMockId = this.environment.Spawn(typeof(AddAgentMock));

            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ip = null;
            foreach (IPAddress address in entry.AddressList)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip = address;
                    break;
                }
            }

            this.routingEndPoint = new IPEndPoint(ip, 11090);
        }

        private TestContext testContextInstance;
        private RuntimeEnvironment environment;
        private string agentMockId;
        private IPEndPoint routingEndPoint;

        protected IRemoteRouting RemoteRouter
        {
            get { return this.environment.RoutingEngine as IRemoteRouting; }
        }

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
        public void Get_untyped_agent_channel()
        {
            IUntypedChannel channel = this.environment.RoutingEngine.GetUntypedChannel(this.agentMockId, false);
            channel.Send<AddMessage>(new AddMessage(1.0));
        }

        //[TestMethod]
        //public void Get_typed_agent_channel()
        //{
        //    IChannel<AddMessage> channel = this.environment.RoutingEngine.GetChannel<AddMessage>(this.agentMockId);
        //    channel.Send(new AddMessage(1.0));
        //}

        [TestMethod]
        public void Send_message()
        {
            this.environment.RoutingEngine.Send<AddMessage>(this.agentMockId, new AddMessage(1.0));
        }

        [TestMethod]
        public void Register_unregister_remote_node()
        {
            if (this.RemoteRouter == null)
                Assert.Fail();
            
            string nodeId = "http://env.org/";

            // Register
            this.RemoteRouter.RegisterNode(nodeId, this.routingEndPoint);
            
            // Assert
            IPEndPoint endPoint = this.RemoteRouter.ResolveNode(nodeId);
            Assert.IsNotNull(endPoint);
            Assert.IsTrue(endPoint.ToString() == this.routingEndPoint.ToString());

            // Unregister
            this.RemoteRouter.UnregisterNode(nodeId);

            // Assert
            endPoint = null;
            endPoint = this.RemoteRouter.ResolveNode(nodeId);
            Assert.IsNull(endPoint);
        }

        [TestMethod]
        public void Resolve_remote_node()
        {
            if (this.RemoteRouter == null)
                Assert.Fail();

            // Register
            Uri nodeUri = RuntimeUri.Create("runtime");
            this.RemoteRouter.RegisterNode(nodeUri.ToString(), this.routingEndPoint);

            // Create worker id
            string workerId = string.Format("{0}/{1}", nodeUri.GetLeftPart(UriPartial.Authority), Guid.NewGuid());

            // Resolve
            IPEndPoint endPoint = this.RemoteRouter.ResolveNode(workerId);

            // Assert
            Assert.IsNotNull(endPoint);
            Assert.IsTrue(endPoint.ToString() == this.routingEndPoint.ToString());

            // Unregister
            this.RemoteRouter.UnregisterNode(nodeUri.ToString());
        }

        [TestMethod]
        public void Get_remote_channel()
        {
            if (this.RemoteRouter == null)
                Assert.Fail();

            // Create worker id
            string workerId = string.Format("http://runtime/{0}", Guid.NewGuid());

            LocalRef remoteChannel = this.RemoteRouter.GetRemoteChannel(workerId, this.routingEndPoint) as LocalRef;
            Assert.IsNotNull(remoteChannel);
            Assert.IsTrue(((RemotingChannel)remoteChannel).ResId == workerId);
            Assert.IsTrue(((RemotingChannel)remoteChannel).EndPoint.ToString() == this.routingEndPoint.ToString());
        }

        [TestMethod]
        public void Implicit_get_registered_node_remote_channel()
        {
            if (this.RemoteRouter == null)
                Assert.Fail();

            // Register
            Uri nodeUri = RuntimeUri.Create("runtime");
            this.RemoteRouter.RegisterNode(nodeUri.ToString(), this.routingEndPoint);

            // Create worker id
            string workerId = string.Format("{0}{1}", nodeUri.ToString(), Guid.NewGuid());

            // Routing engine
            IRoutingEngine router = this.RemoteRouter as IRoutingEngine;
            
            // Get channel
            IUntypedChannel channel = router.GetUntypedChannel(workerId, false);

            // Assert
            Assert.IsNotNull(channel);

            // Get agent ref
            LocalRef agent = channel as LocalRef;

            // Assert
            Assert.IsNotNull(agent);

            // Unregister
            this.RemoteRouter.UnregisterNode(nodeUri.ToString());
        }

        [TestMethod]
        public void Get_agent_by_type()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(SimpleAgent));

            // Create remote endpoints
            Uri pUri = new Uri(pid);
            string remotePid = string.Format("http://runtime/{0}", pUri.Segments[1]);

            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint remoteEP = null;
            foreach (IPAddress address in entry.AddressList)
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    remoteEP = new IPEndPoint(address, 11050);

            // Get by id
            LocalRef agent1 = env.GetRef(pid);
            Assert.IsNotNull(agent1);
            Assert.IsTrue(pid == agent1.ResId);
            Assert.IsFalse(agent1 is SynchronousChannel);

            // Get by type
            LocalRef agent2 = env.GetRef(typeof(SimpleAgent));
            Assert.IsNotNull(agent2);
            Assert.IsTrue(agent2.ResId == pid);
            Assert.IsFalse(agent2 is SynchronousChannel);

            // Get synchronous channel by id
            LocalRef agent3 = env.GetRef(pid, true);
            Assert.IsNotNull(agent3);
            Assert.IsTrue(agent3.ResId == pid);
            Assert.IsTrue(agent3 is SynchronousChannel);

            // Get remoting channel by id
            LocalRef agent4 = env.GetRef(remotePid, remoteEP, 30.Seconds());
            Assert.IsNotNull(agent4);
            Assert.IsTrue(agent4.ResId == remotePid);
            Assert.IsTrue(agent4 is RemotingChannel);

            // Get synchronous channel by type
            LocalRef agent5 = env.GetRef(typeof(SimpleAgent), true);
            Assert.IsNotNull(agent5);
            Assert.IsTrue(agent5.ResId == pid);
            Assert.IsTrue(agent5 is SynchronousChannel);

            // Get remoting channel by id, address, and port
            LocalRef agent6 = env.GetRef(remotePid, remoteEP.Address.ToString(), remoteEP.Port);
            Assert.IsNotNull(agent6);
            Assert.IsTrue(agent6.ResId == remotePid);
            Assert.IsTrue(agent6 is RemotingChannel);
            Assert.IsTrue(((RemotingChannel)agent6).EndPoint.ToString() == remoteEP.ToString());

            // Get remoting channel by type
            LocalRef agent7 = env.GetRef(typeof(SimpleAgent), remoteEP, 30.Seconds());
            Assert.IsNotNull(agent7);
            Assert.IsTrue(agent7 is RemotingChannel);
            Assert.IsTrue(((RemotingChannel)agent7).DestinationType == typeof(SimpleAgent));
        }
    }
}
