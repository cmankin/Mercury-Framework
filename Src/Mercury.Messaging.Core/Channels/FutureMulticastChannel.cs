using System;
using System.Collections.Generic;
using System.Threading;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Microsoft.Ccr.Core;

namespace Mercury.Messaging.Channels
{
    /// <summary>
    /// A channel that can wait on multiple future values.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    public class FutureMulticastChannel<TResult>
        : ChannelBase, Future<TResult>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Messaging.Channels.FutureMulticastChannel{TResult}"/> 
        /// with the specified runtime environment, timeout value, and array of futures.
        /// </summary>
        /// <param name="environment">The environment on which to prepare this future channel.</param>
        /// <param name="timeout">A time span that represents the number of milliseconds to wait for each future channel to receive a response.</param>
        /// <param name="futures">An array of futures on which to wait.</param>
        public FutureMulticastChannel(RuntimeEnvironment environment, TimeSpan timeout, params Future<TResult>[] futures)
            : base(null)
        {
            this.SetEnvironmentIfNull(environment);
            this._timeout = timeout != TimeSpan.Zero ? timeout : TimeSpan.FromSeconds(15.0);
            this._futures = futures;
            if (this._futures != null && this._futures.Length > 0)
            {
                this._initialCount = this._futures.Length;
                this._currentCount = this._initialCount;
            }
            if (this._initialCount == 0)
            {
                this._signal.Set();
                this._completed = true;
            }
        }

        private Future<TResult>[] _futures;
        private TimeSpan _timeout;
        private int _initialCount;
        private int _currentCount;
        private ManualResetEventSlim _signal = new ManualResetEventSlim();
        private bool _completed;
        private bool signalState = true;

        /// <summary>
        /// Gets the result of the future.  If the result 
        /// has not yet been sent, this call will block 
        /// until the future is resolved or the default 
        /// timeout expires.
        /// </summary>
        public TResult Get
        {
            get
            {
                if (!this.Completed)
                    this.WaitUntilCompleted(this._timeout);
                if (this.Completed && this.Count > 0 && this._futures[0] != null)
                    return this._futures[0].Get;
                return default(TResult);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this future has completed.
        /// </summary>
        public bool Completed
        {
            get { return this._completed; }
        }

        /// <summary>
        /// Gets the number of futures on this channel.
        /// </summary>
        public int Count
        {
            get { return this._initialCount; }
        }

        
        /// <summary>
        /// Gets the result of the future at the specified index.
        /// </summary>
        /// <param name="index">The index of the future on which to get a result.</param>
        /// <returns>The result of the future at the specified index.</returns>
        public TResult this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Count)
                    throw new ArgumentOutOfRangeException("index");
                if (this._futures[index] != null)
                    return this._futures[index].Get;
                return default(TResult);
            }
        }

        /// <summary>
        /// Blocks the current thread until the future is resolved or the timeout expires.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait, or -1 to wait indefinitely.</param>
        /// <returns>True if the future is resolved; otherwise, false.</returns>
        public bool WaitUntilCompleted(int timeout)
        {
            return this.WaitUntilCompleted(TimeSpan.FromMilliseconds(timeout));
        }

        /// <summary>
        /// Blocks the current thread until the future is resolved or the timeout expires.
        /// </summary>
        /// <param name="timeout">A time span that represents the number of milliseconds to wait.  
        /// Use a time span representing -1 milliseconds to wait indefinitely.</param>
        /// <returns>True if the future is resolved; otherwise, false.</returns>
        public bool WaitUntilCompleted(TimeSpan timeout)
        {
            if (this._signal == null)
                return true;
            this._signal.Wait(timeout);
            this.Dispose();
            return this.Completed;
        }

        /// <summary>
        /// Sends a message to all futures on this channel.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        public override void Send<T>(T message)
        {
            var tupleArgs = new System.Tuple<TimeSpan, Action<bool>>(this._timeout, _SignalFutureCompletion);
            for (int i = 0; i < this._initialCount; i++)
            {
                this.Environment.Tasks.Enqueue(new Task<Future<TResult>, T, System.Tuple<TimeSpan, Action<bool>>>(this._futures[i], message, tupleArgs, (ftr, msg, args) =>
                    {
                        try
                        {
                            ftr.Send(msg);
                            var flag = ftr.WaitUntilCompleted(args.Item1);
                            args.Item2.Invoke(flag);
                        }
                        catch
                        {
                            args.Item2.Invoke(false);
                        }
                    }));
            }
        }

        /// <summary>
        /// This method is not supported on this channel.
        /// </summary>
        /// <param name="context">The routing context to post.</param>
        protected internal override void Post(Routing.IRoutingContext context)
        {
            throw new NotSupportedException("Cannot post directly to a future resource.");
        }

        private void _SignalFutureCompletion(bool completed)
        {
            this.signalState = (this.signalState & completed);
            if (Interlocked.Decrement(ref this._currentCount) == 0)
            {
                if (_signal == null)
                    return;
                this._completed = this.signalState;
                Thread.MemoryBarrier();
                this._signal.Set();
            }
        }

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposedValue;

        /// <summary>
        /// Performs the actual dispose.
        /// </summary>
        /// <param name="disposing">A value indicating whether the object is currently being disposed.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }

                var sig = Interlocked.Exchange(ref this._signal, null);
                if (sig != null)
                    sig.Dispose();
            }
            this.disposedValue = true;
        }
        #endregion
    }
}
