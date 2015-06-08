using System;
using System.Net;
using System.Net.Sockets;

namespace Mercury.Net
{
    /// <summary>
    /// A helper class for generating and connecting sockets for a TCP protocol.
    /// </summary>
    public static class TcpSockets
    {
        private static Exception _lastError;

        /// <summary>
        /// Gets the last error value that occurred while using the TcpSockets connection methods.
        /// </summary>
        public static Exception LastError
        {
            get { return _lastError; }
            private set { _lastError = value; }
        }
        
        /// <summary>
        /// Returns a newly created socket that has been connected 
        /// to the end point at the specified host and port.
        /// </summary>
        /// <param name="host">The name of the host server, or the string representation of its IP address.</param>
        /// <param name="port">The port number on which to connect.</param>
        /// <returns>A newly created socket that has been connected 
        /// to the end point at the specified host and port or null.</returns>
        public static Socket Connect(string host, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                Socket tempSocket = Connect(new IPEndPoint(address, port));
                if (tempSocket != null)
                    return tempSocket;
            }
            return null;
        }

        /// <summary>
        /// Returns a newly created socket that has been connected 
        /// to the end point at the specified IP address and port.
        /// </summary>
        /// <param name="ipAddress">The IP address represented as a 64-bit integer.</param>
        /// <param name="port">The port number on which to connect.</param>
        /// <returns>A newly created socket that has been connected to 
        /// the end point at the specified IP address and port or null.</returns>
        public static Socket Connect(long ipAddress, int port)
        {
            return Connect(new IPEndPoint(ipAddress, port));
        }

        /// <summary>
        /// Returns a newly created socket that has been connected to the specified end point.
        /// </summary>
        /// <param name="endPoint">The IP end point on which to connect.</param>
        /// <returns>A socket that has been created for and connected to the specified end point or null.</returns>
        public static Socket Connect(IPEndPoint endPoint)
        {
            // Generate socket
            Socket tempSocket = GetSocket(endPoint, SocketType.Stream);

            // Attempt to connect
            bool flag = TryConnectSocket(tempSocket, endPoint);
            if (flag)
                return tempSocket;
            return null;
        }

        /// <summary>
        /// Returns a socket created from the specified IP address, port and socket type.
        /// </summary>
        /// <param name="ipAddress">The IP address represented as a 64-bit integer.</param>
        /// <param name="port">The port number to use.</param>
        /// <param name="type">The type of socket to create.</param>
        /// <returns>A socket created from the specified IP address, port and socket type.</returns>
        public static Socket GetSocket(long ipAddress, int port, SocketType type)
        {
            return GetSocket(new IPEndPoint(ipAddress, port), type);
        }

        /// <summary>
        /// Returns a socket created from the specified end point and socket type.
        /// </summary>
        /// <param name="endPoint">The IP end point to use.</param>
        /// <param name="type">The type of socket to create.</param>
        /// <returns>A socket created from the specified end point and socket type.</returns>
        public static Socket GetSocket(IPEndPoint endPoint, SocketType type)
        {
            return new Socket(endPoint.AddressFamily, type, ProtocolType.Tcp);
        }

        /// <summary>
        /// Attempts to connect the specified socket to the specified end point.
        /// </summary>
        /// <param name="socket">The socket to connect.</param>
        /// <param name="endPoint">The IP end point on which to connect.</param>
        /// <returns>True if the the socket was able to connect; otherwise, false.</returns>
        public static bool TryConnectSocket(Socket socket, IPEndPoint endPoint)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");

            try
            {
                socket.Connect(endPoint);
            }
            catch (SocketException ex)
            {
                TcpSockets.LastError = ex;
                return false;
            }

            return socket.Connected;
        }
    }
}
