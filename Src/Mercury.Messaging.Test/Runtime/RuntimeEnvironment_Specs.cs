using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Test.Agents.Mock;

namespace Mercury.Messaging.Test.Runtime
{
    /// <summary>
    /// Summary description for RuntimeEnvironment_Specs
    /// </summary>
    [TestClass]
    public class RuntimeEnvironment_Specs
    {
        public RuntimeEnvironment_Specs()
        {
            this.environment = new RuntimeEnvironment("prometheus.org");
        }

        private TestContext testContextInstance;
        private RuntimeEnvironment environment;

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
        public void Spawn_agent()
        {
            string agentId = this.environment.Spawn(typeof(AddAgentMock));
            
            // Assert
            LocalRef reference = this.environment.GetRef(agentId);
            Assert.IsFalse(string.IsNullOrEmpty(agentId));
            Assert.IsNotNull(reference);
            Assert.IsTrue(reference.ResId == agentId);

            AgentPort port = this.environment.FindAgentPort(agentId);
            Assert.IsNotNull(port);
            Assert.IsTrue(port.Id == agentId);
        }

        [TestMethod]
        public void Kill_agent()
        {
            // Spawn new agent
            string agentId = this.environment.Spawn(typeof(AddAgentMock));

            // Assert agent spawned
            AgentPort port = this.environment.FindAgentPort(agentId);
            Assert.IsNotNull(port);
            Assert.IsTrue(port.Id == agentId);

            // Kill
            this.environment.Kill(agentId);

            // Assert agent killed
            LocalRef reference = this.environment.GetRef(agentId);
            Assert.IsNull(reference);
            AgentPort oldPort = this.environment.FindAgentPort(agentId);
            Assert.IsNull(reference);
        }
    }
}
