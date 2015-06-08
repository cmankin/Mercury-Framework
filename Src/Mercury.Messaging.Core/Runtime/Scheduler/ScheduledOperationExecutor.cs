using System;
using Microsoft.Ccr.Core;

namespace Mercury.Messaging.Runtime.Scheduler
{
    /// <summary>
    /// Implementation of IScheduledOperationExecutor interface.
    /// </summary>
    public class ScheduledOperationExecutor : IScheduledOperationExecutor
    {
        /// <summary>
        /// Initializes a default instance of the ScheduledOperationExecutor class with the specified values.
        /// </summary>
        /// <param name="environment">The runtime environment on which to execute.</param>
        /// <param name="scheduledAt">The date and time of the next scheduled run.</param>
        /// <param name="operation">A handler delegate to the operation to execute.</param>
        public ScheduledOperationExecutor(RuntimeEnvironment environment, DateTime scheduledAt, Handler operation)
        {
            this._environment = environment;
            this._scheduledAt = scheduledAt;
            this._operation = operation;
        }

        private RuntimeEnvironment _environment;
        private Handler _operation;
        private DateTime _scheduledAt;

        /// <summary>
        /// Gets the date and time at which the scheduled operation is set to execute.
        /// </summary>
        public DateTime ScheduledAt
        {
            get { return this._scheduledAt; }
            set { this._scheduledAt = value; }
        }

        private bool _cancelled;

        /// <summary>
        /// Cancels the scheduled operation.
        /// </summary>
        public void Cancel()
        {
            this._cancelled = true;
        }

        /// <summary>
        /// Executes the operation.
        /// </summary>
        public void Execute()
        {
            if (this._cancelled)
                return;

            this._environment.TimerScheduledTasks.Enqueue(new Task(this._operation));
        }
    }
}
