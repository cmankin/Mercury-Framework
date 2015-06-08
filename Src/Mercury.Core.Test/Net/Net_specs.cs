using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Net;

namespace Mercury.Common.Test.Net
{
    /// <summary>
    /// Summary description for Net_specs
    /// </summary>
    [TestClass]
    public class Net_specs
    {
        public Net_specs()
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
        public void Create_packet_protocol()
        {
            string withVia = "http://env/worker1";
            string messageData = "A quick message.";
            PacketHeader header = new PacketHeader(0x01, 0x01, CommunicationMode.SingletonSized, withVia, 
                EnvelopeEncoding.Unicode, EnvelopeStructure.FormattedString);
            PacketProtocol packet = new PacketProtocol(header, messageData);
            byte[] byteData = packet.GetPacket();
            
            // Assert
            // Record 0 - Version
            Assert.IsTrue(byteData[0] == 0x00);
            Assert.IsTrue(byteData[1] == 0x01);
            Assert.IsTrue(byteData[2] == 0x01);

            // Record 1 - Mode
            Assert.IsTrue(byteData[3] == 0x01);
            Assert.IsTrue(byteData[4] == (byte)CommunicationMode.SingletonSized);

            // Record 2 - Via
            Assert.IsTrue(byteData[5] == 0x02);
            byte[] viaSizeBytes = byteData.ExtractCopy(6, 2);
            UInt16 viaSize = PacketProtocol.ConvertToUint16(viaSizeBytes, 0);
            string via = PacketProtocol.GetData(EnvelopeEncoding.Utf8.Encoding, byteData, 8, viaSize);
            Assert.IsTrue(via == withVia);

            // Record 3 - Encoding
            int rec3Index = 8 + viaSize;
            Assert.IsTrue(byteData[rec3Index] == 0x03);
            Assert.IsTrue(byteData[rec3Index + 1] == 0x02);

            // Record 4 - Structure
            int rec4Index = rec3Index + 2;
            Assert.IsTrue(byteData[rec4Index] == 0x04);
            Assert.IsTrue(byteData[rec4Index + 1] == 0x00);

            // Record 9 - End Preamble
            int rec9Index = rec4Index + 2;
            Assert.IsTrue(byteData[rec9Index] == 0x09);

            // Record 5 - Sized envelope record
            int rec5Index = rec9Index + 1;
            Assert.IsTrue(byteData[rec5Index] == 0x05);
            byte[] recSizeBytes = byteData.ExtractCopy(rec5Index + 1, 4);
            int recordSize = (int)PacketProtocol.ConvertToUint32(recSizeBytes, 0);
            int startIndex = rec5Index + 7;

            // Record data
            string recordData = PacketProtocol.GetData(EnvelopeEncoding.Unicode.Encoding, byteData, startIndex, recordSize);
            Assert.IsTrue(recordData == messageData);
        }

        
    }
}
