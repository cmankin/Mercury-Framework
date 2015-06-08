using System;

namespace Mercury.Messaging.Serialization
{
    /// <summary>
    /// A type receiver interface.
    /// </summary>
    public interface ITypeReceive
    {
        /// <summary>
        /// Gets the type to receive.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the base type to receive.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Gets the instance to receive.
        /// </summary>
        object Instance { get; }
    }
}
