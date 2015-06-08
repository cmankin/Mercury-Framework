using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a message sent to a supervisor to permanently delete a child specification.
    /// </summary>
    public class DeleteChild
    {
        /// <summary>
        /// Initializes a default instance of the DeleteChild class.
        /// </summary>
        public DeleteChild()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the DeleteChild 
        /// class with the specified child name identifier.
        /// </summary>
        /// <param name="childName"></param>
        public DeleteChild(string childName)
        {
            this.ChildName = childName;
        }

        /// <summary>
        /// Gets the internal ID of the child to delete.
        /// </summary>
        public string ChildName { get; set; }
    }
}
