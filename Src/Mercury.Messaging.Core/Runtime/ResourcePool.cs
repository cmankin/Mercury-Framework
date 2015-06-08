using System;
using System.Globalization;
using Mercury.Messaging.Core;

namespace Mercury.Messaging.Runtime
{
    /// <summary>
    /// Represents a pool of resources that can be managed and collected over time.
    /// </summary>
    public class ResourcePool 
        : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Initializes a default instance of the ResourcePool class.
        /// </summary>
        public ResourcePool()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a default instance of the ResourcePool 
        /// class with the specified generated ID prefix.
        /// </summary>
        /// <param name="generatedIdPrefix">A string value prefixed to the generated resource ID.</param>
        public ResourcePool(string generatedIdPrefix)
        {
            this._idPrefix = generatedIdPrefix;
            this._resources = new Catalog<IResource>();
        }

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
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this._resources != null)
                    {
                        foreach (string key in this._resources.Keys)
                            this.Delete(key);
                    }
                }
            }
            this.disposedValue = true;
        }

        #endregion

        #region Thresholds

        /// <summary>
        /// The upper bound resource limit.
        /// </summary>
        private int _resourceLimit = 1000000;

        /// <summary>
        /// Gets the maximum number of resources that this pool can manage.
        /// </summary>
        public int ResourceLimit
        {
            get
            {
                return this._resourceLimit;
            }
        }

        #endregion

        #region Resources

        internal Catalog<IResource> _resources;
        private string _idPrefix;

        /// <summary>
        /// Adds the resource to the pool and returns 
        /// the added resource's identifier string.
        /// </summary>
        /// <param name="resource">The IResource to add.</param>
        /// <returns>The added resource's identifier string.</returns>
        public string Add(IResource resource)
        {
            // Create id and store
            string id = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this._idPrefix, Guid.NewGuid());
            this.Store(resource, id);
            return id;
        }

        /// <summary>
        /// Stores a resource in the pool with the specified ID.
        /// </summary>
        /// <param name="resource">The resource to store.</param>
        /// <param name="id">The resource's identifier string.</param>
        internal void Store(IResource resource, string id)
        {
            // If resource limit reached...
            if (this._resources.Count == this.ResourceLimit)
                throw new ResourceLimitException("Resource cannot be added.  Resource limit reached.");

            // Set resource ID.
            InternalResource ir = resource as InternalResource;
            if (ir != null)
                ir.Id = id;

            // Add
            if (this._resources.ContainsKey(id))
                throw new ArgumentException("The specified resource ID is non-unique.");
            this._resources.Add(id, resource);
        }

        /// <summary>
        /// Deletes the resource with the specified identifier from the pool.
        /// </summary>
        /// <param name="id">The identifier of the IResource to delete.</param>
        public void Delete(string id)
        {
            IDisposable disp = this._resources.GetReference(id) as IDisposable;
            if (disp != null)
                disp.Dispose();
            this._resources.Remove(id);
        }

        /// <summary>
        /// Clears all resources from the pool.
        /// </summary>
        public void Clear()
        {
            this._resources.Clear();
        }

        /// <summary>
        /// Returns the IResource associated with the specified ID string.
        /// </summary>
        /// <param name="id">The resource identifier string.</param>
        /// <returns>The IResource associated with the specified ID string.</returns>
        public IResource Get(string id)
        {
            return this._resources.GetReference(id);
        }

        /// <summary>
        /// Gets the number of resources contained in this resource pool.
        /// </summary>
        /// <returns>The number of resources contained in this resource pool.</returns>
        public int Count
        {
            get { return this._resources.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the specified ID 
        /// is currently contained within this resource pool.
        /// </summary>
        /// <param name="id">The ID of the resource to locate.</param>
        /// <returns>True if the resource is contained in this resource pool; otherwise, false.</returns>
        public bool Contains(string id)
        {
            return this._resources.ContainsKey(id);
        }
        #endregion
    }
}
