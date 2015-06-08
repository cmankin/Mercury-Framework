using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Logging;

namespace Mercury.Logging.Test.Mock
{
    public class NoFormattingFormatter : LogFormatter
    {
        public override string Format(LogEntry entry)
        {
            return entry.Message;
        }
    }
}
