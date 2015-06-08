using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Logging;

namespace Mercury.Logging.Test.Mock
{
    public class MockMemoryLogger
        : Logger
    {
        public MockMemoryLogger(bool allowWrites)
        {
            this.AllowWrites = allowWrites;
        }

        private IList<string> _logFile = new List<string>();

        public IList<string> LogFile
        {
            get { return this._logFile; }
        }

        public bool AllowWrites { get; set; }

        protected override bool WriteLog(string message)
        {
            if (!this.AllowWrites)
                return false;
            this.LogFile.Add(message);
            return true;
        }

        public override void Flush()
        {
        }
    }
}
