using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Logging;

namespace Mercury.Logging.Test.Mock
{
    public class MockEntry : LogEntry
    {
        public MockEntry(string rawMessage, object[] args)
            : base("mock", LogSeverity.Info, 99, DateTime.Now, 0L, "", null, rawMessage, args, false, false)
        {
        }
    }
}
