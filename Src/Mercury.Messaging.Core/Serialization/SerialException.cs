using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Mercury.Messaging.Serialization
{
    /// <summary>
    /// An in-memory, serializable element representing an exception
    /// </summary>
    public class SerialException 
        : IXmlSerializable
    {
        #region Constructors
        /// <summary>
        /// Initializes a default instance of the SerialException class.
        /// </summary>
        public SerialException()
        {
        }

        /// <summary>
        /// Initializes a default instance of the SerialException class with the specified values.
        /// </summary>
        /// <param name="originalType">The original exception type.</param>
        /// <param name="message">The error message explaining the cause of the exception.</param>
        /// <param name="stackTrace">The stack trace for the exception.</param>
        /// <param name="innerException">The [serialized] exception that caused this exception.</param>
        /// <param name="source">The source data string.</param>
        /// <param name="helpLink">Help link information.</param>
        public SerialException(Type originalType, string message, string stackTrace, SerialException innerException,
            string source, string helpLink)
            : this(originalType != null ? originalType.FullName : null,
                    originalType != null ? originalType.AssemblyQualifiedName : null,
                    message, stackTrace, innerException, source, helpLink)
        {
            this._cachedType = originalType;
        }

        internal SerialException(string originalTypeName, string originalTypeQualified, string message, string stackTrace, 
            SerialException innerException, string source, string helpLink)
        {
            this._originalTypeName = originalTypeName;
            this._originalTypeQualified = originalTypeQualified;
            this._message = message;
            this._stackTrace = stackTrace;
            this._innerException = innerException;
            this._source = source;
            this._helpLink = helpLink;
        }
        #endregion

        #region Properties
        private string _originalTypeQualified;
        private string _originalTypeName;
        private Type _cachedType;

        /// <summary>
        /// Gets the full name of the original exception type.
        /// </summary>
        public string OriginalTypeName
        {
            get { return this._originalTypeName; }
        }

        /// <summary>
        /// Gets the original exception type if it exists and can be loaded.
        /// </summary>
        public Type OriginalType
        {
            get
            {
                if (this._cachedType == null)
                    this._cachedType = Type.GetType(this._originalTypeQualified);
                return this._cachedType;
            }
        }

        private string _message;

        /// <summary>
        /// Gets a message for this exception.
        /// </summary>
        public string Message
        {
            get { return this._message; }
        }

        private string _helpLink;

        /// <summary>
        /// Gets a help link for this exception.
        /// </summary>
        public string HelpLink
        {
            get { return this._helpLink; }
        }

        private string _source;

        /// <summary>
        /// Gets the source of this exception.
        /// </summary>
        public string Source
        {
            get { return this._source; }
        }

        private string _stackTrace;

        /// <summary>
        /// Gets the stack trace for this exception.
        /// </summary>
        public string StackTrace
        {
            get { return this._stackTrace; }
        }

        private SerialException _innerException;

        /// <summary>
        /// Gets the inner exception element.
        /// </summary>
        public SerialException InnerException
        {
            get { return this._innerException; }
        }
        #endregion

        #region Static
        /// <summary>
        /// Creates and returns a serial exception from the specified exception.
        /// </summary>
        /// <param name="exception">The exception from which to create a serial exception.</param>
        /// <returns>A serial exception created from the specified exception.</returns>
        public static SerialException Create(Exception exception)
        {
            if (exception == null)
                return null;

            SerialException inner = null;
            if (exception.InnerException != null)
                inner = Create(exception.InnerException);

            return new SerialException(exception.GetType(), exception.Message, exception.StackTrace, inner, exception.Source, exception.HelpLink);
        }

        /// <summary>
        /// Creates and returns a serial exception from the specified XML string.
        /// </summary>
        /// <param name="xml">The XML to parse into a serial exception.</param>
        /// <returns>A serial exception created from the specified XML string.</returns>
        public static SerialException Create(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            using (StringReader sr = new StringReader(xml))
            {
                using (XmlTextReader reader = new XmlTextReader(sr))
                {
                    return Create(reader);
                }
            }
        }

        /// <summary>
        /// Creates and returns a serial exception from the contents of the specified XML reader.
        /// </summary>
        /// <param name="reader">The XML reader containing the XML to parse into a serial exception.</param>
        /// <returns>A serial exception created from the contents of the specified XML reader.</returns>
        public static SerialException Create(XmlReader reader)
        {
            if (reader == null)
                return null;

            // Read
            string originalQualified = null;
            string originalName = null;
            string message = string.Empty;
            string source = string.Empty;
            string helpLink = string.Empty;
            string stackTrace = string.Empty;
            SerialException inner = null;

            if (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == SerialException.SerialExceptionElement)
                        reader.Read();
                    if (reader.LocalName == SerialException.OriginalTypeElement)
                    {
                        originalQualified = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == SerialException.OriginalTypeNameElement)
                    {
                        originalName = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == SerialException.MessageElement)
                    {
                        message = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == SerialException.HelpLinkElement)
                    {
                        helpLink = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == SerialException.SourceElement)
                    {
                        source = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == SerialException.StackTraceElement)
                    {
                        stackTrace = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == SerialException.InnerExceptionElement)
                    {
                        inner = Create(reader.ReadInnerXml());
                        reader.Read();
                    }
                }
            }
            reader.Read();

            // Return element
            return new SerialException(originalName, originalQualified, message, stackTrace, inner, source, helpLink);
        }

        /// <summary>
        /// SerialException static property name.
        /// </summary>
        public static readonly string SerialExceptionElement = "SerialException";
        /// <summary>
        /// OriginalType static property name.
        /// </summary>
        public static readonly string OriginalTypeElement = "OriginalType";
        /// <summary>
        /// OriginalTypeName static property name.
        /// </summary>
        public static readonly string OriginalTypeNameElement = "OriginalTypeName";
        /// <summary>
        /// Message static property name.
        /// </summary>
        public static readonly string MessageElement = "Message";
        /// <summary>
        /// HelpLink static property name.
        /// </summary>
        public static readonly string HelpLinkElement = "HelpLink";
        /// <summary>
        /// Source static property name.
        /// </summary>
        public static readonly string SourceElement = "Source";
        /// <summary>
        /// StackTrace static property name.
        /// </summary>
        public static readonly string StackTraceElement = "StackTrace";
        /// <summary>
        /// InnerException static property name.
        /// </summary>
        public static readonly string InnerExceptionElement = "InnerException";
        #endregion

        #region IXmlSerializable
        /// <summary>
        /// Returns the XML schema.
        /// </summary>
        /// <returns>Default value is null.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Deserializes the response from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            SerialException newException = Create(reader);
            this._originalTypeQualified = newException._originalTypeQualified;
            this._originalTypeName = newException._originalTypeName;
            this._message = newException.Message;
            this._helpLink = newException.HelpLink;
            this._source = newException.Source;
            this._stackTrace = newException.StackTrace;
            this._innerException = newException.InnerException;
        }

        /// <summary>
        /// Serializes the response to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public void WriteXml(XmlWriter writer)
        {
            // Write
            if (!string.IsNullOrEmpty(this._originalTypeQualified))
                writer.WriteElementString(SerialException.OriginalTypeElement, this._originalTypeQualified);
            if (!string.IsNullOrEmpty(this.OriginalTypeName))
                writer.WriteElementString(SerialException.OriginalTypeNameElement, this.OriginalTypeName);
            if (!string.IsNullOrEmpty(this.Message))
                writer.WriteElementString(SerialException.MessageElement, this.Message);
            if (!string.IsNullOrEmpty(this.HelpLink))
                writer.WriteElementString(SerialException.HelpLinkElement, this.HelpLink);
            if (!string.IsNullOrEmpty(this.Source))
                writer.WriteElementString(SerialException.SourceElement, this.Source);
            if (!string.IsNullOrEmpty(this.StackTrace))
                writer.WriteElementString(SerialException.StackTraceElement, this.StackTrace);
            
            // Write inner exception
            if (this.InnerException != null)
            {
                writer.WriteStartElement(SerialException.InnerExceptionElement);
                writer.WriteStartElement(SerialException.SerialExceptionElement);
                this.InnerException.WriteXml(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
