using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// Provides networking information about the current machine.
    /// </summary>
    public static class Machine
    {
        private static IPHostEntry _entry;

        /// <summary>
        /// Gets the current host entry for this machine.
        /// </summary>
        public static IPHostEntry Host
        {
            get
            {
                if (_entry == null)
                    _entry = Dns.GetHostEntry(Dns.GetHostName());
                return _entry;
            }
        }

        /// <summary>
        /// Gets the IPv4 IP address for this machine, if available.
        /// </summary>
        public static IPAddress IPv4
        {
            get
            {
                return Machine.Host.GetAddress((address) => (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
            }
        }

        /// <summary>
        /// Gets the IPv6 IP address for this machine, if available.
        /// </summary>
        public static IPAddress IPv6
        {
            get
            {
                return Machine.Host.GetAddress((address) => (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6));
            }
        }
    }
}
