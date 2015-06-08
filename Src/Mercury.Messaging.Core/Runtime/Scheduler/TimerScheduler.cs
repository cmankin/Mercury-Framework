using System;
using System.Threading;
using Microsoft.Ccr.Core;

namespace Mercury.Messaging.Runtime.Scheduler
{
    /// <summary>
    /// Represents a timer-based scheduler.
    /// </summary>
    public class TimerScheduler : 
        IScheduler, IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the TimerScheduler class with the specified runtime environment.
        /// </summary>
        /// <param name="environment">The runtime environment on which to schedule operations.</param>
        public TimerScheduler(RuntimeEnvironment environment)
        {
            this.Environment = environment;
        }
        #endregion

        #region Schedule Actions

        /// <summary>
        /// Gets the current operations count
        /// </summary>
        public int Count
        {
            get { return this._operations.Count; }
        }

        /// <summary>
        /// Gets the runtime environment for this scheduler.
        /// </summary>
        public RuntimeEnvironment Environment { get; private set; }

        /// <summary>
        /// Schedules the specified action.
        /// </summary>
        /// <param name="action"></param>
        protected void Schedule(IScheduledOperationExecutor action)
        {
            this.Environment.TimerScheduledTasks.Enqueue(new Task(() => 
                {
                    this._operations.Add(action);

                    // Execute expired actions
                    ExecuteExpiredActions();
                }));
        }

        /// <summary>
        /// Schedules the next timer operation.
        /// </summary>
        protected void ScheduleTimer()
        {
            DateTime now = Now;

            DateTime scheduledAt;
            if (this._operations.GetNextScheduledTime(now, out scheduledAt))
            {
                lock (this._lock)
                {
                    TimeSpan dueTime = scheduledAt - now;
                    if (this._timer != null)
                        this._timer.Change(dueTime, this._noPeriod);
                    else
                        this._timer = new Timer(x =>
                            this.Environment.TimerScheduledTasks.Enqueue(new Task(ExecuteExpiredActions)),
                            this, dueTime, this._noPeriod);
                }
            }
        }

        /// <summary>
        /// Executes actions whose scheduled wait period has elapsed.
        /// </summary>
        protected void ExecuteExpiredActions()
        {
            if (this._stopped)
                return;

            IScheduledOperationExecutor[] expiredActions;
            while ((expiredActions = _operations.GetExpiredOperations(Now)).Length > 0)
            {
                foreach (IScheduledOperationExecutor action in expiredActions)
                {
                    try
                    {
                        action.Execute();
                    }
                    catch
                    {
                    }
                }
            }

            // Schedule next
            ScheduleTimer();
        }

        #endregion

        #region IScheduler
        /// <summary>
        /// Schedules an operation to execute after the specified interval has elapsed.
        /// </summary>
        /// <param name="interval">The duration of the interval before execution.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>A scheduled operation.</returns>
        public IScheduledOperation Schedule(TimeSpan interval, Action operation)
        {
            var scheduled = new ScheduledOperationExecutor(this.Environment, GetScheduledTime(interval), new Handler(operation));
            Schedule(scheduled);

            return scheduled;
        }

        /// <summary>
        /// Schedules an operation to execute after the specified interval has elapsed 
        /// and for every specified periodic interval after initial execution.
        /// </summary>
        /// <param name="interval">The duration of the interval before execution.</param>
        /// <param name="periodic">The periodic interval between subsequent executions.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>A scheduled operation.</returns>
        public IScheduledOperation Schedule(TimeSpan interval, TimeSpan periodic, Action operation)
        {
            ScheduledOperationExecutor scheduled = null;
            scheduled = new ScheduledOperationExecutor(this.Environment, GetScheduledTime(interval), () =>
            {
                try
                {
                    operation();
                }
                catch
                {
                }
                finally
                {
                    scheduled.ScheduledAt = GetScheduledTime(periodic);
                    Schedule(scheduled);
                }
            });

            // Schedule action
            Schedule(scheduled);
            return scheduled;
        }

        /// <summary>
        /// Stops the scheduler, preventing any further operations from being executed.
        /// </summary>
        public void Stop()
        {
            if (!this._stopped)
            {
                this._stopped = true;

                lock (this._lock)
                {
                    this.Dispose();
                }
            }
        }

        /// <summary>
        /// Stops the scheduler, preventing any further operations from being executed.
        /// </summary>
        /// <param name="timeout">The time period to wait for the scheduler to shutdown.</param>
        public void Stop(TimeSpan timeout)
        {
            var scheduled = new ScheduledOperationExecutor(this.Environment, GetScheduledTime(timeout), () => 
                {
                    Stop();
                });
            Schedule(scheduled);
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Gets the date and time of the next action as a string.
        /// </summary>
        protected string NextActionTime
        {
            get
            {
                DateTime scheduledAt;
                if (this._operations.GetNextScheduledTime(Now, out scheduledAt))
                    return scheduledAt.ToString();
                return "None";
            }
        }

        /// <summary>
        /// Gets the current UTC date and time.
        /// </summary>
        public static DateTime Now
        {
            get { return DateTime.UtcNow; }
        }

        /// <summary>
        /// Gets a scheduled time interval based on the current UTC date and time.
        /// </summary>
        /// <param name="interval">The time span interval to wait.</param>
        /// <returns>A scheduled time interval based on the current UTC date and time.</returns>
        public static DateTime GetScheduledTime(TimeSpan interval)
        {
            return Now + interval;
        }

        #endregion

        #region Data
        private readonly ScheduledOperationList _operations = new ScheduledOperationList();
        private readonly TimeSpan _noPeriod = -1.Milliseconds();
        private readonly object _lock = new object();
        private bool _stopped;
        private Timer _timer;
        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposedValue;

        /// <summary>
        /// Performs the actual dispose.
        /// </summary>
        /// <param name="disposing">A value indicating whether the object is currently being disposed.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    OnManagedDispose();
                }

                if (this._timer != null)
                    this._timer.Dispose();
                OnUnmanagedDispose();
            }
            this.disposedValue = true;
        }

        /// <summary>
        /// Disposes managed resources.
        /// </summary>
        protected virtual void OnManagedDispose()
        {
        }

        /// <summary>
        /// Disposes unmanaged resources.
        /// </summary>
        protected virtual void OnUnmanagedDispose()
        {
        }
        #endregion
    }
}
