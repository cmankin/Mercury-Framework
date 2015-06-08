using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Messages;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public interface ForwardRequest<T> : 
        IMessage<T>,
        IMessageHeader
    {
        string ForwarderId { get; }
    }
}
