using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;

namespace Samples.Performance
{
    public class Counter : Agent
    {
        public Counter(AgentPort port, string adminId)
        {
            RuntimeEnvironment env = port.Environment;

            port.Receive<Tuple<string, int>>((msg) =>
                {
                    if (msg.Item1 == "count")
                    {
                        for (int i = 0; i < msg.Item2; i++)
                        {
                            // Do nothing
                        }
                        
                        // Get admin
                        LocalRef adminAgent = env.GetRef(adminId);
                        if (adminAgent != null)
                            adminAgent.Send<string>(string.Empty);
                    }
                });
        }
    }
}
