using System;
using Mercury.Messaging.Routing;

namespace Mercury.Messaging.Runtime
{
    /// <summary>
    /// Internal resource base class.
    /// </summary>
    public abstract class InternalResource : 
        IResource
    {
        /// <summary>
        /// Initializes a default instance of the InternalResource class.
        /// </summary>
        protected InternalResource()
        {
        }

        private string _id;

        /// <summary>
        /// Gets the routable address of this resource instance.
        /// </summary>
        public string Id
        {
            get { return this._id; }
            internal set
            {
                this._id = value;
            }
        }

        private DateTime _lastAccess;

        /// <summary>
        /// Gets the date and time of the last message accessing this resource.
        /// </summary>
        public DateTime LastAccess
        {
            get { return this._lastAccess; }
            internal set
            {
                this._lastAccess = value;
            }
        }

        /// <summary>
        /// Gets the runtime environment for this resource.
        /// </summary>
        public RuntimeEnvironment Environment { get; private set; }

        /// <summary>
        /// Sets the environment to the specified value if the current value is uninitialized (null).
        /// </summary>
        /// <param name="environment">The environment to set.</param>
        internal void SetEnvironmentIfNull(RuntimeEnvironment environment)
        {
            if (this.Environment == null)
                this.Environment = environment;
        }

        /// <summary>
        /// Posts a routing context to the resource.
        /// </summary>
        /// <param name="context">The IRoutingContext to post.</param>
        protected internal virtual void Post(IRoutingContext context)
        {
            this.UpdateAccess();
        }

        /// <summary>
        /// Updates the LastAccess date and time to the current date and time.
        /// </summary>
        protected virtual void UpdateAccess()
        {
            this._lastAccess = DateTime.UtcNow;
        }

        #region Shutdown
        private bool _isShuttingDown = false;

        /// <summary>
        /// Gets a value indicating whether the port is currently shutting down.
        /// </summary>
        public bool IsShuttingDown
        {
            get { return this._isShuttingDown; }
        }

        /// <summary>
        /// Transitions this resource into a shutdown state.
        /// </summary>
        protected void Shutdown()
        {
            this._isShuttingDown = true;
        }
        #endregion

        #region Equals
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            InternalResource resource = obj as InternalResource;
            if (resource != null)
                return (this.Id == resource.Id);
            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.Id.GetHashCode());
        }
        #endregion
    }
}
