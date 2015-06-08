using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Channels;

namespace Mercury.Messaging.Routing
{
    internal class PostedOperation
    {
        internal PostedOperation(int operationId, RemotingChannel channel, TimeSpan timeout)
        {
            this.OperationId = operationId;
            this.Channel = channel;
            this.Timeout = timeout;
            this._created = DateTime.UtcNow;
        }

        private DateTime _created;
        internal RemotingChannel Channel { get; private set; }
        internal TimeSpan Timeout { get; private set; }
        internal int OperationId { get; private set; }

        private bool _isExpired;

        internal bool IsExpired
        {
            get
            {
                if (!this._isExpired)
                {
                    var current = DateTime.UtcNow;
                    var diff = current.Subtract(this._created);
                    if (diff.CompareTo(this.Timeout) > 0)
                        this._isExpired = true;
                }
                return this._isExpired;
            }
        }
    }
}
