using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Messaging.Core;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Serialization;
using Mercury.Messaging.ServiceModel;

namespace Mercury.Messaging.Test
{
    /// <summary>
    /// Summary description for Serialization_specs
    /// </summary>
    [TestClass]
    public class Serialization_specs
    {
        public Serialization_specs()
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
        public void Runtime_serializer_generic_serialize()
        {
            // Get header
            string header = RuntimeSerializer.GenerateHeader(Encoding.UTF8);
            Assert.IsFalse(string.IsNullOrEmpty(header));

            // Create exit message
            Exit exitMessage = new Exit("address/id");
            string exitXml = RuntimeSerializer.Serialize(exitMessage, Encoding.UTF8);

            // Assert 
            Assert.IsFalse(string.IsNullOrEmpty(exitXml));

            // Serialize list
            IList<string> sList = new List<string>();
            sList.Add("hello, ");
            sList.Add("he ");
            sList.Add("said, tenuously");
            string listXml = RuntimeSerializer.Serialize(sList, Encoding.UTF8);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(listXml));
            IList<string> newList = RuntimeSerializer.Deserialize(listXml, typeof(List<string>)) as List<string>;
            Assert.IsNotNull(newList);

            // Serialize int
            int xInt = 100;
            string intXml = RuntimeSerializer.Serialize(xInt, Encoding.UTF8);

            // Serialize exception
            Exception exc = new Exception("Exception generated from outer block.", 
                new ArgumentException("Argument incorrectly formatted.", 
                    new ArgumentNullException("param", "Argument was set to null.")));
            SerialException serialExc = SerialException.Create(exc);
            string exceptionXml = RuntimeSerializer.Serialize(serialExc, Encoding.UTF8);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(exceptionXml));
            serialExc = null;
            Assert.IsNull(serialExc);
            serialExc = RuntimeSerializer.Deserialize<SerialException>(exceptionXml);
            Assert.IsNotNull(serialExc);

            // Create fault message
            Fault fault = new Fault(new ArgumentException());
            string faultXml = RuntimeSerializer.Serialize(fault, Encoding.UTF8);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(faultXml));
            fault = null;
            Assert.IsNull(fault);
            fault = RuntimeSerializer.Deserialize<Fault>(faultXml);
            Assert.IsNotNull(fault);

            // Create message
            MessageBase<Stop> newMsg = new MessageBase<Stop>(new Stop());
            string stopMessageXml = RuntimeSerializer.Serialize(newMsg, Encoding.UTF8);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(stopMessageXml));
            newMsg = null;
            Assert.IsNull(newMsg);
            newMsg = RuntimeSerializer.Deserialize<MessageBase<Stop>>(stopMessageXml);
            Assert.IsNotNull(newMsg);
        }

        [TestMethod]
        public void Serialize_deserialize_request_and_response()
        {
            // Request
            Request<Stop> req1 = new RequestBase<Stop>(null, new Stop(), Guid.NewGuid().ToString());
            string reqXml = RuntimeSerializer.Serialize(req1, Encoding.UTF8);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(reqXml));
            Request<Stop> req2 = RuntimeSerializer.Deserialize<RequestBase<Stop>>(reqXml);
            Assert.IsNotNull(req2);
            Assert.IsTrue(req1.GetHashCode() != req2.GetHashCode());
            Assert.IsTrue(req1.RequestId == req2.RequestId);

            // Response
            Response<Stop, Fault> resp1 = new ResponseBase<Stop, Fault>(req1, 
                new Fault(new ArgumentException("Exception thrown.")));
            string respXml = RuntimeSerializer.Serialize(resp1, Encoding.UTF8);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(respXml));
            ResponseBase<Stop, Fault> resp2 = RuntimeSerializer.Deserialize<ResponseBase<Stop, Fault>>(respXml);
            Assert.IsNotNull(resp2);
            Assert.IsTrue(resp1.GetHashCode() != resp2.GetHashCode());
            Assert.IsTrue(resp1.RequestId == resp2.RequestId);

            Response<Stop> resp3 = new ResponseBase<Stop>(new Stop(), req1.RequestId);
            string xml = RuntimeSerializer.Serialize(resp3, Encoding.UTF8);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(xml));
            Response<Stop> resp4 = RuntimeSerializer.Deserialize<ResponseBase<Stop>>(xml);
            Assert.IsNotNull(resp4);
            Assert.IsTrue(resp3.GetHashCode() != resp4.GetHashCode());
            Assert.IsTrue(resp3.RequestId == resp4.RequestId);
        }

        [TestMethod]
        public void Serialize_serial_message_for_packet()
        {
            string destination = "http://env/worker1";
            // Request
            Request<Stop> req1 = new RequestBase<Stop>(null, new Stop(), Guid.NewGuid().ToString());
            // Create message
            SerialMessage message = new SerialMessage(req1, destination, false);
            string messageXml = RuntimeSerializer.Serialize(message);

            // Get packet
            var messageId = 112997;
            byte[] packet = RuntimePacketProtocol.GetPacket(messageXml, messageId);

            // Assert
            Assert.IsTrue(packet[0] == 0x00);
            Assert.IsTrue(packet[1] == 0x00);
            Assert.IsTrue(packet[2] == 0x00);
            int test = BitConverter.ToInt32(packet, 3);
            Assert.IsTrue(test == messageId);
            Assert.IsTrue(packet[7] == 0x05);
            int recordSize = BitConverter.ToInt32(packet, 8);

            // Extract
            int offset = 12;
            string xmlData = RuntimePacketProtocol.GetDataString(packet, offset, recordSize);
            SerialMessage msg2 = RuntimeSerializer.Deserialize(xmlData, typeof(SerialMessage)) as SerialMessage;
            
            // Assert
            Assert.IsNotNull(msg2);
            Assert.IsTrue(((Request<Stop>)message.Message).RequestId == ((Request<Stop>)msg2.Message).RequestId);
        }
    }
}
