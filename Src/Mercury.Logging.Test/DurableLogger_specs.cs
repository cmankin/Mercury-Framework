using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Test
{
    /// <summary>
    /// Summary description for DurableLogger_specs
    /// </summary>
    [TestClass]
    public class DurableLogger_specs
    {
        public DurableLogger_specs()
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
        public void Can_write_to_durable_log_and_primary()
        {
            string fileName = "durable.log";
            string fPath = FileLogger_specs.GetPathInCurrentAssembly(fileName);
            if (System.IO.File.Exists(fPath))
                System.IO.File.Delete(fPath);

            Encoding encoding = Encoding.UTF8;
            MemoryLogger memLog = new MemoryLogger(60);
            DurableLogger logger = new DurableLogger(memLog, fPath, DurableLogger.DurabilityMode.WriteThrough);
            Assert.IsNotNull(logger);
            Assert.IsNotNull(logger.Primary);
            Assert.IsFalse(string.IsNullOrEmpty(logger.DurableLogPath));
            Assert.IsTrue(logger.Mode == DurableLogger.DurabilityMode.WriteThrough);

            logger.Info(1001, "Started durable log testing...");
            logger.Debug(5002, "Another entry to test.");
            string lastLine = "Testing a direct write with 123";
            logger.Write(lastLine);
            logger.Flush();
            logger.Dispose();

            string text = FileLogger_specs.ReadFullText(FileLogger_specs.GetPathInCurrentAssembly(fileName), encoding);
            Assert.IsNotNull(text);
            Assert.IsTrue(text.Contains(lastLine));

            var buffer = memLog.CopyLog();
            var primaryLast = buffer[2].Message;
            Assert.IsTrue(primaryLast == lastLine);

            if (System.IO.File.Exists(fPath))
                System.IO.File.Delete(fPath);
        }

        [TestMethod]
        public void Durable_logger_supports_delayed_write_to_primary()
        {
            string fileName = "durable.log";
            string fPath = FileLogger_specs.GetPathInCurrentAssembly(fileName);
            if (System.IO.File.Exists(fPath))
                System.IO.File.Delete(fPath);

            Encoding encoding = Encoding.UTF8;
            MemoryLogger memLog = new MemoryLogger(100);
            DurableLogger logger = new DurableLogger(memLog, fPath, DurableLogger.DurabilityMode.WriteDisk);
            Assert.IsTrue(logger.Mode == DurableLogger.DurabilityMode.WriteDisk);
            logger.Formatter.Options = LogOptions.ProcessId | LogOptions.ThreadId | LogOptions.DateTime;

            string debugFormat = "Write debug line:{0}";
            logger.Info(1001, "Started durable log delayed write testing...");
            for (int i = 0; i < 1000; i++)
                logger.Debug(5001, debugFormat, i);
            Assert.IsFalse(memLog.Filled);
            Assert.IsNull(memLog.CopyLog()[0]);
            logger.Flush();
            logger.Dispose();
            Assert.IsTrue(memLog.Filled);

            string text = FileLogger_specs.ReadFullText(FileLogger_specs.GetPathInCurrentAssembly(fileName), encoding);
            Assert.IsNotNull(text);

            var log = memLog.CopyLog();
            Assert.IsNotNull(log);
            Assert.IsNotNull(log[0]);
            var arr = log[0].Message.Split(new string[] { " : " }, StringSplitOptions.None);
            var testLn = arr[1].Trim();
            var frmt = string.Format(debugFormat, 999);
            testLn = testLn.Substring(0, frmt.Length);
            Assert.IsTrue(testLn == frmt);

            if (System.IO.File.Exists(fPath))
                System.IO.File.Delete(fPath);
        }

        [TestMethod]
        public void Durable_logger_supports_batched_writing_to_primary()
        {
            string fileName = "durable.log";
            string fPath = FileLogger_specs.GetPathInCurrentAssembly(fileName);
            if (System.IO.File.Exists(fPath))
                System.IO.File.Delete(fPath);

            Encoding encoding = Encoding.UTF8;
            MemoryLogger memLog = new MemoryLogger(100);
            DurableLogger logger = new DurableLogger(memLog, fPath, DurableLogger.DurabilityMode.BatchThrough);
            logger.Threshold = 30;

            string errFormat = "Write error at line:{0}";
            logger.Info(1001, "Started durable log batch write testing...");
            for (int i = 0; i < 1000; i++)
            {
                logger.Error(7001, errFormat, i);
                if (i == 27)
                    Assert.IsNull(memLog.CopyLog()[0]);
                else if (i == 28)
                    Assert.IsNotNull(memLog.CopyLog()[0]);
            }
            logger.Flush();
            logger.Dispose();

            string text = FileLogger_specs.ReadFullText(FileLogger_specs.GetPathInCurrentAssembly(fileName), encoding);
            Assert.IsNotNull(text);

            var log = memLog.CopyLog();
            Assert.IsNotNull(log);
            Assert.IsNotNull(log[0]);
            var arr = log[0].Message.Split(new string[] { " : " }, StringSplitOptions.None);
            var testLn = arr[1].Trim();
            Assert.IsTrue(testLn == string.Format(errFormat, 999));

            if (System.IO.File.Exists(fPath))
                System.IO.File.Delete(fPath);
        }
    }
}
