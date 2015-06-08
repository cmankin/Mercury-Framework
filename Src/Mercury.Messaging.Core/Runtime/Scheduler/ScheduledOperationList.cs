using System;
using System.Collections.Generic;
using System.Linq;

namespace Mercury.Messaging.Runtime.Scheduler
{
    /// <summary>
    /// A list of scheduled operations.
    /// </summary>
    internal class ScheduledOperationList
    {
        /// <summary>
        /// Initializes a default instance of the ScheduledOperationList class.
        /// </summary>
        public ScheduledOperationList()
        {
            this._operations = new SortedList<DateTime, List<IScheduledOperationExecutor>>();
        }

        private readonly object _lock = new object();
        private SortedList<DateTime, List<IScheduledOperationExecutor>> _operations;

        /// <summary>
        /// Gets the number of operations in this list.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this._lock)
                {
                    return this._operations.Count;
                }
            }
        }

        /// <summary>
        /// Gets an array of operations that have elapsed according to the specified date and time.
        /// </summary>
        /// <param name="now">The date and time to compare.</param>
        /// <returns>An array of operations that have elapsed according to the specified date and time.</returns>
        public IScheduledOperationExecutor[] GetExpiredOperations(DateTime now)
        {
            lock (this._lock)
            {
                IScheduledOperationExecutor[] expired = this._operations
                    .Where(x => x.Key <= now)
                    .OrderBy(x => x.Key)
                    .SelectMany(x => x.Value)
                    .ToArray();

                foreach (IScheduledOperationExecutor executor in expired)
                {
                    if (this._operations.ContainsKey(executor.ScheduledAt))
                        this._operations.Remove(executor.ScheduledAt);
                }

                return expired;
            }
        }

        /// <summary>
        /// Gets the date and time of the next scheduled action.
        /// </summary>
        /// <param name="now">The current date and time.</param>
        /// <param name="scheduledAt">Out. The next scheduled date and time.</param>
        /// <returns>True if the next scheduled date and time was retrieved; otherwise, false.</returns>
        public bool GetNextScheduledTime(DateTime now, out DateTime scheduledAt)
        {
            scheduledAt = now;

            lock (this._lock)
            {
                if (this._operations.Count == 0)
                    return false;

                foreach (KeyValuePair<DateTime, List<IScheduledOperationExecutor>> pair in this._operations)
                {
                    if (now >= pair.Key)
                        return true;

                    scheduledAt = pair.Key;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds an operation executor to the list.
        /// </summary>
        /// <param name="executor">The operation executor to add.</param>
        public void Add(IScheduledOperationExecutor executor)
        {
            lock (this._lock)
            {
                List<IScheduledOperationExecutor> list;
                if (this._operations.TryGetValue(executor.ScheduledAt, out list))
                    list.Add(executor);
                else
                {
                    list = new List<IScheduledOperationExecutor>()
                        {
                            executor
                        };
                    this._operations[executor.ScheduledAt] = list;
                }
            }
        }
    }
}
