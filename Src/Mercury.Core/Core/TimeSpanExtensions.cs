using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury
{
    /// <summary>
    /// Core extensions to the TimeSpan class.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Returns a time span for the specified number of days.
        /// </summary>
        /// <param name="value">The number of days.</param>
        /// <returns>A time span for the specified number of days.</returns>
        public static TimeSpan Days(this int value)
        {
            return new TimeSpan(value, 0, 0, 0, 0);
        }

        /// <summary>
        /// Returns a time span for the specified number of hours.
        /// </summary>
        /// <param name="value">The number of hours.</param>
        /// <returns>A time span for the specified number of hours.</returns>
        public static TimeSpan Hours(this int value)
        {
            return new TimeSpan(0, value, 0, 0, 0);
        }

        /// <summary>
        /// Returns a time span for the specified number of minutes.
        /// </summary>
        /// <param name="value">The number of minutes.</param>
        /// <returns>A time span for the specified number of minutes.</returns>
        public static TimeSpan Minutes(this int value)
        {
            return new TimeSpan(0, 0, value, 0, 0);
        }

        /// <summary>
        /// Returns a time span for the specified number of seconds.
        /// </summary>
        /// <param name="value">The number of seconds.</param>
        /// <returns>A time span for the specified number of seconds.</returns>
        public static TimeSpan Seconds(this int value)
        {
            return new TimeSpan(0, 0, 0, value, 0);
        }

        /// <summary>
        /// Returns a time span for the specified number of milliseconds.
        /// </summary>
        /// <param name="value">The number of milliseconds.</param>
        /// <returns>A time span for the specified number of milliseconds.</returns>
        public static TimeSpan Milliseconds(this int value)
        {
            return new TimeSpan(0, 0, 0, 0, value);
        }
    }
}
