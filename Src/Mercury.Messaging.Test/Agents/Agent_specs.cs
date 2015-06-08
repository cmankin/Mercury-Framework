using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Ccr.Core;
using Microsoft.Ccr.Core.Arbiters;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Test.Agents.Mock;

namespace Mercury.Messaging.Test
{
    /// <summary>
    /// Summary description for Agent_specs
    /// </summary>
    [TestClass]
    public class Agent_specs
    {
        public Agent_specs()
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
        public void Startup_a_single_agent()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(AddAgentMock));
            LocalRef agent = env.GetRef(pid);

            // Send multiple messages
            agent.Send<AddMessage>(new AddMessage(1.0));
            agent.Send<AddMessage>(new AddMessage(1.0));
            agent.Send<AddMessage>(new AddMessage(1.0));
            agent.Send<AddMessage>(new AddMessage(1.0));

            // Sleep
            System.Threading.Thread.Sleep(5000);

            // Assert
            AgentPort port = env.FindAgentPort(pid);
            Assert.IsTrue(((AddAgentMock)port.AgentHandle).State == 4.0);
            // Verify receiver count
            VerifyReceiverTaskCount(agent as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Agent_ordered_message_processing()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");

            // Create anonymous 1
            List<int> agent1List = new List<int>();
            LocalRef agent1 = AnonymousAgent.New(env, (port) => 
                {
                    port.Receive<AddMessage>((msg) => 
                        {
                            agent1List.Add(Convert.ToInt32(msg.AddValue));
                        });  
                });

            // Create anonymous 2
            List<int> agent2List = new List<int>();
            LocalRef agent2 = AnonymousAgent.New(env, (port) => 
                {
                    port.Receive<AddMessage>((msg) => 
                        {
                            agent2List.Add(Convert.ToInt32(msg.AddValue));
                        });
                });

            // Send even and odd messages and process in order
            for (int i = 0; i < 100; i++)
            {
                if (i % 2 == 0)
                    agent1.Send<AddMessage>(new AddMessage(i));
                else
                    agent2.Send<AddMessage>(new AddMessage(i));
            }

            Thread.Sleep(2000);

            int lastNum = -1;
            foreach (int odd in agent1List)
            {
                if (odd > lastNum)
                    lastNum = odd;
                else
                    Assert.Fail("Agent 1 processed out of order.");
            }

            lastNum = -1;
            foreach (int even in agent2List)
            {
                if (even > lastNum)
                    lastNum = even;
                else
                    Assert.Fail("Agent 2 processed out of order.");
            }

            // Verify receiver count
            VerifyReceiverTaskCount(agent1 as LocalRefChannel);
            // Verify receiver count
            VerifyReceiverTaskCount(agent2 as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Agent_request_response()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(AddAgentMock));
            LocalRef agent = env.GetRef(pid);

            // Add to state
            agent.Send<AddMessage>(new AddMessage(1.0));
            agent.Send<AddMessage>(new AddMessage(3.0));

            // Send request and response
            string rid = env.Spawn(typeof(AddAgentMock));
            LocalRef responseAgent = env.GetRef(rid);
            agent.Request<AddMessage>(new AddMessage(1.0), responseAgent);

            // Sleep
            System.Threading.Thread.Sleep(10000);

            // Assert
            AgentPort port = env.FindAgentPort(rid);
            Assert.IsNotNull(port);
            Assert.IsNotNull(port.AgentHandle);
            Assert.IsInstanceOfType(port.AgentHandle, typeof(AddAgentMock));
            Assert.IsTrue(((AddAgentMock)port.AgentHandle).State == 5.0);
            // Verify receiver count
            VerifyReceiverTaskCount(agent as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Agent_ping_pong()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(Paddle));
            LocalRef paddle1 = env.GetRef(pid);

            // Assert that agent exists
            AgentPort testPort = env.FindAgentPort(pid);
            Assert.IsNotNull(testPort);

            // Get paddle 2
            string pid2 = env.Spawn(typeof(Paddle));
            LocalRef paddle2 = env.GetRef(pid2);

            paddle2.Send<Ping>(new Ping(pid));

            // Sleep
            System.Threading.Thread.Sleep(5000);

            // Assert
            AgentPort port = env.FindAgentPort(pid);
            Assert.IsNull(port);
            // Verify receiver count
            VerifyReceiverTaskCount(paddle1 as LocalRefChannel);
            // Verify receiver count
            VerifyReceiverTaskCount(paddle2 as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Agent_futures()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(AddAgentMock));
            LocalRef agent = env.GetRef(pid);

            // Get future value
            agent.Send<AddMessage>(new AddMessage(3.0));
            agent.Send<AddMessage>(new AddMessage(4.0));
            Future<double> fut = agent.SendFuture<double, GetValueMessage>(new GetValueMessage());

            // Get value
            double result = fut.Get;

            // Assert
            Assert.IsTrue(result == 7.0);
            // Verify receiver count
            VerifyReceiverTaskCount(agent as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Link_two_agents_and_fail_two_way()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            LocalRef agent1 = env.Spawn<AddAgentMock>();
            LocalRef agent2 = env.Spawn<AddAgentMock>();

            // Link
            env.Link(agent1.ResId, agent2.ResId);

            // Fail 1
            agent1.Send<AddMessage>(new AddMessage(-99999.99));

            // Sleep 2 seconds
            Thread.Sleep(2000);

            // Assert
            AgentPort p1 = env.FindAgentPort(agent1.ResId);
            AgentPort p2 = env.FindAgentPort(agent2.ResId);

            Assert.IsNull(p1);
            Assert.IsNull(p2);

            // Create and link
            agent1 = env.Spawn<AddAgentMock>();
            agent2 = env.Spawn<AddAgentMock>();
            env.Link(agent1.ResId, agent2.ResId);

            // Fail 2
            agent2.Send<AddMessage>(new AddMessage(-99999.99));

            // Sleep 2 seconds
            Thread.Sleep(2000);

            // Assert
            p1 = env.FindAgentPort(agent1.ResId);
            p2 = env.FindAgentPort(agent2.ResId);

            Assert.IsNull(p1);
            Assert.IsNull(p2);
            env.Shutdown();
        }

        [TestMethod]
        public void Custom_supervisor_agent_tests()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            LocalRef supervisor = env.Spawn<SupervisorAgent>();

            // Add three children w/ synchronous channel
            IChannel<AddChild> channel = ChannelFactory.Create<SynchronousChannel<AddChild>, AddChild>(env, supervisor.ResId);
            channel.Send(new AddChild(typeof(AddAgentMock), "001"));
            channel.Send(new AddChild(typeof(AddAgentMock), "002"));
            channel.Send(new AddChild(typeof(AddAgentMock), "003"));

            // Send message to each
            supervisor.Send<ForwardRequest<AddMessage>>(new ForwardRequestBase<AddMessage>("001", new AddMessage(7.0)));
            supervisor.Send<ForwardRequest<AddMessage>>(new ForwardRequestBase<AddMessage>("002", new AddMessage(8.0)));
            supervisor.Send<ForwardRequest<AddMessage>>(new ForwardRequestBase<AddMessage>("003", new AddMessage(9.0)));

            // Stop child 3
            supervisor.Send<ForwardRequest<Stop>>(new ForwardRequestBase<Stop>("003", new Stop()));

            // Sleep 5 seconds
            Thread.Sleep(2000);

            // Attempt to send request to restarted child
            supervisor.Send<ForwardRequest<AddMessage>>(new ForwardRequestBase<AddMessage>("003", new AddMessage(8.0)));

            // Cause error in child 2
            supervisor.Send<ForwardRequest<AddMessage>>(new ForwardRequestBase<AddMessage>("002", new AddMessage(-99999.99)));

            // Sleep 2 seconds
            Thread.Sleep(2000);

            // Assert 
            LocalRef refSup = env.GetRef(supervisor.ResId);
            Assert.IsNull(refSup);
            env.Shutdown();
        }

