

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Describes the restart mode for a supervisor child agent.
    /// </summary>
    public enum RestartMode
    {
        /// <summary>
        /// Child agent is always restarted.
        /// </summary>
        Permanent,

        /// <summary>
        /// Child agent is never restarted.
        /// </summary>
        Temporary,

        /// <summary>
        /// Child agent is only restarted on abnormal termination (i.e. a fault).
        /// </summary>
        Transient
    }
}