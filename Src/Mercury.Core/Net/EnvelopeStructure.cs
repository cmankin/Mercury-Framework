
namespace Mercury.Net
{
    /// <summary>
    /// Describes the type of data being sent.
    /// </summary>
    public enum EnvelopeStructure
    {
        /// <summary>
        /// Represents an arbitrary formatted string.
        /// </summary>
        FormattedString = 0x00,

        /// <summary>
        /// XML formatted string data.
        /// </summary>
        XmlFormattedString = 0x01,

        /// <summary>
        /// CLR or .Net serialized object.
        /// </summary>
        SerializedObject = 0x02,

        /// <summary>
        /// Raw byte data.
        /// </summary>
        Raw = 0x03
    }
}