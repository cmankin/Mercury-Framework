using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class Paddle : Agent
    {
        public Paddle(AgentPort port)
        {
            port.Receive<Ping>((msg) => 
                {
                    if (this.Count < 3)
                    {
                        IncrementCount();
                        port.Environment.RoutingEngine.Send<Pong>(msg.Instance, new Pong(port.Id));
                    }
                    else
                    {
                        port.Environment.RoutingEngine.Send<PongFinal>(msg.Instance, new PongFinal());
                        port.Environment.Kill(port.Id);
                    }
                });

            port.Receive<Pong>((msg) => 
                {
                    port.Environment.RoutingEngine.Send<Ping>(msg.Instance, new Ping(port.Id));
                });

            port.Receive<PongFinal>((msg) => 
                {
                    port.Environment.Kill(port.Id);
                });
        }

        private int _count;
        public int Count 
        { 
            get { return this._count; } 
        }

        protected void IncrementCount()
        {
            System.Threading.Interlocked.Add(ref this._count, 1);
        }
    }
}
