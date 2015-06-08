using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mercury.Configuration;

namespace xsd2config.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Generator_specs
    {
        public Generator_specs()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        #region test code
        private const string TestCode = @"
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"" elementFormDefault=""qualified""
           xmlns:tns=""http://framework.mercury.com/Logging/LogConfigSchema""
           targetNamespace=""http://framework.mercury.com/Logging/LogConfigSchema"">
  <!--<Annotate kind=""section"" name=""DataConfigurationSection""/>-->
  <xs:element name=""dataConfig"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""dataItem"" type=""tns:DataElement"" maxOccurs=""unbounded""/>
      </xs:sequence>
    </xs:complexType>
    <xs:unique name=""uniqueName"">
      <xs:selector xpath=""tns:dataItem""/>
      <xs:field xpath=""@name""/>
    </xs:unique>
  </xs:element>

  <!--<Annotate evaluateChildren=""false"" onMember=""value""/>-->
  <xs:complexType name=""DataElement"">
    <xs:sequence>
      <xs:element name=""instance"" minOccurs=""0"" maxOccurs=""1"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""delegate"" type=""tns:DataDelegateElement"" maxOccurs=""unbounded""/>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <xs:element name=""value"" minOccurs=""0"" maxOccurs=""1"">
        <xs:complexType mixed=""true"">
          <xs:sequence>
            <xs:any minOccurs=""0"" maxOccurs=""unbounded"" processContents=""skip""/>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:sequence>
    <xs:attribute name=""name"" type=""xs:string"" use=""required""/>
    <xs:attribute name=""type"" type=""xs:string""/>
    <xs:attribute name=""ref"" type=""xs:string""/>
  </xs:complexType>

  <xs:complexType name=""DataDelegateElement"" final=""#all"">
    <xs:attribute name=""category"" type=""tns:DataDelegateCategory"" use=""required""/>
    <xs:attribute name=""type"" type=""xs:string"" use=""required""/>
    <xs:attribute name=""methodName"" type=""xs:string"" use=""required""/>
  </xs:complexType>

  <xs:simpleType name=""DataDelegateCategory"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""Activator""/>
      <xs:enumeration value=""Initializer""/>
      <xs:enumeration value=""Transform""/>
    </xs:restriction>
  </xs:simpleType>


  <!--<Annotate kind=""section"" name=""TestConfigurationSection""/>-->
  <xs:element name=""testConfig"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""context"" type=""tns:TestContextConfigurationElement"" maxOccurs=""unbounded""/>
      </xs:sequence>
    </xs:complexType>
    <xs:unique name=""uniqueActiveContext"">
      <xs:selector xpath=""tns:context""/>
      <xs:field xpath=""@active""/>
    </xs:unique>
  </xs:element>

  <xs:complexType name=""TestContextConfigurationElement"">
    <xs:sequence>
      <xs:element name=""values"" minOccurs=""0"" maxOccurs=""1"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""add"" type=""tns:ValueElement"" maxOccurs=""unbounded""/>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <!--<Annotate useType=""System.Configuration.ConnectionStringSettings"" typeAssembly=""System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a""/>-->
      <xs:element name=""connectionStrings"" minOccurs=""0"" maxOccurs=""1"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""add"" type=""tns:ConnectionStringElement"" maxOccurs=""unbounded""/>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
      <xs:element name=""data"" minOccurs=""0"" maxOccurs=""1"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""dataItem"" type=""tns:DataElement"" maxOccurs=""unbounded""/>
          </xs:sequence>
        </xs:complexType>
        <xs:unique name=""uniqueDataItemName"">
          <xs:selector xpath=""tns:dataItem""/>
          <xs:field xpath=""@name""/>
        </xs:unique>
      </xs:element>
    </xs:sequence>
    <xs:attribute name=""name"" type=""xs:string"" use=""required""/>
    <xs:attribute name=""active"" type=""xs:boolean""/>
  </xs:complexType>

  <xs:complexType name=""ValueElement"">
    <xs:attribute name=""key"" type=""xs:string"" use =""required""/>
    <xs:attribute name=""value"" type=""xs:string"" use=""required""/>
    <xs:attribute name=""type"" type=""xs:string""/>
  </xs:complexType>

  <xs:complexType name=""ConnectionStringElement"">
    <xs:attribute name=""name"" type=""xs:string""/>
    <xs:attribute name=""connectionString"" type=""xs:string""/>
    <xs:attribute name=""providerName"" type=""xs:string""/>
  </xs:complexType>
</xs:schema>";
        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Can_generator_export_types_from_xsd()
        {
            var ns = ConfigurationGenerator.TransformText(TestCode, "Mercury.Configuration");
            Assert.IsNotNull(ns);
            Assert.IsInstanceOfType(ns, typeof(CodeNamespace));
        }

        [TestMethod]
        public void Can_generate_code_from_xsd()
        {
            CodeNamespace ns = ConfigurationGenerator.TransformText(TestCode, "Mercury.Configuration");
            CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            string filePath = Util.GetPathInCurrentAssembly("Config.g.cs");

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    provider.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
                }
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
}
