using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mercury.Logging;

namespace Mercury.Instrumentation
{
    /// <summary>
    /// Allows a block of code to be observed.
    /// </summary>
    public abstract class Observer
        : IDisposable
    {
        /// <summary>
        /// Initializes a default instance of the Mercury.Instrumentation.Observer class.
        /// </summary>
        protected Observer()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Mercury.Instrumentation.Observer 
        /// class with the specified method context and logger.
        /// </summary>
        /// <param name="context">The method context being observed.  This can be obtained 
        /// through a call to System.Reflection.MethodBase.GetCurrentMethod().</param>
        /// <param name="logger">The logger to use.</param>
        protected Observer(MethodBase context, ILog logger)
        {
            this._context = context;
            this._logger = logger;
        }

        private MethodBase _context;

        /// <summary>
        /// Gets information on the observed class member.
        /// </summary>
        public MethodBase Context
        {
            get { return this._context; }
        }

        private ILog _logger;

        /// <summary>
        /// Gets the logger used by this observer.
        /// </summary>
        public ILog Logger
        {
            get { return this._logger; }
        }

        /// <summary>
        /// Executes an entry into the observer block.
        /// </summary>
        public virtual void Enter()
        {
            this.EnterObserverBlock(this.Context, this.Logger);
        }

        /// <summary>
        /// Executes an exit of the observer block.
        /// </summary>
        public virtual void Exit()
        {
            this.ExitObserverBlock(this.Context, this.Logger);
        }

        /// <summary>
        /// Executes an entry into the observer block.
        /// </summary>
        /// <param name="context">The method context for this block.</param>
        /// <param name="logger">The current logger.</param>
        protected abstract void EnterObserverBlock(MethodBase context, ILog logger);

        /// <summary>
        /// Executes an exit of the observer block.
        /// </summary>
        /// <param name="context">The method context for this block.</param>
        /// <param name="logger">The current logger.</param>
        protected abstract void ExitObserverBlock(MethodBase context, ILog logger);

        #region IDisposable
        private bool _disposedValue;

        /// <summary>
        /// Disposes this instance of the Mercury.Instrumentation.Observer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance of the Mercury.Instrumentation.Observer.
        /// </summary>
        /// <param name="disposing">A value indicating whether this instance is disposing.</param>
        protected void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    // Exiting observed member part.
                    this.ExitObserverBlock(this.Context, this.Logger);
                    this._context = null;
                    this._logger = null;
                }
            }
            this._disposedValue = true;
        }
        #endregion
    }
}
