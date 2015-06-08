using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a specification for retry criteria on message delivery.
    /// </summary>
    public class RetrySpecification
    {
        /// <summary>
        /// Initializes a default instance of the RetrySpecification class with the specified values.
        /// </summary>
        /// <param name="attempts">The number of attempts allowed before the message is lost.</param>
        /// <param name="waitInterval">The wait interval between resend attempts.</param>
        public RetrySpecification(int attempts, TimeSpan waitInterval)
        {
            this._attempts = attempts;
            this._waitInterval = waitInterval;
        }

        private readonly int _attempts;

        /// <summary>
        /// Gets the maximum number of attempts allowed before the message is considered lost.  
        /// If the value is set to a number less than 0, the number of retry attempts will be 
        /// unbounded.
        /// </summary>
        public int Attempts
        {
            get { return this._attempts; }
        }

        private readonly TimeSpan _waitInterval;

        /// <summary>
        /// Gets the interval of time to wait between resend attempts.
        /// </summary>
        public TimeSpan WaitInterval
        {
            get { return this._waitInterval; }
        }
    }
}
