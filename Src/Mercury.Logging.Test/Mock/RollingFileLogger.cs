using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Logging;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Test.Mock
{
    public class SimpleRollingFileLogger : FileLogger
    {
        private int _backupCount;

        public SimpleRollingFileLogger()
        {
        }
        public SimpleRollingFileLogger(string filePath, Encoding encoding, int maxFileSize, int maxBackupFiles, bool writeOnly)
            : base(filePath, encoding, writeOnly)
        {
            this.MaxFileSize = maxFileSize;
            this.MaxBackupFiles = maxBackupFiles;
        }

        public int MaxFileSize { get; set; }
        public int MaxBackupFiles { get; set; }

        protected override Stream GetFileStream(string logFilePath, bool isWriteOnly, int defaultBufferSize)
        {
            return base.GetFileStream(this.GetNewestFileName(), isWriteOnly, defaultBufferSize);
        }

        protected override void WriteToStream(Stream stream, byte[] buffer)
        {
            base.WriteToStream(stream, buffer);
            if (this.MaxFileSize > 0)
            {
                if ((stream.Length + buffer.LongLength) > (long)this.MaxFileSize)
                {
                    stream.Flush();
                    this.UnsafeDisposeStream();
                    if (this.MaxBackupFiles == 0)
                        File.Delete(this.FilePath);
                    if (this.MaxBackupFiles > 0)
                    {
                        if (this._backupCount >= this.MaxBackupFiles)
                            File.Delete(this.GetOldestFileName());
                        this._backupCount++;
                    }
                }
            }
        }

        private string GetOldestFileName()
        {
            var filenum = this._backupCount - this.MaxBackupFiles;
            return FormatFileName(this.FilePath, filenum < 0 ? 0 : filenum);
        }
        private string GetNewestFileName()
        {
            return FormatFileName(this.FilePath, this._backupCount);
        }
        private static string FormatFileName(string filePath, int backupCount)
        {
            if (backupCount == 0)
                return filePath;
            return Path.ChangeExtension(filePath, string.Format(".{0}{1}", backupCount, Path.GetExtension(filePath)));
        }
    }

    /// <summary>
    /// An implementation of a rolling file logger.
    /// </summary>
    public class RollingFileLogger : FileLogger
    {
        private int _backupCount;
        
        public RollingFileLogger()
        {
        }

        public RollingFileLogger(string filePath, Encoding encoding, int maxFileSize, int maxBackupFiles, bool writeOnly)
            : base(filePath, encoding, writeOnly)
        {
            this.MaxFileSize = maxFileSize;
            this.MaxBackupFiles = maxBackupFiles;
            this.Init();
        }

        /// <summary>
        /// Gets the maximum size of the file in bytes before a rollover occurs.  Log entries are not split to maintain an 
        /// exact maximum file size.  Some variation may occur.  If set to 0, it will be considered an unbounded log file.
        /// </summary>
        public int MaxFileSize { get; private set; }

        /// <summary>
        /// Gets the maximum number of backup files that will be maintained before 
        /// the oldest is erased.  If set to 0, no backup logs will be kept.
        /// </summary>
        public int MaxBackupFiles { get; private set; }

        protected override void Init()
        {
            base.Init();
        }

        protected override Stream GetFileStream(string logFilePath, bool isWriteOnly, int defaultBufferSize)
        {
            return base.GetFileStream(this.GetNewestFileName(), isWriteOnly, defaultBufferSize);
        }

        protected override void WriteToStream(Stream stream, byte[] buffer)
        {
            base.WriteToStream(stream, buffer);
            if (stream != null && this.MaxFileSize > 0)
            {
                if ((stream.Length + buffer.LongLength) > (long)this.MaxFileSize)
                {
                    stream.Flush();
                    this.UnsafeDisposeStream();
                    this.PrepareNextLogFile();
                    if (this.MaxBackupFiles > 0)
                        this._backupCount++;
                }
            }
        }

        private void PrepareNextLogFile()
        {
            if (this.MaxBackupFiles == 0)
            {
                try
                {
                    if (File.Exists(this.FilePath))
                        File.Delete(this.FilePath);
                }
                catch
                {
                }
            }
            else if (this.MaxBackupFiles > 0 && this._backupCount >= this.MaxBackupFiles)
            {
                try
                {
                    var oldest = this.GetOldestFileName();
                    if (File.Exists(oldest))
                        File.Delete(oldest);
                }
                catch
                {
                }
            }
        }

        private string GetOldestFileName()
        {
            var filenum = this._backupCount - this.MaxBackupFiles;
            filenum = filenum < 0 ? 0 : filenum;
            return FormatFileName(this.FilePath, filenum);
        }

        private string GetNewestFileName()
        {
            return FormatFileName(this.FilePath, this._backupCount);
        }

        private static string FormatFileName(string filePath, int backupCount)
        {
            if (backupCount == 0)
                return filePath;
            return Path.ChangeExtension(filePath, string.Format(".{0}{1}", backupCount, Path.GetExtension(filePath)));
        }
    }
}
