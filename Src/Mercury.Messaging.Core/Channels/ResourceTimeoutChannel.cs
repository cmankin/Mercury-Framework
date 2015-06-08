using System;
using Mercury;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Runtime.Scheduler;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// A resource channel that persists itself and shuts down when its timeout expires.
    /// </summary>
    public class ResourceTimeoutChannel : 
        LocalRefChannel,
        ITimeoutChannel, IDisposable
    {
        /// <summary>
        /// Initializes a default instance of the ResourceTimeoutChannel class.
        /// </summary>
        public ResourceTimeoutChannel()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceTimeoutChannel 
        /// with the specified environment and referenced resource.
        /// </summary>
        /// <param name="environment">The runtime environment for this channel.</param>
        /// <param name="resource">The resource referenced by this channel.</param>
        public ResourceTimeoutChannel(RuntimeEnvironment environment, InternalResource resource)
            : base(resource)
        {
            // Persist
            ChannelFactory.Persist(environment, this);

            // Get scheduler and start
            this._scheduler = environment.GetScheduler();
            this.ScheduleNextTimeout(this._timeout);
        }

        /// <summary>
        /// The scheduler used to schedule timeout actions.
        /// </summary>
        private IScheduler _scheduler;
        private IScheduledOperation _operation;

        private TimeSpan _timeout = 30.Seconds();

        /// <summary>
        /// Gets the time span to wait before the channel expires.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return this._timeout; }
            set
            {
                if (this._timeout != value)
                {
                    this._timeout = value;
                    this.ScheduleNextTimeout(value);
                }
            }
        }

        /// <summary>
        /// Schedules the next timeout for this channel using the specified timeout value.
        /// </summary>
        /// <param name="timeout">The timeout value to schedule.</param>
        protected void ScheduleNextTimeout(TimeSpan timeout)
        {
            if (this._operation != null)
                this._operation.Cancel();
            if (this._scheduler!=null)
                this._operation = this._scheduler.Schedule(timeout, this.DoExpireChannelResource);
        }

        /// <summary>
        /// Expires the channel resource.
        /// </summary>
        protected void DoExpireChannelResource()
        {
            if (this._operation != null)
                this._operation.Cancel();
            if (this._scheduler != null)
                this._scheduler.Stop();

            // Shutdown
            this.Shutdown();
            ChannelFactory.Expire(this.Environment, this);
        }

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

                if (this._scheduler != null)
                    this._scheduler.Stop();

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
