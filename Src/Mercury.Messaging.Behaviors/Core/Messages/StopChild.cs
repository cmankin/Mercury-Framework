using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a message to a supervisor to stop the specified child.
    /// </summary>
    public class StopChild
    {
        /// <summary>
        /// Initializes a default instance of the StopChild class.
        /// </summary>
        public StopChild()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the StopChild 
        /// class with the specified internal child name.
        /// </summary>
        /// <param name="childName">The internal name of the child to stop.</param>
        public StopChild(string childName)
        {
            this.ChildName = childName;
        }

        /// <summary>
        /// Gets the internal name of the child to stop.
        /// </summary>
        public string ChildName { get; set; }
    }
}
