using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// A set of related information for a child agent.
    /// </summary>
    /// <remarks>TODO: Support IXmlSerializable.</remarks>
    public class ChildInfo
    {
        /// <summary>
        /// Initializes a default instance of the ChildInfo class with the specified values.
        /// </summary>
        /// <param name="spec">The child specification to use.</param>
        /// <param name="id">The agent ID corresponding to the child specification.</param>
        public ChildInfo(ChildSpecification spec, string id)
        {
            this._childSpecification = spec;
            this._id = id;
        }

        private ChildSpecification _childSpecification;

        /// <summary>
        /// Gets the child specification.
        /// </summary>
        public ChildSpecification ChildSpecification
        {
            get { return this._childSpecification; }
        }

        private string _id;

        /// <summary>
        /// Gets the ID of the corresponding agent.
        /// </summary>
        public string Id
        {
            get { return this._id; }
        }
    }
}
