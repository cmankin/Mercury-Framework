using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Logging;

namespace Mercury.Core.Test
{
    public class TestLogger
        : ILog, ILog<TestLogger>
    {
        public TestLogger()
        {
            this._logLines = new List<string>();
        }

        private List<string> _logLines;

        public IList<string> LogLines
        {
            get { return this._logLines; }
        }

        private string _name;
        public string Name
        {
            get { return this._name; }
        }

        public void Initialize(string logName)
        {
            this._name = logName;
        }

        public virtual void Debug(Func<string> message)
        {
            this.Debug(0, message);
        }

        public virtual void Debug(int id, Func<string> message)
        {
            this.Debug(id, message.Invoke(), new object[] { });
        }

        public virtual void Debug(string message, params object[] format)
        {
            this.Debug(0, message, format);
        }

        public virtual void Debug(int id, string message, params object[] format)
        {
            this.LogLines.Add(string.Format(message, format));
        }

        public virtual void Info(Func<string> message)
        {
            this.Info(0, message);
        }

        public virtual void Info(int id, Func<string> message)
        {
            this.Info(id, message.Invoke(), new object[] { });
        }

        public virtual void Info(string message, params object[] format)
        {
            this.Info(0, message, format);
        }

        public virtual void Info(int id, string message, params object[] format)
        {
            this.LogLines.Add(string.Format(message, format));
        }

        public virtual void Warn(Func<string> message)
        {
            this.Warn(0, message);
        }

        public virtual void Warn(int id, Func<string> message)
        {
            this.Warn(id, message.Invoke(), new object[] { });
        }

        public virtual void Warn(string message, params object[] format)
        {
            this.Warn(0, message, format);
        }

        public virtual void Warn(int id, string message, params object[] format)
        {
            this.LogLines.Add(string.Format(message, format));
        }

        public virtual void Error(Func<string> message)
        {
            this.Error(0, message);
        }

        public virtual void Error(int id, Func<string> message)
        {
            this.Error(id, message.Invoke(), new object[] { });
        }

        public virtual void Error(string message, params object[] format)
        {
            this.Error(0, message, format);
        }

        public virtual void Error(int id, string message, params object[] format)
        {
            this.LogLines.Add(string.Format(message, format));
        }

        public virtual void Critical(Func<string> message)
        {
            this.Critical(0, message);
        }

        public virtual void Critical(int id, Func<string> message)
        {
            this.Critical(id, message.Invoke(), new object[] { });
        }

        public virtual void Critical(string message, params object[] format)
        {
            this.Critical(0, message, format);
        }

        public virtual void Critical(int id, string message, params object[] format)
        {
            this.LogLines.Add(string.Format(message, format));
        }
    }
}
