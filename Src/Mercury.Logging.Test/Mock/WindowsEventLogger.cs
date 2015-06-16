using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Mercury.Logging;
using Mercury.Logging.Loggers;
using Mercury.Logging.Configuration;

namespace Mercury.Logging.Test.Mock
{
    public class WindowsEventLogger : Logger, IInitializable
    {
        private EventLog evtLogger;

        public WindowsEventLogger()
        {
        }

        public WindowsEventLogger(string loggerName, string sourceName)
        {
            this.Name = loggerName;
            this.SourceName = sourceName;
            this.Init();
        }

        public string SourceName { get; set; }

        void IInitializable.Initialize()
        {
            this.Init();
        }

        protected virtual void Init()
        {
            if (string.IsNullOrEmpty(this.Name))
                throw new InvalidOperationException("Cannot create a valid Windows event logger instance without a valid logger name.");
            if (string.IsNullOrEmpty(this.SourceName))
                throw new InvalidOperationException("Cannot create a valid Windows event logger instance without a valid source name.");
            if (!EventLog.SourceExists(this.Name, this.SourceName))
                EventLog.CreateEventSource(this.SourceName, this.Name);
        }

        public override void Flush()
        {
        }

        protected override bool DoLogEntry(LogEntry entry)
        {
            try
            {
                this.evtLogger.WriteEntry(entry.Message, this.MapType(entry.Severity), entry.EventId);
                return true;
            }
            catch
            {
            }
            return false;
        }

        protected override bool WriteLog(string message)
        {
            try
            {
                this.evtLogger.WriteEntry(message, EventLogEntryType.Information, 0);
                return true;
            }
            catch
            {
            }
            return false;
        }

        private EventLogEntryType MapType(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Warning:
                    return EventLogEntryType.Warning;
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    return EventLogEntryType.Error;
                case LogSeverity.Debug:
                case LogSeverity.Info:
                default:
                    return EventLogEntryType.Information;
            }
        }
    }
}
