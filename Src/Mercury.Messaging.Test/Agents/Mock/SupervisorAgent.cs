using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using System.Collections.Concurrent;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class SupervisorAgent : Agent
    {
        public SupervisorAgent(AgentPort port)
        {
            this.Children = new ConcurrentDictionary<string, SuperContext>();
            this.ProcessChildren = new ConcurrentDictionary<string, SuperContext>();

            port.Receive<AddChild>((msg) => 
                {
                    if (msg.AgentType != null && !string.IsNullOrEmpty(msg.WithId))
                    {
                        this.TrySpawnChild(msg.AgentType, msg.WithId, port.Environment, port.Id);
                    }
                });

            port.Receive<ForwardRequest<AddMessage>>((msg) => 
                {
                    if (!string.IsNullOrEmpty(msg.ForwarderId))
                    {
                        SuperContext context = null;
                        this.Children.TryGetValue(msg.ForwarderId, out context);
                        if (context != null)
                            context.Instance.Send<AddMessage>(msg.Body);
                    }
                });

            port.Receive<ForwardRequest<Stop>>((msg) => 
                {
                    if (!string.IsNullOrEmpty(msg.ForwarderId))
                    {
                        SuperContext context = null;
                        this.Children.TryGetValue(msg.ForwarderId, out context);
                        if (context != null)
                            context.Instance.Send<Stop>(msg.Body);
                    }
                });

            port.Receive<Exit>((msg) => 
                {
                    if (!string.IsNullOrEmpty(msg.InstanceId))
                    {
                        this.TryRestart(msg.InstanceId, port.Environment, port.Id);
                    }
                });

            port.Receive<Fault>((msg) => 
                {
                    port.Shutdown(new Fault(null, msg, typeof(Fault)));
                });
        }

        protected ConcurrentDictionary<string, SuperContext> Children { get; private set; }
        protected ConcurrentDictionary<string, SuperContext> ProcessChildren { get; private set; }

        protected bool TryRestart(string pid, RuntimeEnvironment environment, string linkId)
        {
            SuperContext context = null;
            this.ProcessChildren.TryRemove(pid, out context);
            if (context != null)
            {
                SuperContext ctx;
                this.Children.TryRemove(context.AtId, out ctx);
                bool flag = this.TrySpawnChild(context.AgentType, context.AtId, environment, linkId);
                if (flag)
                    return true;
            }
            return false;
        }

        protected bool TrySpawnChild(Type agentType, string withId, RuntimeEnvironment environment, string linkId)
        {
            LocalRef reference = environment.GetRef(environment.Spawn(agentType));
            if (reference != null)
            {
                SuperContext context = new SuperContext(agentType, reference, withId);
                environment.Link(linkId, context.Instance.ResId);
                this.Children.TryAdd(withId, context);
                this.ProcessChildren.TryAdd(reference.ResId, context);
                return true;
            }
            return false;
        }
    }
}
