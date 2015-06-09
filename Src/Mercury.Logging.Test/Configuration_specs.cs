using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging.Configuration;
using Mercury.Logging.Loggers;
using Mercury.Logging;

namespace Mercury.Logging.Test
{
    /// <summary>
    /// Summary description for Configuration_specs
    /// </summary>
    [TestClass]
    public class Configuration_specs
    {
        public Configuration_specs()
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
        public void Can_load_loggers_from_configuration()
        {
            var logger = LogFramework.GetLogger(typeof(Configuration_specs));
            Assert.IsNotNull(logger);
            Assert.IsTrue(logger.Name == typeof(Configuration_specs).FullName);
            Assert.IsInstanceOfType(logger, typeof(CompositeLogger));

            // Assert formatter initialization
            var rootLogger = logger as CompositeLogger;
            Assert.IsInstanceOfType(rootLogger.Formatter, typeof(DefaultLogFormatter));
            Assert.IsTrue(rootLogger.Formatter.Options == (LogOptions.ProcessId | LogOptions.ThreadId));

            // Assert filter initialization
            var fltr = rootLogger.Filter as PassFilter;
            Assert.IsNotNull(fltr);
            Assert.IsFalse(fltr.Fail);
            Assert.IsTrue(rootLogger.Formatter.Options == (LogOptions.ProcessId | LogOptions.ThreadId));

            // Assert child loggers
            var children = rootLogger.GetAll();
            Assert.IsTrue(children.Count == 3);

            // Assert top-level error logger
            Assert.IsInstanceOfType(children[0], typeof(MemoryLogger));
            var errorlog = children[0] as MemoryLogger;
            Assert.IsTrue(errorlog.BufferSize == 100);
            Assert.IsTrue(errorlog.SeverityThreshold == LogSeverity.Error);
            Assert.IsTrue(errorlog.Formatter.Options == (LogOptions.DateTime | LogOptions.ProcessId | LogOptions.ThreadId | LogOptions.Callstack));

            // Assert top-level persistent logger
            Assert.IsInstanceOfType(children[1], typeof(PersistentLogger));
            var persistent = children[1] as PersistentLogger;
            Assert.IsTrue(persistent.BufferSize == 200);
            Assert.IsTrue(persistent.RetryInterval == 1);
            Assert.IsNotNull(persistent.Logger);
            Assert.IsInstanceOfType(persistent.Logger, typeof(ConsoleLogger));
            Assert.IsTrue(persistent.Logger.Name == "Console");

            // Assert top-level composite logger
            Assert.IsInstanceOfType(children[2], typeof(CompositeLogger));
            var composite = children[2] as CompositeLogger;
            var compositeChildren = composite.GetAll();
            Assert.IsTrue(compositeChildren.Count == 3);

            // Assert composited MemoryLogger init
            Assert.IsInstanceOfType(compositeChildren[0], typeof(MemoryLogger));
            var memlog = compositeChildren[0] as MemoryLogger;
            Assert.IsTrue(memlog.BufferSize == 250);
            Assert.IsInstanceOfType(memlog.Filter, typeof(PassFilter));
            Assert.IsTrue(((PassFilter)memlog.Filter).Fail);

            // Assert composited error logger
            Assert.IsInstanceOfType(compositeChildren[1], typeof(MemoryLogger));
            var el = compositeChildren[1] as MemoryLogger;
            Assert.IsTrue(el.BufferSize == 100);
            Assert.IsTrue(el.SeverityThreshold == LogSeverity.Error);
            Assert.IsTrue(el.Formatter.Options == (LogOptions.DateTime | LogOptions.ProcessId | LogOptions.ThreadId | LogOptions.Callstack));

            // Assert composited console logger
            Assert.IsInstanceOfType(compositeChildren[2], typeof(ConsoleLogger));
            Assert.IsTrue(compositeChildren[2].Name == "Console");
        }

        [TestMethod]
        public void Can_load_configuration_from_custom_section()
        {
            var logger = LogFramework.GetLogger("", null, "customLog");
            Assert.IsNotNull(logger);
            Assert.IsInstanceOfType(logger, typeof(MemoryLogger));
            Assert.IsTrue(logger.Name == "memLogger");
            Assert.IsTrue(((MemoryLogger)logger).BufferSize == 10);
        }
    }
}
