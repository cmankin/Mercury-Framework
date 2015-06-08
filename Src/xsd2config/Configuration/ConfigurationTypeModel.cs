using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Threading;
using System.Configuration;
using System.Globalization;

namespace Mercury.Configuration
{
    /// <summary>
    /// Represents a model that abstracts the CodeDom for configuration types.
    /// </summary>
    public class ConfigurationTypeModel
    {
        private const string FIELD_TEXT_APPENDED = "Field";

        private TypeAnnotationHint _hint;
        private CodeTypeDeclaration _declaration;
        private CodeTypeDeclaration _originalDeclaration;

        public ConfigurationTypeModel(TypeAnnotationHint hint, CodeTypeDeclaration originalDeclaration)
        {
            if (hint == null)
                throw new ArgumentNullException("hint");
            this._hint = hint;
            this._originalDeclaration = originalDeclaration;
            this.__Init(originalDeclaration);
        }

        /// <summary>
        /// Gets the resulting type declaration based upon the provided hints.
        /// </summary>
        public CodeTypeDeclaration Declaration
        {
            get { return this._declaration; }
        }

        /// <summary>
        /// Gets the original type declaration provided to this model.
        /// </summary>
        public CodeTypeDeclaration Original
        {
            get { return this._originalDeclaration; }
        }

        // THIS METHOD IS ONLY EXPECTED TO RUN ONCE AT OBJECT CONSTRUCTION.
        //
        // DO NOT CALL THIS METHOD FROM OTHER METHODS OR CALL VIRTUAL 
        // METHODS FROM INSIDE THIS METHOD.
        private void __Init(CodeTypeDeclaration originalDeclaration)
        {
            if (this._hint.UseType != null && !string.IsNullOrEmpty(this._hint.OnMember))
                return;

            var name = !string.IsNullOrEmpty(this._hint.Name) ? this._hint.Name : originalDeclaration.Name;
            this._declaration = new CodeTypeDeclaration(name);

            switch (this._hint.Kind)
            {
                case ConfigurationKind.None:
                case ConfigurationKind.Element:
                    this._declaration.BaseTypes.Add(new CodeTypeReference(typeof(ConfigurationElement)));
                    break;
                case ConfigurationKind.CollectionElement:
                    var baseCollRef = new CodeTypeReference(typeof(BaseConfigurationElementCollection<>));
                    baseCollRef.TypeArguments.Add(new CodeTypeReference(this._declaration.Name));
                    this._declaration.BaseTypes.Add(baseCollRef);
                    break;
                case ConfigurationKind.Section:
                    this._declaration.BaseTypes.Add(new CodeTypeReference(typeof(ConfigurationSection)));
                    break;
            }

            this.CreateFieldAndPropertyMembers(originalDeclaration);

            if (!this._hint.EvaluateChildren)
                this.AddOnDeserializeUnrecognizedElementOverride();
        }

