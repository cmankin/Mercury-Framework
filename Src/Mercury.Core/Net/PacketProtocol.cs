using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mercury.Net
{
    /// <summary>
    /// Represents a data packet protocol for message framing
    /// </summary>
    public class PacketProtocol
    {
        /// <summary>
        /// Initializes a default instance of the PacketProtocol 
        /// class with the specified header and string data.
        /// </summary>
        /// <param name="header">The PacketHeader to prepend to this packet.</param>
        /// <param name="data">The string data contained in this packet.</param>
        public PacketProtocol(PacketHeader header, string data)
            : this(header, header.EnvelopeEncoding.Encoding.GetBytes(data))
        {
        }
        
        /// <summary>
        /// Initializes a default instance of the PacketProtocol 
        /// class with the specified header and byte data.
        /// </summary>
        /// <param name="header">The PacketHeader to prepend to this packet.</param>
        /// <param name="data">The byte data that comprises the packet.</param>
        public PacketProtocol(PacketHeader header, byte[] data)
        {
            this._header = header;
            this._data = data;
        }

        private PacketHeader _header;

        /// <summary>
        /// Gets the data packet header.
        /// </summary>
        public PacketHeader Header
        {
            get { return this._header; }
        }

        private byte[] _data;

        /// <summary>
        /// Gets the byte data of this packet.
        /// </summary>
        public byte[] Data
        {
            get { return this._data; }
        }

        /// <summary>
        /// Returns a byte array representation of this packet.
        /// </summary>
        /// <returns>A byte array representation of this packet.</returns>
        public byte[] GetPacket()
        {
            return PacketProtocol.GetPacket(this.Header, this.Data);
        }

        /// <summary>
        /// Returns a byte array packet assembled from the specified values.
        /// </summary>
        /// <param name="header">The packet header to use.</param>
        /// <param name="data">The data byte array.</param>
        /// <returns>A byte array packet assembled from the specified values.</returns>
        public static byte[] GetPacket(PacketHeader header, byte[] data)
        {
            if (data == null)
                data = new byte[] { };

            byte[] headerBytes = null;
            if (header != null)
                headerBytes = header.GetBytes();

            // Get packet
            return PacketProtocol.GetPacket(headerBytes, data);
        }

        /// <summary>
        /// Returns a byte array packet assembled from the specified byte array values.
        /// </summary>
        /// <param name="header">The header byte array.</param>
        /// <param name="data">The data byte array.</param>
        /// <returns>A byte array packet assembled from the specified byte array values.</returns>
        public static byte[] GetPacket(byte[] header, byte[] data)
        {
            return GetPacket(header, data, (ushort)0);
        }

        /// <summary>
        /// Returns a byte array packet assembled from the specified byte array values.
        /// </summary>
        /// <param name="header">The header byte array.</param>
        /// <param name="data">The data byte array.</param>
        /// <param name="extra">Extra info for the envelope record.</param>
        /// <returns>A byte array packet assembled from the specified byte array values.</returns>
        [CLSCompliant(false)]
        public static byte[] GetPacket(byte[] header, byte[] data, ushort extra)
        {
            if (data == null)
                data = new byte[] { };
            if (header == null)
                header = new byte[] { };

            // Get packet
            byte[] envelope = GetSizedEnvelope((uint)data.Length, extra);
            byte[] packet = new byte[header.Length + envelope.Length + data.Length];
            GetPacket(packet, header, envelope, data);

            return packet;
        }

        /// <summary>
        /// Fills the specified packet byte array with the bytes from the specified header, envelope, and data.
        /// </summary>
        /// <param name="packet">The packet byte array to fill.  
        /// This must be sized to contain all specified byte data.</param>
        /// <param name="header">The header byte array.</param>
        /// <param name="envelope">The envelope byte array.</param>
        /// <param name="data">The data byte array.</param>
        public static void GetPacket(byte[] packet, byte[] header, byte[] envelope, byte[] data)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");
            if (packet.Length < (header.Length + envelope.Length + data.Length))
                throw new ArgumentException("The packet argument must be sized to include the header, envelope, and data.");

            packet.Merge(header, 0, header.Length);
            packet.Merge(envelope, header.Length, envelope.Length);
            packet.Merge(data, header.Length + envelope.Length, data.Length);
        }

        /// <summary>
        /// Returns a packet header generated from the specified values.
        /// </summary>
        /// <param name="via">The URI address destination for subsequent messages.</param>
        /// <returns>A packet header generated from the specified values.</returns>
        public static PacketHeader GenerateHeader(string via)
        {
            return GenerateHeader(CommunicationMode.SingletonSized, via, EnvelopeEncoding.Utf8, EnvelopeStructure.XmlFormattedString);
        }

        /// <summary>
        /// Returns a packet header generated from the specified values.
        /// </summary>
        /// <param name="mode">The communication mode being used.</param>
        /// <param name="via">The URI address destination for subsequent messages.</param>
        /// <param name="encoding">The envelope encoding scheme.</param>
        /// <param name="structure">The envelope structure.</param>
        /// <returns>A packet header generated from the specified values.</returns>
        public static PacketHeader GenerateHeader(CommunicationMode mode, string via, EnvelopeEncoding encoding, EnvelopeStructure structure)
        {
            return new PacketHeader(PacketProtocol.MajorVersion, PacketProtocol.MinorVersion, mode, via, encoding, structure);
        }

        /// <summary>
        /// The protocol major version.
        /// </summary>
        public static readonly byte MajorVersion = 1;

        /// <summary>
        /// The protocol minor version.
        /// </summary>
        public static readonly byte MinorVersion = 1;

        /// <summary>
        /// Returns a sized envelope record (5).
        /// </summary>
        /// <param name="payloadSize">The size of the payload.</param>
        /// <param name="extra">Extra info.</param>
        /// <returns>A sized envelope record</returns>
        [CLSCompliant(false)]
        public static byte[] GetSizedEnvelope(uint payloadSize, ushort extra)
        {
            // Sized envelope - 5
            byte[] bytes = new byte[6];
            bytes.Merge(GetBytes(payloadSize), 0, 4);
            bytes.Merge(GetBytes(extra), 4, 2);
            return GetRecord(0x05, bytes);
        }

        /// <summary>
        /// Returns a preamble ack record (8).
        /// </summary>
        /// <returns>A preamble ack record</returns>
        public static byte[] GetPreambleAck()
        {
            // Preamble ack record - 8
            return GetRecord(0x08);
        }

        /// <summary>
        /// Returns a packet fault record (7).
        /// </summary>
        /// <param name="error">The packet error associated with this record.</param>
        /// <returns>A packet fault record.</returns>
        public static byte[] GetPacketFault(PacketError error)
        {
            // Packet fault record - 7
            return GetRecord(0x07, (byte)error);
        }

        /// <summary>
        /// Returns a packet end record (6).
        /// </summary>
        /// <returns>A packet end record</returns>
        public static byte[] GetPacketEnd()
        {
            // Packet end record - 6
            return GetRecord(0x06);
        }

        /// <summary>
        /// Returns the string representation of the specified byte data and encoding.
        /// </summary>
        /// <param name="encoding">The encoding used for the byte data.</param>
        /// <param name="data">A byte array containing the data to decode.</param>
        /// <param name="offset">The offset integer at which to start decoding.</param>
        /// <param name="length">The number of bytes to decode.</param>
        /// <returns>The string representation of the specified byte data and encoding.</returns>
        public static string GetData(Encoding encoding, byte[] data, int offset, int length)
        {
            return encoding.GetString(data, offset, length);
        }

        /// <summary>
        /// Returns a byte array of the specified value.
        /// </summary>
        /// <param name="value">The value to convert to a byte array.</param>
        /// <returns>A byte array of the specified value.</returns>
        [CLSCompliant(false)]
        public static byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Returns a byte array of the specified value.
        /// </summary>
        /// <param name="value">The value to convert to a byte array.</param>
        /// <returns>A byte array of the specified value.</returns>
        [CLSCompliant(false)]
        public static byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the specified byte data into a 16-bit unsigned integer.
        /// </summary>
        /// <param name="data">The byte data to convert.</param>
        /// <param name="offset">The integer offset at which to start.</param>
        /// <returns>A 16-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt16 ConvertToUint16(byte[] data, int offset)
        {
            return BitConverter.ToUInt16(data, offset);
        }

        /// <summary>
        /// Converts the specified byte data into a 32-bit unsigned integer.
        /// </summary>
        /// <param name="data">The byte data to convert.</param>
        /// <param name="offset">The integer offset at which to start.</param>
        /// <returns>A 32-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static uint ConvertToUint32(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }

        /// <summary>
        /// Returns a record-formatted byte array with the specified record ID and contents.
        /// </summary>
        /// <param name="recordId">The ID of the record to create.</param>
        /// <param name="contents">The byte array contents of the record.</param>
        /// <returns>A record-formatted byte array.</returns>
        protected static byte[] GetRecord(byte recordId, params byte[] contents)
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
