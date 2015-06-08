using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Test.Runtime.Mock;
using Mercury;

namespace Mercury.Messaging.Test
{
    /// <summary>
    /// Summary description for ResourcePool_Specs
    /// </summary>
    [TestClass]
    public class ResourcePool_Specs
    {
        public ResourcePool_Specs()
        {
            this._prefix = "http://resources/";
            this._resources = new ResourcePool(this._prefix);
        }

        private TestContext testContextInstance;
        private ResourcePool _resources;
        private string _prefix;

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
        public void Add_to_resource_pool_and_get()
        {
            string rid = this._resources.Add(new MockResource());
  
            // Assert
            IResource res = this._resources.Get(rid);
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(MockResource));
            Assert.IsTrue(rid.Left(17) == this._prefix);
        }

        [TestMethod]
        public void Delete_from_resource_pool()
        {
            int count = this._resources.Count;
            string rid = this._resources.Add(new MockResource());
            this._resources.Delete(rid);

            // Assert
            IResource res = this._resources.Get(rid);
            Assert.IsNull(res);
            Assert.IsTrue(this._resources.Count == count);
        }

        [TestMethod]
        public void Allocate_max_resources_and_delete()
        {
            this._resources.Clear();
            int maxCount = 1000000;

            List<string> resIds = new List<string>();
            for (int i = 0; i < maxCount; i++)
                resIds.Add(this._resources.Add(new MockResource()));

            // Assert
            Assert.IsTrue(this._resources.Count == maxCount);

            foreach (string key in resIds)
                this._resources.Delete(key);

            // Assert
            Assert.IsTrue(this._resources.Count == 0);
        }

        [TestMethod]
        public void Exceed_max_resource_allocation()
        {
            this._resources.Clear();
            int maxCount = 1000000;

            for (int i = 0; i < maxCount; i++)
                this._resources.Add(new MockResource());

            // Assert
            Assert.IsTrue(this._resources.Count == maxCount);

            // Assert exceeding max resource allocation throws ResourceLimitException
            Exception expected=null;
            try
            {
                this._resources.Add(new MockResource());
            }
            catch (ResourceLimitException ex)
            {
                expected = ex;
            }

            if (expected == null)
                Assert.Fail("Expected ResourceLimitException on attempt to exceed maximum resource threshold.");

            this._resources.Clear();
        }

        [TestMethod]
        public void Dispose_resources()
        {
            ResourcePool testResources = new ResourcePool(this._prefix);
            for (int i = 0; i < 100000; i++)
                testResources.Add(new MockResource());

            // Get resource
            string resId = testResources.Add(new MockResource());
            MockResource res = testResources.Get(resId) as MockResource;

            // Dispose
            testResources.Dispose();

            // Assert
            Assert.IsTrue(testResources._resources.Count == 0);
            Assert.IsTrue(res.IsDisposed == true);

            // Get new resources
            testResources = new ResourcePool();
            for (int i = 0; i < 100000; i++)
                testResources.Add(new MockResource());

            // Get resource
            resId = testResources.Add(new MockResource());
            res = testResources.Get(resId) as MockResource;

            // Set to null
            testResources = null;

            // Assert
            Assert.IsTrue(res.IsDisposed == false);
        }

        //[TestMethod]
        //public void Collect_expired_resources()
        //{
        //    ResourcePool testResources = new ResourcePool(this._prefix);
        //    for (int i = 0; i < 10000; i++)
        //        testResources.Add(new MockResource());

        //    // Add extra resource
        //    string res1 = testResources.Add(new MockResource());
        //    MockResource mr = testResources.Get(res1) as MockResource;
        //    Thread.Sleep(300.Milliseconds());

        //    // Access resource and add new
        //    mr.UpdateLastAccess();
        //    string res2 = testResources.Add(new MockResource());

        //    // Collect all resources older than 250 milliseconds.
        //    testResources.Collect(250.Milliseconds());

        //    // Assert
        //    Assert.IsNotNull(testResources);
        //    Assert.IsTrue(testResources.Count == 2);
        //    IResource resource = testResources.Get(res1);
        //    Assert.IsNotNull(resource);
        //    resource = testResources.Get(res2);
        //    Assert.IsNotNull(resource);
        //}
    }
}
