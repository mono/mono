//
// System.Xml.Serialization.SoapSchemaExporterTests
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
// 

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class SoapSchemaExporterTests
	{
		[Test]
		[Category ("NotWorking")]
		public void ExportStruct ()
		{
			SoapReflectionImporter ri = new SoapReflectionImporter ("NSTimeSpan");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (TimeSpan));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSTimeSpan\" elementFormDefault=\"qualified\" targetNamespace=\"NSTimeSpan\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSTimeSpan\" targetNamespace=\"NSTimeSpan\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}


		[Test]
		[Category ("NotWorking")]
		public void ExportClass ()
		{
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			SoapAttributes attr = new SoapAttributes ();
			SoapElementAttribute element = new SoapElementAttribute ();
			element.ElementName = "saying";
			element.IsNullable = true;
			attr.SoapElement = element;
			overrides.Add (typeof (SimpleClass), "something", attr);

			SoapReflectionImporter ri = new SoapReflectionImporter (overrides, "NSSimpleClass");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (SimpleClass));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSSimpleClass\" elementFormDefault=\"qualified\" targetNamespace=\"NSSimpleClass\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSSimpleClass\" targetNamespace=\"NSSimpleClass\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"saying\" nillable=\"true\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"saying\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))] // Cannot use wildcards at the top level of a schema.
		public void ExportClass_XmlNode ()
		{
			XmlReflectionImporter ri = new XmlReflectionImporter ("NS1");
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter sx = new XmlSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (XmlNode));
			sx.ExportTypeMapping (tm);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))] // Cannot use wildcards at the top level of a schema.
		public void ExportClass_XmlElement ()
		{
			XmlReflectionImporter ri = new XmlReflectionImporter ("NS1");
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter sx = new XmlSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (XmlElement));
			sx.ExportTypeMapping (tm);
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportClass_Array ()
		{
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			SoapAttributes attr = new SoapAttributes ();
			SoapElementAttribute element = new SoapElementAttribute ();
			element.ElementName = "saying";
			element.IsNullable = true;
			attr.SoapElement = element;
			overrides.Add (typeof (SimpleClass), "something", attr);

			SoapReflectionImporter ri = new SoapReflectionImporter (overrides, "NSSimpleClassArray");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (SimpleClass[]));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSSimpleClassArray\" elementFormDefault=\"qualified\" targetNamespace=\"NSSimpleClassArray\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSSimpleClassArray\" targetNamespace=\"NSSimpleClassArray\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"ArrayOfSimpleClass\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"tns:SimpleClass[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"saying\" nillable=\"true\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"saying\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportEnum ()
		{
			SoapReflectionImporter ri = new SoapReflectionImporter ("NSEnumDefaultValue");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (EnumDefaultValue));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSEnumDefaultValue\" elementFormDefault=\"qualified\" targetNamespace=\"NSEnumDefaultValue\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSEnumDefaultValue\" targetNamespace=\"NSEnumDefaultValue\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:simpleType name=\"EnumDefaultValue\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"e1\" />{0}" +
				"          <xs:enumeration value=\"e2\" />{0}" +
				"          <xs:enumeration value=\"e3\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			ri = new SoapReflectionImporter ("NSEnumDefaultValueNF");
			schemas = new XmlSchemas ();
			sx = new SoapSchemaExporter (schemas);
			tm = ri.ImportTypeMapping (typeof (EnumDefaultValueNF));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#3");

			sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSEnumDefaultValueNF\" elementFormDefault=\"qualified\" targetNamespace=\"NSEnumDefaultValueNF\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSEnumDefaultValueNF\" targetNamespace=\"NSEnumDefaultValueNF\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:simpleType name=\"EnumDefaultValueNF\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:enumeration value=\"e1\" />{0}" +
				"      <xs:enumeration value=\"e2\" />{0}" +
				"      <xs:enumeration value=\"e3\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportXsdPrimitive ()
		{
			ArrayList types = new ArrayList ();
			types.Add (new TypeDescription (typeof (object), true, "anyType", "Object"));
			types.Add (new TypeDescription (typeof (byte), true, "unsignedByte", "Byte"));
			types.Add (new TypeDescription (typeof (sbyte), true, "byte", "Byte"));
			types.Add (new TypeDescription (typeof (bool), true, "boolean", "Boolean"));
			types.Add (new TypeDescription (typeof (short), true, "short", "Short"));
			types.Add (new TypeDescription (typeof (int), true, "int", "Int"));
			types.Add (new TypeDescription (typeof (long), true, "long", "Long"));
			types.Add (new TypeDescription (typeof (float), true, "float", "Float"));
			types.Add (new TypeDescription (typeof (double), true, "double", "Double"));
			types.Add (new TypeDescription (typeof (decimal), true, "decimal", "Decimal"));
			types.Add (new TypeDescription (typeof (ushort), true, "unsignedShort", "UnsignedShort"));
			types.Add (new TypeDescription (typeof (uint), true, "unsignedInt", "UnsignedInt"));
			types.Add (new TypeDescription (typeof (ulong), true, "unsignedLong", "UnsignedLong"));
			types.Add (new TypeDescription (typeof (DateTime), true, "dateTime", "DateTime"));
#if NET_2_0
			types.Add (new TypeDescription (typeof (XmlQualifiedName), true, "QName", "QName", true));
#else
			types.Add (new TypeDescription (typeof (XmlQualifiedName), true, "QName", "QName"));
#endif
			types.Add (new TypeDescription (typeof (string), true, "string", "String", true));

			foreach (TypeDescription typeDesc in types) {
				SoapReflectionImporter ri = new SoapReflectionImporter (typeDesc.Type.Name);
				XmlSchemas schemas = new XmlSchemas ();
				SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
				XmlTypeMapping tm = ri.ImportTypeMapping (typeDesc.Type);
				sx.ExportTypeMapping (tm);

				Assert.AreEqual (0, schemas.Count, typeDesc.Type.FullName + "#1");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportXsdPrimitive_ByteArray ()
		{
			SoapReflectionImporter ri = new SoapReflectionImporter ("ByteArray");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (byte[]));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (0, schemas.Count, "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportXsdPrimitive_Arrays ()
		{
			ArrayList types = new ArrayList ();
			types.Add (new TypeDescription (typeof (object[]), true, "anyType", "AnyType"));
			types.Add (new TypeDescription (typeof (sbyte[]), true, "byte", "Byte"));
			types.Add (new TypeDescription (typeof (bool[]), true, "boolean", "Boolean"));
			types.Add (new TypeDescription (typeof (short[]), true, "short", "Short"));
			types.Add (new TypeDescription (typeof (int[]), true, "int", "Int"));
			types.Add (new TypeDescription (typeof (long[]), true, "long", "Long"));
			types.Add (new TypeDescription (typeof (float[]), true, "float", "Float"));
			types.Add (new TypeDescription (typeof (double[]), true, "double", "Double"));
			types.Add (new TypeDescription (typeof (decimal[]), true, "decimal", "Decimal"));
			types.Add (new TypeDescription (typeof (ushort[]), true, "unsignedShort", "UnsignedShort"));
			types.Add (new TypeDescription (typeof (uint[]), true, "unsignedInt", "UnsignedInt"));
			types.Add (new TypeDescription (typeof (ulong[]), true, "unsignedLong", "UnsignedLong"));
			types.Add (new TypeDescription (typeof (DateTime[]), true, "dateTime", "DateTime"));
#if NET_2_0
			types.Add (new TypeDescription (typeof (XmlQualifiedName[]), true, "QName", "QName", true));
#else
			types.Add (new TypeDescription (typeof (XmlQualifiedName[]), true, "QName", "QName"));
#endif
			types.Add (new TypeDescription (typeof (string[]), true, "string", "String", true));

			foreach (TypeDescription typeDesc in types) {
				SoapReflectionImporter ri = new SoapReflectionImporter (typeDesc.Type.Name);
				XmlSchemas schemas = new XmlSchemas ();
				SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
				XmlTypeMapping tm = ri.ImportTypeMapping (typeDesc.Type);
				sx.ExportTypeMapping (tm);

				Assert.AreEqual (1, schemas.Count, typeDesc.Type.FullName + "#1");

				StringWriter sw = new StringWriter ();
				schemas[0].Write (sw);

				Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
					"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
					"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
					"<xs:schema xmlns:tns=\"{1}\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
					"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
					"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
					"  <xs:complexType name=\"ArrayOf{2}\">{0}" +
					"    <xs:complexContent mixed=\"false\">{0}" +
					"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
					"        <xs:attribute d5p1:arrayType=\"xs:{3}[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
					"      </xs:restriction>{0}" +
					"    </xs:complexContent>{0}" +
					"  </xs:complexType>{0}" +
					"</xs:schema>", Environment.NewLine, typeDesc.Type.Name, typeDesc.ArrayType, typeDesc.XmlType, 
					typeDesc.XsdType ? "xs" : "tns", typeDesc.IsNillable ? "nillable=\"true\" " : ""),
					sw.ToString (), typeDesc.Type.FullName + "#2");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportNonXsdPrimitive_Guid ()
		{
			SoapReflectionImporter ri = new SoapReflectionImporter ("NSPrimGuid");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (Guid));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" elementFormDefault=\"qualified\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:simpleType name=\"guid\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:pattern value=\"[0-9a-fA-F]{{8}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{12}}\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ExportNonXsdPrimitive_Char ()
		{
			SoapReflectionImporter ri = new SoapReflectionImporter ("NSPrimChar");
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (typeof (Char));
			sx.ExportTypeMapping (tm);

			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" elementFormDefault=\"qualified\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:simpleType name=\"char\">{0}" +
				"    <xs:restriction base=\"xs:unsignedShort\" />{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		public class Employee : IXmlSerializable
		{
			private string _firstName;
			private string _lastName;
			private string _address;

			public XmlSchema GetSchema ()
			{
				return null;
			}

			public void WriteXml (XmlWriter writer)
			{
				writer.WriteStartElement ("employee", "urn:devx-com");
				writer.WriteAttributeString ("firstName", _firstName);
				writer.WriteAttributeString ("lastName", _lastName);
				writer.WriteAttributeString ("address", _address);
				writer.WriteEndElement ();
			}

			public void ReadXml (XmlReader reader)
			{
				XmlNodeType type = reader.MoveToContent ();
				if (type == XmlNodeType.Element && reader.LocalName == "employee") {
					_firstName = reader["firstName"];
					_lastName = reader["lastName"];
					_address = reader["address"];
				}
			}
		}

		private class TypeDescription
		{
			public TypeDescription (Type type, bool xsdType, string xmlType, string arrayType)
				: this (type, xsdType, xmlType, arrayType, false)
			{
			}

			public TypeDescription (Type type, bool xsdType, string xmlType, string arrayType, bool isNillable)
			{
				_type = type;
				_xsdType = xsdType;
				_xmlType = xmlType;
				_arrayType = arrayType;
				_isNillable = isNillable;
			}

			public Type Type
			{
				get { return _type; }
			}

			public string XmlType
			{
				get { return _xmlType; }
			}

			public string ArrayType
			{
				get { return _arrayType; }
			}

			public bool XsdType
			{
				get { return _xsdType; }
			}

			public bool IsNillable
			{
				get { return _isNillable; }
			}

			private Type _type;
			private bool _xsdType;
			private string _xmlType;
			private string _arrayType;
			private bool _isNillable;
		}
	}
}
