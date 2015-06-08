using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class InterruptAgent : Agent
    {
        public InterruptAgent(AgentPort port)
        {
            port.Receive<Ping>((msg) => 
                {
                    Thread.Sleep(2000);
                    this.NumValue++;
                });

            port.Receive<Pong>((msg) => 
                {
                    if (this.NumValue != 1)
                        throw new ArgumentException();
                    this.NumValue++;
                });

            port.Receive<Request<GetValueMessage>>((msg) => 
                {
                    if (msg.Body.IsWaitSet)
                        Thread.Sleep(msg.Body.Wait);
                    msg.ResponseChannel.Send<int>(this.NumValue);
                });

            port.Receive<PrepInterrupt>((msg) => 
                {
                    LocalRef agent = port.Environment.GetRef(port.Id);
                    agent.Send<Interrupt>(
                        new Interrupt(
                            new RoutingContext<AddMessage, IUntypedChannel>(new AddMessage(5.0), null, false)));
                    Thread.Sleep(2000);
                });

            port.Receive<AddMessage>((msg) => 
                {
                    this.NumValue += Convert.ToInt32(msg.AddValue);
                });
        }

        public int NumValue = 0;
    }
}
