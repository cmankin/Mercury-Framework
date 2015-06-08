using System;
using System.Net.Sockets;
using System.Text;

namespace Mercury.Messaging.ServiceModel
{
    /// <summary>
    /// Runtime packet object for asynchronous data reads.
    /// </summary>
    public class PacketState
    {
        /// <summary>
        /// Initializes a default instance of the PacketState class.
        /// </summary>
        public PacketState()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a default instance of the PacketState class with the specified socket.
        /// </summary>
        /// <param name="workSocket">The socket on which to receive data.</param>
        public PacketState(Socket workSocket)
        {
            this.WorkSocket = workSocket;
        }

        /// <summary>
        /// Gets or sets an object representing data that can be maintained across asynchronous calls to the work socket.
        /// </summary>
        public object Storage { get; set; }

        private int _messageId = -1;

        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        public int MessageId
        {
            get { return this._messageId; }
            set { this._messageId = value; }
        }

        /// <summary>
        /// Client socket.
        /// </summary>
        public Socket WorkSocket { get; set; }

        /// <summary>
        /// Gets the value of byte index 0 for this message.
        /// </summary>
        public byte Byte0 { get; set; }

        /// <summary>
        /// Gets the value of byte index 1 for this message.
        /// </summary>
        public byte Byte1 { get; set; }

        /// <summary>
        /// Gets the value of byte index 2 for this message.
        /// </summary>
        public byte Byte2 { get; set; }

        /// <summary>
        /// The size of the receive buffer.
        /// </summary>
        public const int BUFFER_SIZE = 8192;

        /// <summary>
        /// The end record byte.
        /// </summary>
        public const byte END_RECORD = 0x0c;

        private byte[] _buffer = new byte[BUFFER_SIZE];

        /// <summary>
        /// Gets the receive buffer.
        /// </summary>
        public byte[] Buffer 
        {
            get { return this._buffer; }
        }

        private int _appendPos;
        private byte[] _builder;

        /// <summary>
        /// Gets a byte builder for data collection.
        /// </summary>
        public byte[] Builder
        {
            get { return this._builder; }
        }

        /// <summary>
        /// Initializes the builder array to the specified size.
        /// </summary>
        /// <param name="size">The size of the builder array.</param>
        public void InitializeBuilder(int size)
        {
            this._builder = new byte[size];
        }

        /// <summary>
        /// Appends the specified bytes to the builder at the specified offset and length.
        /// </summary>
        /// <param name="bytes">The byte array to append.</param>
        /// <param name="offset">The starting offset on the byte array for the append.</param>
        /// <param name="length"></param>
        public void Append(byte[] bytes, int offset, int length)
        {
            if (this.Builder != null)
            {
                Array.Copy(bytes, offset, this.Builder, this._appendPos, length);
                this._appendPos = this._appendPos + length;
            }
        }

        private StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        /// Gets the string builder for building received data strings.
        /// </summary>
        public StringBuilder StringBuilder
        {
            get { return this._stringBuilder; }
        }

        /// <summary>
        /// The number of bytes received.
        /// </summary>
        public int ReceivedBytes = 0;

        /// <summary>
        /// A value indicating whether the packet header has been fully received.
        /// </summary>
        public bool ReceivedHeader = false;

        private int _recordSize = 0;

        /// <summary>
        /// Gets the expected size in bytes of the data collected by the builder.
        /// </summary>
        public int RecordSize
        {
            get { return this._recordSize; }
            set
            {
                this._recordSize = value;
                this.InitializeBuilder(this._recordSize);
            }
        }

    }
}
