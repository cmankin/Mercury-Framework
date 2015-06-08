using System;
using Mercury.Messaging.Routing;

namespace Mercury.Messaging.Messages
{
    /// <summary>
    /// Represents an interrupt message.
    /// </summary>
    public class Interrupt
    {
        /// <summary>
        /// Initializes a default instance of the Interrupt class.
        /// </summary>
        /// <param name="context">The routing context for this interrupt.</param>
        public Interrupt(IRoutingContext context)
        {
            this._context = context;
        }

        private IRoutingContext _context;

        /// <summary>
        /// Gets the routing context for this interrupt.
        /// </summary>
        public IRoutingContext Context
        {
            get { return this._context; }
        }
    }
}
