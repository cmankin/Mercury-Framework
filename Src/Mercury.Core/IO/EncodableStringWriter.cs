using System;
using System.IO;
using System.Text;

namespace Mercury.IO
{
    /// <summary>
    /// A string writer for which a text encoding may be specified.
    /// </summary>
    public class EncodableStringWriter : StringWriter
    {
        /// <summary>
        /// Initializes a default instance of the EncodableStringWriter 
        /// class utilizing the specified System.Text.Encoding.
        /// </summary>
        /// <param name="encoding">The encoding in which the output will be written.</param>
        public EncodableStringWriter(Encoding encoding)
            : this(encoding, new StringBuilder())
        {
        }

        /// <summary>
        /// Initializes a default instance of the EncodableStringWriter class 
        /// utilizing the specified System.Text.Encoding and string builder.
        /// </summary>
        /// <param name="encoding">The encoding in which the output will be written.</param>
        /// <param name="stringBuilder">The string builder on which to output.</param>
        public EncodableStringWriter(Encoding encoding, StringBuilder stringBuilder)
            : base(stringBuilder)
        {
            this._encoding = encoding;
        }

        /// <summary>
        /// The internal encoding.
        /// </summary>
        private readonly Encoding _encoding;

        /// <summary>
        /// Gets the System.Text.Encoding in which the output is written.
        /// </summary>
        public override Encoding Encoding
        {
            get { return this._encoding; }
        }
    }
}
