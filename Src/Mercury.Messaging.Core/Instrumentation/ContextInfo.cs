using System;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using Mercury.Messaging.Routing;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Mercury.Messaging.Serialization;

namespace Mercury.Messaging.Instrumentation
{
    /// <summary>
    /// Provides information on the context of a single concurrent work item.
    /// </summary>
    /// <remarks>Derived from a simple formula: a context requires/utilizes a resource on an environment.  Thus, the context refers specifically to the work being done</remarks>
    public sealed class ContextInfo
        : IXmlSerializable
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Messaging.Instrumentation.ContextInfo"/> class.
        /// </summary>
        public ContextInfo()
        {
        }

        internal ContextInfo(IRoutingContext context, IResource resource, RuntimeEnvironment environment, MethodBase methodContext, IPEndPoint receiverEP, string receiverId, int processId, int threadId)
        {
            try
            {
                if (context != null)
                    this.MessageInfo = new MessageInfo(context, receiverEP, receiverId);
                if (resource != null)
                    this.ResourceInfo = new ResourceInfo(resource);
                if (environment != null)
                    this.EnvironmentInfo = new EnvironmentInfo(environment);
                if (methodContext != null)
                    this.MethodContext = new MethodContextInfo(methodContext);
            }
            catch (Exception ex)
            {
                this.Message = ex.ToString();
            }
            this.ProcessId = processId;
            this.ThreadId = threadId;
            this.Created = DateTime.UtcNow;
        }

