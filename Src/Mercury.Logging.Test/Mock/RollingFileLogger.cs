using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Logging;
using Mercury.Logging.Loggers;

namespace Mercury.Logging.Test.Mock
{
    /// <summary>
    /// An implementation of a rolling file logger.
    /// </summary>
    public class RollingFileLogger : FileLogger
    {
        private int _backupCount;
        
        public RollingFileLogger()
        {
        }

        /// <summary>
        /// Gets the maximum size of the file in bytes.  If set to 0, it 
        /// will be considered an unbounded log file.
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
            if (stream != null)
            {
                if ((stream.Length + buffer.LongLength) > (long)this.MaxFileSize)
                {
                    this.UnsafeDisposeStream();
                    this.PrepareNextLogFile();
                    if (this.MaxBackupFiles > 0)
                        this._backupCount++;

                    using (var fStream = this.GetFileStream(this.FilePath, this.WriteOnly, FileLogger.DEFAULT_BUFFER_SIZE))
                    {
                        fStream.Write(buffer, 0, buffer.Length);
                        this.WritePosition = fStream.Position;
                    }
                    return;
                }
            }
            base.WriteToStream(stream, buffer);
        }

        private void PrepareNextLogFile()
        {
            if (this.MaxBackupFiles == 0)
            {
                if (File.Exists(this.FilePath))
                    File.Delete(this.FilePath);
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
            return Path.ChangeExtension(filePath, string.Format(".{0}.{1}", backupCount, Path.GetExtension(filePath)));
        }
    }
}
