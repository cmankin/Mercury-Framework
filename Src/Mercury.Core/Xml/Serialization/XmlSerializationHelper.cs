using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Mercury.IO;

namespace Mercury.Xml.Serialization
{
    /// <summary>
    /// A helper class for XML serialization.
    /// </summary>
    public static class XmlSerializationHelper
    {
        private static Func<Type, string> _format;
        private static Encoding _encoding = Encoding.Unicode;

        #region Definitions
        /// <summary>
        /// Gets a new line character.
        /// </summary>
        public static readonly string NewLine = string.Format("{0}{1}", '\r', '\n');
        #endregion

        #region Encoding
                /// <summary>
        /// Gets the current default encoding.
        /// </summary>
        public static Encoding CurrentEncoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// Sets the default encoding scheme to the specified encoding.
        /// </summary>
        /// <param name="encoding">The System.Text.Encoding to set.</param>
        public static void SetEncoding(Encoding encoding)
        {
            _encoding = encoding;
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Returns the assembly qualified name or the custom formatted name of the specified System.Type.
        /// </summary>
        /// <param name="type">The System.Type to use.</param>
        /// <returns>The assembly qualified name or the custom formatted name of the specified System.Type.</returns>
        public static string GetSerializableTypeName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return XmlSerializationHelper.GetSerializedTypeFormatInternal(type);
        }

        /// <summary>
        /// Serializes the specified object instance to an XML string using the Unicode (little endian) encoding.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <returns>An object serialized XML string.</returns>
        public static string Serialize(object data)
        {
            return XmlSerializationHelper.Serialize(data, XmlSerializationHelper.CurrentEncoding);
        }

        /// <summary>
        /// Serializes the specified object instance to an XML string with the specified encoding.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>An object serialized XML string with the specified encoding.</returns>
        public static string Serialize(object data, Encoding encoding)
        {
            Type tData = data as Type;
            if (tData != null)
            {
                return SerializeType(tData, encoding);
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(data.GetType());
                StringWriter writer = new EncodableStringWriter(encoding);
                serializer.Serialize(writer, data);
                writer.Flush();
                return writer.ToString();
            }
        }

        /// <summary>
        /// Serializes the specified object instance using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <param name="writer">The System.Xml.XmlWriter to use.</param>
        public static void Serialize(object data, XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(writer, data);
        }

        /// <summary>
        /// Serializes the specified System.Type to an XML string using the Unicode (little endian) encoding.
        /// </summary>
        /// <param name="type">The System.Type to serialize.</param>
        /// <returns>An XML string describing the specified System.Type.</returns>
        public static string SerializeType(Type type)
        {
            return XmlSerializationHelper.SerializeType(type, XmlSerializationHelper.CurrentEncoding);
        }

        /// <summary>
        /// Serializes the specified System.Type to an XML string with the specified encoding.
        /// </summary>
        /// <param name="type">The System.Type to serialize.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>An XML string describing the specified System.Type.</returns>
        public static string SerializeType(Type type, Encoding encoding)
        {
            using (StringWriter sw = new EncodableStringWriter(encoding))
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    SerializeType(type, writer);
                }
                return sw.ToString();
            }
        }

        /// <summary>
        /// Serializes the specified System.Type using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="type">The System.Type to serialize.</param>
        /// <param name="writer">The System.Xml.XmlWriter to use.</param>
        public static void SerializeType(Type type, XmlWriter writer)
        {
            writer.WriteElementString("Type", XmlSerializationHelper.GetSerializedTypeFormatInternal(type));
        }
        #endregion

        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML to an instance of the specified type.
        /// </summary>
        /// <param name="xml">An XML serialized object.</param>
        /// <param name="expectedType">The type of the instance to return.</param>
        /// <returns>An instance of the specified type deserialized from the specified XML or null.</returns>
        public static object Deserialize(string xml, Type expectedType)
        {
            if (expectedType.Equals(typeof(Type)))
            {
                return DeserializeType(xml);
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(expectedType);
                StringReader reader = new StringReader(xml);
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserializes the specified XML to an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the instance to return.</typeparam>
        /// <param name="xml">An XML serialized object.</param>
        /// <returns>An instance of the specified type deserialized from the specified XML or null.</returns>
        public static T Deserialize<T>(string xml)
        {
            return (T)Deserialize(xml, typeof(T));
        }

        /// <summary>
        /// Deserializes a System.Type from the specified XML string.
        /// </summary>
        /// <param name="xml">An XML serialized type.</param>
        /// <returns>A System.Type deserialized from the specified XML string or null.</returns>
        public static Type DeserializeType(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                using (XmlTextReader reader = new XmlTextReader(sr))
                {
                    return XmlSerializationHelper.DeserializeType(reader);
                }
            }
        }

        /// <summary>
        /// Deserializes a System.Type using the specified System.Xml.XmlReader.
        /// </summary>
        /// <param name="reader">The XML reader to use.</param>
        /// <returns>A System.Type deserialized from the specified System.Xml.XmlReader.</returns>
        public static Type DeserializeType(XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Type")
                {
                    string inner = reader.ReadInnerXml();
                    if (!string.IsNullOrEmpty(inner))
                        return Type.GetType(inner);
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Generates an XML document header with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use for the XML document header.</param>
        /// <returns>An XML document header generated with the specified encoding.</returns>
        public static string GenerateHeader(Encoding encoding)
        {
            using (StringWriter sw = new EncodableStringWriter(encoding))
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    writer.WriteStartDocument();
                    writer.WriteRaw(XmlSerializationHelper.NewLine);
                    //writer.WriteEndDocument();
                }
                return sw.ToString();
            }
        }

        /// <summary>
        /// Sets the custom XML serialized type format.  If format is null, then the default will be used.
        /// </summary>
        /// <param name="format">A method that can format the serialized type or null to use the default.</param>
        public static void SetCustomTypeFormat(Func<Type, string> format)
        {
            XmlSerializationHelper._format = format;
        }

        private static string GetSerializedTypeFormatInternal(Type type)
        {
            if (XmlSerializationHelper._format == null)
                return type.AssemblyQualifiedName;
            return XmlSerializationHelper._format(type);
        }
    }
}
