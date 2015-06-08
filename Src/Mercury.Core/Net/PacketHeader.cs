using System;
using System.IO;
using Mercury.IO;
using System.Xml;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// A header for a message packet sent over the network.
    /// </summary>
    public class PacketHeader
    {
        #region Constructors

        /// <summary>
        /// Initializes a default instance of the PacketHeader class.
        /// </summary>
        public PacketHeader()
            : this(0x01, 0x01)
        {
        }

        /// <summary>
        /// Initializes a default instance of the PacketHeader class with the specified values.
        /// </summary>
        /// <param name="majorVersion">The major version of the packet protocol.</param>
        /// <param name="minorVersion">The minor version of the packet protocol.</param>
        public PacketHeader(byte majorVersion, byte minorVersion)
            : this(majorVersion, minorVersion, CommunicationMode.SingletonSized, string.Empty,
            EnvelopeEncoding.Unicode, EnvelopeStructure.XmlFormattedString)
        {
        }

        /// <summary>
        /// Initializes a default instance of the PacketHeader class with the specified values.
        /// </summary>
        /// <param name="majorVersion">The major version of the packet protocol.</param>
        /// <param name="minorVersion">The minor version of the packet protocol.</param>
        /// <param name="mode">The communication mode being used.</param>
        /// <param name="via">The URI address destination for subsequent messages.</param>
        /// <param name="encoding">The envelope encoding scheme.</param>
        /// <param name="structure">The envelope structure.</param>
        public PacketHeader(byte majorVersion, byte minorVersion, CommunicationMode mode, string via,
            EnvelopeEncoding encoding, EnvelopeStructure structure)
        {
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.Mode = mode;
            this.Via = via;
            this.EnvelopeEncoding = encoding;
            this.EnvelopeStructure = structure;
        }
        #endregion

        /// <summary>
        /// The default string encoding.
        /// </summary>
        public static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

        private byte _majorVersion;

        /// <summary>
        /// Gets the major version of this protocol.
        /// </summary>
        public byte MajorVersion 
        {
            get { return this._majorVersion; }
            set { this._majorVersion = value; }
        }

        private byte _minorVersion;

        /// <summary>
        /// Gets the minor version of this protocol.
        /// </summary>
        public byte MinorVersion
        {
            get { return this._minorVersion; }
            set { this._minorVersion = value; }
        }
        
        private EnvelopeStructure _envelopeStructure;

        /// <summary>
        /// Gets the type of data following this header.
        /// </summary>
        public EnvelopeStructure EnvelopeStructure
        {
            get { return this._envelopeStructure; }
            set { this._envelopeStructure = value; }
        }

        private EnvelopeEncoding _envelopeEncoding;

        /// <summary>
        /// Gets the encoding for the message envelope.
        /// </summary>
        public EnvelopeEncoding EnvelopeEncoding
        {
            get { return this._envelopeEncoding; }
            set { this._envelopeEncoding = value; }
        }

        private CommunicationMode _mode;

        /// <summary>
        /// Gets the communication mode.
        /// </summary>
        public CommunicationMode Mode
        {
            get { return this._mode; }
            set { this._mode = value; }
        }

        private string _via;

        /// <summary>
        /// Gets the URI for which subsequent messages are bound.
        /// </summary>
        public string Via
        {
            get { return this._via; }
            set { this._via = value; }
        }

        /// <summary>
        /// Gets a byte array containing the complete packet header information.
        /// </summary>
        /// <returns>A byte array containing the complete packet header information.</returns>
        public byte[] GetBytes()
        {
            // Calculate size
            int totalByteSize = CalculateHeaderSize();
            // Create byte array
            byte[] headerBytes = new byte[totalByteSize];
            int offset = 0;

            // Version
            byte[] version = GetRecord(0x00, this.MajorVersion, this.MinorVersion);
            headerBytes.Merge(version, offset, version.Length);
            offset += version.Length;

            // Mode
            byte[] mode = GetRecord(0x01, (byte)this.Mode);
            headerBytes.Merge(mode, offset, mode.Length);
            offset += mode.Length;

            // Via
            int viaLength=PacketHeader.DefaultStringEncoding.GetByteCount(this.Via);
            if (viaLength > UInt16.MaxValue)
                throw new ArgumentException("The Via URI exceeds supported length restrictions.");
            byte[] newVia = new byte[2 + viaLength];
            newVia.Merge(GetSizeBytes((UInt16)viaLength), 0, 2);
            PacketHeader.DefaultStringEncoding.GetBytes(this.Via, 0, viaLength, newVia, 2);
            byte[] via = GetRecord(0x02, newVia);

            headerBytes.Merge(via, offset, via.Length);
            offset += via.Length;

            // Encoding
            byte[] encoding = GetRecord(0x03, this.EnvelopeEncoding.Id);
            headerBytes.Merge(encoding, offset, encoding.Length);
            offset += encoding.Length;

            // Structure
            byte[] structure = GetRecord(0x04, (byte)this.EnvelopeStructure);
            headerBytes.Merge(structure, offset, structure.Length);
            offset += structure.Length;

            // Preamble end
            headerBytes.Merge(GetRecord(0x09), offset, 1);

            return headerBytes;
        }

        /// <summary>
        /// Gets the size in bytes of the packet header.
        /// </summary>
        /// <returns>The size in bytes of the packet header.</returns>
        public int CalculateHeaderSize()
        {
            int total = 0;

            // Version
            total += 3;
            // Mode
            total += 2;
            // Via
            int viaLength = PacketHeader.DefaultStringEncoding.GetByteCount(this.Via);
            total += (3 + viaLength);
            // Encoding
            total += 2;
            // Structure
            total += 2;
            // Preamble end
            total += 1;

            return total;
        }

        /// <summary>
        /// Returns a byte array of the specified size value.
        /// </summary>
        /// <param name="size">The size value to convert into a byte array.</param>
        /// <returns>A byte array of the specified size value.</returns>
        [CLSCompliant(false)]
        protected byte[] GetSizeBytes(UInt16 size)
        {
            return BitConverter.GetBytes(size);
        }

        /// <summary>
        /// Returns a byte array of the specified size value.
        /// </summary>
        /// <param name="size">The size value to convert into a byte array.</param>
        /// <returns>A byte array of the specified size value.</returns>
        [CLSCompliant(false)]
        protected byte[] GetSizeBytes(UInt32 size)
        {
            return BitConverter.GetBytes(size);
        }

        /// <summary>
        /// Returns a record-formatted byte array with the specified record ID and contents.
        /// </summary>
        /// <param name="recordId">The ID of the record to create.</param>
        /// <param name="contents">The byte array contents of the record.</param>
        /// <returns>A record-formatted byte array.</returns>
        protected byte[] GetRecord(byte recordId, params byte[] contents)
        {
            byte[] record;
            if (contents != null && contents.Length > 0)
            {
                record = new byte[contents.Length + 1];
                record[0] = recordId;
                record.Merge(contents, 1, contents.Length);
            }
            else
            {
                record = new byte[] { recordId };
            }
            return record;
        }
    }
}
