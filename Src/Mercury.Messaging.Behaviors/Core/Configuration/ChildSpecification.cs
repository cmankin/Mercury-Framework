using System;
using System.Text;
using System.Xml.Serialization;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Behaviors
{
    /// <summary>
    /// Represents the specification for creating a child on a supervisor.
    /// </summary>
    /// <remarks>TODO: Support IXmlSerializable.</remarks>
    public class ChildSpecification
    {
        /// <summary>
        /// Initializes a default instance of the ChildSpecification class with the specified values.
        /// </summary>
        /// <param name="name">The unique name identifier for the child.</param>
        /// <param name="startup">The startup function called by the supervisor.  
        /// Func{[environment],[supervisor ID],[agent type],[constructor args]}</param>
        /// <param name="restart">The restart mode for this child.</param>
        /// <param name="shutdown">The interval in milliseconds to wait for an exit 
        /// signal on a supervisor stop before an environment.Kill is issued.</param>
        /// <param name="type">The type of the child agent.</param>
        /// <param name="constructorArgs">Constructor arguments for agent construction.</param>
        public ChildSpecification(string name, Func<RuntimeEnvironment, string, Type, object[], string> startup, 
            RestartMode restart, int shutdown, Type type, params object[] constructorArgs)
        {
            this._name = name;
            this._startup = startup;
            this._restart = restart;
            this._shutdown = shutdown;
            this._type = type;
            this._constructorArgs = constructorArgs;
        }

        internal bool Terminate = false;

        private readonly string _name;

        /// <summary>
        /// Gets the internal name used by the supervisor.
        /// </summary>
        public string Name
        {
            get { return this._name; }
        }

        private readonly RestartMode _restart;

        /// <summary>
        /// Gets the restart mode for this child.
        /// </summary>
        public RestartMode Restart
        {
            get { return this._restart; }
        }

        private readonly int _shutdown;

        /// <summary>
        /// Gets the interval in milliseconds to wait for an exit signal on a 
        /// stop message sent by the supervisor before a forced kill is issued.
        /// </summary>
        /// <returns>The interval in milliseconds to wait for an exit signal on a 
        /// stop message sent by the supervisor before a forced kill is issued.  
        /// If 0, the supervisor will immediately kill the agent.  If -1, the 
        /// supervisor will wait until an exit signal is received.</returns>
        public int Shutdown
        {
            get { return this._shutdown; }
        }

        private readonly Type _type;

        /// <summary>
        /// Gets the type of the child.
        /// </summary>
        public Type Type
        {
            get { return this._type; }
        }

        private object[] _constructorArgs;

        /// <summary>
        /// Gets an array of additional constructor arguments 
        /// to be passed to the startup function.
        /// </summary>
        public object[] ConstructorArgs
        {
            get { return this._constructorArgs; }
        }

        private readonly Func<RuntimeEnvironment, string, Type, object[], string> _startup;

        /// <summary>
        /// Gets the startup action to be performed on the child.  This must 
        /// result in the equivalent of a spawn link on the supervisor agent.
        /// </summary>
        public Func<RuntimeEnvironment, string, Type, object[], string> Startup
        {
            get { return this._startup; }
        }
    }
}
