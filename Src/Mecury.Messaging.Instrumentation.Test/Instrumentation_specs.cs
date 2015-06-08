using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Instrumentation;
using Mercury.Messaging.Serialization;
using Mercury.Instrumentation;

namespace Mercury.Messaging.Instrumentation.Test
{
    /// <summary>
    /// Summary description for Instrumentation_specs
    /// </summary>
    [TestClass]
    public class Instrumentation_specs
    {
        public Instrumentation_specs()
        {
        }

        private List<string> _localDebugLog = new List<string>();
        private List<string> _localErrorLog = new List<string>();
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

        [TestMethod]
        public void Can_create_context_info()
        {
            IList<ContextInfo> log = new List<ContextInfo>();
            RuntimeEnvironment env = new RuntimeEnvironment("env");

            bool flag = true;
            Action<ContextInfo> action = new Action<ContextInfo>((ci) =>
                {
                    log.Add(ci);
                    flag = false;
                });
            LocalRef agent = env.Spawn<TestAgent>(action);

            agent.Send<TestMessage>(new TestMessage("hello", 0));
            while (flag)
            {
                Thread.Sleep(250);
            }

            Assert.IsTrue(log.Count == 1);
            ContextInfo current = log[0];
            Assert.IsNotNull(current);
        }

        [TestMethod]
        public void Can_serialize_context_info()
        {
            ContextInfo ci = GetContextInfo(0);
            Assert.IsNotNull(ci);
            ci.Fault = new Messages.Fault(new ArgumentNullException("instance"));
            
            // Capture state
            int pid = ci.ProcessId;
            int tid = ci.ThreadId;
            string route = ci.MessageInfo.RouteId;
            string message = ci.Message;
            bool sync = ci.MessageInfo.IsSynchronous;
            Type actual = ci.MessageInfo.ActualType;
            int resCount = ci.EnvironmentInfo.ResourceCount;
            string addr = ci.EnvironmentInfo.Address;
            string resId = ci.ResourceInfo.Id;
            
            // Serialize
            string xml = RuntimeSerializer.Serialize(ci);
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            // Deserialize
            ContextInfo deserialized = RuntimeSerializer.Deserialize<ContextInfo>(xml);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(pid == ci.ProcessId);
            Assert.IsTrue(tid == ci.ThreadId);
            Assert.IsTrue(route == ci.MessageInfo.RouteId);
            Assert.IsTrue(message == ci.Message);
            Assert.IsTrue(sync == ci.MessageInfo.IsSynchronous);
            Assert.IsTrue(actual == ci.MessageInfo.ActualType);
            Assert.IsTrue(resCount == ci.EnvironmentInfo.ResourceCount);
            Assert.IsTrue(addr == ci.EnvironmentInfo.Address);
            Assert.IsTrue(resId == ci.ResourceInfo.Id);
        }

