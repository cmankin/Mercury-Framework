using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Configuration;

namespace Mercury.Configuration
{
    /// <summary>
    /// Specifies a set of transformations to perform on a <see cref=""/>.
    /// </summary>
    public class ConfigurationTypeDeclarationTransform
    {
        private TypeAnnotationHint _hint;
        private bool _isMemberHint;

        public ConfigurationTypeDeclarationTransform(TypeAnnotationHint hint)
        {
            if (hint == null)
                throw new ArgumentNullException("hint");
            this._hint = hint;
            this._isMemberHint = !string.IsNullOrEmpty(this._hint.OnMember);
        }

        public void Transform(CodeTypeDeclaration input)
        {
            if (input == null)
                return;

            this.EnsureBaseTypes(input);
            if (this._isMemberHint)
            {
                var member = FindMember(input.Members, this._hint.OnMember);
                if (member != null)
                {
                    if (this._hint.UseType != null)
                    {
                        
                    }
                    this.ConfigureMemberOrDeclaration(member);
                }
            }
            else
            {
                this.ConfigureMemberOrDeclaration(input);
            }
        }

        private void EnsureBaseTypes(CodeTypeDeclaration declaration)
        {
            switch (this._hint.Kind)
            {
                case ConfigurationKind.None:
                case ConfigurationKind.Element:
                    declaration.BaseTypes.Add(new CodeTypeReference(typeof(ConfigurationElement)));
                    break;
                case ConfigurationKind.CollectionElement:
                    var baseCollRef = new CodeTypeReference(typeof(BaseConfigurationElementCollection<>));
                    baseCollRef.TypeArguments.Add(new CodeTypeReference(declaration.Name));
                    declaration.BaseTypes.Add(baseCollRef);
                    break;
                case ConfigurationKind.Section:
                    declaration.BaseTypes.Add(new CodeTypeReference(typeof(ConfigurationSection)));
                    break;
            }
        }

        private void ConfigureMemberOrDeclaration(CodeTypeMember memberOrDeclaration)
        {
            if (!string.IsNullOrEmpty(this._hint.Name))
                memberOrDeclaration.Name = this._hint.Name;
        }

        private static CodeTypeMember GetReplacementMember(CodeTypeMember member, TypeAnnotationHint hint)
        {
            CodeTypeMember instance = null;
            string name = string.IsNullOrEmpty(hint.Name) ? member.Name : hint.Name;
            var typeRef = new CodeTypeReference(hint.UseType);

            if (member is CodeMemberField)
            {
                var field = new CodeMemberField(typeRef, name);
                instance = field;
            }
            else if (member is CodeMemberProperty)
            {
                var property = new CodeMemberProperty();
                property.Name = name;
                property.Type = typeRef;
                property.HasGet = property.HasSet = true;
                
                //property.
            }
            return null;
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
