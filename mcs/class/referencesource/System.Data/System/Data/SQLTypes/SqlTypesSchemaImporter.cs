//------------------------------------------------------------------------------
// <copyright file="SqlTypesSchemaImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">dondu</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------


namespace System.Data.SqlTypes {

    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.Serialization.Advanced;

    public class SqlTypesSchemaImporterExtensionHelper : SchemaImporterExtension {
        private string m_name;
        private string m_targetNamespace;
        private string[] m_references;
        private CodeNamespaceImport[] m_namespaceImports;
        private string m_destinationType;
        private bool m_direct;
       public SqlTypesSchemaImporterExtensionHelper(string name,
                                                     string targetNamespace,
                                                     string[] references,
                                                     CodeNamespaceImport[] namespaceImports,
                                                     string destinationType,
                                                     bool direct) {
            Init(name, targetNamespace, references, namespaceImports, destinationType, direct);
        }
        public SqlTypesSchemaImporterExtensionHelper(string name, string destinationType) {
            Init(name, SqlTypesNamespace, null, null, destinationType, true);
        }
        public SqlTypesSchemaImporterExtensionHelper(string name, string destinationType, bool direct) {
            Init(name, SqlTypesNamespace, null, null, destinationType, direct);
        }
        private void Init(string name,
                          string targetNamespace,
                          string[] references,
                          CodeNamespaceImport[] namespaceImports,
                          string destinationType,
                          bool direct) {
            m_name = name;
            m_targetNamespace = targetNamespace;
            if (references == null) {
                m_references = new string[1];
                m_references[0] = "System.Data.dll";
            } else {
                m_references = references;
            }
            if (namespaceImports == null) {
                m_namespaceImports = new CodeNamespaceImport[2];
                m_namespaceImports[0] = new CodeNamespaceImport("System.Data");
                m_namespaceImports[1] = new CodeNamespaceImport("System.Data.SqlTypes");
            } else {
                m_namespaceImports = namespaceImports;
            }
            m_destinationType = destinationType;
            m_direct = direct;
        }
        public override string ImportSchemaType(string name,
                                                string xmlNamespace,
                                                XmlSchemaObject context,
                                                XmlSchemas schemas,
                                                XmlSchemaImporter importer,
                                                CodeCompileUnit compileUnit,
                                                CodeNamespace mainNamespace,
                                                CodeGenerationOptions options,
                                                CodeDomProvider codeProvider) {
            if (m_direct) {
                if (context is XmlSchemaElement) {
                    if ((0 == string.CompareOrdinal(m_name, name)) && (0 == string.CompareOrdinal(m_targetNamespace, xmlNamespace))) {
                        compileUnit.ReferencedAssemblies.AddRange(m_references);
                        mainNamespace.Imports.AddRange(m_namespaceImports);
                        return m_destinationType;
                    }
                }
            }
            return null;
        }
        public override string ImportSchemaType(XmlSchemaType type,
                                                XmlSchemaObject context,
                                                XmlSchemas schemas,
                                                XmlSchemaImporter importer,
                                                CodeCompileUnit compileUnit,
                                                CodeNamespace mainNamespace,
                                                CodeGenerationOptions options,
                                                CodeDomProvider codeProvider) {
            if (!m_direct) {
                if ((type is XmlSchemaSimpleType) && (context is XmlSchemaElement)) {
                    XmlSchemaType basetype = ((XmlSchemaSimpleType) type).BaseXmlSchemaType;
                    XmlQualifiedName qname = basetype.QualifiedName;
                    if ((0 == string.CompareOrdinal(m_name, qname.Name)) && (0 == string.CompareOrdinal(m_targetNamespace, qname.Namespace))) {
                        compileUnit.ReferencedAssemblies.AddRange(m_references);
                        mainNamespace.Imports.AddRange(m_namespaceImports);
                        return m_destinationType;
                    }
                }
            }
            return null;
        }

        protected static readonly string SqlTypesNamespace = "http://schemas.microsoft.com/sqlserver/2004/sqltypes";
    };
    
    public sealed class TypeCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeCharSchemaImporterExtension() : base("char", "System.Data.SqlTypes.SqlString", false) { }
    }

    public sealed class TypeNCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeNCharSchemaImporterExtension() : base("nchar", "System.Data.SqlTypes.SqlString", false) { }
    }

    public sealed class TypeVarCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeVarCharSchemaImporterExtension() : base("varchar", "System.Data.SqlTypes.SqlString", false) { }
    }

    public sealed class TypeNVarCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeNVarCharSchemaImporterExtension() : base("nvarchar", "System.Data.SqlTypes.SqlString", false) { }
    }

    public sealed class TypeTextSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeTextSchemaImporterExtension() : base("text", "System.Data.SqlTypes.SqlString", false) { }
    }

    public sealed class TypeNTextSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeNTextSchemaImporterExtension() : base("ntext", "System.Data.SqlTypes.SqlString", false) { }
    }

    public sealed class TypeVarBinarySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeVarBinarySchemaImporterExtension() : base("varbinary", "System.Data.SqlTypes.SqlBinary", false) { }
    }

    public sealed class TypeBinarySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeBinarySchemaImporterExtension() : base("binary", "System.Data.SqlTypes.SqlBinary", false) { }
    }

    public sealed class TypeVarImageSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeVarImageSchemaImporterExtension() : base("image", "System.Data.SqlTypes.SqlBinary", false) { }
    }