        private static ContextInfo GetContextInfo(int mode)
        {
            ContextInfo context = null;
            RuntimeEnvironment env = new RuntimeEnvironment("env");

            bool flag = true;
            Action<ContextInfo> action = new Action<ContextInfo>((ci) =>
            {
                context = ci;
                flag = false;
            });
            LocalRef agent = env.Spawn<TestAgent>(action);

            agent.Send<TestMessage>(new TestMessage("hello", mode));
            while (flag)
            {
                Thread.Sleep(250);
            }
            return context;
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

        
        //[TestMethod]
        //[HostType("Moles")]
        //public void Verify_instrumentation_initialization()
        //{
        //    // Setup log
        //    this.ClearInternalLogs();
        //    InitializeMoles();

        //    // Pre-Initialize
        //    MessagingCoreInstrumentation.Initialize("", true);
        //    MessagingCoreInstrumentation.IsEnabled = true;

        //    // Get new runtime environment
        //    RuntimeEnvironment env = new RuntimeEnvironment("env");

        //    Assert.IsNotNull(env);
        //    Assert.IsNotNull(MessagingCoreInstrumentation.Log);
        //    Assert.IsInstanceOfType(MessagingCoreInstrumentation.Log, typeof(Mercury.Logging.DiagnosticsLog));
        //}

        //[TestMethod]
        //[HostType("Moles")]
        //public void Log_with_default_settings()
        //{
        //    // Setup log
        //    this.ClearInternalLogs();
        //    InitializeMoles();

        //    // Initialize instrumentation
        //    MessagingCoreInstrumentation.Initialize("", true);
        //    MessagingCoreInstrumentation.ErrorEnabled = true;
        //    MessagingCoreInstrumentation.SwitchLevel = System.Diagnostics.SourceLevels.All;
        //    Assert.IsNotNull(MessagingCoreInstrumentation.Log);

        //    MessagingCoreInstrumentation.Log.Info("test info message");
        //    MessagingCoreInstrumentation.Log.Error("Error occurred: {0}", new ArgumentException().ToString());
        //    Assert.IsTrue(this._localDebugLog.Count == 0);
        //    Assert.IsTrue(this._localErrorLog.Count == 1);
        //}

        //[TestMethod]
        //[HostType("Moles")]
        //public void Verify_observer_format()
        //{
        //    // Setup log
        //    this.ClearInternalLogs();
        //    InitializeMoles();
            
        //    MessagingCoreInstrumentation.Initialize("", true);
        //    MessagingCoreInstrumentation.ErrorEnabled = true;
        //    MessagingCoreInstrumentation.DebugEnabled = true;
        //    MessagingCoreInstrumentation.SwitchLevel = System.Diagnostics.SourceLevels.All;

        //    int i=0;
        //    using (MessagingCoreObserver obs = new MessagingCoreObserver(MethodBase.GetCurrentMethod(), () => 
        //        {
        //            return string.Format("State:[{0}={1}]", "i", i);
        //        }))
        //    {
        //        i = 23;
        //    }

            
        //    Assert.IsTrue(this._localDebugLog.Count == 2);
        //    Assert.IsTrue(this._localErrorLog.Count == 0);
        //}

        //private void ClearInternalLogs()
        //{
        //    this._localDebugLog.Clear();
        //    this._localErrorLog.Clear();
        //}

        //[HostType("Moles")]
        //private void InitializeMoles()
        //{
        //    MInstrumentationUtil.EnsurePathString = InstrumentationUtil_EnsurePath;
        //    MTextWriterTraceListener.AllInstances.WriterGet = (l) => { return new System.IO.StringWriter(); };
        //    MTextWriterTraceListener.AllInstances.WriteLineString = TextWriterTraceListener_WriteLineString;
        //    MTextWriterTraceListener.AllInstances.WriteString = TextWriterTraceListener_WriteString;
        //    MTextWriterTraceListener.AllInstances.Flush = (listener) =>
        //        {
        //            // Do nothing
        //        };
        //}

        //private string InstrumentationUtil_EnsurePath(string path)
        //{
        //    return InstrumentationUtil.GetExpandedFilePath(path);
        //}

        //private void TextWriterTraceListener_WriteLineString(System.Diagnostics.TextWriterTraceListener listener, string text)
        //{
        //    System.Diagnostics.SourceLevels level = ((System.Diagnostics.EventTypeFilter)listener.Filter).EventType;
        //    if (level == System.Diagnostics.SourceLevels.Error)
        //    {
        //        this.WriteLog(this._writeErrorIndex, this._localErrorLog, text);
        //        this._writeErrorIndex++;
        //    }
        //    else
        //    {
        //        this.WriteLog(this._writeDebugIndex, this._localDebugLog, text);
        //        this._writeDebugIndex++;
        //    }
        //}

        //private void TextWriterTraceListener_WriteString(System.Diagnostics.TextWriterTraceListener listener, string text)
        //{
        //    // Do nothing
        //    System.Diagnostics.SourceLevels level = ((System.Diagnostics.EventTypeFilter)listener.Filter).EventType;
        //    if (level == System.Diagnostics.SourceLevels.Error)
        //        this.WriteLog(this._writeErrorIndex, this._localErrorLog, text);
        //    else
        //        this.WriteLog(this._writeDebugIndex, this._localDebugLog, text);
        //}

        //private int _writeErrorIndex = 0;
        //private int _writeDebugIndex = 0;

        //private void WriteLog(int index, IList<string> log, string text)
        //{
        //    if (index == log.Count)
        //        log.Add(text);
        //    else
        //        log[index] += text;
        //}
    }
}
