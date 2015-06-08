using System;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Provides context data on a method.
    /// </summary>
    public sealed class MethodContextInfo
        : IXmlSerializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/> class.
        /// </summary>
        public MethodContextInfo()
        {
        }

        internal MethodContextInfo(MethodBase context)
        {
            this.ObjectName = context.DeclaringType.FullName;
            this.MethodName = context.Name;
        }

        /// <summary>
        /// Gets the name of the object on which the method was declared.
        /// </summary>
        public string ObjectName { get; private set; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; private set; }
        
        /// <summary>
        /// Returns the XML schema.
        /// </summary>
        /// <returns>Default value is null.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Deserializes the context info from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == "ObjectName")
                    {
                        this.ObjectName = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "MethodName")
                    {
                        this.MethodName = reader.ReadString();
                        reader.Read();
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Serializes the context info to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("ObjectName", this.ObjectName);
            writer.WriteElementString("MethodName", this.MethodName);
        }

        /// <summary>
        /// Returns a System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/>.
        /// </summary>
        /// <returns>A System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[Class:{0}, Member:{1}]",
                this.ObjectName != null ? this.ObjectName : string.Empty,
                this.MethodName != null ? this.MethodName : string.Empty);
        }
    }
}
