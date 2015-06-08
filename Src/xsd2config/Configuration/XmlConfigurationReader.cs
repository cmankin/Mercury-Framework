using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Mercury.Configuration
{
    /// <summary>
    /// A reader that recognizes configuration elements and annotations while reading.
    /// </summary>
    public class XmlConfigurationReader
        : XmlReader
    {
        private XmlReader impl;
        private List<TypeAnnotationHint> _configurationTypeHints = new List<TypeAnnotationHint>();
        private bool previousIsAnnotate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Configuration.XmlConfigurationReader"/> with the specified XML reader.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> to use.</param>
        public XmlConfigurationReader(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            this.impl = reader;
        }

        internal class ConfigurationNode
        {
            public string Name;
            public string ElementName;
            public string ElementNamespaceURI;
            public int LineNumber;
            public int LinePosition;

            internal void BuildFromReader(XmlReader reader)
            {
                this.Name = reader.GetAttribute("name");
                this.ElementName = reader.LocalName;
                this.ElementNamespaceURI = reader.NamespaceURI;
                var info = (IXmlLineInfo)reader;
                this.LineNumber = info.LineNumber;
                this.LinePosition = info.LinePosition;
            }
        }

        private IXmlLineInfo LineInfo
        {
            get { return (IXmlLineInfo)this.impl; }
        }

        /// <summary>
        /// Gets a list of hints used to clarify aspects of the configuration element type to build.
        /// </summary>
        public IList<TypeAnnotationHint> ConfigurationTypeHints
        {
            get { return this._configurationTypeHints; }
        }

        /// <summary>
        /// Gets a <see cref="Mercury.Configuration.ConfigurationTypeResolver"/> built from the current state of this reader.
        /// </summary>
        public ConfigurationTypeResolver TypeResolver
        {
            get { return new ConfigurationTypeResolver(this._configurationTypeHints.AsReadOnly()); }
        }

        /// <summary>
        /// Gets the number of attributes on the current node.
        /// </summary>
        public override int AttributeCount
        {
            get { return this.impl.AttributeCount; }
        }

        /// <summary>
        /// Gets the base URI of the current node.
        /// </summary>
        public override string BaseURI
        {
            get { return this.impl.BaseURI; }
        }

        /// <summary>
        /// Gets the depth of the current node in the XML document.
        /// </summary>
        public override int Depth
        {
            get { return this.impl.Depth; }
        }

        /// <summary>
        /// Gets a value indicating whether the reader is positioned at the end of the stream.
        /// </summary>
        public override bool EOF
        {
            get { return this.impl.EOF; }
        }

        /// <summary>
        /// Gets a value indicating whether the current node is an empty element <example>(for example, &lt;MyElement/&gt;)</example>.
        /// </summary>
        public override bool IsEmptyElement
        {
            get { return this.impl.IsEmptyElement; }
        }

        /// <summary>
        /// Gets the local name of the current node.
        /// </summary>
        public override string LocalName
        {
            get { return this.impl.LocalName; }
        }

        /// <summary>
        /// Gets the <see cref="System.Xml.XmlNameTable"/> associated with this implementation.
        /// </summary>
        public override XmlNameTable NameTable
        {
            get { return this.impl.NameTable; }
        }

        /// <summary>
        /// Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.
        /// </summary>
        public override string NamespaceURI
        {
            get { return this.impl.NamespaceURI; }
        }

        /// <summary>
        /// Gets the type of the current node.
        /// </summary>
        public override XmlNodeType NodeType
        {
            get { return this.impl.NodeType; }
        }

        /// <summary>
        /// Gets the namespace prefix associated with the current node.
        /// </summary>
        public override string Prefix
        {
            get { return this.impl.Prefix; }
        }

        /// <summary>
        /// Gets the state of the reader.
        /// </summary>
        public override ReadState ReadState
        {
            get { return this.impl.ReadState; }
        }

        /// <summary>
        /// Gets the text value of the current node.
        /// </summary>
        public override string Value
        {
            get { return this.impl.Value; }
        }

        /// <summary>
        /// Changes the underlying <see cref="System.Xml.XmlReader.ReadState"/> to closed.
        /// </summary>
        public override void Close()
        {
            this.impl.Close();
        }

        /// <summary>
        /// Gets the value of the attribute with the specified index.
        /// </summary>
        /// <param name="i">The index of the attribute.  The index is zero-based.</param>
        /// <returns>The value of the attribute with the specified index.</returns>
        public override string GetAttribute(int i)
        {
            return this.impl.GetAttribute(i);
        }

        /// <summary>
        /// Gets the value of the attribute with the specified System.Xml.XmlReader.LocalName and System.Xml.XmlReader.NamespaceURI.
        /// </summary>
        /// <param name="name">The local name of the attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute.</param>
        /// <returns>The value of the attribute with the specified System.Xml.XmlReader.LocalName and System.Xml.XmlReader.NamespaceURI.</returns>
        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.impl.GetAttribute(name, namespaceURI);
        }

        /// <summary>
        /// Gets the value of the attribute with the specified System.Xml.XmlReader.Name.
        /// </summary>
        /// <param name="name">The qualified name of the attribute.</param>
        /// <returns>The value of the attribute with the specified System.Xml.XmlReader.Name.</returns>
        public override string GetAttribute(string name)
        {
            return this.impl.GetAttribute(name);
        }

        /// <summary>
        /// Resolves a namespace prefix in the current element's scope.
        /// </summary>
        /// <param name="prefix">The prefix whose namespace URI you want to resolve.  To match the default namespace, pass an empty string.</param>
        /// <returns>The resolved namespace associated with the prefix in the current element's scope.</returns>
        public override string LookupNamespace(string prefix)
        {
            return this.impl.LookupNamespace(prefix);
        }

        /// <summary>
        /// Moves to the attribute with the specified System.Xml.XmlReader.LocalName and System.Xml.XmlReader.NamespaceURI.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns>True if the </returns>
        public override bool MoveToAttribute(string name, string ns)
        {
            return this.impl.MoveToAttribute(name, ns);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.impl.MoveToAttribute(name);
        }

        public override bool MoveToElement()
        {
            return this.impl.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.impl.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.impl.MoveToNextAttribute();
        }

        public override bool Read()
        {
            this.ParseAndStoreAnnotationComment();
            return this.impl.Read();
        }

        public override bool ReadAttributeValue()
        {
            return this.impl.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            this.impl.ResolveEntity();
        }

        private void ParseAndStoreAnnotationComment()
        {
            if (this.impl.NodeType == XmlNodeType.Comment)
            {
                // capture comment
                var comment = this.impl.Value;
                var hint = TypeAnnotationHint.Create(this.LineInfo.LineNumber, comment);
                if (hint != null)
                {
                    this._configurationTypeHints.Add(hint);
                    previousIsAnnotate = true;
                }
            }
            else
            {
                if (previousIsAnnotate && this.impl.NodeType == XmlNodeType.Element)
                {
                    previousIsAnnotate = false;
                    var node = new ConfigurationNode();
                    node.BuildFromReader(this.impl);
                    if (this._configurationTypeHints.Count > 0)
                        this._configurationTypeHints[this._configurationTypeHints.Count - 1].AssociatedNode = node;
                }
            }
        }
    }
}
