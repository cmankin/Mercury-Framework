using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// Describes the severity of a logged message.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Debugging information.
        /// </summary>
        Debug,

        /// <summary>
        /// Informational messages, possibly unrelated to debug information.
        /// </summary>
        Info,

        /// <summary>
        /// An alert message that does not constitute an error.
        /// </summary>
        Warning,

        /// <summary>
        /// An error message.
        /// </summary>
        Error,

        /// <summary>
        /// Critical application error.
        /// </summary>
        Critical
    }
}
