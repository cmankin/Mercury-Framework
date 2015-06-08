using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace Mercury.Configuration
{
    /// <summary>
    /// Provides a set of hints for creating a System.Type that fulfills the XSD contract.
    /// </summary>
    public class TypeAnnotationHint
    {
        public const string ElementName = "Annotate";
        private const string KindAttribute = "kind";
        private const string NameAttribute = "name";
        private const string UseTypeAttribute = "useType";
        private const string TypeAssemblyAttribute = "typeAssembly";
        private const string EvaluateChildrenAttribute = "evaluateChildren";
        private const string OnMemberAttribute = "onMember";
        private static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment };

        private ConfigurationKind _kind;
        private string _name;
        private Type _useType;
        private bool _evaluateChildren;
        private int _lineNumber;
        private string _onMember;
        private XmlConfigurationReader.ConfigurationNode associatedNode;

        internal TypeAnnotationHint(int lineNumber, ConfigurationKind kind, string name, Type useType, bool evaluateChildren, string onMember)
        {
            this._lineNumber = lineNumber;
            this._kind = kind;
            this._name = name;
            this._useType = useType;
            this._evaluateChildren = evaluateChildren;
            this._onMember = onMember;
        }

        internal XmlConfigurationReader.ConfigurationNode AssociatedNode
        {
            get { return this.associatedNode; }
            set { this.associatedNode = value; }
        }

        /// <summary>
        /// Gets a value indicating the kind of configuration element to create.
        /// </summary>
        public ConfigurationKind Kind
        {
            get { return this._kind; }
        }

        /// <summary>
        /// Gets the name of the System.Type to create.
        /// </summary>
        public string Name
        {
            get { return this._name; }
        }

        /// <summary>
        /// Gets the System.Type that should be used instead of generating a new type.
        /// </summary>
        public Type UseType
        {
            get { return this._useType; }
        }

        /// <summary>
        /// Gets a value indicating whether child elements should be evaluated as part of the configuration XML tree.
        /// </summary>
        public bool EvaluateChildren
        {
            get { return this._evaluateChildren; }
        }

        /// <summary>
        /// Gets the line number of the original annotation comment.
        /// </summary>
        public int LineNumber
        {
            get { return this._lineNumber; }
        }

        /// <summary>
        /// Gets the name of the child member of the associated node to which this hint applies.
        /// </summary>
        public string OnMember
        {
            get { return this._onMember; }
        }

        public static TypeAnnotationHint Create(int lineNumber, string xml)
        {
            if (!string.IsNullOrEmpty(xml))
            {
                using (var sr = new StringReader(xml))
                {
                    return ParseAnnotateXml(sr, lineNumber);
                }
            }
            return null;
        }

        private static TypeAnnotationHint ParseAnnotateXml(StringReader sr, int lineNumber)
        {
            using (var reader = XmlReader.Create(sr, TypeAnnotationHint.ReaderSettings))
            {
                reader.Read();
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == TypeAnnotationHint.ElementName)
                    {
                        if (reader.MoveToFirstAttribute())
                        {
                            string kindStr, name, useTypeStr, typeAsmStr, evalCh, onMember;
                            kindStr = name = useTypeStr = typeAsmStr = evalCh = onMember = null;

                            do
                            {
                                switch (reader.LocalName)
                                {
                                    case TypeAnnotationHint.KindAttribute:
                                        kindStr = reader.Value;
                                        break;
                                    case TypeAnnotationHint.NameAttribute:
                                        name = reader.Value;
                                        break;
                                    case TypeAnnotationHint.UseTypeAttribute:
                                        useTypeStr = reader.Value;
                                        break;
                                    case TypeAnnotationHint.TypeAssemblyAttribute:
                                        typeAsmStr = reader.Value;
                                        break;
                                    case TypeAnnotationHint.EvaluateChildrenAttribute:
                                        evalCh = reader.Value;
                                        break;
                                    case TypeAnnotationHint.OnMemberAttribute:
                                        onMember = reader.Value;
                                        break;
                                }
                            } while (reader.MoveToNextAttribute());
                            return BuildFrom(lineNumber, kindStr, name, useTypeStr, typeAsmStr, evalCh, onMember);
                        }
                    }
                }
                return null;
            }
        }

        private static TypeAnnotationHint BuildFrom(int lineNumber, string kindStr, string name, string useTypeStr, string typeAsmString, string evaluateChildrenStr, string onMember)
        {
            bool flag;
            bool eval = true;
            Type useType = GetType(useTypeStr, typeAsmString);
            ConfigurationKind kind = ConfigurationKind.None;

            Enum.TryParse<ConfigurationKind>(kindStr, true, out kind);
            if (bool.TryParse(evaluateChildrenStr, out flag))
                eval = flag;

            return new TypeAnnotationHint(lineNumber, kind, name, useType, eval, onMember);
        }

        private static Type GetType(string typeStr, string asmString)
        {
            Type reflectedType;
            Assembly asm = GetAssembly(asmString);
            if (!string.IsNullOrEmpty(typeStr))
            {
                if (asm != null)
                    reflectedType = asm.GetType(typeStr);
                else
                    reflectedType = Type.GetType(typeStr);
                return reflectedType;
            }
            return null;
        }

        private static Assembly GetAssembly(string asmString)
        {
            Assembly asm = null;
            try
            {
                if (!string.IsNullOrEmpty(asmString))
                {
                    try
                    {
                        asm = Assembly.LoadFile(asmString);
                    }
                    catch
                    {
                    }
                    if (asm == null)
                        asm = Assembly.Load(asmString);
                }
            }
            catch 
            { 
            }
            return asm;
        }
    }
}
