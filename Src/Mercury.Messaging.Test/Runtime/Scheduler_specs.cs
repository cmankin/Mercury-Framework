using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Runtime.Scheduler;

namespace Mercury.Messaging.Test.Runtime
{
    /// <summary>
    /// Summary description for Scheduler_specs
    /// </summary>
    [TestClass]
    public class Scheduler_specs
    {
        public Scheduler_specs()
        {
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
        public void Schedule_action_with_timer()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string message = string.Empty;
            Stopwatch sw = new Stopwatch();

            // Get scheduler
            IScheduler scheduler = env.GetScheduler();

            // Schedule
            sw.Start();
            scheduler.Schedule(1.Seconds(), () =>
            {
                sw.Stop();
                message = "Hello World";
            });


            while (message == string.Empty)
            {
                System.Threading.Thread.Sleep(1.Seconds());
            }

            // Assert
            Assert.IsTrue(message == "Hello World");
            Assert.IsTrue(sw.Elapsed >= 1.Seconds(), "Elapsed time should be greater than or equal to 1 second");
            Assert.IsTrue(sw.Elapsed < 2.Seconds(), "Elapsed time should be less than 2 seconds");
            env.Shutdown();
        }

        [TestMethod]
        public void Schedule_multiple_periodic_actions_with_timer()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            // Get scheduler
            IScheduler scheduler = env.GetScheduler();

            int counter = 2;
            Stopwatch sw = new Stopwatch();

            // Schedule
            sw.Start();
            scheduler.Schedule(1.Seconds(), 1.Seconds(), () =>
            {
                if (counter == 2)
                {
                    counter--;
                    scheduler.Schedule(0.Seconds(), 1.Seconds(), () =>
                    {
                        if (counter == 0)
                        {
                            sw.Stop();
                            scheduler.Stop();
                        }
                        counter--;
                    });
                }
            });

            while (counter > -1)
            {
                System.Threading.Thread.Sleep(1.Seconds());
            }

            // Assert
            Assert.IsTrue(counter == -1);
            Assert.IsTrue(sw.Elapsed < 3.Seconds());
            Assert.IsTrue(sw.Elapsed >= 2.Seconds());
            env.Shutdown();
        }

        [TestMethod]
        public void Stop_and_destroy_scheduler_before_scheduled_job_runs()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string message = string.Empty;

            // Schedule
            IScheduler scheduler = env.GetScheduler();
            try
            {
                scheduler.Schedule(1.Seconds(), () =>
                {
                    message = "NONE";
                });

            }
            catch
            {
                Assert.Fail();
            }

            // Stop
            scheduler.Stop();
            scheduler = null;
            System.Threading.Thread.Sleep(2.Seconds());

            // Assert
            Assert.IsNull(scheduler);
            Assert.IsTrue(message == string.Empty);
            env.Shutdown();
        }

        [TestMethod]
        public void Cancel_scheduled_job()
        {
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            string message = string.Empty;

            // Schedule
            IScheduler scheduler = env.GetScheduler();
            IScheduledOperation operation = null;
            try
            {
                operation = scheduler.Schedule(1.Seconds(), 1.Seconds(), () =>
                {
                    message = "NONE";
                });
            }
            catch
            {
                Assert.Fail();
            }

            // Cancel operation
            operation.Cancel();
            System.Threading.Thread.Sleep(2.Seconds());

            // Assert
            Assert.IsTrue(message == string.Empty);
            env.Shutdown();
        }
    }
}
