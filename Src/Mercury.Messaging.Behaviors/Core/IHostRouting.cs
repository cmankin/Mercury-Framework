using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Common interface for host routing tables.
    /// </summary>
    public interface IHostRouting
    {
        /// <summary>
        /// Returns an agent reference if the specified agent address is found.
        /// </summary>
        /// <param name="address">The address of the agent to find.</param>
        /// <returns>An agent reference or null.</returns>
        LocalRef TryFind(string address);
    }
}
