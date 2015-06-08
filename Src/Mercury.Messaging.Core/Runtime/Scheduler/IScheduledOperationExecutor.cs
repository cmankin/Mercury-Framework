using System;

namespace Mercury.Messaging.Runtime.Scheduler
{
    /// <summary>
    /// Represents an executable scheduled operation.
    /// </summary>
    public interface IScheduledOperationExecutor : 
        IScheduledOperation
    {
        /// <summary>
        /// Executes the operation.
        /// </summary>
        void Execute();
    }
}
