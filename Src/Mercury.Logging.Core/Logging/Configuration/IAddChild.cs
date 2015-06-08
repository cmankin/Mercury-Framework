using System;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Provides a common interface to add a child object to a logging framework object.
    /// </summary>
    public interface IAddChild
    {
        /// <summary>
        /// Adds the specified child object to the framework object.
        /// </summary>
        /// <param name="child">The child object to add.</param>
        void AddChild(object child);
    }
}
