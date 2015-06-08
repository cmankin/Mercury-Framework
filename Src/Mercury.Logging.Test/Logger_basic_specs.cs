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
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Logger_basic_specs
    {
        public Logger_basic_specs()
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
        public void Can_create_an_in_memory_logger()
        {
            MemoryLogger memLog = new MemoryLogger(60);
            Assert.IsNotNull(memLog);
            Assert.IsNotNull(memLog.Formatter);
            Assert.IsNotNull(memLog.Filter);
            Assert.IsTrue(memLog.Name == null);

            memLog.Debug("Testing");
            memLog.Debug(() => { return "A test statement."; });
            memLog.Debug(22, "Testing message code 22.");
            memLog.Debug(22, () => { return "Another test statement."; });
            memLog.Debug("This message has a '{0}' severity level.", LogSeverity.Debug);
            memLog.Debug(22, "Another message with event ID {0}.", 22);

            Assert.IsTrue(memLog.Head == 6);
            var log = memLog.GetLogAsEnumerable();
            var logList = new List<string>();
            foreach (LogEntry entry in log)
            {
                if (entry == null)
                    break;
                logList.Add(entry.Message);
            }
            Assert.IsTrue(logList.Count == 6);
        }

        [TestMethod]
        public void Can_log_with_options()
        {
            MemoryLogger memLog = new MemoryLogger(60);
            memLog.Formatter.Options = LogOptions.Callstack;

            memLog.Info("Test message with callstack.");
            var testLog = memLog.CopyLog();
            Assert.IsNotNull(testLog);
            Assert.IsTrue(testLog[0].Message.Length > 100);

            memLog.Formatter.Options = LogOptions.Callstack | LogOptions.ProcessId | LogOptions.ThreadId | LogOptions.DateTime | LogOptions.Timestamp;
            memLog.Error(70, () => { return "An error message with formatter options."; });
            testLog = memLog.CopyLog();
            Assert.IsTrue(testLog[1].Message.Length > 100);
        }

        [TestMethod]
        public void Can_use_severity_and_category_filters()
        {
            MemoryLogger memLog = new MemoryLogger(60);
            memLog.SeverityThreshold = LogSeverity.Critical;

            // Severity threshold filters...
            memLog.Debug("Will not work!");
            memLog.Info("Will not work!");
            memLog.Warn("Will not work!");
            memLog.Error("Will not work!");
            var testLog = memLog.CopyLog();
            Assert.IsTrue(testLog[0] == null);

            memLog.Critical("This will work!");
            testLog = memLog.CopyLog();
            Assert.IsFalse(testLog[0] == null);

            // Category filters...
            memLog.Clear();
            var fltr = new CategoryFilter(true);
            fltr.AddCategory("DataCore");
            fltr.AddCategory("CoreLogic");
            memLog.Filter = fltr;

            memLog.WithFilter("Data").Critical(9000, "Critical data error occurred.");
            testLog = memLog.CopyLog();
            Assert.IsTrue(testLog[0] == null);

            memLog.WithFilter("DataCore").Critical(9000, "Critical data error occurred.  This will work!");
            testLog = memLog.CopyLog();
            Assert.IsFalse(testLog[0] == null);

            var exFltr = new CategoryFilter(false);
            exFltr.AddCategory("Entity");
            exFltr.AddCategory("UI");
            memLog.Filter = exFltr;

            memLog.Clear();
            memLog.WithFilter("Entity").Critical(9000, "Cannot write critical message when category is blacklisted.");
            testLog = memLog.CopyLog();
            Assert.IsTrue(testLog[0] == null);

            memLog.WithFilter("").Critical(9000, "Can write on any non-blacklisted category.");
            testLog = memLog.CopyLog();
            Assert.IsFalse(testLog[0] == null);
        }

        [TestMethod]
        public void Can_log_with_arguments_without_automatic_message_string_format()
        {
            MemoryLogger memLog = new MemoryLogger(10) { FormatMessageArguments = false, Formatter = new Mock.NoFormattingFormatter() };
            
            // Nothing affected on non-args entries.
            memLog.Info("1");
            memLog.Debug(() => "2");
            memLog.Warn(34, "5");
            memLog.Error(6, () => "7");

            var copied = memLog.CopyLog();
            Assert.IsTrue(copied[0].Message == "1");
            Assert.IsTrue(copied[1].Message == "2");
            Assert.IsTrue(copied[2].EventId == 34 && copied[2].Message == "5");
            Assert.IsTrue(copied[3].EventId == 6 && copied[3].Message == "7");

            memLog.Clear();

            // Standard entries when write direct
            memLog.Write("{0}");
            memLog.Write("{1}");

            copied = memLog.CopyLog();
            Assert.IsTrue(copied[0].Message == "{0}");
            Assert.IsTrue(copied[1].Message == "{1}");

            memLog.Clear();

            // Args entries are not formatted.
            memLog.Info("{0}{1}", 23);
            memLog.Info("{4}   ", 5, 6);

            copied = memLog.CopyLog();
            Assert.IsTrue(copied[0].Message == "{0}{1}" && (int)copied[0].Args[0] == 23);
            Assert.IsTrue(copied[1].Message == "{4}   ");
            Assert.IsTrue((int)copied[1].Args[0] == 5);
            Assert.IsTrue((int)copied[1].Args[1] == 6);
            Assert.IsNull(copied[2]);
            Assert.IsNull(copied[9]);
        }

        [TestMethod]
        public void Can_propagate_log_entry_throughout_the_logger_tree()
        {
            MemoryLogger memLog = new MemoryLogger(10);
            ConsoleLogger consLog = new ConsoleLogger();
            CompositeLogger composite = new CompositeLogger(memLog, consLog);
            string message = "hello world!";

            var builder = new StringBuilder();
            using (var consoleOut = new System.IO.StringWriter(builder))
            {
                Console.SetOut(consoleOut);

                var entry = new Mock.MockEntry(message, new object[] { "1", 2, 3, "4" });
                composite.Log(entry);

                // Verify memory log entry is intact
                var entries = memLog.CopyLog();
                Assert.IsTrue(entries[0].LoggerName == "mock");
                Assert.IsTrue(entries[0].Severity == LogSeverity.Info);
                Assert.IsTrue(entries[0].EventId == 99);
                Assert.IsTrue(entries[0].RawMessage == string.Format("\r\nmock Info [99] : {0}", message));

                string output = builder.ToString();
                Assert.IsNotNull(output);
                Assert.IsTrue(output == string.Format("\r\nmock Info [99] : {0}", message));
            }
        }
    }
}
