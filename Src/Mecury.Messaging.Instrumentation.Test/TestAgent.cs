using System;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Mercury.Messaging.Core;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Instrumentation.Test
{
    public class TestAgent
        : Agent
    {
        public TestAgent(AgentPort port, Action<ContextInfo> logger)
        {
            this.Port = port;
            this.Logger = logger;

            port.Receive<TestMessage>((msg) =>
                {
                    RoutingContext ctx = null;
                    ContextInfo ci = null;
                    switch (msg.Mode)
                    {
                        case 0:
                            // Log an agent context
                            ctx = new RoutingContext(msg, null, false);
                            ci = ContextInfo.NewInfo(ctx, port, port.Environment, MethodBase.GetCurrentMethod(), null, null, "A simple test message");
                            this.Logger.Invoke(ci);
                            break;
                        case 1:
                            // Log channel message
                            ctx = new RoutingContext(msg, ChannelFactory.Create<LocalRefChannel>(new object[] { null }), false);
                            ci = ContextInfo.NewInfo(ctx, null, null, MethodBase.GetCurrentMethod(), null, null, "A simple test message");
                            this.Logger.Invoke(ci);
                            break;
                        case 2:
                            // Log remote channel message
                            IPAddress ip = new IPAddress(new byte[] { 255, 255, 255, 0 });
                            ctx = new RoutingContext(msg, ChannelFactory.Create<LocalRefChannel>(new object[] { null }), false);
                            ci = ContextInfo.NewInfo(ctx, null, null, MethodBase.GetCurrentMethod(), new IPEndPoint(ip, 1200), Guid.NewGuid().ToString(), "A simple test message");
                            this.Logger.Invoke(ci);
                            break;
                    }
                });
        }

        private AgentPort Port;
        private Action<ContextInfo> Logger;
    }
}
