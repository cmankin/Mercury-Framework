using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Instrumentation.Test
{
    public class TestMessage
    {
        public TestMessage(string message, int mode)
        {
            this.Message = message;
            this.Mode = mode;
        }

        public string Message { get; private set; }

        public int Mode { get; private set; }
    }
}
