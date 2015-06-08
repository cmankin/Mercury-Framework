
namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Describes the restart mode for a supervisor's child agents.
    /// </summary>
    public enum SupervisorRestartMode
    {
        /// <summary>
        /// If a child agent terminates, only that agent is restarted.
        /// </summary>
        OneForOne,
        
        /// <summary>
        /// If a child agent terminates, all remaining child 
        /// agents are terminated then restarted, along with 
        /// the originally terminated agent.
        /// </summary>
        OneForAll
    }
}
