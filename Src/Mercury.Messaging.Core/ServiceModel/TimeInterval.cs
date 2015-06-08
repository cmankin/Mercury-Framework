using System;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// Describes an interval of time.
    /// </summary>
    public enum TimeInterval
    {
        /// <summary>
        /// The interval is in days.
        /// </summary>
        Days=0x01,

        /// <summary>
        /// The interval is in hours.
        /// </summary>
        Hours=0x02,

        /// <summary>
        /// The interval is in minutes.
        /// </summary>
        Minutes=0x03,

        /// <summary>
        /// The interval is in seconds.
        /// </summary>
        Seconds=0x04,

        /// <summary>
        /// The interval is in milliseconds.
        /// </summary>
        Milliseconds=0x05
    }
}
