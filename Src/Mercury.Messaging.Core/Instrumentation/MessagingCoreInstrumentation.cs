using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using Mercury.Logging;
using Mercury.Messaging.Serialization;
using Mercury.Messaging.ServiceModel;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Mercury.Net;
using Microsoft.Ccr.Core;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// A set of functions to access and modify the messaging core instrumentation.
    /// </summary>
    public static class MessagingCoreInstrumentation
    {
        private const string _logName = "MessagingCore";
        private static ILog _log;
        private static CultureInfo culture = CultureInfo.InvariantCulture;
        private static DispatcherQueue loggingTasks;
        
        /// <summary>
        /// Gets the messaging core logger.
        /// </summary>
        public static ILog Log
        {
            get { return _log; }
        }

        private static bool _initialized;

        /// <summary>
        /// Initializes the messaging core instrumentation.
        /// </summary>
        /// <param name="environmentName">The name of the environment on which to initialize instrumentation.</param>
        /// <param name="disp">The environment dispatcher to use for handling logging tasks.</param>
        /// <param name="forceInitialize">Optional. A value indicating whether to force initialization.</param>
        public static void Initialize(string environmentName, Dispatcher disp, bool forceInitialize = false)
        {
            if (!_initialized || forceInitialize)
            {
                // Setup signal event
                _signal = new ManualResetEvent(false);

                // Setup log
                _log = new DiagnosticsLog();
                string setName = _logName;
                if (!string.IsNullOrEmpty(environmentName))
                    setName = string.Format(culture, "{0}[{1}]", setName, environmentName);
                _log.Initialize(setName);

                // Set logging task queue
                MessagingCoreInstrumentation.loggingTasks = new DispatcherQueue("logging-tasks", disp);
                MessagingCoreInstrumentation.BootstrapLogDispatchers();

                // Initialize logger
                TraceSource current = MessagingCoreInstrumentation.CurrentTrace;
                if (current != null)
                {
                    current.Switch = new SourceSwitch(string.Format(culture, "{0}-switch", current.Name));
                    current.Switch.Level = SourceLevels.All;
                    MessagingCoreInstrumentation.RebuildDefaultListeners(current);
                }

                _initialized = true;
            }
        }

        /// <summary>
        /// Flushes any waiting log messages.  Will attempt flush until complete.
        /// </summary>
        public static void Flush()
        {
            MessagingCoreInstrumentation.Flush(TimeSpan.FromMilliseconds(-1.0));
        }

        private static ManualResetEvent _signal;
        
        /// <summary>
        /// Flushes any waiting log messages until the specified timeout period elapses.
        /// </summary>
        /// <param name="timeout">A TimeSpan representing the period to wait for the flush operation to finish.</param>
        public static void Flush(TimeSpan timeout)
        {
            if (_signal != null && !_signal.SafeWaitHandle.IsClosed)
            {
                _signal.Reset();
                // post empty record
                _loggerPort.Post(new System.Tuple<bool, int, string, object[]>(true, 0, null, null));
                _signal.WaitOne(timeout);
            }
        }

        /// <summary>
        /// Enables the instrumentation to be re-initialized by another runtime environment.
        /// </summary>
        public static void Unset()
        {
            _initialized = false;
            if (MessagingCoreInstrumentation.loggingTasks != null)
                MessagingCoreInstrumentation.loggingTasks.Dispose();
            if (_signal != null)
                _signal.Dispose();
            _signal = null;
        }

        #region Properties
        /// <summary>
        /// Gets the current trace source for the default logger.
        /// </summary>
        private static TraceSource CurrentTrace
        {
            get
            {
                DiagnosticsLog current = MessagingCoreInstrumentation.Log as DiagnosticsLog;
                return current != null ? current.Trace : null;
            }
        }

        /// <summary>
        /// Gets or sets the source level for the default logger switch.
        /// </summary>
        public static SourceLevels SwitchLevel
        {
            get { return MessagingCoreInstrumentation.CurrentTrace != null ? MessagingCoreInstrumentation.CurrentTrace.Switch.Level : SourceLevels.Off; }
            set
            {
                if (MessagingCoreInstrumentation.SwitchLevel != value)
                {
                    TraceSource current = MessagingCoreInstrumentation.CurrentTrace;
                    if (current != null)
                        current.Switch.Level = value;
                }
            }
        }

        private static LogSource _logSource;

        /// <summary>
        /// Gets or sets the source information of the log.
        /// </summary>
        public static LogSource LogSource
        {
            get { return _logSource; }
            set
            {
                if (_logSource != value)
                {
                    // Set value
                    _logSource = value;

                    // Rebuild listeners on core logger
                    MessagingCoreInstrumentation.RebuildDefaultListeners(MessagingCoreInstrumentation.CurrentTrace);
                }
            }
        }

        private static InstrumentationMode _mode = InstrumentationMode.None;

        /// <summary>
        /// Gets or sets the operating mode for the instrumentation.
        /// </summary>
        public static InstrumentationMode Mode
        {
            get { return _mode; }
            set
            {
                if (MessagingCoreInstrumentation._mode != value)
                {
                    _mode = value;

                    // Rebuild listeners on core logger
                    MessagingCoreInstrumentation.RebuildDefaultListeners(MessagingCoreInstrumentation.CurrentTrace);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether environment context information 
        /// will capture a snapshot of the current running resources.
        /// </summary>
        public static bool SnapshotResources { get; set; }

        /// <summary>
        /// Gets or sets the IP end point to which monitoring information will be sent.
        /// </summary>
        public static IPEndPoint MonitorEndPoint { get; set; }


        #endregion

        #region Internal Methods
        private static string _newLineTab;

        /// <summary>
        /// Gets a string representing a single tab indented new line.
        /// </summary>
        internal static string NewLineTab
        {
            get
            {
                if (string.IsNullOrEmpty(_newLineTab))
                    _newLineTab = System.Environment.NewLine.AppendTab();
                return _newLineTab;
            }
        }

        internal static int GetCurrentThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        internal static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        internal static ContextInfo TryGetContext(MethodBase methodContext, IRoutingContext routeContext, IResource resource, RuntimeEnvironment environment, IPEndPoint receiverEP, string receiverId, string message)
        {
            if (Mode != InstrumentationMode.None)
                return ContextInfo.NewInfo(routeContext, resource, environment, methodContext, receiverEP, receiverId, message,
                    MessagingCoreInstrumentation.GetCurrentProcessId(), MessagingCoreInstrumentation.GetCurrentThreadId());
            return null;
        }

        internal static void TraceError(int eventId, Fault fault, ContextInfo info, string message)
        {
            MessagingCoreInstrumentation.TryMonitor(info);
            if (fault != null)
            {
                if (!string.IsNullOrEmpty(message) && info != null)
                    MessagingCoreInstrumentation.TraceError(eventId, "Fault occurred: {1}{0}{2}{0}Context: {3}", Environment.NewLine, message, fault, info);
                else if (!string.IsNullOrEmpty(message))
                    MessagingCoreInstrumentation.TraceError(eventId, "Fault occurred: {1}{0}{2}", Environment.NewLine, message, fault);
                else
                    MessagingCoreInstrumentation.TraceError(eventId, "Fault occurred: {0}{1}", Environment.NewLine, fault);
            }
        }

        internal static void TraceIfEnabled(ContextInfo info)
        {
            MessagingCoreInstrumentation.TryMonitor(info);
            if (info != null)
                MessagingCoreInstrumentation.TraceIfEnabled("{0}", info);
        }

        private static void TraceIfEnabled(string format, params object[] args)
        {
            if (Mode == InstrumentationMode.Debug || Mode == InstrumentationMode.MonitorDebug)
                _loggerPort.Post(new System.Tuple<bool, int, string, object[]>(false, 0, format, args));
        }

        private static void TraceError(int eventId, string format, params object[] args)
        {
            if (Mode != InstrumentationMode.None && Mode != InstrumentationMode.MonitorOnly)
                _loggerPort.Post(new System.Tuple<bool, int, string, object[]>(true, eventId, format, args));
        }

        private static void TryMonitor(ContextInfo context)
        {
            if (Mode == InstrumentationMode.MonitorOnly || Mode == InstrumentationMode.MonitorDebug)
                _monitorPort.Post(context);
        }
        #endregion

        #region Helpers
        private static void RebuildDefaultListeners(TraceSource ts)
        {
            if (ts != null)
            {
                // Remove old listeners
                ts.Listeners.Clear();

                // Create log
                if (Mode != InstrumentationMode.None && Mode != InstrumentationMode.MonitorOnly)
                {
                    var logListener = MessagingCoreInstrumentation.NewSourceListener(MessagingCoreInstrumentation.LogSource, string.Format(culture, "{0}-output", ts.Name));
                    if (logListener != null)
                    {
                        if (Mode == InstrumentationMode.Debug || Mode == InstrumentationMode.MonitorDebug)
                            logListener.Filter = new EventTypeFilter(SourceLevels.All);
                        else if (Mode == InstrumentationMode.ErrorOnly)
                            logListener.Filter = new EventTypeFilter(SourceLevels.Error);
                        logListener.TraceOutputOptions = TraceOptions.None;
                        ts.Listeners.Add(logListener);
                    }
                }
            }
        }

        private static TraceListener NewSourceListener(LogSource logSource, string logName)
        {
            switch (logSource.OutputSource)
            {
                case TraceOutputSource.WindowsEventLog:
                    return new EventLogTraceListener(logName);
                case TraceOutputSource.File:
                    if (!string.IsNullOrEmpty(LogSource.Connection))
                        return new CoreLogTextWriter(string.Format(culture, @"{0}", logSource.Connection), logName);
                    break;
                case TraceOutputSource.Sql:
                    break;
            }
            return null;
        }
        #endregion

        #region Log
        private static Port<ContextInfo> _monitorPort;
        private static Port<System.Tuple<bool, int, string, object[]>> _loggerPort;

        private static void BootstrapLogDispatchers()
        {
            // Setup monitor port
            _monitorPort = new Port<ContextInfo>();
            AwaitMonitorQueue();

            // Setup logger port
            _loggerPort = new Port<System.Tuple<bool, int, string, object[]>>();
            AwaitLoggerQueue();
        }

        private static void LoggerHandler(System.Tuple<bool, int, string, object[]> info)
        {
            if (info == null)
            {
                AwaitLoggerQueue();
                return;
            }
            if (info.Item1 == true && info.Item2 == 0 && info.Item3 == null && info.Item4 == null)
            {
                _signal.Set();
                AwaitLoggerQueue();
                return;
            }

            // If logging an error...
            if (info.Item1)
            {
                if (Mode != InstrumentationMode.None && Mode != InstrumentationMode.MonitorOnly)
                    MessagingCoreInstrumentation.Log.Error(info.Item2, info.Item3, info.Item4);
            }
            else    // Informational log
            {
                if (Mode == InstrumentationMode.Debug || Mode == InstrumentationMode.MonitorDebug)
                    MessagingCoreInstrumentation.Log.Info(info.Item2, info.Item3, info.Item4);
            }

            if (!loggingTasks.IsDisposed)
                AwaitLoggerQueue();
        }

        private static void MonitorHandler(ContextInfo context)
        {
            if (context == null || MessagingCoreInstrumentation.MonitorEndPoint == null)
                return;

            try
            {
                if (Mode == InstrumentationMode.MonitorOnly || Mode == InstrumentationMode.MonitorDebug)
                {
                    // Get xml
                    string xml = RuntimeSerializer.Serialize(context);

                    // Get packet
                    byte[] packet = RuntimePacketProtocol.GetPacket(xml, -1);

                    // Send
                    using (Socket socket = TcpSockets.Connect(MessagingCoreInstrumentation.MonitorEndPoint))
                    {
                        if (socket != null)
                        {
                            // Send
                            socket.Send(packet, 0, packet.Length, SocketFlags.None);

                            // Shutdown
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                        }
                    }
                }
            }
            catch // Swallow exception: since it is for monitoring purposes only this is OK.
            {
            }

            if (!loggingTasks.IsDisposed)
                AwaitMonitorQueue();
        }

        private static void AwaitMonitorQueue()
        {
            Arbiter.Activate(loggingTasks,
                Arbiter.Receive(false, _monitorPort, MonitorHandler));
        }

        private static void AwaitLoggerQueue()
        {
            Arbiter.Activate(loggingTasks,
                Arbiter.Receive(false, _loggerPort, LoggerHandler));
        }
        #endregion
    }
}
