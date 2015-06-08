using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging;
using Mercury.Logging.Loggers;
using Mercury.Logging.Test.Mock;

namespace Mercury.Logging.Test
{
    /// <summary>
    /// Summary description for PersistentLogger_specs
    /// </summary>
    [TestClass]
    public class PersistentLogger_specs
    {
        public PersistentLogger_specs()
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
        public void Can_persist_across_time_interval()
        {
            MockMemoryLogger memLog = new MockMemoryLogger(false) { Formatter = new Mock.NoFormattingFormatter() };
            PersistentLogger pLog = new PersistentLogger(memLog, 60, 1);

            pLog.Write("0");
            pLog.Write("1");
            pLog.Write("2");
            pLog.Write("3");

            Assert.IsTrue(memLog.LogFile.Count == 0);
            memLog.AllowWrites = true;
            System.Threading.Thread.Sleep(2000);
            //System.Threading.Thread.Sleep(50000);
            Assert.IsTrue(memLog.LogFile.Count == 4);
            var log = memLog.LogFile;
            int i = 0;
            foreach (var msg in log)
            {
                var outVal = int.Parse(msg);
                Assert.IsTrue(i == outVal);
                i++;
            }
        }

        [TestMethod]
        public void Can_flush_cancel_pending_retry()
        {
            MockMemoryLogger memLog = new MockMemoryLogger(false) { Formatter = new Mock.NoFormattingFormatter() };
            PersistentLogger pLog = new PersistentLogger(memLog, 60, 1);

            pLog.Write("0");
            pLog.Write("1");
            pLog.Write("2");
            pLog.Write("3");

            Assert.IsTrue(memLog.LogFile.Count == 0);
            memLog.AllowWrites = true;
            pLog.Flush();
            Assert.IsTrue(memLog.LogFile.Count == 4);
            System.Threading.Thread.Sleep(1200);
            var log = memLog.LogFile;
            int i = 0;
            foreach (var msg in log)
            {
                var outVal = int.Parse(msg);
                Assert.IsTrue(i == outVal);
                i++;
            }
            Assert.IsTrue(i == 4);
        }
    }
}
