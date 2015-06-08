using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class SuperContext
    {
        public SuperContext(Type agentType, LocalRef instance, string atId)
        {
            this.AgentType = agentType;
            this.Instance = instance;
            this.AtId = atId;
        }

        public Type AgentType { get; private set; }
        public LocalRef Instance { get; private set; }
        public string AtId { get; private set; }
    }
}
