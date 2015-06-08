using System;
using System.Collections.Generic;
using Mercury.Logging.Configuration;

namespace Mercury.Logging.Loggers
{
    /// <summary>
    /// Represents an in-memory logger that overwrites its oldest values when the buffer size is exceeded.
    /// </summary>
    public class MemoryLogger
        : Logger, IInitializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.Loggers.MemoryLogger"/> class.
        /// </summary>
        public MemoryLogger()
        {
            this.BufferSize = 53;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.MemoryLogger"/> class with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">The maximum number of elements to log in-memory.</param>
        public MemoryLogger(int bufferSize)
        {
            this.BufferSize = bufferSize;
            this.__Init();
        }

        private object _logLock = new object();
        private LogEntry[] _bufferLog;
        private int _writeIndex;

        /// <summary>
        /// Gets the maximum number of elements that can be logged in-memory.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets a value indicating whether the buffer has been filled.
        /// </summary>
        public bool Filled { get; private set; }

        /// <summary>
        /// Gets the index position of the write head.
        /// </summary>
        public int Head
        {
            get { return this._writeIndex; }
        }

        void IInitializable.Initialize()
        {
            this.__Init();
        }

        private void __Init()
        {
            this._bufferLog = new LogEntry[this.BufferSize];
        }

        /// <summary>
        /// Copies and returns all elements in the buffer as a new array.
        /// </summary>
        /// <returns></returns>
        public LogEntry[] CopyLog()
        {
            LogEntry[] temp = new LogEntry[this.BufferSize];
            Array.Copy(this._bufferLog, 0, temp, 0, this.BufferSize);
            return temp;
        }

        /// <summary>
        /// Returns the log as an enumerable sequence.
        /// </summary>
        /// <returns>An enumerable sequence of log entries.</returns>
        public IEnumerable<LogEntry> GetLogAsEnumerable()
        {
            foreach (LogEntry entry in this._bufferLog)
            {
                yield return entry;
            }
        }

        /// <summary>
        /// Returns the log as an enumerable sequence while sequentially removing its values.
        /// </summary>
        /// <returns>The log as an enumerable sequence.</returns>
        public IEnumerable<LogEntry> GetConsumingEnumerable()
        {
            int i = 0;
            foreach (LogEntry entry in this._bufferLog)
            {
                lock (this._logLock)
                {
                    if (i == this._bufferLog.Length - 1)
                    {
                        this._writeIndex = 0;
                        this.Filled = false;
                    }
                    this._bufferLog[i] = null;
                    i++;
                }
                yield return entry;
            }
            this._SearchAndCompact();
        }

        /// <summary>
        /// Removes all elements from the log.
        /// </summary>
        public void Clear()
        {
            lock (this._logLock)
            {
                Array.Clear(this._bufferLog, 0, this._bufferLog.Length);
                this._writeIndex = 0;
                this.Filled = false;
            }
        }

        /// <summary>
        /// Compacts any null spaces in the buffer and resets the write head.
        /// </summary>
        public void Compact()
        {
            this._SearchAndCompact();
        }

        /// <summary>
        /// Attempts to remove the value at the specified index.
        /// </summary>
        /// <param name="atIndex">The index of the value to be removed.</param>
        /// <param name="value">Out. The value that was removed.</param>
        /// <returns>True if the value at the specified index was removed; otherwise, false.</returns>
        public bool Remove(int atIndex, out string value)
        {
            value = null;
            LogEntry entry = null;
            if (this.Remove(atIndex, out entry))
            {
                value = entry.Message;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to remove the log entry at the specified index.
        /// </summary>
        /// <param name="atIndex">The index of the value to be removed.</param>
        /// <param name="value">Out. The log entry that was removed.</param>
        /// <returns>True if the log entry at the specified index was removed; otherwise, false.</returns>
        public bool Remove(int atIndex, out LogEntry value)
        {
            value = null;
            if (atIndex < 0 || atIndex >= this._bufferLog.Length)
                return false;
            return this.RemoveFromBuffer(atIndex, out value);
        }

        /// <summary>
        /// Writes the specified output directly to the memory buffer at the specified index.
        /// </summary>
        /// <param name="index">The index at which to write in the memory buffer.</param>
        /// <param name="output">The output text to write.</param>
        /// <returns>True if the write operation was successful; otherwise, false.</returns>
        public bool Write(int index, string output)
        {
            return this.Write(index, this.GetEntry(this.Name, LogOptions.None, LogSeverity.Info, 0, null, output, null, false, true));
        }

        /// <summary>
        /// Writes the specified output directly to the memory buffer at the specified index.
        /// </summary>
        /// <param name="index">The index at which to write in the memory buffer.</param>
        /// <param name="output">The log entry to write.</param>
        /// <returns>True if the write operation was successful; otherwise, false.</returns>
        public bool Write(int index, LogEntry output)
        {
            if (index < 0 || index >= this.BufferSize)
                return false;
            lock (this._logLock)
            {
                this._bufferLog[index] = output;
                return true;
            }
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Logs the specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to log.</param>
        /// <returns>True if the log was successful; otherwise, false.</returns>
        protected override bool DoLogEntry(LogEntry entry)
        {
            try
            {
                var formattedEntry = this.ConstructEntry(entry.LoggerName, entry.Severity, entry.EventId, entry.LoggedDateTime, entry.Timestamp,
                    entry.Callstack, null, this.Formatter.Format(entry), entry.Args, entry.FormatMessageArguments, entry.WriteDirect);
                this.WriteToBuffer(formattedEntry);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            try
            {
                this.WriteToBuffer(this.GetEntry(this.Name, LogOptions.None, LogSeverity.Info, 0, null, message, null, false, true));
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes to the in-memory buffer.
        /// </summary>
        /// <param name="entry">The entry string to write.</param>
        protected void WriteToBuffer(LogEntry entry)
        {
            lock (this._logLock)
            {
                if (this._writeIndex == this.BufferSize - 1)
                    this.Filled = true;
                if (this._writeIndex >= this.BufferSize)
                    this._writeIndex = 0;
                this._bufferLog.SetValue(entry, this._writeIndex);
                this._writeIndex++;
            }
        }

        /// <summary>
        /// Removes the value at the specified index from the buffer.
        /// </summary>
        /// <param name="atIndex">The index of the value to remove.</param>
        /// <param name="value">Out. The value that was removed.</param>
        /// <returns>True if the value at the specified index was removed; otherwise, false.</returns>
        protected bool RemoveFromBuffer(int atIndex, out LogEntry value)
        {
            lock (this._logLock)
            {
                value = null;
                if (atIndex < 0 || atIndex >= this._bufferLog.Length)
                    return false;
                value = this._bufferLog[atIndex];
                this._DeleteIndexAndCompactBuffer(atIndex);
                return true;
            }
        }

        private void _DeleteIndexAndCompactBuffer(int index)
        {
            if (index > -1 && index < this.BufferSize)
            {
                this._bufferLog[index] = null;
                // Calculate compaction
                var lastItemIndex = Math.Max(this._writeIndex - 1, this.BufferSize - 1);
                var copyLength = lastItemIndex - index;
                if (copyLength > 0)
                {
                    Array.Copy(this._bufferLog, index + 1, this._bufferLog, index, copyLength);
                    this._bufferLog[lastItemIndex] = null;
                }
                if (this._writeIndex > index)
                    this._writeIndex--;
            }
        }

        private void _SearchAndCompact()
        {
            LogEntry value = null;
            int lastNullIndex;
            int compactIndex;
            int copyLength;
            for (int i = 0; i < this._bufferLog.Length; i++)
            {
                value = this._bufferLog[i];
                if (value == null)
                {
                    this.Filled = false;
                    lastNullIndex = i;
                    compactIndex = i;
                    //search
                    while (true)
                    {
                        compactIndex++;
                        if (compactIndex >= this._bufferLog.Length)
                            break;
                        value = this._bufferLog[compactIndex];
                        if (value != null)
                            break;
                    }
                    this._writeIndex = lastNullIndex;
                    if (compactIndex >= this._bufferLog.Length)
                        break;
                    copyLength = this._bufferLog.Length - compactIndex;
                    Array.Copy(this._bufferLog, compactIndex, this._bufferLog, lastNullIndex, copyLength);
                    // clear end of array
                    this._writeIndex = lastNullIndex + copyLength;
                    for (int j = this._writeIndex; j < this._bufferLog.Length; j++)
                        this._bufferLog[j] = null;
                }
            }
        }
    }
}
