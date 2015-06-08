using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging.Loggers;
using Mercury.Logging;

namespace Mercury.Logging.Test
{
    /// <summary>
    /// Summary description for CompositeLogger_specs
    /// </summary>
    [TestClass]
    public class CompositeLogger_specs
    {
        public CompositeLogger_specs()
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
        public void CompositeLogger_can_log_to_multiple_loggers()
        {
            string fileName = "test-with-composite.log";
            string filePath = FileLogger_specs.GetPathInCurrentAssembly(fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            FileLogger fLog = new FileLogger(filePath);
            ConsoleLogger consLog = new ConsoleLogger();
            fLog.SeverityThreshold = LogSeverity.Error;
            var fltr = new CategoryFilter(true);
            fltr.AddCategory("Memory");
            MemoryLogger memLog = new MemoryLogger(100);
            memLog.Filter = fltr;

            fLog.Name = "testFile";
            memLog.Name = "memlog";
            consLog.Name = "console";
            CompositeLogger logger = new CompositeLogger(fLog, memLog, consLog);
            Assert.IsNotNull(logger);
            Assert.IsNotNull(logger.Find("memlog"));

            logger.WithFilter("Memory").Info("Composite logger start.  Send message only to console and memory logs...");
            logger.Debug("Log entry {0}", 1);
            logger.Debug("Log entry {0}", 2);
            logger.Debug("Log entry {0}", 3);
            logger.Error("Error occurred!!!");
            logger.Error("Last error entry.");
            logger.Critical("Critical failure!");
            logger.Warn("Warning not received on file logger.");
            logger.WithFilter("Memory").Info("Test complete.");
            logger.Flush();
            logger.Dispose();

            string text = FileLogger_specs.ReadFullText(filePath, Encoding.Unicode);
            Assert.IsNotNull(text);

            var log = memLog.CopyLog();
            Assert.IsNotNull(log);
            Assert.IsNotNull(log[1]);
            Assert.IsNull(log[2]);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}
