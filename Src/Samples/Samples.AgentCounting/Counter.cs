using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;

namespace Samples.AgentCounting
{
    public class Counter : Agent
    {
        public Counter(AgentPort port)
        {
            port.Receive<Tuple<string, int>>((msg) =>
                {
                    if (msg.Item1 == "count")
                    {
                        for (int i = 0; i < msg.Item2; i++)
                        {
                            // Do nothing
                        }
                        port.Shutdown(null);
                    }
                });
        }
    }
}
