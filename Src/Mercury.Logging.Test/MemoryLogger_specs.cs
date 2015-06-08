using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Test
{
    /// <summary>
    /// Summary description for Memory_logger_specs
    /// </summary>
    [TestClass]
    public class MemoryLogger_specs
    {
        public MemoryLogger_specs()
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
        public void Can_write_to_memory_log()
        {
            MemoryLogger memLog = new MemoryLogger(10);
            Assert.IsFalse(memLog.Filled);
            memLog.Write("0");
            memLog.Write("1");
            memLog.Write("2");
            memLog.Write("3");
            memLog.Write("4");
            memLog.Write("5");
            memLog.Write("6");
            memLog.Write("7");
            memLog.Write("8");
            memLog.Write("9");
            Assert.IsTrue(memLog.Head == 10);
            Assert.IsTrue(memLog.Filled);

            var log = memLog.GetLogAsEnumerable();
            int i = 0;
            foreach (LogEntry output in log)
            {
                Assert.IsTrue(output.Message == string.Format("{0}", i));
                i++;
            }
        }

        [TestMethod]
        public void Can_remove_and_compact_values()
        {
            MemoryLogger memLog = new MemoryLogger(10);
            Assert.IsFalse(memLog.Filled);
            memLog.Write("0");
            memLog.Write("1");
            memLog.Write("2");
            memLog.Write("3");
            memLog.Write("4");
            memLog.Write("5");
            memLog.Write("6");
            memLog.Write("7");
            memLog.Write("8");
            memLog.Write("9");
            Assert.IsTrue(memLog.Head == 10);
            Assert.IsTrue(memLog.Filled);

            string outVal = null;
            Assert.IsTrue(memLog.Remove(9, out outVal));
            Assert.IsTrue(outVal == "9");
            Assert.IsTrue(memLog.Remove(7, out outVal));
            Assert.IsTrue(outVal == "7");
            Assert.IsTrue(memLog.Remove(5, out outVal));
            Assert.IsTrue(outVal == "5");
            Assert.IsTrue(memLog.Remove(3, out outVal));
            Assert.IsTrue(outVal == "3");
            Assert.IsTrue(memLog.Remove(1, out outVal));
            Assert.IsTrue(outVal == "1");

            var testLog = memLog.CopyLog();
            for (int i = 0; i < testLog.Length; i++)
            {
                if (i < 5)
                    Assert.IsNotNull(testLog[i]);
                else
                    Assert.IsNull(testLog[i]);
            }

            memLog.Write(null);
            memLog.Write(null);
            memLog.Write("1");
            memLog.Write("3");
            memLog.Write("5");

            var seq = memLog.GetConsumingEnumerable();
            foreach (LogEntry entry in seq)
            {
                break;
            }
            memLog.Compact();
            Assert.IsFalse(memLog.Filled);
            Assert.IsTrue(memLog.Head == 9);
            var copiedValues = memLog.CopyLog();
            Assert.IsTrue(copiedValues[0].Message == "2");
            Assert.IsFalse(string.IsNullOrEmpty(copiedValues[3].Message));
            Assert.IsNull(copiedValues[4].Message);
            Assert.IsNull(copiedValues[5].Message);
            Assert.IsFalse(string.IsNullOrEmpty(copiedValues[6].Message));
            Assert.IsNull(copiedValues[9]);
        }

        [TestMethod]
        public void Can_clear_and_compact_values()
        {
            MemoryLogger memLog = new MemoryLogger(10);
            Assert.IsFalse(memLog.Filled);
            memLog.Write("0");
            memLog.Write("1");
            memLog.Write("2");
            memLog.Write("3");
            memLog.Write("4");
            memLog.Write("5");
            memLog.Write("6");
            memLog.Write("7");
            memLog.Write("8");
            memLog.Write("9");
            Assert.IsTrue(memLog.Head == 10);
            Assert.IsTrue(memLog.Filled);

            var enumerator = memLog.GetLogAsEnumerable();
            foreach (LogEntry entry in enumerator)
            {
                Assert.IsNotNull(entry);
                Assert.IsFalse(string.IsNullOrEmpty(entry.Message));
            }

            memLog.Clear();
            Assert.IsTrue(memLog.Head == 0);
            Assert.IsFalse(memLog.Filled);
            enumerator = memLog.GetLogAsEnumerable();
            foreach (LogEntry entry in enumerator)
                Assert.IsNull(entry);
        }

        [TestMethod]
        public void Can_remove_and_compact_with_consuming_enumerable()
        {
            MemoryLogger memLog = new MemoryLogger(10);
            Assert.IsFalse(memLog.Filled);
            memLog.Write("0");
            memLog.Write("1");
            memLog.Write("2");
            memLog.Write("3");
            memLog.Write("4");
            memLog.Write("5");
            memLog.Write("6");
            memLog.Write("7");
            memLog.Write("8");
            memLog.Write("9");
            Assert.IsTrue(memLog.Head == 10);
            Assert.IsTrue(memLog.Filled);

            var enumerator = memLog.GetConsumingEnumerable();
            foreach (LogEntry entry in enumerator)
            {
                Assert.IsNotNull(entry);
                Assert.IsFalse(string.IsNullOrEmpty(entry.Message));
            }

            Assert.IsTrue(memLog.Head == 0);
            Assert.IsFalse(memLog.Filled);
            var log = memLog.CopyLog();
            foreach (LogEntry entry in log)
                Assert.IsNull(entry);
        }
    }
}