        [TestMethod]
        public void Synchronous_message_blocks_until_complete()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(InterruptAgent));
            LocalRef syncAgent = env.GetRef(pid, true);
            LocalRef agent = env.GetRef(pid);

            // Send synchronous and asynchronous messages
            syncAgent.Send<Ping>(new Ping(string.Empty));
            agent.Send<Pong>(new Pong(string.Empty));

            // Sleep
            Thread.Sleep(250.Milliseconds());

            // Assert
            LocalRef instance = env.GetRef(pid);
            Assert.IsNotNull(instance);
            Future<int> fut = instance.SendFuture<int, GetValueMessage>(new GetValueMessage());
            int value = fut.Get;
            Assert.IsTrue(value == 2);
            // Verify receiver count
            VerifyReceiverTaskCount(instance as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Interrupt_message_interleaves_current_messages()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(InterruptAgent));

            // Get agent
            LocalRef agent = env.GetRef(pid);
            agent.Send<PrepInterrupt>(new PrepInterrupt());

            // Sleep
            Thread.Sleep(2250.Milliseconds());

            // Assert
            Future<int> fut = agent.SendFuture<int, GetValueMessage>(new GetValueMessage());
            int value = fut.Get;
            Assert.IsTrue(value == 5);
            // Verify receiver count
            VerifyReceiverTaskCount(agent as LocalRefChannel);
            env.Shutdown();
        }

        [TestMethod]
        public void Future_can_be_called_twice_without_consequences()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(InterruptAgent));

            // Get agent
            LocalRef agent = env.GetRef(pid);
            agent.Send<PrepInterrupt>(new PrepInterrupt());

            // Sleep
            Thread.Sleep(250.Milliseconds());

            // Assert
            Future<int> fut = agent.SendFuture<int, GetValueMessage>(new GetValueMessage());
            int value = fut.Get;
            Assert.IsTrue(value == 5);
            
            // Second send
            fut.Send<GetValueMessage>(new GetValueMessage());

            // Sleep
            Thread.Sleep(250.Milliseconds());

            // Assert
            value = fut.Get;
            Assert.IsTrue(value == 5);
            env.Shutdown();
        }

        [TestMethod]
        public void Future_accumulates_messages_while_waiting_for_result()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string pid = env.Spawn(typeof(InterruptAgent));

            // Get agent
            LocalRef agent = env.GetRef(pid);
            agent.Send<PrepInterrupt>(new PrepInterrupt());

            // Sleep
            Thread.Sleep(250.Milliseconds());

            // Get future
            Future<int> fut = agent.SendFuture<int, GetValueMessage>(new GetValueMessage(2.Seconds()));
            // Send updates
            fut.Send(new AddMessage(1.0));
            fut.Send(new AddMessage(1.0));
            fut.Send(new AddMessage(1.0));

            int value = fut.Get;
            // Assert
            Assert.IsTrue(value == 5);
            Thread.Sleep(500);
            Thread.MemoryBarrier();
            VerifyInterruptAgentValue(agent as LocalRefChannel, 8);
            env.Shutdown();
        }

        [TestMethod]
        public void Can_wait_on_multicast_future()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            LocalRef aR = env.Spawn<AddAgentMock>();
            LocalRef bR = env.Spawn<AddAgentMock>();
            LocalRef cR = env.Spawn<AddAgentMock>();
            LocalRef dR = env.Spawn<AddAgentMock>();

            AddMessage msg = new AddMessage(372.39);
            // Get futures
            Future<Response<AddMessage>> ftr = env.GetFuture<Response<AddMessage>>(TimeSpan.FromSeconds(5.0),
                env.GetFuture<Response<AddMessage>>(aR.ResId), env.GetFuture<Response<AddMessage>>(bR.ResId),
                env.GetFuture<Response<AddMessage>>(cR.ResId), env.GetFuture<Response<AddMessage>>(dR.ResId));
            // Send and wait
            ftr.Send(msg);
            var flag = ftr.WaitUntilCompleted(TimeSpan.FromSeconds(5.0));
            Assert.IsTrue(flag);
            var channel = ftr as FutureMulticastChannel<Response<AddMessage>>;
            Assert.IsNotNull(channel);
            Assert.IsTrue(channel.Completed);
            Assert.IsTrue(channel.Count == 4);
            Assert.IsNotNull(channel.Get);
            for (int i = 0; i < channel.Count; i++)
            {
                var reply = channel[i];
                Assert.IsNotNull(reply);
                Assert.IsTrue(reply.Body.AddValue == 372.39);
            }
            env.Shutdown();
        }

        [TestMethod]
        public void Multicast_future_can_show_failure()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            LocalRef aR = env.Spawn<AddAgentMock>();
            LocalRef bR = env.Spawn<AddAgentMock>();
            LocalRef cR = env.Spawn<AddAgentMock>();
            LocalRef dR = env.Spawn<AddAgentMock>();

            AddMessage msg = new AddMessage(372.39);
            // Get futures
            Future<Response<AddMessage>> ftr = env.GetFuture<Response<AddMessage>>(TimeSpan.FromSeconds(5.0),
                env.GetFuture<Response<AddMessage>>(aR.ResId), env.GetFuture<Response<AddMessage>>(bR.ResId),
                env.GetFuture<Response<AddMessage>>(""), env.GetFuture<Response<AddMessage>>(dR.ResId));
            // Try send
            ftr.Send(msg);
            var flag = ftr.WaitUntilCompleted(TimeSpan.FromSeconds(5.0));
            Assert.IsFalse(flag);
            var channel = ftr as FutureMulticastChannel<Response<AddMessage>>;
            Assert.IsNotNull(channel);
            Assert.IsFalse(channel.Completed);
            var test = channel[2];
            Assert.IsNull(test);
            env.Shutdown();
        }

        protected void VerifyInterruptAgentValue(LocalRefChannel channel, int value)
        {
            Assert.IsNotNull(channel);
            AgentPort port = channel.Resource as AgentPort;
            Assert.IsNotNull(port);
            InterruptAgent agent = port.AgentHandle as InterruptAgent;
            Assert.IsNotNull(agent);
            Assert.IsTrue(agent.NumValue == value, "Assert.IsTrue failed. Expected value was {0}.  The actual value was {1}.", value, agent.NumValue);
        }

        protected void VerifyReceiverTaskCount(LocalRefChannel channel)
        {
            Assert.IsNotNull(channel);
            VerifyReceiverTaskCount(channel.Resource as AgentPort);
        }

        protected void VerifyReceiverTaskCount(AgentPort port)
        {
            IPortReceive receivePort = port.AgentQueue as IPortReceive;
            Assert.IsNotNull(receivePort);
            ReceiverTask[] tasks = receivePort.GetReceivers();
            Assert.IsTrue(tasks.Length == 1);
        }
    }
}
