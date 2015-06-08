using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Test.Behaviors.Mock
{
    public class AgentSink : Agent
    {
        public AgentSink(AgentPort port)
        {
            port.Receive<ServerPing>((msg) => 
                {
                    if (msg != null)
                        Interlocked.Increment(ref this.PingCount);
                });
        }

        public int PingCount;
    }
}
