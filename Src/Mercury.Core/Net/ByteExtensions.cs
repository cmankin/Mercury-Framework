using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// A set of extension methods for a byte.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Appends the specified bytes to the tail of the current byte array.
        /// </summary>
        /// <param name="current">The byte array on which to append.</param>
        /// <param name="bytes">The byte array to append.</param>
        /// <returns>The specified bytes appended to the tail of the current byte array.</returns>
        public static byte[] Append(this byte[] current, byte[] bytes)
        {
            if (current == null)
                throw new ArgumentNullException("current");
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            byte[] newBytes = new byte[(current.Length + bytes.Length)];
            Array.Copy(current, 0, newBytes, 0, current.Length);
            Array.Copy(bytes, 0, newBytes, current.Length, bytes.Length);

            // Return combined
            return newBytes;
        }

        /// <summary>
        /// Appends one byte array to another.
        /// </summary>
        /// <param name="current">The byte array on which to append.</param>
        /// <param name="appendByte">The byte array to append.</param>
        /// <returns>A byte array containing the appended bytes.</returns>
        public static byte[] Append(this byte[] current, byte appendByte)
        {
            if (current == null)
                throw new ArgumentNullException("current");
            return current.Append(new byte[] { appendByte });
        }

        /// <summary>
        /// Prepends the specified bytes to the head of the current byte array.
        /// </summary>
        /// <param name="current">The byte array on which to prepend.</param>
        /// <param name="bytes">The byte array to prepend.</param>
        /// <returns>The specified bytes prepended to the head of the current byte array.</returns>
        public static byte[] Prepend(this byte[] current, byte[] bytes)
        {
            if (current == null)
                throw new ArgumentNullException("current");
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            
            byte[] newBytes = new byte[(current.Length + bytes.Length)];
            Array.Copy(bytes, 0, newBytes, 0, bytes.Length);
            Array.Copy(current, 0, newBytes, bytes.Length, current.Length);

            // Return combined
            return newBytes;
        }

        /// <summary>
        /// Prepends one byte to a byte array.
        /// </summary>
        /// <param name="current">The byte array on which to prepend.</param>
        /// <param name="appendByte">The byte to prepend.</param>
        /// <returns>A byte array containing the prepended byte.</returns>
        public static byte[] Prepend(this byte[] current, byte appendByte)
        {
            if (current == null)
                throw new ArgumentNullException("current");
            return current.Prepend(new byte[] { appendByte });
        }

        /// <summary>
        /// Extracts a copy of the byte array beginning at the specified offset index and with the specified length.
        /// </summary>
        /// <param name="current">The byte array from which to extract.</param>
        /// <param name="offset">The index at which to begin the extraction.</param>
        /// <param name="length">The number of bytes to extract.</param>
        /// <returns>A copy of the byte array beginning at the specified offset index and with the specified length.</returns>
        public static byte[] ExtractCopy(this byte[] current, int offset, int length)
        {
            if (offset >= current.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (offset + length > current.Length)
                throw new ArgumentException("The offset and length must not exceed the length of the current byte array.");
            
            byte[] newBytes = new byte[length];
            Array.Copy(current, offset, newBytes, 0, length);

            return newBytes;
        }

        /// <summary>
        /// Merges one byte array into another.
        /// </summary>
        /// <param name="current">The byte array on which to merge.</param>
        /// <param name="mergeBytes">The byte array to merge.</param>
        /// <param name="offset">The offset at which to start merging.</param>
        /// <param name="length">The number of bytes to merge.</param>
        public static void Merge(this byte[] current, byte[] mergeBytes, int offset, int length)
        {
            if (offset >= current.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (offset + length > current.Length)
                throw new ArgumentException("The offset and length must not exceed the length of the current byte array.");

            Array.Copy(mergeBytes, 0, current, offset, length);
        }
    }
}
