using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// Describes the communication mode over a network.
    /// </summary>
    public enum CommunicationMode
    {
        /// <summary>
        /// The initiating stream for a single one-way message 
        /// or for a pair of messages in a request-reply 
        /// manner between two nodes.
        /// </summary>
        SingletonUnsized = 0x01,

        /// <summary>
        /// The initiating stream for multiple bi-directional 
        /// messages between two nodes.
        /// </summary>
        Duplex = 0x02,

        /// <summary>
        /// The initiating stream for multiple one-way messages 
        /// from a single source.
        /// </summary>
        Simplex = 0x03,

        /// <summary>
        /// The initiating stream for a single one-way message 
        /// from a single source.
        /// </summary>
        SingletonSized = 0x04
    }
}
