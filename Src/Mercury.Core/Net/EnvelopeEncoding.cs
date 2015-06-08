using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// Describes the encoding used for a message envelope.
    /// </summary>
    public class EnvelopeEncoding
    {
        /// <summary>
        /// Initializes a default EnvelopeEncoding class with the specified encoding ID value.
        /// </summary>
        /// <param name="encoding">The encoding ID value.</param>
        public EnvelopeEncoding(byte encoding)
        {
            this._id = encoding;
        }

        /// <summary>
        /// Gets the envelope encoding.
        /// </summary>
        public Encoding Encoding
        {
            get { return GetEncoding(this.Id); }
        }

        private byte _id;

        /// <summary>
        /// Gets the identifier for this envelope encoding.
        /// </summary>
        public byte Id
        {
            get { return this._id; }
        }

        /// <summary>
        /// Returns the encoding associated with the specified encoding ID.
        /// </summary>
        /// <param name="encoding">The ID of the encoding to retrieve.</param>
        /// <returns>The encoding associated with the specified encoding ID.</returns>
        protected Encoding GetEncoding(byte encoding)
        {
            if (encoding == 0x00)
                return System.Text.Encoding.UTF8;
            else if (encoding == 0x01)
                return System.Text.Encoding.BigEndianUnicode;
            else if (encoding == 0x02)
                return System.Text.Encoding.Unicode;
            else if (encoding == 0x03)
                return null;
            return null;
        }

        #region Static

        /// <summary>
        /// UTF-8 encoding.
        /// </summary>
        public static readonly EnvelopeEncoding Utf8 = new EnvelopeEncoding(0x00);
        /// <summary>
        /// UTF-16, big-endian, encoding.
        /// </summary>
        public static readonly EnvelopeEncoding Utf16 = new EnvelopeEncoding(0x01);
        /// <summary>
        /// Unicode, little-endian, encoding.
        /// </summary>
        public static readonly EnvelopeEncoding Unicode = new EnvelopeEncoding(0x02);
        /// <summary>
        /// Raw binary.
        /// </summary>
        public static readonly EnvelopeEncoding Binary = new EnvelopeEncoding(0x03);

        #endregion
    }
}
