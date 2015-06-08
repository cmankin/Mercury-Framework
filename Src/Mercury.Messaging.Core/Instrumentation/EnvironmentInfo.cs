using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Globalization;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Provides monitoring information about the current <see cref="Mercury.Messaging.Runtime.RuntimeEnvironment"/>.
    /// </summary>
    public sealed class EnvironmentInfo
        : IXmlSerializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Messaging.Instrumentation.EnvironmentInfo"/> class.
        /// </summary>
        public EnvironmentInfo()
        {
        }

        internal EnvironmentInfo(RuntimeEnvironment environment)
        {
            // Set initial
            this.ResourceCount = environment.ResourceCount;
            this.Address = environment.RuntimeAddress.ToString();
            this.EndPoint = environment.EndPoint;

            //// Capture all resources
            //if (MessagingCoreInstrumentation.SnapshotResources)
            //{
            //    IEnumerable<IResource> resources = environment.GetAllInternalResources();
            //    foreach (IResource res in resources)
            //    {
            //        if (res != null)
            //            this._currentResources.Add(new ResourceInfo(res));
            //    }
            //}
        }

        /// <summary>
        /// Gets the address of the runtime environment.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Gets the IP end point of this runtime environment.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the number of currently active resources managed on this environment.
        /// </summary>
        public int ResourceCount { get; private set; }

        private IList<ResourceInfo> _currentResources = new List<ResourceInfo>();

        /// <summary>
        /// Gets a list of the managed resources for this environment.
        /// </summary>
        public IList<ResourceInfo> CurrentResources
        {
            get { return this._currentResources; }
        }

        /// <summary>
        /// Returns a System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/>.
        /// </summary>
        /// <returns>A System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "EnvironmentInfo: {1}{0}End Point={2}{0}Resource Count={3}",
                Environment.NewLine, this.Address, this.EndPoint != null ? this.EndPoint.ToString() : string.Empty, this.ResourceCount);
        }

        /// <summary>
        /// Returns the XML schema.
        /// </summary>
        /// <returns>Default value is null.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Deserializes the environment info from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == "Address")
                    {
                        this.Address = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "EndPoint")
                    {
                        var str = reader.ReadString();
                        if (!string.IsNullOrEmpty(str))
                            this.EndPoint = RemotingUtil.ParseRemoteFormat(str);
                        reader.Read();
                    }
                    if (reader.LocalName == "ResourceCount")
                    {
                        var str = reader.ReadString();
                        int count;
                        if (!string.IsNullOrEmpty(str) && int.TryParse(str, out count))
                            this.ResourceCount = count;
                        reader.Read();
                    }
                    if (reader.LocalName == "CurrentResources")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this._currentResources = RuntimeSerializer.Deserialize<List<ResourceInfo>>(xml);
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Serializes the environment info to the XML writer.
        /// </summary>
        /// <param name="writer">The XML writer on which to write.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Address", this.Address);
            if (this.EndPoint != null)
                writer.WriteElementString("EndPoint", RemotingUtil.GetRemoteFormat(this.EndPoint));
            writer.WriteElementString("ResourceCount", this.ResourceCount.ToString());
            if (this.CurrentResources.Count > 0)
            {
                writer.WriteStartElement("CurrentResources");
                RuntimeSerializer.Serialize(this.CurrentResources, writer);
                writer.WriteEndElement();
            }
        }
    }
}
