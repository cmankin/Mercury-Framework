using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StubsTutorial;
using StubsTutorial.Moles;

namespace StubsTutorialTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestReaderTest
    {
        public TestReaderTest()
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
        [HostType("Moles")]
        public void CheckValidFileWithMoles_stubbed_interface()
        {
            // arrange
            var fileName = "test.txt";
            var content = "test";

            var fs = new SIFileSystem();
            fs.ReadAllTextString = delegate(string f)
                {
                    Assert.IsTrue(f == fileName);
                    return content;
                };

            // act
            var test = new TestReaderWithStubs(fs);
            test.LoadFile(fileName);
            // assert
            Assert.AreEqual(content, test.Content);
        }

        [TestMethod]
        [HostType("Moles")]
        public void CheckValidFileWithMoles_rerouted_method()
        {
            // arrange
            var fileName = "test.txt";
            var content = "test";

            // Re-route
            MFileSystem.ReadAllTextString = delegate(string f)
                {
                    Assert.IsTrue(f == fileName);
                    return content;
                };

            // act
            var content2 = FileSystem.ReadAllText(fileName);
            // assert
            Assert.AreEqual(content, content2);
        }
    }
}
