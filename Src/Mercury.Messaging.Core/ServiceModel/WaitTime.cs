using System;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// Specifies an interval of time to wait for a message.
    /// </summary>
    public class WaitTime
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the WaitTime class.
        /// </summary>
        public WaitTime()
            : this(0, TimeInterval.Milliseconds)
        {
        }

        /// <summary>
        /// Initializes a default instance of the WaitTime class with the specified wait.
        /// </summary>
        /// <param name="wait">The number of milliseconds to wait.</param>
        public WaitTime(TimeSpan wait)
            : this((int)wait.TotalMilliseconds, TimeInterval.Milliseconds)
        {
        }

        /// <summary>
        /// Initializes a default instance of the WaitTime class with the specified wait and interval.
        /// </summary>
        /// <param name="wait">The number of units to wait according to the specified time interval.</param>
        /// <param name="interval">The time interval to wait.</param>
        public WaitTime(int wait, TimeInterval interval)
        {
            this._wait = wait;
            this._interval = interval;
        }
        #endregion
        
        private int _wait;

        /// <summary>
        /// Gets the interval to wait.
        /// </summary>
        public int Wait
        {
            get { return this._wait; }
        }

        private TimeInterval _interval;

        /// <summary>
        /// Gets the interval type.
        /// </summary>
        public TimeInterval Interval
        {
            get { return this._interval; }
        }

        /// <summary>
        /// Gets the message framing record for a wait time message.
        /// </summary>
        public byte[] Record
        {
            get { return GetWaitRecord(); }
        }

        /// <summary>
        /// Gets the total number of milliseconds described by this wait time.
        /// </summary>
        public double TotalMilliseconds
        {
            get
            {
                switch (this.Interval)
                {
                    case (TimeInterval.Seconds):
                        return (this.Wait * 1000);
                    case (TimeInterval.Minutes):
                        return (this.Wait * 60 * 1000);
                    case (TimeInterval.Hours):
                        return (this.Wait * 3600 * 1000);
                    case (TimeInterval.Days):
                        return (this.Wait * 86400 * 1000);
                    default:
                        return this.Wait;
                }
            }
        }

        /// <summary>
        /// Returns a wait time record.
        /// </summary>
        /// <returns>A wait time record.</returns>
        public byte[] GetWaitRecord()
        {
            return RuntimePacketProtocol.GetPacketWaitTime(this.Wait, this.Interval);
        }

        /// <summary>
        /// Creates a wait time instance from the specified wait time record.
        /// </summary>
        /// <param name="waitTime">The serialized wait time record to use.</param>
        /// <returns>A wait time instance constructed from the specified wait time record.</returns>
        public static WaitTime Create(byte[] waitTime)
        {
            if (waitTime == null || waitTime.Length == 0 || waitTime[0] != 0x0a)
                return null;

            // set interval
            TimeInterval interval = (TimeInterval)waitTime[1];
            // set time
            int wait = BitConverter.ToInt32(waitTime, 2);

            return new WaitTime(wait, interval);
        }
    }
}
