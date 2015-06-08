using System;
using System.Net;
using System.Globalization;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// A set of utilities for basic remoting procedures.
    /// </summary>
    public static class RemotingUtil
    {
        /// <summary>
        /// Gets the XML-compatible string format of the specified IP end point.
        /// </summary>
        /// <param name="endPoint">The IP end point to format.</param>
        /// <returns>The XML-compatible string format of the specified IP end point.</returns>
        public static string GetRemoteFormat(IPEndPoint endPoint)
        {
            var culture = CultureInfo.InvariantCulture;
            return string.Format(culture, "{0}@{1}", endPoint.Address.ToString(), endPoint.Port.ToString(culture));
        }

        /// <summary>
        /// Gets an IP end point from its XML-compatible string format.
        /// </summary>
        /// <param name="value">The XML-compatible string format of the IP end point.</param>
        /// <returns>An IP end point constructed from its XML-compatible string format.</returns>
        public static IPEndPoint ParseRemoteFormat(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            // Split at delimiter than
            string[] endPointArr = value.Split('@');
            if (endPointArr != null && endPointArr.Length == 2)
            {
                IPAddress address = null;
                var flag1 = IPAddress.TryParse(endPointArr[0], out address);
                int port = 0;
                var flag2 = int.TryParse(endPointArr[1], out port);
                if (flag1 && flag2)
                    return new IPEndPoint(address, port);
            }

            return null;
        }
    }
}
