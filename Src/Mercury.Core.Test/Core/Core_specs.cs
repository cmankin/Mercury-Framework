using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury;

namespace Mercury.Core.Test.Core
{
    /// <summary>
    /// Summary description for Core_specs
    /// </summary>
    [TestClass]
    public class Core_specs
    {
        public Core_specs()
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
        public void Ring_cache_can_buffer()
        {
            RingCache<string> cache = new RingCache<string>(20);
            Assert.IsNotNull(cache);
            Assert.IsTrue(cache.BufferSize == 20);
            Assert.IsTrue(cache.Count == 0);

            for (int i = 0; i < 20; i++)
            {
                var key = string.Format("ID{0}", i);
                var item = string.Format("Item#{0}", i + 1);
                cache.Add(key, item);
            }

            string test;
            Assert.IsTrue(cache.Add("ID20", "Item#21", out test));
            Assert.IsNotNull(test);
            Assert.IsTrue(test == "Item#1");
        }

        [TestMethod]
        public void Ring_cache_can_buffer_continuously()
        {
            RingCache<string> cache = new RingCache<string>(20);
            for (int i = 0; i < 10000; i++)
            {
                var key = string.Format("ID{0}", i);
                var item = string.Format("Item#{0}", i + 1);
                cache.Add(key, item);
            }

            Assert.IsTrue(cache.BufferSize == 20);
            Assert.IsTrue(cache.Count == 20);
            for (int i = 9980; i < 10000; i++)
            {
                string test = cache.GetValue(string.Format("ID{0}", i));
                Assert.IsNotNull(test);
                Assert.IsTrue(test == string.Format("Item#{0}", i + 1));
            }
        }

        [TestMethod]
        public void Ring_cache_can_remove_and_compact()
        {
            RingCache<string> cache = new RingCache<string>(20);
            for (int i = 0; i < 10000; i++)
            {
                var key = string.Format("ID{0}", i);
                var item = string.Format("Item#{0}", i + 1);
                cache.Add(key, item);
            }

            Assert.IsTrue(cache.BufferSize == 20);
            Assert.IsTrue(cache.Count == 20);
            cache.Remove("ID9999");
            Assert.IsNull(cache.GetValue("ID9999"));
            cache.Remove("ID9980");
            Assert.IsNull(cache.GetValue("ID9980"));
            cache.Remove("ID9990");
            Assert.IsNull(cache.GetValue("ID9990"));
            Assert.IsTrue(cache.Count == 17);
        }

        [TestMethod]
        public void Ring_cache_dispose_handler_occurs_on_overwrite()
        {
            string key = "TestKey";
            bool disposeFlag = false;
            RingCache<DisposableItem> cache = new RingCache<DisposableItem>(2, (item) =>
                {
                    if (item != null)
                    {
                        item.Dispose();
                        disposeFlag = true;
                    }
                });

            cache.Add(key, new DisposableItem(3338));
            Assert.IsFalse(disposeFlag);
            var test = cache.GetValue(key);
            Assert.IsNotNull(test);
            Assert.IsTrue(test.Value == 3338);
            Assert.IsFalse(test.IsDisposed);
            Assert.IsFalse(disposeFlag);
            cache.Add("newItem", new DisposableItem(4495));
            Assert.IsFalse(disposeFlag);
            Assert.IsTrue(cache.Remove(key, out test));
            Assert.IsFalse(disposeFlag);
            cache.Add("key2", new DisposableItem(-3353));
            Assert.IsFalse(disposeFlag);
            cache.Add("key3", new DisposableItem(4129), out test);
            Assert.IsTrue(disposeFlag);
            Assert.IsNotNull(test);
            Assert.IsTrue(test.Value == 0);
            Assert.IsTrue(test.IsDisposed == true);
        }

        [TestMethod]
        public void Can_clear_items_from_cache()
        {
            // Clear full cache
            RingCache<string> cache = new RingCache<string>(20);
            for (int i = 0; i < 20; i++)
            {
                var key = string.Format("ID{0}", i);
                var item = string.Format("Item#{0}", i + 1);
                cache.Add(key, item);
            }

            Assert.IsTrue(cache.Count == 20);
            cache.Clear(true);

            // Assert cache was cleared
            Assert.IsTrue(cache.Count == 0);
            Assert.IsNull(cache.GetValue("ID0"));
            Assert.IsNull(cache.GetValue("ID19"));
            Assert.IsNull(cache.GetValue("ID10"));

            // Clear partial cache
            for (int i = 0; i < 10; i++)
            {
                var key = string.Format("ID{0}", i);
                var item = string.Format("Item#{0}", i + 1);
                cache.Add(key, item);
            }
            Assert.IsTrue(cache.Count == 10);
            cache.Clear(true);
            Assert.IsTrue(cache.Count == 0);
        }
    }
}
