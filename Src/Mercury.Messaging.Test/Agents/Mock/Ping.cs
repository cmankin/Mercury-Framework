using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Test.Agents.Mock
{
    class Ping
    {
        public Ping(string instance)
        {
            this.Instance = instance;
        }

        public string Instance { get; private set; }
    }
}
