
namespace Mercury.Net
{
    /// <summary>
    /// Describes the acknowledgment of a received message to the sender.
    /// </summary>
    public enum Acknowledgment
    {
        /// <summary>
        /// The message was incorrectly formatted.
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// The message did not arrive in the expected amount of time.
        /// </summary>
        ReceiveTimeout,

        /// <summary>
        /// The message was received.
        /// </summary>
        Receive
    }
}
