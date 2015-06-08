using System;

namespace Mercury.Messaging.Runtime.Scheduler
{
    /// <summary>
    /// Represents a scheduler that can schedule work at discrete time intervals.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Schedules an operation to execute after the specified interval has elapsed.
        /// </summary>
        /// <param name="interval">The duration of the interval before execution.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>A scheduled operation.</returns>
        IScheduledOperation Schedule(TimeSpan interval, Action operation);

        /// <summary>
        /// Schedules an operation to execute after the specified interval has elapsed 
        /// and for every specified periodic interval after initial execution.
        /// </summary>
        /// <param name="interval">The duration of the interval before execution.</param>
        /// <param name="periodic">The periodic interval between subsequent executions.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>A scheduled operation.</returns>
        IScheduledOperation Schedule(TimeSpan interval, TimeSpan periodic, Action operation);

        /// <summary>
        /// Stops the scheduler, preventing any further operations from being executed.
        /// </summary>
        /// <param name="timeout">The time period to wait for the scheduler to shutdown.</param>
        void Stop(TimeSpan timeout);

        /// <summary>
        /// Stops the scheduler, preventing any further operations from being executed.
        /// </summary>
        void Stop();
    }
}
