using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Test.Runtime.Mock
{
    public class MockResource 
        : IResource,
        IDisposable
    {

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this._isDisposed = true;
                }
            }
            this.disposedValue = true;
        }

        #endregion

        private bool _isDisposed = false;

        public bool IsDisposed
        {
            get { return this._isDisposed; }
        }

        private bool _isAlive = true;

        public bool IsAlive
        {
            get
            {
                return this._isAlive;
            }
            set
            {
                this._isAlive = value;
            }
        }

        private int _generation = 0;

        public int Generation
        {
            get
            {
                return this._generation;
            }
            set
            {
                this._generation = value;
            }
        }

        public string Id
        {
            get { return "nothing"; }
        }

        private DateTime _lastAccess = DateTime.UtcNow;

        public DateTime LastAccess
        {
            get { return this._lastAccess; }
        }

        public void UpdateLastAccess()
        {
            this._lastAccess = DateTime.UtcNow;
        }
    }
}
