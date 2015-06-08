using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Test
{
    /// <summary>
    /// Summary description for FileLogger_specs
    /// </summary>
    [TestClass]
    public class FileLogger_specs
    {
        public FileLogger_specs()
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
        public void Can_create_log_file()
        {
            var logName = "testlog.log";
            var logPath = Path.Combine(CurrentAssemblyPath, logName);
            FileLogger logger = new FileLogger(logPath, Encoding.Unicode);
            Assert.IsNotNull(logger);
            Assert.IsTrue(logger.Threshold == 1);
            Assert.IsTrue(logger.Encoding == Encoding.Unicode);
            Assert.IsTrue(logger.FilePath == logPath);
            Assert.IsTrue(File.Exists(logPath));
            logger.Threshold = 60;

            logger.Log(LogSeverity.Info, "Created new file logger for {0}.", logName);
            logger.Write("Something interesting...");
            logger.Write("1, ");
            logger.Write("2");
            logger.Flush();
            logger.Dispose();

            string text = ReadFullText(logPath, Encoding.Unicode);
            Assert.IsNotNull(text);
            var lastChar = text.Substring(text.Length - 1, 1);
            Assert.IsTrue(lastChar == "2");

            if (File.Exists(logPath))
                File.Delete(logPath);
        }

        [TestMethod]
        public void Can_flush_and_continue_writing()
        {
            var logName = "testlog.log";
            var logPath = Path.Combine(CurrentAssemblyPath, logName);
            using (var logger = new FileLogger(logPath, Encoding.Unicode))
            {
                Assert.IsTrue(File.Exists(logPath));
                logger.Threshold = 10;

                for (int i = 0; i < 5000; i++)
                {
                    logger.Log(LogSeverity.Info, "Created new file logger for {0}.", logName);
                    logger.WriteLine("1");
                    logger.Flush();
                    logger.WriteLine("2");
                    logger.WriteLine("3");
                    logger.WriteLine("4");
                    logger.Flush();
                    logger.WriteLine("5");
                    logger.WriteLine("6");
                    logger.WriteLine("7");
                    logger.Flush();
                }
            }
            string text = ReadFullText(logPath, Encoding.Unicode);
            Assert.IsNotNull(text);
            var noLnText = text.Replace(Environment.NewLine, "");
            var lastSigChar = noLnText.Substring(noLnText.Length - 1, 1);
            Assert.IsTrue(lastSigChar == "7");

            if (File.Exists(logPath))
                File.Delete(logPath);
        }

        internal static string ReadFullText(string filePath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;
            if (!File.Exists(filePath))
                throw new ArgumentException("File does not exist.");
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] builder = new byte[stream.Length];
                int offset = 0;
                int bytesToRead = (int)stream.Length;
                int bytesRead = 0;
                while (bytesToRead > 0)
                {
                    bytesRead = stream.Read(builder, offset, bytesToRead);
                    offset += bytesRead;
                    bytesToRead -= bytesRead;
                }
                return encoding.GetString(builder);
            }
        }

        internal static readonly string CurrentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static string GetPathInCurrentAssembly(string fileName)
        {
            return Path.Combine(CurrentAssemblyPath, fileName);
        }
    }
}
