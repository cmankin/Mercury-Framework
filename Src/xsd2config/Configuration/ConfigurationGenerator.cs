using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mercury.Configuration
{
    /// <summary>
    /// Provides methods for generating configuration element classes from an XSD.
    /// </summary>
    public static class ConfigurationGenerator
    {
        /// <summary>
        /// Returns a <see cref="System.CodeDom.CodeNamespace"/> of configuration classes generated from the specified XSD text and target namespace.
        /// </summary>
        /// <param name="xsd">A string of text representing the XML Schema Definition (XSD) to transform.</param>
        /// <param name="targetNamespace">The target namespace for the generated classes.</param>
        /// <returns>A <see cref="System.CodeDom.CodeNamespace"/> of configuration classes generated from the specified XSD text and target namespace.</returns>
        public static CodeNamespace TransformText(string xsd, string targetNamespace)
        {
            using (var sr = new StringReader(xsd))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationType = ValidationType.Schema;

                using (var reader = new XmlConfigurationReader(XmlReader.Create(sr, settings)))
                {
                    
                    XmlSchema schema = XmlSchema.Read(reader, null);
                    return TransformInternal(reader.TypeResolver, schema, targetNamespace);
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.CodeDom.CodeNamespace"/> of configuration classes generated in the target namespace from the XSD at the specified file path.
        /// </summary>
        /// <param name="xsdFilePath">A complete file path to the XML Schema Definition (XSD) document to transform.</param>
        /// <param name="targetNamespace">The target namespace for the generated classes.</param>
        /// <returns>A <see cref="System.CodeDom.CodeNamespace"/> of configuration classes generated in the target namespace from the XSD at the specified file path.</returns>
        public static CodeNamespace Transform(string xsdFilePath, string targetNamespace)
        {
            XmlSchema xsd;

            using (FileStream fs = new FileStream(xsdFilePath, FileMode.Open))
            {
                using (var reader = new XmlConfigurationReader(XmlReader.Create(fs)))
                {
                    xsd = XmlSchema.Read(fs, null);
                    return TransformInternal(reader.TypeResolver, xsd, targetNamespace);
                }
            }
        }

        private static CodeNamespace TransformInternal(ConfigurationTypeResolver resolver, XmlSchema xsd, string targetNamespace)
        {
            if (xsd == null)
                throw new ArgumentNullException("xsd");

            XmlSchemas schemas = new XmlSchemas();
            schemas.Add(xsd);
            schemas.Compile(null, true);
            XmlSchemaImporter importer = new XmlSchemaImporter(schemas);

            CodeNamespace ns = new CodeNamespace(targetNamespace);
            XmlCodeExporter exporter = new XmlCodeExporter(ns);

            foreach (XmlSchemaElement element in xsd.Elements.Values)
            {
                
                XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);
                
                //exporter.IncludeMetadata.Clear();
                //exporter.IncludeMetadata.Add(GetLineInfoAttribute(element.LineNumber, element.LinePosition));
                exporter.ExportTypeMapping(mapping);
            }

            if (ns.Types.Count > 0)
            {
                CodeTypeDeclaration decl;
                for (int i = 0; i < ns.Types.Count; i++)
                {
                    decl = ns.Types[i];
                    resolver.UpdateTypeDeclaration(ref decl);
                    if (decl == null)
                    {
                        ns.Types.RemoveAt(i);
                    }
                    else
                    {
                        ns.Types[i] = decl;
                    }
                }

                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Configuration"));
                ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
                ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            }

            return ns;
        }

        private static CodeAttributeDeclaration GetLineInfoAttribute(int line, int column)
        {
            return new CodeAttributeDeclaration(new CodeTypeReference(typeof(LineInfoAttribute)),
                new CodeAttributeArgument(new CodePrimitiveExpression(line)), new CodeAttributeArgument(new CodePrimitiveExpression(column)));
        }
    }
}
