using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury
{
    /// <summary>
    /// A flag that allows only one owner during a given time.
    /// </summary>
    public class MutexFlag
    {
        /// <summary>
        /// Initializes a default instance of the Mercury.MutexFlag class.
        /// </summary>
        public MutexFlag()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Mercury.MutexFlag class with the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state of the flag.</param>
        public MutexFlag(bool initialState)
        {
            this._initialState = initialState;
            this._flag = initialState;
        }

        private readonly bool _initialState;
        private bool _flag;

        /// <summary>
        /// Gets a value indicating whether this flag may be entered.
        /// </summary>
        public bool CanEnter
        {
            get
            {
                return (this._flag == this._initialState);
            }
        }

        /// <summary>
        /// Attempts to enter the flag.
        /// </summary>
        /// <returns>A MutexFlagHandler designating ownership.  The handler must be closed by 
        /// calling MutexFlagHandler.Dispose() before ownership may be changed.</returns>
        public MutexFlagHandler Enter()
        {
            if (this._flag != this._initialState)
                throw new InvalidOperationException();
            return new MutexFlagHandler(this);
        }

        #region MutexFlagHandler
        /// <summary>
        /// Restricts access to the owning MutexFlag.
        /// </summary>
        public class MutexFlagHandler
            : IDisposable
        {
            /// <summary>
            /// Initializes a new instance of the Mercury.MutexFlagHandler class.
            /// </summary>
            /// <param name="owner">The owning BoolFlag.</param>
            internal MutexFlagHandler(MutexFlag owner)
            {
                this._owner = owner;
                this._owner._flag = (!this._owner._initialState);
            }

            private MutexFlag _owner;

            private bool disposedValue;

            /// <summary>
            /// Disposes of this instance of the Mercury.MutexFlagHandler.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disposes of this instance of the Mercury.MutexFlagHandler.
            /// </summary>
            /// <param name="disposing">A value indicating whether to dispose.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue)
                {
                    if (disposing)
                    {
                        this._owner._flag = this._owner._initialState;
                    }
                }
                this.disposedValue = true;
            }
        }
        #endregion
    }
}