/*
  <xsd:simpleType name=\"timestamp\">
    <xsd:restriction base=\"xsd:base64Binary\">
      <xsd:maxLength value=\"8\"/>
    </xsd:restriction>
  </xsd:simpleType>
*/

/*
  <xsd:simpleType name=\"timestampNumeric\">
  <!-- The timestampNumeric type supports a legacy format of timestamp. -->
    <xsd:restriction base=\"xsd:long\"/>
  </xsd:simpleType>
*/

    public sealed class TypeDecimalSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeDecimalSchemaImporterExtension() : base("decimal", "System.Data.SqlTypes.SqlDecimal", false) { }
    }

    public sealed class TypeNumericSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeNumericSchemaImporterExtension() : base("numeric", "System.Data.SqlTypes.SqlDecimal", false) { }
    }

    public sealed class TypeBigIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeBigIntSchemaImporterExtension() : base("bigint", "System.Data.SqlTypes.SqlInt64") { }
    }

    public sealed class TypeIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeIntSchemaImporterExtension() : base("int", "System.Data.SqlTypes.SqlInt32") { }
    }

    public sealed class TypeSmallIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeSmallIntSchemaImporterExtension() : base("smallint", "System.Data.SqlTypes.SqlInt16") { }
    }

    public sealed class TypeTinyIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeTinyIntSchemaImporterExtension() : base("tinyint", "System.Data.SqlTypes.SqlByte") { }
    }

    public sealed class TypeBitSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeBitSchemaImporterExtension() : base("bit", "System.Data.SqlTypes.SqlBoolean") { }
    }

    public sealed class TypeFloatSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeFloatSchemaImporterExtension() : base("float", "System.Data.SqlTypes.SqlDouble") { }
    }

    public sealed class TypeRealSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeRealSchemaImporterExtension() : base("real", "System.Data.SqlTypes.SqlSingle") { }
    }

/*
  <xsd:simpleType name=\"utcdatetime\">
    <xsd:restriction base=\"xsd:dateTime\">
      <xsd:pattern value=\"((000[1-9])|(00[1-9][0-9])|(0[1-9][0-9]{2})|([1-9][0-9]{3}))-((0[1-9])|(1[0,1,2]))-((0[1-9])|([1,2][0-9])|(3[0,1]))T(([0,1][0-9])|(2[0-3]))(:[0-5][0-9]){2}\.[0-9]{7}(Z|(\-|\+)(((0[1-9])|(1[0-2])):[0-5][0-9]))\"/>
      <xsd:maxInclusive value=\"9999-12-31T23:59:59.9999999Z\"/>
      <xsd:minInclusive value=\"0001-01-01T00:00:00.0000000Z\"/>
    </xsd:restriction>
  </xsd:simpleType>
*/

    public sealed class TypeDateTimeSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeDateTimeSchemaImporterExtension() : base("datetime", "System.Data.SqlTypes.SqlDateTime") { }
    }

    public sealed class TypeSmallDateTimeSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeSmallDateTimeSchemaImporterExtension() : base("smalldatetime", "System.Data.SqlTypes.SqlDateTime") { }
    }

    public sealed class TypeMoneySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeMoneySchemaImporterExtension() : base("money", "System.Data.SqlTypes.SqlMoney") { }
    }

    public sealed class TypeSmallMoneySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeSmallMoneySchemaImporterExtension() : base("smallmoney", "System.Data.SqlTypes.SqlMoney") { }
    }

    public sealed class TypeUniqueIdentifierSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper {
        public TypeUniqueIdentifierSchemaImporterExtension() : base("uniqueidentifier", "System.Data.SqlTypes.SqlGuid") { }
    }

/*
  <!-- sql_variant directly maps to xsd:anyType -->
*/

/*
  <xsd:complexType name=\"xml\" mixed=\"true\">
    <xsd:sequence>
      <xsd:any minOccurs=\"0\" maxOccurs=\"unbounded\" processContents=\"skip\" />
    </xsd:sequence>
  </xsd:complexType>
*/

/*
  <xsd:simpleType name=\"dbobject\">
    <xsd:restriction base=\"xsd:anyURI\" />
  </xsd:simpleType>
*/

}
