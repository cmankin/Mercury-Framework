using System;
using System.Net;
using System.Net.Sockets;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// A message sent to notify the runtime listener to begin intercepting and routing remote requests.
    /// </summary>
    public class StartServer
    {
        /// <summary>
        /// Initializes a default instance of the StartServer class witht the specified port.
        /// </summary>
        /// <param name="port">The port number associated with the address, or 0 to specify any port.</param>
        public StartServer(int port)
            : this(Dns.GetHostName(), port, AddressFamily.InterNetwork)
        {
        }

        /// <summary>
        /// Initializes a default instance of the StartServer class with the specified values.
        /// </summary>
        /// <param name="host">A host name or IP address string to use.</param>
        /// <param name="port">The port number associated with the address, or 0 to specify any port.</param>
        /// <param name="addressFamily">The address family describing the address to use.</param>
        public StartServer(string host, int port, AddressFamily addressFamily)
        {
            IPHostEntry entry = Dns.GetHostEntry(host);
            foreach (IPAddress address in entry.AddressList)
            {
                if (address.AddressFamily == addressFamily)
                {
                    this._endPoint = new IPEndPoint(address, port);
                    break;
                }
            }
        }

        /// <summary>
        /// Initializes a default instance of the StartServer class with the specified IP end point.
        /// </summary>
        /// <param name="endPoint">The IP end point to use.</param>
        public StartServer(IPEndPoint endPoint)
        {
            this._endPoint = endPoint;
        }

        private IPEndPoint _endPoint;

        /// <summary>
        /// Gets the IP end point on which to start listening.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return this._endPoint; }
        }
    }
}
