using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Text;

namespace Mercury.Core.Test
{
    /// <summary>
    /// Summary description for Text_specs
    /// </summary>
    [TestClass]
    public class Text_specs
    {
        public Text_specs()
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
        public void Can_generate_simple_difference()
        {
            string textA = "a,b,c,a,b,b,a";
            string textB = "c,b,a,b,a,c";

            // Replace comma with newline
            textA = textA.Replace(",", Environment.NewLine);
            textB = textB.Replace(",", Environment.NewLine);

            var results = Difference.Calculate(textA, textB, false, false, false);
            Assert.IsNotNull(results);
            
        }
    }
}
