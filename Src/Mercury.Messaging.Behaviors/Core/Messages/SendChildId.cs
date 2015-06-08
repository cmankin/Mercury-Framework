using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a request to a supervisor to return the id of a named child.
    /// </summary>
    public class SendChildId
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the SendChildId class.
        /// </summary>
        public SendChildId()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the SendChildId class 
        /// with the specified internal name and agent ID.
        /// </summary>
        /// <param name="name">The internal name of the child.</param>
        /// <param name="id">The agent ID of the child.</param>
        public SendChildId(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }
        #endregion

        /// <summary>
        /// Gets or sets the internal child name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the agent ID.
        /// </summary>
        public string Id { get; set; }
    }
}
