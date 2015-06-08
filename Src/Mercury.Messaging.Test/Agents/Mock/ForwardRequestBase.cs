using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercury.Messaging.Messages;

namespace Mercury.Messaging.Test.Agents.Mock
{
    public class ForwardRequestBase<T> : 
        MessageBase<T>, 
        ForwardRequest<T>
    {
        public ForwardRequestBase()
        {
        }

        public ForwardRequestBase(string forwarderId, T message)
            : base(message)
        {
            this._forwarderId = forwarderId;
        }

        private string _forwarderId;

        public string ForwarderId
        {
            get { return this._forwarderId; }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.LocalName == "ForwarderId")
                    {
                        this._forwarderId = reader.ReadString();
                        reader.Read();
                    }
                    if (reader.LocalName == "_base")
                        base.ReadXml(reader);
                }
            }
            reader.Read();
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            if (!string.IsNullOrEmpty(this.ForwarderId))
                writer.WriteElementString("ForwarderId", this.ForwarderId);

            writer.WriteStartElement("_base");
            base.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
