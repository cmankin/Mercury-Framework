using System;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents a strategy for restarting child agents of a supervisor.
    /// </summary>
    public class RestartStrategy
    {
        /// <summary>
        /// Initializes a default instance of the RestartStrategy class with the specified values.
        /// </summary>
        /// <param name="restartMode">The supervisor restart mode to use.</param>
        /// <param name="restarts">The maximum number of allowable restarts within a restart interval.</param>
        /// <param name="restartInterval">The interval of time in which the maximum
        /// number of child restarts may occur before the supervisor shuts down.</param>
        public RestartStrategy(SupervisorRestartMode restartMode, int restarts, TimeSpan restartInterval)
        {
            this._restartMode = restartMode;
            this._restartInterval = restartInterval;
            this._restarts = restarts;
        }

        private readonly SupervisorRestartMode _restartMode;

        /// <summary>
        /// Gets the supervisor's child restart strategy.
        /// </summary>
        public SupervisorRestartMode RestartMode
        {
            get { return this._restartMode; }
        }

        private readonly int _restarts;

        /// <summary>
        /// Gets the maximum number of allowable restarts within a restart interval.
        /// </summary>
        public int Restarts
        {
            get { return this._restarts; }
        }

        private readonly TimeSpan _restartInterval;

        /// <summary>
        /// Gets the interval of time in which the maximum number of restarts may occur.
        /// </summary>
        public TimeSpan RestartInterval
        {
            get { return this._restartInterval; }
        }
    }
}
