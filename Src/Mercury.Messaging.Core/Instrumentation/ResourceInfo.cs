using System;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Provides information on a runtime managed resource that can execute tasks.
    /// </summary>
    public sealed class ResourceInfo
        : IXmlSerializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Messaging.Instrumentation.ResourceInfo"/> class.
        /// </summary>
        public ResourceInfo()
        {
        }

        internal ResourceInfo(IResource resource)
        {
            if (resource != null)
            {
                // Set initial
                this.Id = resource.Id;
                this.LastPost = resource.LastAccess;
                this.Type = resource.GetType();

                // Capture internal resource properties
                InternalResource res1 = resource as InternalResource;
                if (res1 != null)
                {
                    this.IsShuttingDown = res1.IsShuttingDown;
                }

                // Capture agent port properties
                AgentPort res2 = resource as AgentPort;
                if (res2 != null)
                {
                    this.IsShuttingDown = res2.IsShuttingDown;
                    this.IsSynchronous = res2.IsSynchronous;
                    this.AgentType = res2.AgentType;
                    if (MessagingCoreInstrumentation.SnapshotResources && res2.LinkedPorts.Count > 0)
                    {
                        this.LinkedPorts = new string[res2.LinkedPorts.Count];
                        int index = 0;
                        foreach (string pid in res2.LinkedPorts.Keys)
                        {
                            this.LinkedPorts[index] = pid;
                            index++;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the unique identifier of the managed resource.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the type of the agent being hosted, if an agent port.
        /// </summary>
        public Type AgentType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to synchronously execute a task on an agent.
        /// </summary>
        public bool IsSynchronous { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this resource is in the process of unloading.
        /// </summary>
        public bool IsShuttingDown { get; private set; }

        /// <summary>
        /// Gets the date and time of the last post operation to this resource.
        /// </summary>
        public DateTime LastPost { get; private set; }

        /// <summary>
        /// Gets an array of unique identifiers for linked agent ports on this resource.
        /// </summary>
        public string[] LinkedPorts { get; private set; }

        /// <summary>
        /// Returns a System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/>.
        /// </summary>
        /// <returns>A System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.MethodContextInfo"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "ResourceInfo: {1}{0}Type={2}{0}Agent Type={3}{0}Is Synchronous={4}{0}Is Shutting Down={5}{0}Last Post={6}",
                Environment.NewLine, this.Id, this.Type,
                this.AgentType != null ? this.AgentType.FullName : string.Empty,
                this.IsSynchronous, this.IsShuttingDown, this.LastPost);
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
        /// Deserializes the context info from the XML reader.
        /// </summary>
        /// <param name="reader">The XML reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == "Id")
                    {
                        this.Id = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "Type")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.Type = RuntimeSerializer.DeserializeType(xml);
                    }
                    if (reader.LocalName == "AgentType")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.AgentType = RuntimeSerializer.DeserializeType(xml);
                    }
                    if (reader.LocalName == "IsSynchronous")
                    {
                        string str = reader.ReadString();
                        bool sync;
                        if (!string.IsNullOrEmpty(str) && bool.TryParse(str, out sync))
                            this.IsSynchronous = sync;
                        reader.Read();
                    }
                    if (reader.LocalName == "IsShuttingDown")
                    {
                        string str = reader.ReadString();
                        bool shutdown;
                        if (!string.IsNullOrEmpty(str) && bool.TryParse(str, out shutdown))
                            this.IsShuttingDown = shutdown;
                        reader.Read();
                    }
                    if (reader.LocalName == "LastPost")
                    {
                        string str = reader.ReadString();
                        DateTime dtPost;
                        if (!string.IsNullOrEmpty(str) && DateTime.TryParse(str, out dtPost))
                            this.LastPost = dtPost;
                        reader.Read();
                    }
                    if (reader.LocalName == "LinkedPorts")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.LinkedPorts = RuntimeSerializer.Deserialize<string[]>(xml);
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
            writer.WriteElementString("Id", this.Id);
            if (this.Type != null)
            {
                writer.WriteStartElement("Type");
                RuntimeSerializer.SerializeType(this.Type, writer);
                writer.WriteEndElement();
            }
            if (this.AgentType != null)
            {
                writer.WriteStartElement("AgentType");
                RuntimeSerializer.SerializeType(this.AgentType, writer);
                writer.WriteEndElement();
            }
            if (this.IsSynchronous)
                writer.WriteElementString("IsSynchronous", this.IsSynchronous.ToString());
            if (this.IsShuttingDown)
                writer.WriteElementString("IsShuttingDown", this.IsShuttingDown.ToString());
            writer.WriteElementString("LastPost", this.LastPost.ToString());
            if (this.LinkedPorts != null && this.LinkedPorts.Length > 0)
            {
                writer.WriteStartElement("LinkedPorts");
                RuntimeSerializer.Serialize(this.LinkedPorts, writer);
                writer.WriteEndElement();
            }
        }
    }
}
