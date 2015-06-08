using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class AddAgentMock : Agent
    {
        public AddAgentMock(AgentPort port)
        {
            port.Receive<AddMessage>((msg) => 
                {
                    if (msg.AddValue == -99999.99)
                        throw new ArgumentException("Encountered unexpected value.");
                    AddToState(msg.AddValue);
                });

            port.Receive<Request<AddMessage>>((msg) => 
                {
                    double newValue = AddToState(msg.Body.AddValue);
                    msg.Respond<AddMessage>(new AddMessage(newValue));
                });

            port.Receive<Response<AddMessage>>((msg) =>
                {
                    AddToState(msg.Body.AddValue);
                });

            port.Receive<Request<GetValueMessage>>((msg) => 
                {
                    msg.ResponseChannel.Send<double>(this.State);
                });
        }

        public double State { get; set; }

        protected double AddToState(double value)
        {
            this.State += value;
            return this.State;
        }
    }
}
