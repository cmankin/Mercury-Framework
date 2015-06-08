using System;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a handle for an agent receive action.
    /// </summary>
    /// <typeparam name="T">The type of the message to handle.</typeparam>
    /// <param name="message">The message to handle.</param>
    public delegate void ReceiveHandler<in T>(T message) where T : class;
}
