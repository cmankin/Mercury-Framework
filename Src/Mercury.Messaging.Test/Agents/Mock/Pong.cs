using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Test.Agents.Mock
{
    class Pong
    {
        public Pong(string instance)
        {
            this.Instance = instance;
        }

        public string Instance { get; private set; }
    }
}
