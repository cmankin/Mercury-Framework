using System;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Provides a method that can be used to initialize the framework object.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Initializes the framework object.
        /// </summary>
        void Initialize();
    }
}
