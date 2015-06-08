using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Mercury.Logging
{
    /// <summary>
    /// A System.Diagnostics.TraceListener that can log to a SQL database.
    /// </summary>
    public class SqlTraceListener
        : TraceListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.SqlTraceListener"/> 
        /// class with the specified connection information.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="tableName">The name of the database table on which to write.</param>
        public SqlTraceListener(string connectionString, string tableName)
            : this(connectionString, null, tableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.SqlTraceListener"/> 
        /// class with the specified connection information.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="schema">The schema of the database table on which to write.</param>
        /// <param name="tableName">The name of the database table on which to write.</param>
        public SqlTraceListener(string connectionString, string schema, string tableName)
        {
            this._connectionString = connectionString;
        }

        private const string SQL_WRITE = "INSERT INTO {0} (Source,EventType,EventId,Message,RelatedActivityId) VALUES('{1}',{2},{3},{4},'{5}')";
        private string _connectionString;
        private string _schema;
        private string _tableName;
        private string _cachedTableFullName;

        /// <summary>
        /// Gets the full name of the table on which to write.
        /// </summary>
        protected string TableFullName
        {
            get
            {
                if (string.IsNullOrEmpty(this._cachedTableFullName))
                {
                    if (!string.IsNullOrEmpty(this._schema) && !string.IsNullOrEmpty(this._tableName))
                        this._cachedTableFullName = string.Format("{0}.{1}", this._schema, this._tableName);
                    else if (!string.IsNullOrEmpty(this._tableName))
                        this._cachedTableFullName = this._tableName;
                    else
                        this._cachedTableFullName = string.Empty;
                }
                return this._cachedTableFullName;
            }
        }
        
        private void WriteRecord(TraceEventCache eventCache, string source, TraceEventType eventType, int eventId, string message, Guid relatedActivityId)
        {
            if (!string.IsNullOrEmpty(this._connectionString))
            {
                using (SqlConnection conn = new SqlConnection(this._connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        string sql = string.Format(SQL_WRITE, this.TableFullName, source, eventType, eventId, message,
                            (relatedActivityId != Guid.Empty ? relatedActivityId.ToString() : "NULL"));
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.TraceEvent(eventCache, source, eventType, id);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.TraceEvent(eventCache, source, eventType, id, string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.WriteRecord(eventCache, source, eventType, id, message, Guid.Empty);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            this.WriteRecord(eventCache, source, TraceEventType.Transfer, id, message, relatedActivityId);
        }

        public override void Write(string message)
        {
            this.WriteLine(message);
        }

        public override void WriteLine(string message)
        {
            this.WriteRecord(null, "", TraceEventType.Information, 0, message, Guid.Empty);
        }
    }
}
