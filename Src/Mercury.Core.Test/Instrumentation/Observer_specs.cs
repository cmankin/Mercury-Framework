using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Mercury.Instrumentation;
using Mercury.Logging;

namespace Mercury.Core.Test.Instrumentation
{
    /// <summary>
    /// Summary description for ObserverSpecs
    /// </summary>
    [TestClass]
    public class Observer_specs
    {
        public Observer_specs()
        {
            Log.InitializeWith<TestLogger>();
            this._logger = DEFAULT_LOGGER.Log() as TestLogger;
        }

        public const string DEFAULT_LOGGER = "default";
        private TestLogger _logger;
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
        public void Observe_on_block()
        {
            using (TraceObserver obs = new TraceObserver(MethodBase.GetCurrentMethod(), DEFAULT_LOGGER.Log()))
            {
                // Assert entry logged
                Assert.IsTrue(this._logger.LogLines.Count == 1);

                // Do some additional processing
                string s = "";
                bool flag = false;
                if (!string.IsNullOrEmpty(s) && !flag)
                    flag = true;
                s = string.Empty;

                // Assert exit not logged prematurely.
                Assert.IsTrue(this._logger.LogLines.Count == 1);
            }

            // Assert exit logged.
            Assert.IsTrue(this._logger.LogLines.Count == 2);
            // Clear lines
            this._logger.LogLines.Clear();
        }

        [TestMethod]
        public void Delay_observe_on_block()
        {
            using (TraceObserver obs = new TraceObserver(MethodBase.GetCurrentMethod(), DEFAULT_LOGGER.Log(), null, true))
            {
                // Assert entry NOT logged
                Assert.IsTrue(this._logger.LogLines.Count == 0);
                // Do processing...

                // Enter
                obs.Enter();

                // Assert entry logged
                Assert.IsTrue(this._logger.LogLines.Count == 1);

                // Do processing...
            }

            // Assert exit logged.
            Assert.IsTrue(this._logger.LogLines.Count == 2);
            // Clear lines
            this._logger.LogLines.Clear();
        }

        [TestMethod]
        public void Explicit_enter_and_exit_block()
        {
            TraceObserver obs = new TraceObserver(MethodBase.GetCurrentMethod(), DEFAULT_LOGGER.Log(), null, true);
            
            // Do some processing..

            // Assert entry NOT logged
            Assert.IsTrue(this._logger.LogLines.Count == 0);

            // Enter here
            obs.Enter();

            

            // Do some processing..

            // Attempt reentrancy
            obs.Enter();

            // Assert no additional entry logs
            Assert.IsTrue(this._logger.LogLines.Count == 1);

            // Exit here
            obs.Exit();

            // Assert exit logged.
            Assert.IsTrue(this._logger.LogLines.Count == 2);

            // Dispose of observer
            obs.Dispose();

            // Assert exit reentrancy produced no additional exit logs.
            Assert.IsTrue(this._logger.LogLines.Count == 2);
            // Clear lines
            this._logger.LogLines.Clear();
        }

        [TestMethod]
        public void Exception_exits_observer_block()
        {
            Exception caught = null;
            try
            {
                using (TraceObserver obs = new TraceObserver(MethodBase.GetCurrentMethod(), DEFAULT_LOGGER.Log()))
                {
                    // Do some processing...

                    // Assert entry logged
                    Assert.IsTrue(this._logger.LogLines.Count == 1);

                    // Throw error
                    throw new ArgumentException();
                }
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert exception occurred
            Assert.IsTrue(caught != null);

            // Assert exit logged.
            Assert.IsTrue(this._logger.LogLines.Count == 2);
            // Clear lines
            this._logger.LogLines.Clear();
        }

        [TestMethod]
        public void Provide_custom_state_to_observer_block()
        {
            int count = 0;
            bool initialized = false;
            string init_label = "initialized";
            string count_label = "count";

            using (TraceObserver obs = new TraceObserver(MethodBase.GetCurrentMethod(), DEFAULT_LOGGER.Log(), () =>
                {
                    return string.Format("{0}={1}, {2}={3}", init_label, initialized, count_label, count);
                }))
            {
                // Assert entry logged
                Assert.IsTrue(this._logger.LogLines.Count == 1);
                string expr = this._logger.LogLines[0];
                Assert.AreEqual(expr.Left(init_label.Length), init_label);
                Assert.AreEqual(bool.Parse(expr.Mid(init_label.Length + 1, (expr.IndexOf(",") - (init_label.Length + 1)))), false);

                // Change values
                if (!initialized)
                {
                    initialized = true;
                    for (int i = 0; i < 100; i++)
                        count++;
                }
            }

            // Assert exit logged.
            Assert.IsTrue(this._logger.LogLines.Count == 2);
            string expr2 = this._logger.LogLines[1];
            Assert.AreEqual(expr2.Left(init_label.Length), init_label);
            Assert.AreEqual(bool.Parse(expr2.Mid(init_label.Length + 1, (expr2.IndexOf(",") - (init_label.Length + 1)))), true);
            // Clear lines
            this._logger.LogLines.Clear();
        }
    }
}
