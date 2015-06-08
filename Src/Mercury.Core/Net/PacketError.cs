using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// Describes an error on a data packet.
    /// </summary>
    public enum PacketError
    {
        /// <summary>
        /// No error was encountered.
        /// </summary>
        None,

        /// <summary>
        /// The message format was invalid.
        /// </summary>
        InvalidMessage,

        /// <summary>
        /// The message did not contain the appropriate "end message" construct.
        /// </summary>
        UnexpectedEndOfMessage
    }
}
