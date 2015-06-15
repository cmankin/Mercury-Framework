using System;
using System.Collections.Generic;
using System.Linq;
using Mercury.Logging.Configuration;

namespace Mercury.Logging.Loggers
{
    /// <summary>
    /// A logger that can log to a number of hosted loggers.
    /// </summary>
    public class CompositeLogger
        : Logger, IDisposable, IAddChild
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.Loggers.CompositeLogger"/> class.
        /// </summary>
        public CompositeLogger()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Loggers.CompositeLogger"/> 
        /// class with the specified loggers.
        /// </summary>
        /// <param name="loggers">An array of named log objects.</param>
        public CompositeLogger(params Logger[] loggers)
        {
            if (loggers != null && loggers.Length > 0)
            {
                foreach (var log in loggers)
                    this._loggers.Add(log);
            }
        }

        private IList<Logger> _loggers = new List<Logger>();
        private bool isDisposed;

        void IAddChild.AddChild(object child)
        {
            this.Add((Logger)child);
        }

        /// <summary>
        /// Adds the specified log name and logger to the composite collection.
        /// </summary>
        /// <param name="logName">The name associated with the specified logger.</param>
        /// <param name="logger">The logger to add.</param>
        public void Add(string logName, Logger logger)
        {
            logger.Name = logName;
            this.Add(logger);
        }

        /// <summary>
        /// Adds the specified named log to the composite collection.
        /// </summary>
        /// <param name="logger">A named log object to add.</param>
        public void Add(Logger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            this._loggers.Add(logger);
        }

        /// <summary>
        /// Returns the first instance of a logger associated with the specified name.
        /// </summary>
        /// <param name="logName">The name of the logger to find.</param>
        /// <returns>The first instance of a logger associated with the specified name or null.</returns>
        public Logger Find(string logName)
        {
            if (logName == null)
                return null;
            var result = this._loggers.FirstOrDefault<Logger>((log) => log.Name == logName);
            return result;
        }

        /// <summary>
        /// Returns all loggers that are associated with the specified name.
        /// </summary>
        /// <param name="logName">The name of the loggers to find.</param>
        /// <returns>A list of all loggers that are associated with the specified name.</returns>
        public IList<Logger> FindAll(string logName)
        {
            var resList = new List<Logger>();
            if (logName != null)
            {
                foreach (var log in this._loggers)
                {
                    if (log.Name == logName)
                        resList.Add(log);
                }
            }
            return resList;
        }

        /// <summary>
        /// Gets a list of all <see cref="Mercury.Logging.Logger"/> objects attached to this instance.
        /// </summary>
        /// <returns>A list of all <see cref="Mercury.Logging.Logger"/> objects attached to this instance.</returns>
        public IList<Logger> GetAll()
        {
            var list = new List<Logger>();
            foreach (var log in this._loggers)
            {
                list.Add(log);
            }
            return list;
        }

        /// <summary>
        /// Removes the first instance of a logger associated with the specified name.
        /// </summary>
        /// <param name="logName">The name of the logger to remove.</param>
        /// <returns>True if the logger was found and removed; otherwise, false.</returns>
        public bool Remove(string logName)
        {
            return this.RemoveInternal(logName, false);
        }

        /// <summary>
        /// Removes all loggers associated with the specified name.
        /// </summary>
        /// <param name="logName">The name of the logger to remove.</param>
        /// <returns>True if any loggers were found and removed; otherwise, false.</returns>
        public bool RemoveAll(string logName)
        {
            return this.RemoveInternal(logName, true);
        }

        private bool RemoveInternal(string logName, bool allEntries)
        {
            Logger log = null;
            bool flag = false;
            for (int i = 0; i < this._loggers.Count; i++)
            {
                log = this._loggers[i];
                if (log.Name == logName)
                {
                    this._loggers.RemoveAt(i);
                    flag = true;
                    i--;
                    if (!allEntries)
                        break;
                }
            }
            return flag;
            
        }

        /// <summary>
        /// Releases any resources on hosted loggers by verifying if the logger implements IDisposable and calling Dispose.
        /// </summary>
        public void Close()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Flushes any output buffers, forcing data to be written to the log.
        /// </summary>
        public override void Flush()
        {
            foreach (var log in this._loggers)
                log.Flush();
        }

        /// <summary>
        /// Logs the specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool DoLogEntry(LogEntry entry)
        {
            if (entry.Severity < this.SeverityThreshold)
                return false;
            
            Logger log = null;
            for (int i = 0; i < this._loggers.Count; i++)
            {
                log = this._loggers[i];
                if (CompositeLogger._AllowEntry(log, entry))
                    log.Log(entry);
            }
            return true;
        }

        private static bool _AllowEntry(Logger log, LogEntry entry)
        {
            if (log == null)
                return false;
            if (log.Filter == null)
                return true;
            if (entry.Severity < log.SeverityThreshold)
                return false;
            return log.Filter.Allow(entry);
        }

        /// <summary>
        /// Determines whether the specified log entry can pass filtering.
        /// </summary>
        /// <param name="entry">The log entry to test.</param>
        /// <returns>True if the log entry passes filtering; otherwise, false.</returns>
        protected override bool AllowEntry(LogEntry entry)
        {
            if (entry.Severity < this.SeverityThreshold)
                return false;
            if (this.Filter == null)
                return true;
            return this.Filter.Allow(entry);
        }

        /// <summary>
        /// Logs the specified message string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        /// <returns>True if logging was successful; otherwise, false.</returns>
        protected override bool WriteLog(string message)
        {
            foreach (var log in this._loggers)
            {
                log.Write(message);
            }
            return true;
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
                    foreach (var log in this._loggers)
                    {
                        IDisposable dispObj = log as IDisposable;
                        if (dispObj != null)
                            dispObj.Dispose();
                    }
                }
            }
            this.isDisposed = true;
        }
    }
}
