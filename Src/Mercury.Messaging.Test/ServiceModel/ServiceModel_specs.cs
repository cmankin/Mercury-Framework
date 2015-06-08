using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Net;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Serialization;
using Mercury.Messaging.ServiceModel;

namespace Mercury.Messaging.Test.ServiceModel
{
    /// <summary>
    /// Summary description for ServiceModel_specs
    /// </summary>
    [TestClass]
    public class ServiceModel_specs
    {
        public ServiceModel_specs()
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
        public void Run_simple_packet_protocol()
        {
            // Request
            Request<Stop> req1 = new RequestBase<Stop>(null, new Stop(), Guid.NewGuid().ToString());
            // Test response
            Response<Stop, Fault> resp1 = new ResponseBase<Stop, Fault>(req1,
                new Fault(new ArgumentException("Exception thrown.")));
            string respXml = RuntimeSerializer.Serialize(resp1);

            var messageId = 138827;
            byte[] packet = RuntimePacketProtocol.GetPacket(RuntimePacketProtocol.GetBytes(respXml), messageId);

            // Assert
            Assert.IsTrue(packet[0] == 0x00);
            Assert.IsTrue(packet[1] == 0x00);
            Assert.IsTrue(packet[2] == 0x00);
            int test = BitConverter.ToInt32(packet, 3);
            Assert.IsTrue(test == messageId);
            Assert.IsTrue(packet[7] == 0x05);
            int recordSize = BitConverter.ToInt32(packet, 8);
            Assert.IsTrue(recordSize == 1596);
            Assert.IsTrue(packet.Length == 1609);
            Assert.IsTrue(packet[1608] == 0x0c);

            // Extract
            int offset = 12;
            string messageData = RuntimePacketProtocol.GetDataString(packet, offset, recordSize);
            Assert.IsFalse(string.IsNullOrEmpty(messageData));
            ResponseBase<Stop, Fault> resp2 = RuntimeSerializer.Deserialize<ResponseBase<Stop, Fault>>(messageData);
            Assert.IsNotNull(resp2);
            Assert.IsTrue(resp1.RequestId == resp2.RequestId);
            Assert.IsTrue(resp1.Body.Exception.Message == resp2.Body.Exception.Message);
            Assert.IsTrue(resp1.Body.Exception.OriginalType == typeof(ArgumentException));
            Assert.IsTrue(resp1.Body.Exception.OriginalTypeName == "System.ArgumentException");
        }

        [TestMethod]
        public void Server_hosting_startup_and_shutdown_specs()
        {
            // Setup
            int port = 11501;
            IPAddress address = Machine.IPv4;
            IPEndPoint ep = new IPEndPoint(address, port);
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            MessagingService server = new MessagingService(ep, env);

            // Start server
            server.Start();

            // Sleep
            System.Threading.Thread.Sleep(520);

            Assert.IsNotNull(server);
            Assert.IsNotNull(server.ServerThread);
            Assert.IsTrue(server.Server == server.ServerThread.ManagedThreadId.ToString());
            Assert.IsTrue(server.IsStarted);

            // Stop server
            server.Stop();

            Assert.IsNotNull(server);
            Assert.IsNotNull(server.ServerThread);
            Assert.IsFalse(server.IsStarted);
        }

        [TestMethod]
        public void Server_restart_throws_objectdisposedexception()
        {
            // Setup
            int port = 11501;
            IPAddress address = Machine.IPv4;
            IPEndPoint ep = new IPEndPoint(address, port);
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            MessagingService server = new MessagingService(ep, env);

            // Start server
            server.Start();

            // Sleep
            System.Threading.Thread.Sleep(520);

            Assert.IsTrue(server.IsStarted);

            // Try to start again
            server.Start();

            // Stop server
            server.Stop();

            Assert.IsNotNull(server);
            Assert.IsFalse(server.IsStarted);

            // Restart throws 
            ObjectDisposedException caught = null;
            try
            {
                server.Start();
            }
            catch (ObjectDisposedException ex)
            {
                caught = ex as ObjectDisposedException;
            }

            Assert.IsNotNull(caught);
            Assert.IsTrue(caught is ObjectDisposedException);
            Assert.IsFalse(server.IsStarted);
        }
    }
}
