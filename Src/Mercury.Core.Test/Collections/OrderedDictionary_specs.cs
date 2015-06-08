using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Collections;

namespace Mercury.Core.Test.Collections
{
    /// <summary>
    /// Summary description for OrderedDictionary_specs
    /// </summary>
    [TestClass]
    public class OrderedDictionary_specs
    {
        public OrderedDictionary_specs()
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
        public void In_order_entries()
        {
            IOrderedDictionary<string, int> entries = new OrderedDictionary<string, int>();

            int max = 10000;
            for (int i = 0; i < max; i++)
                entries.Add(i.ToString(), i);

            // Assert
            int j = 0;
            foreach (int entry in entries.Values)
            {
                Assert.AreEqual(entry, j);
                j++;
            }

            Assert.AreEqual(entries["200"], 200);
        }
    }
}
