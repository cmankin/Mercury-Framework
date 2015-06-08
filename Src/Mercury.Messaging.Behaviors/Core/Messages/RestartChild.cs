using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// A message sent to a supervisor to restart a child agent.
    /// </summary>
    public class RestartChild
    {
        /// <summary>
        /// Initializes a default instance of the RestartChild class.
        /// </summary>
        public RestartChild()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the RestartChild 
        /// class with the specified internal child name.
        /// </summary>
        /// <param name="childName">The internal name of the child to restart.</param>
        public RestartChild(string childName)
        {
            this.ChildName = childName;
        }

        /// <summary>
        /// Gets the internal child name.
        /// </summary>
        public string ChildName { get; set; }
    }
}
