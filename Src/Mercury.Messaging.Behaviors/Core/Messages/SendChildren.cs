using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// A message from a supervisor containing an array of child info objects for all of its children.
    /// </summary>
    public class SendChildren
    {
        /// <summary>
        /// Initializes a default instance of the SendChildren class.
        /// </summary>
        public SendChildren()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SendChildren 
        /// class with the specified children.
        /// </summary>
        /// <param name="children">An array of ChildInfo objects describing the supervisor children.</param>
        public SendChildren(ChildInfo[] children)
        {
            this.Children = children;
        }

        /// <summary>
        /// Gets an array of child info objects describing all child agents of a supervisor.
        /// </summary>
        public ChildInfo[] Children { get; set; }
    }
}
