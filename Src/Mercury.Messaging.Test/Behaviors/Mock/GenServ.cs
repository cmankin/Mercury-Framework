using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Behaviors;
using Mercury.Messaging.Behaviors.IPC2;

namespace Mercury.Messaging.Test.Behaviors.Mock
{
    public class GenServ : GenericServer
    {
        public GenServ(AgentPort port, RetrySpecification spec)
            : base(port, spec)
        {
            this.State = "Waiting...";
            base.Receive<ServerPing>((msg) => 
                {
                    this.State = "Pinged.";
                });
        }

        public string State { get; private set; }
    }
}
