using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// A filter that allows or rejects everything.
    /// </summary>
    public class PassFilter
        : LogFilter
    {
        private bool _fail;

        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.PassFilter"/>.
        /// </summary>
        public PassFilter()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.PassFilter"/> with the specified fail value.
        /// </summary>
        /// <param name="fail">Determines whether to pass or fail requests.</param>
        public PassFilter(bool fail)
        {
            this._fail = fail;
        }

        /// <summary>
        /// Determines whether to pass or fail requests.
        /// </summary>
        public bool Fail
        {
            get { return this._fail; }
            set { this._fail = value; }
        }

        /// <summary>
        /// Returns a value indicating whether the specified log entry can pass the filter.
        /// </summary>
        /// <param name="entry">The log entry to test.</param>
        /// <returns>True if the specified log entry can pass the filter; otherwise, false.</returns>
        public override bool Allow(LogEntry entry)
        {
            return !this._fail;
        }
    }
}
