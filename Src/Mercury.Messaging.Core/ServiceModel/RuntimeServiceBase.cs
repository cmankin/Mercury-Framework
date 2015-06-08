using System;
using System.Net;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// The base class for all service implementations.
    /// </summary>
    public class RuntimeServiceBase
    {
        /// <summary>
        /// Initializes a default instance of the RuntimeServiceBase 
        /// class with the specified IP end point.
        /// </summary>
        /// <param name="endPoint">The System.Net.IPEndPoint to use.</param>
        public RuntimeServiceBase(IPEndPoint endPoint)
        {
            this._endPoint = endPoint;
        }

        private IPEndPoint _endPoint;

        /// <summary>
        /// Gets the IP end point used by this service.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return this._endPoint; }
        }

        /// <summary>
        /// Gets a value indicating whether the service is started.
        /// </summary>
        public bool IsStarted { get; protected set; }

        /// <summary>
        /// Starts the service.
        /// </summary>
        public virtual void Start()
        {
            this.IsStarted = true;
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public virtual void Stop()
        {
            this.IsStarted = false;
        }
    }
}
