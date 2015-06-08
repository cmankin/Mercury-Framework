using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Configuration
{
    /// <summary>
    /// Describes the kind of configuration element referenced.
    /// </summary>
    public enum ConfigurationKind
    {
        /// <summary>
        /// No element kind was defined.
        /// </summary>
        None,
        /// <summary>
        /// The referenced element is a configuration section.
        /// </summary>
        Section,
        /// <summary>
        /// The referenced element is a collection of configuration elements.
        /// </summary>
        CollectionElement,
        /// <summary>
        /// The referenced element is a configuration element.
        /// </summary>
        Element
    }
}
