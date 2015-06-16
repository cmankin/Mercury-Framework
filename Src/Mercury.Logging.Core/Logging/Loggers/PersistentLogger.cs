using System;
using System.Threading;
using Mercury.Logging.Configuration;

namespace Mercury.Logging.Loggers
{
    /// <summary>
    /// Provides a redundancy mechanism for another logger, ensuring that logs 
    /// will be kept in-memory until they can be written to the log source.
    /// </summary>
    public class PersistentLogger
        : Logger, IInitializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.Loggers.PersistentLogger"/> class.
        /// </summary>
        public PersistentLogger()
        {
            this._bufferSize = 53;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.PersistentLogger"/> class with the specified values.
        /// </summary>
        /// <param name="logger">The logger on which to write.</param>
        /// <param name="bufferSize">The maximum number of log entries to maintain.</param>
        /// <param name="retryInterval">The number of seconds to wait before retrying an unsuccessful log.</param>
        public PersistentLogger(Logger logger, int bufferSize, int retryInterval)
        {
            this.Logger = logger;
            this._bufferSize = bufferSize;
            this.RetryInterval = retryInterval;
            this.Init();
        }

        private int _bufferSize;
        private MemoryLogger _inMemoryCache;
        private Timer _timer;
        private volatile int _canEnter;
        private volatile int _isCanceled;

        /// <summary>
        /// Gets the logger on which to write.
        /// </summary>
        public Logger Logger { get; set; }

        /// <summary>
        /// Gets or sets maximum number of log entries to maintain.
        /// </summary>
        public int BufferSize
        {
            get { return this._bufferSize; }
            set { this._bufferSize = value; }
        }

        /// <summary>
        /// Gets the number of seconds to wait before retrying an unsuccessful log.
        /// </summary>
        public int RetryInterval { get; set; }

        void IInitializable.Initialize()
        {
            this.Init();
        }

        /// <summary>
        /// Initializes the <see cref="Mercury.Logging.Loggers.PersistentLogger"/> instance.
        /// </summary>
        protected virtual void Init()
        {
            this._inMemoryCache = new MemoryLogger(this._bufferSize);
            if (this._timer != null)
                this._timer.Dispose();
            this._timer = new Timer(o => this._RetryWriteFromMemory(), null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
            if (Interlocked.CompareExchange(ref this._isCanceled, 1, 0) == 0)
                this._DirectWriteFromMemory();
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            if (this.Logger.Write(message))
                return true;
            // Write to the in-memory store
            this._inMemoryCache.Write(message);
            if (Interlocked.CompareExchange(ref this._canEnter, 1, 0) == 0)
                this._timer.Change(this.RetryInterval * 1000, Timeout.Infinite);
            return false;
        }

        private void _RetryWriteFromMemory()
        {
            if (Interlocked.CompareExchange(ref this._isCanceled, 0, 0) == 0)
            {
                if (!this._DirectWriteFromMemory())
                    return;
            }
            if (Interlocked.CompareExchange(ref this._canEnter, 0, 1) == 1)
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private bool _DirectWriteFromMemory()
        {
            var log = this._inMemoryCache.GetConsumingEnumerable();
            int i = 0;
            foreach (LogEntry entry in log)
            {
                if (entry == null)
                    continue;
                if (!this.Logger.Log(entry))
                {
                    this._inMemoryCache.Write(i, entry);
                    this._inMemoryCache.Compact();
                    var flag = this._timer.Change(this.RetryInterval * 1000, Timeout.Infinite);
                    return false;
                }
                i++;
            }
            return true;
        }
    }
}