        // THIS METHOD IS ONLY EXPECTED TO RUN ONCE AT OBJECT CONSTRUCTION.
        //
        // DO NOT CALL THIS METHOD FROM OTHER METHODS OR CALL VIRTUAL 
        // METHODS FROM INSIDE THIS METHOD.
        private void CreateFieldAndPropertyMembers(CodeTypeDeclaration original)
        {
            var typeRef = new CodeTypeReferenceExpression(new CodeTypeReference(this._declaration.Name));
            var propCollectionField = new CodeMemberField(new CodeTypeReference(typeof(ConfigurationPropertyCollection)), "ConfigurationPropertyCollection");
            propCollectionField.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            var propCollectionFieldReference = new CodeFieldReferenceExpression(typeRef, propCollectionField.Name);
            this._declaration.Members.Add(propCollectionField);

            // Override ConfigurationElement.Properties property
            var propCollectionProperty = new CodeMemberProperty();
            propCollectionProperty.Name = "Properties";
            propCollectionProperty.Type = propCollectionField.Type;
            propCollectionProperty.Attributes = MemberAttributes.Override;
            propCollectionProperty.GetStatements.Add(new CodeMethodReturnStatement(propCollectionFieldReference));
            propCollectionProperty.HasGet = true;
            propCollectionProperty.HasSet = false;
            this._declaration.Members.Add(propCollectionProperty);

            // Set static constructor
            var staticConstructor = new CodeTypeConstructor();
            staticConstructor.Name = this._declaration.Name;
            var propCollectionInstantiator = new CodeObjectCreateExpression(typeof(ConfigurationPropertyCollection));
            staticConstructor.Statements.Add(new CodeAssignStatement(propCollectionFieldReference, propCollectionInstantiator));
            this._declaration.Members.Add(staticConstructor);

            foreach (CodeMemberField member in original.Members.OfType<CodeMemberField>())
            {
                // Field
                var memberName = ParseFieldName(member.Name);
                var staticField = new CodeMemberField(new CodeTypeReference(typeof(ConfigurationProperty)), string.Format("{0}Property", memberName));
                staticField.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                var fieldRef = new CodeFieldReferenceExpression(typeRef, staticField.Name);
                this._declaration.Members.Add(staticField);

                var configPropertyInstantiator = new CodeObjectCreateExpression(typeof(ConfigurationProperty), new CodePrimitiveExpression(memberName), new CodeTypeOfExpression(member.Type));
                staticConstructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(typeRef, staticField.Name), configPropertyInstantiator));
                staticConstructor.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(propCollectionFieldReference, "Add"), new CodeFieldReferenceExpression(typeRef, staticField.Name)));

                // Property
                var instProperty = new CodeMemberProperty();
                instProperty.Attributes = MemberAttributes.Public;
                instProperty.Name = FirstLetterToUpper(memberName);
                var memberType = member.Type;
                if (string.Equals(memberName, this._hint.OnMember, StringComparison.OrdinalIgnoreCase) && this._hint.UseType != null)
                    memberType = new CodeTypeReference(this._hint.UseType);
                instProperty.Type = member.Type;
                
                var thisArr = new CodeIndexerExpression(new CodeThisReferenceExpression(), fieldRef);
                // get
                var castExpr = new CodeCastExpression(memberType, thisArr);
                var retStatement = new CodeMethodReturnStatement(castExpr);
                instProperty.GetStatements.Add(retStatement);
                instProperty.HasGet = true;

                // set
                var assignStmt = new CodeAssignStatement(thisArr, new CodePropertySetValueReferenceExpression());
                instProperty.SetStatements.Add(assignStmt);
                this._declaration.Members.Add(instProperty);
                instProperty.HasSet = true;
            }
        }

        private void AddOnDeserializeUnrecognizedElementOverride()
        {
            // override ConfigurationElement.OnDeserializeUnrecognizedElement
            var method = new CodeMemberMethod();
            method.Name = "OnDeserializeUnrecognizedElement";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;

            // Set parameters
            var elementNameParam = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "elementName");
            var readerParam = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(System.Xml.XmlReader)), "reader");
            method.Parameters.Add(elementNameParam);
            method.Parameters.Add(readerParam);

            if (!string.IsNullOrEmpty(this._hint.OnMember))
            {
                var fieldName = string.Format("{0}InnerXml", this._hint.OnMember);
                var field = new CodeMemberField(new CodeTypeReference(typeof(string)), fieldName);
                this._declaration.Members.Add(field);

                var onMemberExpr = new CodePrimitiveExpression(this._hint.OnMember);
                var elementNameRef = new CodeVariableReferenceExpression(elementNameParam.Name);
                var condition = new CodeBinaryOperatorExpression(elementNameRef, CodeBinaryOperatorType.ValueEquality, onMemberExpr);

                // Build true block
                var assignFieldXml = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName),
                    new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression(readerParam.Name), "ReadInnerXml"));
                var returnTrueStmt = new CodeMethodReturnStatement(new CodePrimitiveExpression(true));
                // if statement
                var ifStmt = new CodeConditionStatement(condition, assignFieldXml, returnTrueStmt);

                // Build outer block
                var baseRef = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), method.Name,
                    new CodeArgumentReferenceExpression(elementNameParam.Name), new CodeArgumentReferenceExpression(readerParam.Name));
                var defaultReturnStmt = new CodeMethodReturnStatement(baseRef);

                // Add statements to method
                method.Statements.Add(ifStmt);
                method.Statements.Add(defaultReturnStmt);
            }
            else
            {
                method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            }
            this._declaration.Members.Add(method);
        }

        private static string ParseFieldName(string fieldName)
        {
            if (fieldName != null && fieldName.Length > FIELD_TEXT_APPENDED.Length)
            {
                var appendIdx = fieldName.Length - FIELD_TEXT_APPENDED.Length;
                var appended = fieldName.Substring(appendIdx, FIELD_TEXT_APPENDED.Length);
                if (string.Equals(appended, FIELD_TEXT_APPENDED, StringComparison.OrdinalIgnoreCase))
                    return fieldName.Substring(0, appendIdx);
            }
            return fieldName;
        }

        private static string FirstLetterToUpper(string input)
        {
            if (input == null)
                return null;
            if (input.Length > 1)
                return char.ToUpper(input[0]) + input.Substring(1);
            return input.ToUpper();
        }
    }
}
