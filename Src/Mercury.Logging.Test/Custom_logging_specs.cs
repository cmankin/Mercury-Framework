using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Logging;
using Mercury.Logging.Test.Mock;

namespace Mercury.Logging.Test
{
    [TestClass]
    public class Custom_logging_specs
    {
        [TestMethod]
        public void Can_log_to_custom_rolling_file_log()
        {
            var fPath = FileLogger_specs.GetPathInCurrentAssembly("TestDir\\rollingFileLog.log");
            DeleteAllFilesAtParent(fPath, "*.log");

            var logger = new RollingFileLogger(fPath, Encoding.UTF8, 1024, 5, true);
            logger.Formatter.Options = LogOptions.DateTime | LogOptions.ProcessId | LogOptions.ThreadId | LogOptions.Callstack;
            Assert.IsNotNull(logger);
            Assert.IsTrue(logger.MaxFileSize == 1024);
            Assert.IsTrue(logger.MaxBackupFiles == 5);

            try
            {
                logger.Info(23, "In need of lengthier text.................................................................................................................................................................................................................................");
                logger.Info(23, "In need of lengthier text.................................................................................................................................................................................................................................");
                logger.Info(23, "In need of lengthier text.................................................................................................................................................................................................................................");
                logger.Info(23, "In need of lengthier text.................................................................................................................................................................................................................................");
                logger.Info(23, "In need of lengthier text.................................................................................................................................................................................................................................");

                // This write removes the first, original file
                Assert.IsTrue(File.Exists(fPath));
                logger.Warn(55, "This writes over the maximum number of backup files.  This log entry will cause the logger to roll back around............................................................................................................................................");
                Assert.IsFalse(File.Exists(fPath));

                // This write removes the first rollover file - rollingFileLog.1.log
                var firstRolloverPath = FileLogger_specs.GetPathInCurrentAssembly("TestDir\\rollingFileLog.1.log");
                Assert.IsTrue(File.Exists(firstRolloverPath));
                logger.Info(23, "In need of lengthier text.................................................................................................................................................................................................................................");
                Assert.IsFalse(File.Exists(firstRolloverPath));
            }
            finally
            {
                logger.Dispose();
                DeleteAllFilesAtParent(fPath, "*.log");
            }
        }

        public void Can_log_to_a_custom_event_log()
        {
            var evtLogger = new WindowsEventLogger();
            Assert.IsNotNull(evtLogger);

            try
            {
                evtLogger.Info(23, "Test event log was successful!");
            }
            catch(Exception ex)
            {
                Assert.Fail("The event logging failed due to the following error: {0}", ex);
            }

            Assert.IsTrue(evtLogger.SourceName == "Mercury.Logging.TestEventLogger.Source");
        }

        internal static void DeleteAllFilesAtParent(string filePath, string searchPattern)
        {
            var dir = Directory.GetParent(filePath);
            if (dir != null)
            {
                FileInfo[] logFiles = dir.GetFiles(searchPattern);
                foreach (var info in logFiles)
                {
                    info.Delete();
                }
            }
        }
    }
}
