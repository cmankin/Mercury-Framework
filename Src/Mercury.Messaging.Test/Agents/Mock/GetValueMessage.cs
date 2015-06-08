using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class GetValueMessage
    {
        public GetValueMessage()
            : this(TimeSpan.Zero)
        {
        }

        public GetValueMessage(TimeSpan wait)
        {
            this.Wait = wait;
        }

        public bool IsWaitSet
        {
            get
            {
                if (this.Wait != TimeSpan.Zero)
                    return true;
                return false;
            }
        }

        public TimeSpan Wait { get; protected set; }
    }
}
