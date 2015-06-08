using System;

namespace Mercury.Messaging.Runtime.Scheduler
{
    /// <summary>
    /// Represents a scheduled operation.
    /// </summary>
    public interface IScheduledOperation
    {
        /// <summary>
        /// Gets the date and time at which the scheduled operation is set to execute.
        /// </summary>
        DateTime ScheduledAt { get; }

        /// <summary>
        /// Cancels the scheduled operation.
        /// </summary>
        void Cancel();
    }
}
