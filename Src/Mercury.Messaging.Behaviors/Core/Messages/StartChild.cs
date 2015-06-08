using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a message to a supervisor agent to add and 
    /// start the child described by the specification.
    /// </summary>
    public class StartChild
    {
        /// <summary>
        /// Initializes a default instance of the StartChild class.
        /// </summary>
        public StartChild()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the StartChild 
        /// class with the specified child specification.
        /// </summary>
        /// <param name="spec">The child specification to start.</param>
        public StartChild(ChildSpecification spec)
        {
            this.Specification = spec;
        }

        /// <summary>
        /// Gets the specification of the child to start.
        /// </summary>
        public ChildSpecification Specification { get; set; }
    }
}
