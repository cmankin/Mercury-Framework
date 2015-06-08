using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using Mercury.Logging;

namespace Mercury.Instrumentation
{
    /// <summary>
    /// Observes a block of code and outputs trace information at block entry and exit points.
    /// </summary>
    public class TraceObserver
        : Observer
    {
        /// <summary>
        /// Initializes a new instance of the Mercury.Instrumentation.TraceObserver class 
        /// with the specified method context and logger.
        /// </summary>
        /// <param name="context">The method context being observed.  This can be obtained 
        /// through a call to System.Reflection.MethodBase.GetCurrentMethod().</param>
        /// <param name="logger">The logger to use.</param>
        public TraceObserver(MethodBase context, ILog logger)
            : this(context, logger, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Mercury.Instrumentation.TraceObserver class 
        /// with the specified method context, logger, and state retrieval function.
        /// </summary>
        /// <param name="context">The method context being observed.  This can be obtained 
        /// through a call to System.Reflection.MethodBase.GetCurrentMethod().</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="captureState">A function that returns a string indicating the current state.</param>
        public TraceObserver(MethodBase context, ILog logger, Func<string> captureState)
            : this(context, logger, captureState, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Mercury.Instrumentation.TraceObserver class with the 
        /// specified method context, logger, state retrieval function, and delay enter status.
        /// </summary>
        /// <param name="context">The method context being observed.  This can be obtained 
        /// through a call to System.Reflection.MethodBase.GetCurrentMethod().</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="captureState">A function that returns a string indicating the current state.</param>
        /// <param name="delayEnterToCall">If true the Enter() function will NOT be called in the 
        /// constructor; otherwise, the Enter() function will be called during object construction.</param>
        public TraceObserver(MethodBase context, ILog logger, Func<string> captureState, bool delayEnterToCall)
            : base(context, logger)
        {
            this._captureState = captureState;
            if (this._captureState == null)
                this._captureState = GetCurrentState;
            if (!delayEnterToCall)
                this.EnterObserverBlockInternal(context, logger);
        }

        private Func<string> _captureState;

        /// <summary>
        /// Gets a function that returns a string indicating the current state.
        /// </summary>
        public Func<string> CaptureState
        {
            get { return this._captureState; }
        }

        private bool _canCaptureEnterState = true;

        /// <summary>
        /// Gets the current default state information for enter and exit functions.
        /// </summary>
        /// <returns>The current default state information for enter and exit functions.</returns>
        protected virtual string GetCurrentState()
        {
            if (this._canCaptureEnterState)
            {
                this._canCaptureEnterState = false;
                return "Entered code block {0}".FormatWith(InternalGetStateFormattedContextName());
            }
            else
            {
                return "Exited code block {0}".FormatWith(InternalGetStateFormattedContextName());
            }
        }

        private string InternalGetStateFormattedContextName()
        {
            return this.Context != null ? string.Format("[Class:{0}, Method:{1}]", this.Context.DeclaringType.FullName, this.Context.Name) : string.Empty;
        }

        /// <summary>
        /// Executes an entry into the observer block.  This is a write once 
        /// entry.  Subsequent executions will yield no trace info.
        /// </summary>
        /// <param name="context">The method context for this block.</param>
        /// <param name="logger">The current logger.</param>
        protected override void EnterObserverBlock(System.Reflection.MethodBase context, ILog logger)
        {
            this.EnterObserverBlockInternal(context, logger);
        }

        /// <summary>
        /// Internal method for handling an entry into an observer block. 
        /// Can be called from inside the class constructor.
        /// </summary>
        /// <param name="context">The method context for this block.</param>
        /// <param name="logger">The current logger.</param>
        protected void EnterObserverBlockInternal(System.Reflection.MethodBase context, ILog logger)
        {
            if (logger == null)
                return;

            if (!this.ExecutedEnter)
            {
                this.ExecutedEnter = true;
                logger.Info(this.CaptureState);
            }
        }

        /// <summary>
        /// Flag indicating the whether the observer has been entered.
        /// </summary>
        protected bool ExecutedEnter = false;

        /// <summary>
        /// Flag indicating whether the observer has been exited.
        /// </summary>
        protected bool ExecuteExit = false;

        /// <summary>
        /// Executes an exit of the observer block.  This is a write once 
        /// entry.  Subsequent executions will yield no trace info.
        /// </summary>
        /// <param name="context">The method context for this block.</param>
        /// <param name="logger">The current logger.</param>
        protected override void ExitObserverBlock(System.Reflection.MethodBase context, ILog logger)
        {
            if (logger == null)
                return;

            if (!this.ExecuteExit)
            {
                this.ExecuteExit = true;
                logger.Info(this.CaptureState);
            }
        }
    }
}
