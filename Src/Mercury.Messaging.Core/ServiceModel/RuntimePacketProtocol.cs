using System;
using System.Text;
using Mercury.Net;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// A message framing protocol for all runtime packets sent over the network.
    /// </summary>
    public static class RuntimePacketProtocol
    {
        /// <summary>
        /// Max message size of approximately 40 MB.
        /// </summary>
        public static readonly int MaxPacketSize = 41943040;

        /// <summary>
        /// The default encoding for packet message data.
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.Unicode;

        /// <summary>
        /// The defaul encoding used to encode parts of the message header.
        /// </summary>
        public static readonly Encoding FrameEncoding = Encoding.UTF8;

        /// <summary>
        /// Returns a serialized packet that contains the specified XML data.
        /// </summary>
        /// <param name="xmlData">The XML data for which to generate a packet.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A serialized packet that contains the specified XML data.</returns>
        public static byte[] GetPacket(string xmlData, int messageId)
        {
            return GetPacket(GetBytes(xmlData), messageId);
        }

        /// <summary>
        /// Returns a serialized packet that contains the specified byte data.
        /// </summary>
        /// <param name="data">The byte data for which to generate a packet.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A serialized packet that contains the specified byte data.</returns>
        public static byte[] GetPacket(byte[] data, int messageId)
        {
            if (data == null)
                data = new byte[] { };

            if (data.Length > RuntimePacketProtocol.MaxPacketSize)
                throw new ArgumentException(string.Format(Properties.Strings.Culture, Properties.Strings.Packet_Protocol_Size_Violation, RuntimePacketProtocol.MaxPacketSize));

            byte[] header = GetPacketHeader(data.Length, messageId);
            byte[] packet = new byte[header.Length + data.Length + 1];
            packet.Merge(header, 0, header.Length);
            packet.Merge(data, header.Length, data.Length);
            packet.Merge(GetPacketEnd(), header.Length + data.Length, 1);

            return packet;
        }

        /// <summary>
        /// Returns a packet header containing version info and envelope size.
        /// </summary>
        /// <param name="payloadSize">The size of the data in bytes.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A packet header containing version info and envelope size.</returns>
        public static byte[] GetPacketHeader(int payloadSize, int messageId)
        {
            // Get parts
            byte[] version = GetPacketVersion();
            byte[] id = BitConverter.GetBytes(messageId);
            byte[] size = GetSizedEnvelope(payloadSize);
            
            // Merge and return
            byte[] header = new byte[version.Length + id.Length + size.Length];
            header.Merge(version, 0, version.Length);
            header.Merge(id, version.Length, id.Length);
            header.Merge(size, version.Length + id.Length, size.Length);
            return header;
        }

        /// <summary>
        /// Returns a sized envelope record (5).
        /// </summary>
        /// <param name="payloadSize">The size of the data in bytes.</param>
        /// <returns>a sized envelope record (5).</returns>
        public static byte[] GetSizedEnvelope(int payloadSize)
        {
            // Sized envelope - 5
            byte[] bytes = new byte[4];
            bytes.Merge(GetBytes(payloadSize), 0, 4);
            return GetRecord(0x05, bytes);
        }

        /// <summary>
        /// Returns a packet version record (0) containing version info.  
        /// The version should be 0.0, or 0x00.0x00.
        /// </summary>
        /// <returns>A packet version record (0).</returns>
        public static byte[] GetPacketVersion()
        {
            // Record 0 - version info
            return GetRecord(0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Returns a packet fault record (11) with the specified fault code.
        /// </summary>
        /// <param name="faultCode">The number specifying the type of fault.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A packet fault record (11).</returns>
        [CLSCompliant(false)]
        public static byte[] GetPacketFault(ushort faultCode, int messageId)
        {
            // Record 11 - Fault packet
            byte[] payload = new byte[6];
            payload.Merge(GetBytes(faultCode), 0, 2);
            payload.Merge(GetBytes(messageId), 2, 4);
            return GetRecord(0x0b, payload);
        }

        /// <summary>
        /// Returns a packet end record (12).
        /// </summary>
        /// <returns>A packet end record (12).</returns>
        public static byte[] GetPacketEnd()
        {
            // Record 12 - End packet
            return GetRecord(0x0c);
        }

        /// <summary>
        /// Returns a packet ack record (13).
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A packet ack record (13).</returns>
        public static byte[] GetPacketAck(int messageId)
        {
            // Record 13 - packet ack
            return GetRecord(0x0d, GetBytes(messageId));
        }

        /// <summary>
        /// Gets a wait time packet record (10) for the specified wait and time interval.
        /// </summary>
        /// <param name="wait">The number of units to wait according to the specified time interval.</param>
        /// <param name="interval">The time interval to wait.</param>
        /// <returns>A wait time packet record (10) for the specified wait and time interval.</returns>
        public static byte[] GetPacketWaitTime(int wait, TimeInterval interval)
        {
            // Get data
            byte[] data = new byte[5];
            data[0] = (byte)interval;
            byte[] waitBytes = GetBytes(wait);
            data.Merge(waitBytes, 1, waitBytes.Length);

            // Record 10 - Wait time
            return GetRecord(0x0a, data);
        }

        /// <summary>
        /// Returns a record-formatted byte array with the specified record ID and contents.
        /// </summary>
        /// <param name="recordId">The ID of the record to create.</param>
        /// <param name="contents">The byte array contents of the record.</param>
        /// <returns>A record-formatted byte array.</returns>
        public static byte[] GetRecord(byte recordId, params byte[] contents)
        {
            byte[] record = new byte[] { recordId };
            if (contents != null && contents.Length > 0)
                record = record.Append(contents);
            return record;
        }

        /// <summary>
        /// Returns a byte array of the specified value.
        /// </summary>
        /// <param name="value">The value to convert to a byte array.</param>
        /// <returns>A byte array of the specified value.</returns>
        public static byte[] GetBytes(int value)
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
        /// Returns a byte array of the specified value.
        /// </summary>
        /// <param name="value">The value to convert to a byte array.</param>
        /// <returns>A byte array of the specified value.</returns>
        [CLSCompliant(false)]
        public static byte[] GetBytes(UInt16 value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Returns a byte array of the specified value.
        /// </summary>
        /// <param name="value">The value to convert to a byte array.</param>
        /// <returns>A byte array of the specified value.</returns>
        public static byte[] GetBytes(string value)
        {
            return RuntimePacketProtocol.DefaultEncoding.GetBytes(value);
        }

        /// <summary>
        /// Returns the string representation of the specified byte data.
        /// </summary>
        /// <param name="value">A byte array containing the data to decode.</param>
        /// <param name="offset">The offset integer at which to start decoding.</param>
        /// <param name="length">The number of bytes to decode.</param>
        /// <returns>The string representation of the decoded byte data.</returns>
        public static string GetDataString(byte[] value, int offset, int length)
        {
            return RuntimePacketProtocol.DefaultEncoding.GetString(value, offset, length);
        }
    }
}
