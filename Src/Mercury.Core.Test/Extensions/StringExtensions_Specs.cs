using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Common;

namespace Mercury.Common.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class StringExtensions_Specs
    {
        public StringExtensions_Specs()
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
        public void Left_extension_spec()
        {
            string value = "left function test";
            string result = value.Left(13);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result == "left function");

            // Assert below minimum length results in exception
            Exception expected = null;
            try
            {
                result = value.Left(-1);
            }
            catch (ArgumentException ex)
            {
                expected = ex;
            }

            if (expected == null)
                Assert.Fail("Expected argument exception on sub-minimum length did not occur.");
            
            // Assert above max length results in original string.
            try
            {
                result = value.Left(23);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result == value);
        }

        [TestMethod]
        public void Right_extension_spec()
        {
            string value = "test right function";
            string result = value.Right(14);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result == "right function");

            // Assert below minimum length results in exception
            Exception expected = null;
            try
            {
                result = value.Right(-1);
            }
            catch (ArgumentException ex)
            {
                expected = ex;
            }

            if (expected == null)
                Assert.Fail("Expected argument exception on sub-minimum length did not occur.");

            // Assert above max length results in original string.
            try
            {
                result = value.Right(23);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result == value);
        }

        [TestMethod]
        public void Mid_extension_spec()
        {
            string value = "mid function test";
            string result = value.Mid(4, 8);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result == "function");

            // Assert start index below 0 results in exception
            Exception expected = null;
            try
            {
                result = value.Mid(-1);
            }
            catch (ArgumentException ex)
            {
                expected = ex;
            }

            if (expected == null)
                Assert.Fail("Expected argument exception on start index did not occur.");

            // Assert below minimum length results in exception
            expected = null;
            try
            {
                result = value.Mid(0, -1);
            }
            catch (ArgumentException ex)
            {
                expected = ex;
            }

            if (expected == null)
                Assert.Fail("Expected argument exception on sub-minimum length did not occur.");

            // Assert start index greater than max length results in the empty string.
            try
            {
                result = value.Mid(23);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result == string.Empty);

            // Assert length greater than max length results in the remainder of the string.
            try
            {
                result = value.Mid(4, 23);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result == "function test");
        }

        [TestMethod]
        public void ReplaceText_extension_spec()
        {
            string find = "%insertSpaceWorld%";
            string s1 = string.Format("hello{0}{0}{0}", find);
            string replacement = " world";
            string s2 = string.Empty;

            // Throw if count is less than -1
            Exception caught = null;
            try
            {
                s2 = s1.ReplaceText(find, replacement, -2);
            }
            catch (Exception ex)
            {
                caught = ex;
            }
            Assert.IsNotNull(caught);

            // Do not replace if no match is found
            s2 = s1.ReplaceText("helloworld", "null");
            Assert.AreEqual<string>(s2, s1);

            // Do not replace if count is zero
            s2 = s1.ReplaceText(find, replacement, 0);
            Assert.AreEqual<string>(s2, s1);

            // Replace 1 if count is 1;
            s2 = s1.ReplaceText(find, replacement, 1);
            Assert.AreNotEqual<string>(s2, s1);
            int rIdx = s2.IndexOf(replacement);
            Assert.IsTrue(rIdx > -1);
            int findIdx = s2.IndexOf(find);
            Assert.IsTrue(findIdx > -1);

            // Replace all
            s2 = s1.ReplaceText(find, replacement);
            Assert.AreNotEqual<string>(s2, s1);
            findIdx = -1;
            findIdx = s2.IndexOf(find);
            Assert.IsTrue(findIdx < 0);
        }

        [TestMethod]
        public void IsNumeric_extension_spec()
        {
            string hexNum = "0x1234";
            string hexChar = "0xFFF";
            string alphaNumText = "hello world 1.0";
            string doubleText = "1.234";
            string intText = "1234";

            // Hex number displayed as text returns false.
            Assert.IsFalse(hexNum.IsNumeric());

            // Hex characters displayed as text returns false.
            Assert.IsFalse(hexChar.IsNumeric());

            // Alpha numeric text returns false.
            Assert.IsFalse(alphaNumText.IsNumeric());

            // Double value as text returns true.
            Assert.IsTrue(doubleText.IsNumeric());

            // Integer value as text returns true.
            Assert.IsTrue(intText.IsNumeric());
        }
    }
}