        internal static string TrySerialize(IXmlSerializable serial)
        {
            if (serial != null)
            {
                try
                {
                    return RuntimeSerializer.Serialize(serial);
                }
                catch (Exception ex)
                {
                    return string.Format(CultureInfo.InvariantCulture, "Failed to generate XML from serializable object: {0}", ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the date and time this context was created, expressed as Coordinated Universal Time (UTC).
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        /// Gets the unique identifier for the system process on which this context ran.
        /// </summary>
        public int ProcessId { get; private set; }

        /// <summary>
        /// Gets the unique identifier for the managed thread on which this context ran.
        /// </summary>
        public int ThreadId { get; private set; }

        /// <summary>
        /// Gets information on the environment hosting this context execution task.
        /// </summary>
        public EnvironmentInfo EnvironmentInfo { get; private set; }

        /// <summary>
        /// Gets information on the resource executing this context.
        /// </summary>
        public ResourceInfo ResourceInfo { get; private set; }

        /// <summary>
        /// Gets the method in which this context was monitored.
        /// </summary>
        public MethodContextInfo MethodContext { get; private set; }

        /// <summary>
        /// Gets the message information for this context.
        /// </summary>
        public MessageInfo MessageInfo { get; private set; }

        /// <summary>
        /// Gets or sets a fault pertaining to this context.
        /// </summary>
        public Fault Fault { get; set; }

        /// <summary>
        /// Gets or sets a message to accompany this context info.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a new context info object from the specified routing context, method context, and message.
        /// </summary>
        /// <param name="context">The context from which to derive a new context info.</param>
        /// <param name="methodContext">The method in which this context was monitored.</param>
        /// <param name="message">A message to accompany this context info.</param>
        /// <returns>A new context info object derived from the specified routing context, method context, and message.</returns>
        public static ContextInfo NewInfo(IRoutingContext context, MethodBase methodContext, string message)
        {
            return ContextInfo.NewInfo(context, null, null, methodContext, null, null, message);
        }

        /// <summary>
        /// Creates a new context info object from the specified values.
        /// </summary>
        /// <param name="context">The context from which to derive a new context info.</param>
        /// <param name="resource">The resource executing this context.</param>
        /// <param name="environment">The environment hosting the context execution task.</param>
        /// <returns>A new context info object derived from the specified values.</returns>
        public static ContextInfo NewInfo(IRoutingContext context, IResource resource, RuntimeEnvironment environment)
        {
            return ContextInfo.NewInfo(context, resource, environment, null, null, null, null);
        }

        /// <summary>
        /// Creates a new context info object from the specified values.
        /// </summary>
        /// <param name="context">The context from which to derive a new context info.</param>
        /// <param name="resource">The resource executing this context.</param>
        /// <param name="environment">The environment hosting the context execution task.</param>
        /// <param name="methodContext">The method in which this context was monitored.</param>
        /// <param name="receiverEP">The IP end point of a receiver, if the routing context is sending remotely.</param>
        /// <param name="receiverId">The unique identifier of the receiver, if the routing context is sending.</param>
        /// <param name="message">A message to accompany this context info.</param>
        /// <returns>A new context info object derived from the specified values.</returns>
        public static ContextInfo NewInfo(IRoutingContext context, IResource resource, RuntimeEnvironment environment, MethodBase methodContext, IPEndPoint receiverEP, string receiverId, string message)
        {
            return ContextInfo.NewInfo(context, resource, environment, methodContext, receiverEP, receiverId, message,
                MessagingCoreInstrumentation.GetCurrentProcessId(), MessagingCoreInstrumentation.GetCurrentThreadId());
        }

        internal static ContextInfo NewInfo(IRoutingContext context, IResource resource, RuntimeEnvironment environment, MethodBase methodContext, IPEndPoint receiverEP, string receiverId, string message, int processId, int threadId)
        {
            ContextInfo ci = new ContextInfo(context, resource, environment, methodContext, receiverEP, receiverId, processId, threadId);
            ci.Message = message;
            return ci;
        }

        /// <summary>
        /// Returns a System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.ContextInfo"/>.
        /// </summary>
        /// <returns>A System.String that represents the current <see cref="Mercury.Messaging.Instrumentation.ContextInfo"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ContextInfo: {1}{0}Process={2}{0}Thread={3}{0}Created={4}{0}Message={5}{0}Method={6}{0}Resource={7}{0}Environment={8}",
                Environment.NewLine, this.Message != null ? this.Message : string.Empty,
                this.ProcessId, this.ThreadId, this.Created,
                this.MessageInfo != null ? this.MessageInfo.ToString() : string.Empty,
                this.MethodContext != null ? this.MethodContext.ToString() : string.Empty,
                this.ResourceInfo != null ? this.ResourceInfo.ToString() : string.Empty,
                this.EnvironmentInfo != null ? this.EnvironmentInfo.ToString() : string.Empty);
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
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == "ProcessId")
                    {
                        var pidStr = reader.ReadString();
                        int pid=0;
                        if (!string.IsNullOrEmpty(pidStr) && int.TryParse(pidStr, out pid))
                            this.ProcessId = pid;
                        reader.Read();
                    }
                    if (reader.LocalName == "ThreadId")
                    {
                        var threadStr = reader.ReadString();
                        int threadId = 0;
                        if (!string.IsNullOrEmpty(threadStr) && int.TryParse(threadStr, out threadId))
                            this.ThreadId = threadId;
                        reader.Read();
                    }
                    if (reader.LocalName == "Created")
                    {
                        var dtStr = reader.ReadString();
                        DateTime dt;
                        if (!string.IsNullOrEmpty(dtStr) && DateTime.TryParse(dtStr, out dt))
                            this.Created = dt;
                        reader.Read();
                    }
                    if (reader.LocalName == "Message")
                    {
                        this.Message = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "MessageInfo")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.MessageInfo = RuntimeSerializer.Deserialize<MessageInfo>(xml);
                    }
                    if (reader.LocalName == "MethodContext")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.MethodContext = RuntimeSerializer.Deserialize<MethodContextInfo>(xml);
                    }
                    if (reader.LocalName == "ResourceInfo")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.ResourceInfo = RuntimeSerializer.Deserialize<ResourceInfo>(xml);
                    }
                    if (reader.LocalName == "EnvironmentInfo")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.EnvironmentInfo = RuntimeSerializer.Deserialize<EnvironmentInfo>(xml);
                    }
                    if (reader.LocalName == "Fault")
                    {
                        string xml = reader.ReadInnerXml();
                        if (!string.IsNullOrEmpty(xml))
                            this.Fault = RuntimeSerializer.Deserialize<Fault>(xml);
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
            writer.WriteElementString("ProcessId", this.ProcessId.ToString());
            writer.WriteElementString("ThreadId", this.ThreadId.ToString());
            writer.WriteElementString("Created", this.Created.ToString());
            if (!string.IsNullOrEmpty(this.Message))
                writer.WriteElementString("Message", this.Message);

            if (this.MessageInfo != null)
            {
                writer.WriteStartElement("MessageInfo");
                RuntimeSerializer.Serialize(this.MessageInfo, writer);
                writer.WriteEndElement();
            }
            if (this.MethodContext != null)
            {
                writer.WriteStartElement("MethodContext");
                RuntimeSerializer.Serialize(this.MethodContext, writer);
                writer.WriteEndElement();
            }
            if (this.ResourceInfo != null)
            {
                writer.WriteStartElement("ResourceInfo");
                RuntimeSerializer.Serialize(this.ResourceInfo, writer);
                writer.WriteEndElement();
            }
            if (this.EnvironmentInfo != null)
            {
                writer.WriteStartElement("EnvironmentInfo");
                RuntimeSerializer.Serialize(this.EnvironmentInfo, writer);
                writer.WriteEndElement();
            }
            if (this.Fault != null)
            {
                writer.WriteStartElement("Fault");
                RuntimeSerializer.Serialize(this.Fault, writer);
                writer.WriteEndElement();
            }
        }
    }
}
