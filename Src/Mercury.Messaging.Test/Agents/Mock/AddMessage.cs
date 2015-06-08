using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class AddMessage
    {
        public AddMessage(double value)
        {
            this.AddValue = value;
        }

        public double AddValue { get; set; }
    }
}
