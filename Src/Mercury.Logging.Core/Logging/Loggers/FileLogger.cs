using System;
using System.IO;
using System.Text;
using Mercury.Logging.Configuration;

namespace Mercury.Logging.Loggers
{
    /// <summary>
    /// Represents a logger that writes to a text file.
    /// </summary>
    public class FileLogger
        : Logger, IDisposable, IInitializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.Loggers.FileLogger"/> class.
        /// </summary>
        public FileLogger()
        {
            this.Encoding = Encoding.UTF8;
            this.Threshold = 1;
            this.WriteOnly = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.FileLogger"/> 
        /// class with the specified file path.
        /// </summary>
        /// <param name="filePath">The full system path to the log file.</param>
        public FileLogger(string filePath)
            : this(filePath, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.FileLogger"/> 
        /// class with the specified file path, encoding, and writer attributes.
        /// </summary>
        /// <param name="filePath">The full system path to the log file.</param>
        /// <param name="encoding">The text encoding used for this file.</param>
        public FileLogger(string filePath, Encoding encoding)
            : this(filePath, encoding, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.FileLogger"/> 
        /// class with the specified file path, encoding, and writer attributes.
        /// </summary>
        /// <param name="filePath">The full system path to the log file.</param>
        /// <param name="encoding">The text encoding used for this file.</param>
        /// <param name="writeOnly">A value indicating whether this file logger is only enabled for writing.</param>
        public FileLogger(string filePath, Encoding encoding, bool writeOnly)
        {
            this.FilePath = filePath;
            this.Encoding = encoding;
            this.Threshold = 1;
            this.WriteOnly = writeOnly;
            this.__Init();
        }

        private const int BUFFER_SIZE = 8192;
        private object writerLock = new object();
        private Stream m_logFile;
        private int m_writeCount;
        private bool instanceDisposed;
        private long _writePosition;

        /// <summary>
        /// Gets or sets the full system path to the log file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the text encoding used for this file.  The default value is UTF16 (little-endian).
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of append operations that can trigger a flush on the file stream.
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// Determines whether this file logger has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.instanceDisposed; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this file logger is only enabled for writing.
        /// </summary>
        /// <returns>True if this file logger is only enabled for writing; otherwise, false.</returns>
        public bool WriteOnly { get; set; }

        /// <summary>
        /// Gets the current position of the write head in the log file stream.
        /// </summary>
        public long WritePosition
        {
            get { return this._writePosition; }
        }

        void IInitializable.Initialize()
        {
            this.__Init();
        }

        private void __Init()
        {
            var parentDir = Directory.GetParent(this.FilePath).FullName;
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);
        }

        /// <summary>
        /// Closes the underlying file stream.  Subsequent logging calls will attempt to get 
        /// a new stream by calling the GetFileStream method.
        /// </summary>
        public void CloseStream()
        {
            if (this.m_logFile != null)
            {
                lock (this.writerLock)
                {
                    this.__UnsafeDisposeFileStream();
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="System.IO.StreamReader"/> that can read from the log file.  Attempting 
        /// to call this method in WriteOnly mode will throw an InvalidOperationException.
        /// </summary>
        /// <returns>A <see cref="System.IO.StreamReader"/> that can read from the log file.</returns>
        public StreamReader GetReader()
        {
            if (this.WriteOnly)
                throw new InvalidOperationException("Cannot read from a file log operating in write-only mode.");
            return new StreamReader(this.m_logFile, this.Encoding);
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
            lock (this.writerLock)
            {
                this.__UnsafeEnsureFileStream();
                this.m_logFile.Flush();
            }
        }

        internal bool CallEnsurePosition()
        {
            return this.EnsurePosition();
        }

        /// <summary>
        /// Ensures that the position of the write head in the file stream matches the write position.
        /// </summary>
        protected virtual bool EnsurePosition()
        {
            if (this.m_logFile == null)
                return false;
            lock (this.writerLock)
            {
                this.__UnsafeEnsureFileStream();
                this.__UnsafeSetPosition(this.WritePosition);
                return true;
            }
        }

        /// <summary>
        /// Gets the file stream used to write to the log file.
        /// </summary>
        /// <param name="logFilePath">The full system path to the log file.</param>
        /// <param name="isWriteOnly">A value indicating whether this file logger is only enabled for writing.</param>
        /// <param name="defaultBufferSize">The default buffer size used by the logger.</param>
        /// <returns>The file stream used to write to the log file.</returns>
        protected virtual Stream GetFileStream(string logFilePath, bool isWriteOnly, int defaultBufferSize)
        {
            // Ensure parent directory path
            var parentDir = Directory.GetParent(logFilePath).FullName;
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);

            // Create stream
            if (isWriteOnly)
                return new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, defaultBufferSize);
            else
                return new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, defaultBufferSize);
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            if (message == null)
                return false;
            lock (this.writerLock)
            {
                this.__UnsafeEnsureFileStream();
                var written = this.__UnsafeWriteToFile(message);
                var flushed = this.__UnsafeConditionalFlush();
                return written & flushed;
            }
        }

        #region Critical, non-thread-safe methods
        private void __UnsafeEnsureFileStream()
        {
            if (!this.instanceDisposed && this.m_logFile == null)
                this.m_logFile = this.GetFileStream(this.FilePath, this.WriteOnly, BUFFER_SIZE);
        }

        private void __UnsafeDisposeFileStream()
        {
            if (this.m_logFile != null)
            {
                this.m_logFile.Dispose();
                this.m_logFile = null;
            }
        }

        private void __UnsafeSetPosition(long setPosition)
        {
            this.m_logFile.Position = setPosition;
        }

        private bool __UnsafeWriteToFile(string message)
        {
            if (message == null)
                return false;
            if (this.m_logFile == null || !this.m_logFile.CanWrite)
                throw new ObjectDisposedException(this.GetType().Name);
            try
            {
                var buffer = this.Encoding.GetBytes(message);
                this.m_logFile.Write(buffer, 0, buffer.Length);
                this._writePosition = this.m_logFile.Position;
                return true;
            }
            catch
            {
            }
            return false;
        }

        private bool __UnsafeConditionalFlush()
        {
            try
            {
                this.m_writeCount++;
                if (this.m_writeCount >= this.Threshold)
                    this.m_logFile.Flush();
                return true;
            }
            catch // Ignore exceptions and just return false.
            {
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Dispsoses of the resources associated with this logger.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (!this.instanceDisposed)
            {
                lock (this.writerLock)
                {
                    this.__UnsafeDisposeFileStream();
                }
            }
            this.instanceDisposed = true;
        }
    }
}
