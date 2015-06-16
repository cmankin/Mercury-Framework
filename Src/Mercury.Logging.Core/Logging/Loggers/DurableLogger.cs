using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Mercury.Logging.Configuration;

namespace Mercury.Logging.Loggers
{
    /// <summary>
    /// Provides durable log entries by writing to an alternate logger that writes to the disk.
    /// </summary>
    public class DurableLogger
        : Logger, IDisposable, IInitializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.Loggers.DurableLogger"/> class.
        /// </summary>
        public DurableLogger()
        {
            this.Mode = DurabilityMode.WriteThrough;
            this.Threshold = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.DurableLogger"/> 
        /// class with the specified primary logger and file path to use for the durable logger.
        /// </summary>
        /// <param name="primary">The primary logger.</param>
        /// <param name="durableLogPath">The full path to the file to use for the durable log.</param>
        public DurableLogger(Logger primary, string durableLogPath)
            : this(primary, durableLogPath, DurabilityMode.WriteThrough, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.DurableLogger"/> 
        /// class with the specified primary logger, alternate logger and durability mode.
        /// </summary>
        /// <param name="primary">The primary logger.</param>
        /// <param name="durableLogPath">The full path to the file to use for the durable log.</param>
        /// <param name="mode">The durability mode for this logger.</param>
        public DurableLogger(Logger primary, string durableLogPath, DurabilityMode mode)
            : this(primary, durableLogPath, mode, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.DurableLogger"/> class with 
        /// the specified primary logger, alternate logger, durability mode, and batch threshold.
        /// </summary>
        /// <param name="primary">The primary logger.</param>
        /// <param name="durableLogPath">The full path to the file to use for the durable log.</param>
        /// <param name="mode">The durability mode for this logger.</param>
        /// <param name="threshold">The minimum number of write operations that can trigger a flush to the primary logger.  
        /// This is only used when the durability mode is set to DurabilityMode.BatchThrough.</param>
        public DurableLogger(Logger primary, string durableLogPath, DurabilityMode mode, int threshold)
        {
            if (primary == null)
                throw new ArgumentNullException("primary");
            if (durableLogPath == null)
                throw new ArgumentNullException("durableLogPath");
            this.Primary = primary;
            this.DurableLogPath = durableLogPath;
            this.Mode = mode;
            this.Threshold = threshold;
            this.Init();
        }

        private int m_writeCount;
        private object _lock = new object();
        private bool isDisposed;

        private LogEntryReaderWriter durableReaderWriter;

        #region DurableLogger.DurabilityMode
        /// <summary>
        /// Describes the durability setting used by a <see cref="Mercury.Logging.Loggers.DurableLogger"/>.
        /// </summary>
        public enum DurabilityMode
        {
            /// <summary>
            /// Writes to the durable disk log and then to the primary log.
            /// </summary>
            WriteThrough,

            /// <summary>
            /// Writes to durable disk storage.  Data can be flushed manually to the primary log.
            /// </summary>
            WriteDisk,

            /// <summary>
            /// Writes to the durable disk log up to a specified threshold, then flushes to the primary log.
            /// </summary>
            BatchThrough
        }
        #endregion

        #region DurableLogger.LogEntryReaderWriter
        // THIS CLASS IS NOT THREAD-SAFE.
        private class LogEntryReaderWriter : IDisposable
        {
            private const int BUFFER_SIZE = 8192;
            private BinaryFormatter serializer = new BinaryFormatter();
            private FileStream m_stream;
            private long writePosition;

            private bool isDisposed;

            public LogEntryReaderWriter(string filePath)
            {
                this.m_stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, BUFFER_SIZE);
            }

            public void Flush()
            {
                if (this.m_stream != null)
                    this.m_stream.Flush(true);
            }

            public void Write(LogEntry entry)
            {
                // Serialize will fail if the Args array contains objects that are not marked with the [Serialize] attribute.
                this.serializer.Serialize(this.m_stream, entry);
            }

            public LogEntry ReadOne()
            {
                if (this.writePosition < this.m_stream.Length)
                {
                    this.m_stream.Seek(this.writePosition, SeekOrigin.Begin);
                    var entry = this.serializer.Deserialize(this.m_stream) as LogEntry;
                    this.writePosition = this.m_stream.Position;
                    return entry;
                }
                return null;
            }

            public IList<LogEntry> ReadBatch(int maxBatchSize)
            {
                var list = new List<LogEntry>();
                while (this.writePosition < this.m_stream.Length && list.Count < maxBatchSize)
                {
                    this.m_stream.Seek(this.writePosition, SeekOrigin.Begin);
                    var entry = this.serializer.Deserialize(this.m_stream) as LogEntry;
                    list.Add(entry);
                    this.writePosition = this.m_stream.Position;
                }
                return list;
            }
            
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (!isDisposed)
                {
                    if (this.m_stream != null)
                    {
                        this.m_stream.Dispose();
                        this.m_stream = null;
                    }
                }
                isDisposed = true;
            }
        }
        #endregion

        /// <summary>
        /// Gets the primary logger.
        /// </summary>
        public Logger Primary { get; set; }

        /// <summary>
        /// Gets the full path to the file used to provide a durable log.
        /// </summary>
        public string DurableLogPath { get; set; }

        /// <summary>
        /// Gets the durability mode for this logger.
        /// </summary>
        public DurabilityMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of write operations that can trigger a flush to the primary logger.  
        /// This is only used when the durability mode is set to DurabilityMode.BatchThrough.
        /// </summary>
        public int Threshold { get; set; }

        void IInitializable.Initialize()
        {
            this.Init();
        }

        /// <summary>
        /// Initializes the <see cref="Mercury.Logging.Loggers.DurableLogger"/> instance.
        /// </summary>
        protected virtual void Init()
        {
            this.durableReaderWriter = new LogEntryReaderWriter(this.DurableLogPath);
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
            lock (this._lock)
            {
                this.__UnsafeFlush();
            }
        }

        /// <summary>
        /// Logs the specified <see cref="Mercury.Logging.LogEntry"/>.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool DoLogEntry(LogEntry entry)
        {
            return this.WriteLogEntry(entry);
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            return this.WriteLogEntry(this.Primary.GetEntry(this.Name, LogOptions.None, LogSeverity.Critical, 0, null, message, null, false, true));
        }

        /// <summary>
        /// Writes the log entry to the primary and durable loggers as configured.
        /// </summary>
        /// <param name="entry">The <see cref="Mercury.Logging.LogEntry"/> to write.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected virtual bool WriteLogEntry(LogEntry entry)
        {
            switch (this.Mode)
            {
                case DurabilityMode.WriteThrough:
                    this.__WriteToDurable(entry);
                    this.Primary.Log(entry);
                    return true;
                case DurabilityMode.WriteDisk:
                    this.__WriteToDurable(entry);
                    return true;
                case DurabilityMode.BatchThrough:
                    lock (this._lock)
                    {
                        this.__WriteToDurable(entry);
                        this.m_writeCount++;
                        if (this.m_writeCount >= this.Threshold)
                            this.__UnsafeFlush();
                        return true;
                    }
            }
            return false;
        }

        /// <summary>
        /// Dispsoses of the resources associated with this logger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispsoses of the resources associated with this logger.
        /// </summary>
        /// <param name="disposing">True if called through the IDisposable.Dispose method; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (this.durableReaderWriter != null)
                        this.durableReaderWriter.Dispose();
                    IDisposable primaryDisp = this.Primary as IDisposable;
                    if (primaryDisp != null)
                        primaryDisp.Dispose();
                }
            }
            this.isDisposed = true;
        }

        private void __WriteToDurable(LogEntry entry)
        {
            this.durableReaderWriter.Write(entry);
        }

        private void __WriteFromDurable()
        {
            var exceptions = new List<Exception>();
            var batch = this.durableReaderWriter.ReadBatch(20);
            while (batch.Count > 0)
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    try
                    {
                        this.Primary.Log(batch[i]);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
                batch = this.durableReaderWriter.ReadBatch(20);
            }

            if (exceptions.Count > 0)
                throw new AggregateException("Errors were encountered when attempting to write to the primary log.  Those entries that encountered an error are not guaranteed to have been logged.", exceptions.ToArray());
        }

        private void __UnsafeFlush()
        {
            this.durableReaderWriter.Flush();
            if (this.Mode > DurabilityMode.WriteThrough)
            {
                this.__WriteFromDurable();
            }
            this.Primary.Flush();
        }
    }
}
