using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class AddChild
    {
        public AddChild(Type agentType, string withId)
        {
            this.AgentType = agentType;
            this.WithId = withId;
        }

        public Type AgentType { get; private set; }

        public string WithId { get; private set; }
    }
}
