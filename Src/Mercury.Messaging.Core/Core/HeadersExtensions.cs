using System;
using Mercury;
using System.Net;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Core extensions to Mercury.Messaging.Core.IHeaders object.
    /// </summary>
    public static class HeadersExtensions
    {
        /// <summary>
        /// Gets the URI from the header item at the specified key.
        /// </summary>
        /// <param name="headers">The headers dictionary to use.</param>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The URI converted from the string format header item at the specified key.</returns>
        public static Uri GetUri(this IHeaders headers, string key)
        {
            string value = headers[key];
            if (string.IsNullOrEmpty(value))
                return null;

            return new Uri(value);
        }

        /// <summary>
        /// Sets the header item at the specified key to the string format of the specified URI.
        /// </summary>
        /// <param name="headers">The headers dictionary to use.</param>
        /// <param name="key">The key of the item to set.</param>
        /// <param name="value">The URI value.</param>
        public static void SetUri(this IHeaders headers, string key, Uri value)
        {
            if (value == null)
            {
                headers[key] = null;
                return;
            }
            headers[key] = value.ToString();
        }

        /// <summary>
        /// Gets the end point from the header item at the specified key.
        /// </summary>
        /// <param name="headers">The headers dictionary to use.</param>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The end point converted from the string format header item at the specified key.</returns>
        public static IPEndPoint GetEndPoint(this IHeaders headers, string key)
        {
            string value = headers[key];
            return RemotingUtil.ParseRemoteFormat(value);
        }

        /// <summary>
        /// Sets the header item at the specified key to 
        /// the string format of the specified end point.
        /// </summary>
        /// <param name="headers">The headers dictionary to use.</param>
        /// <param name="key">The key of the item to set.</param>
        /// <param name="value">The end point value.</param>
        public static void SetEndPoint(this IHeaders headers, string key, IPEndPoint value)
        {
            if (value == null)
            {
                headers[key] = null;
                return;
            }
            headers[key] = RemotingUtil.GetRemoteFormat(value);
        }
    }
}
