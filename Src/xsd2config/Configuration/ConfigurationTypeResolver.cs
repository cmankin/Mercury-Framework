using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Configuration;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mercury.Configuration
{
    /// <summary>
    /// Provides resolution facilities for type mapping with annotations.
    /// </summary>
    public class ConfigurationTypeResolver
    {
        private IList<TypeAnnotationHint> _configurationTypeHints;
        private Dictionary<string, TypeAnnotationHint> cache = new Dictionary<string, TypeAnnotationHint>();

        internal ConfigurationTypeResolver(ReadOnlyCollection<TypeAnnotationHint> typeHints)
        {
            this._configurationTypeHints = typeHints;
            foreach (TypeAnnotationHint hint in typeHints)
                if (hint.AssociatedNode != null && !string.IsNullOrEmpty(hint.AssociatedNode.Name))
                    this.cache.Add(hint.AssociatedNode.Name, hint);
        }

        /// <summary>
        /// Gets a list of hints used to clarify aspects of the configuration element type to build.
        /// </summary>
        public IList<TypeAnnotationHint> ConfigurationTypeHints
        {
            get { return this._configurationTypeHints; }
        }

        /// <summary>
        /// Updates the <see cref="System.CodeDom.CodeTypeDeclaration"/> with any context hints for the specified schema element.
        /// </summary>
        /// <param name="element">The <see cref="System.Xml.Schema.XmlSchemaElement"/> associated with the type declaration.</param>
        /// <param name="declaration">The <see cref="System.CodeDom.CodeTypeDeclaration"/> to update.</param>
        public void UpdateTypeDeclaration(ref CodeTypeDeclaration declaration)
        {
            if (declaration == null)
                return;

            TypeAnnotationHint hint;
            if (!this.cache.TryGetValue(declaration.Name, out hint))
                hint = new TypeAnnotationHint(-1, ConfigurationKind.Element, null, null, true, null);
            var model = new ConfigurationTypeModel(hint, declaration);
            declaration = model.Declaration;
        }

        private static CodeTypeMember FindMember(CodeTypeMemberCollection collection, string memberName)
        {
            foreach (CodeTypeMember member in collection)
            {
                if (string.Equals(member.Name, memberName, StringComparison.OrdinalIgnoreCase))
                    return member;
            }
            return null;
        }
    }
}
