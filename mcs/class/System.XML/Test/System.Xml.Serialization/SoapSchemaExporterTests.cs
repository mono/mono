//
// System.Xml.Serialization.SoapSchemaExporterTests
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
// 

#if !MOBILE

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
		private XmlSchemas Export (Type type)
		{
			return Export (type, string.Empty);
		}

		private XmlSchemas Export (Type type, string defaultNamespace)
		{
			SoapReflectionImporter ri = new SoapReflectionImporter (defaultNamespace);
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (type);
			sx.ExportTypeMapping (tm);
			return schemas;
		}

		private XmlSchemas Export (Type type, SoapAttributeOverrides overrides)
		{
			return Export (type, overrides, string.Empty);
		}

		private XmlSchemas Export (Type type, SoapAttributeOverrides overrides, string defaultNamespace)
		{
			SoapReflectionImporter ri = new SoapReflectionImporter (overrides, defaultNamespace);
			XmlSchemas schemas = new XmlSchemas ();
			SoapSchemaExporter sx = new SoapSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (type);
			sx.ExportTypeMapping (tm);
			return schemas;
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportStruct ()
		{
			XmlSchemas schemas = Export (typeof (TimeSpan), "NSTimeSpan");
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

			schemas = Export (typeof (TimeSpan));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
/*
#if NET_2_0
		[Category ("NotWorking")] // minOccurs is 1 on Mono
#endif
*/
		public void ExportClass_SimpleClass ()
		{
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			SoapAttributes attr = new SoapAttributes ();
			SoapElementAttribute element = new SoapElementAttribute ();
			element.ElementName = "saying";
			element.IsNullable = true;
			attr.SoapElement = element;
			overrides.Add (typeof (SimpleClass), "something", attr);

			XmlSchemas schemas = Export (typeof (SimpleClass), overrides, "NSSimpleClass");
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
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_StringCollection ()
		{
			XmlSchemas schemas = Export (typeof (StringCollection), "NSStringCollection");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSStringCollection\" elementFormDefault=\"qualified\" targetNamespace=\"NSStringCollection\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSStringCollection\" targetNamespace=\"NSStringCollection\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:string[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_StringCollectionContainer ()
		{
			XmlSchemas schemas = Export (typeof (StringCollectionContainer), "NSStringCollectionContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSStringCollectionContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSStringCollectionContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSStringCollectionContainer\" targetNamespace=\"NSStringCollectionContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"StringCollectionContainer\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"Messages\" type=\"tns:ArrayOfString\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Messages\" type=\"tns:ArrayOfString\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:string[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_ArrayContainer ()
		{
			XmlSchemas schemas = Export (typeof (ArrayContainer), "NSArrayContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSArrayContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSArrayContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSArrayContainer\" targetNamespace=\"NSArrayContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"ArrayContainer\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"items\" type=\"tns:ArrayOfAnyType\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"items\" type=\"tns:ArrayOfAnyType\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:anyType[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_ClassArrayContainer ()
		{
			XmlSchemas schemas = Export (typeof (ClassArrayContainer), "NSClassArrayContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSClassArrayContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSClassArrayContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSClassArrayContainer\" targetNamespace=\"NSClassArrayContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"ClassArrayContainer\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"items\" type=\"tns:ArrayOfSimpleClass\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"items\" type=\"tns:ArrayOfSimpleClass\" />{0}" +
#endif
 "    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
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
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"something\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_SimpleClassWithXmlAttributes ()
		{
			XmlSchemas schemas = Export (typeof (SimpleClassWithXmlAttributes), "NSSimpleClassWithXmlAttributes");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSSimpleClassWithXmlAttributes\" elementFormDefault=\"qualified\" targetNamespace=\"NSSimpleClassWithXmlAttributes\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSSimpleClassWithXmlAttributes\" targetNamespace=\"NSSimpleClassWithXmlAttributes\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"SimpleClassWithXmlAttributes\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"something\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_Field ()
		{
			XmlSchemas schemas = Export (typeof (Field), "NSField");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSField\" elementFormDefault=\"qualified\" targetNamespace=\"NSField\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSField\" targetNamespace=\"NSField\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"Field\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Flags1\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Flags2\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Flags3\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Flags4\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Modifiers\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Modifiers2\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Modifiers3\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Modifiers4\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Modifiers5\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"Names\" type=\"tns:ArrayOfString\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"Street\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Flags1\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Flags2\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Flags3\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Flags4\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Modifiers\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Modifiers2\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Modifiers3\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Modifiers4\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Modifiers5\" type=\"tns:MapModifiers\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Names\" type=\"tns:ArrayOfString\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Street\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"e1\" />{0}" +
				"          <xs:enumeration value=\"e2\" />{0}" +
				"          <xs:enumeration value=\"e4\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"  <xs:simpleType name=\"MapModifiers\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"Public\" />{0}" +
				"          <xs:enumeration value=\"Protected\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:string[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_MyList ()
		{
			XmlSchemas schemas = Export (typeof (MyList), "NSMyList");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSMyList\" elementFormDefault=\"qualified\" targetNamespace=\"NSMyList\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSMyList\" targetNamespace=\"NSMyList\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:anyType[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_Container ()
		{
			XmlSchemas schemas = Export (typeof (Container), "NSContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSContainer\" targetNamespace=\"NSContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"Container\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"Items\" type=\"tns:ArrayOfAnyType\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Items\" type=\"tns:ArrayOfAnyType\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:anyType[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_Container2 ()
		{
			XmlSchemas schemas = Export (typeof (Container2), "NSContainer2");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSContainer2\" elementFormDefault=\"qualified\" targetNamespace=\"NSContainer2\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSContainer2\" targetNamespace=\"NSContainer2\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"Container2\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"Items\" type=\"tns:ArrayOfAnyType\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Items\" type=\"tns:ArrayOfAnyType\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:anyType[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ExportClass_MyElem ()
		{
			Export (typeof (MyElem), "NSMyElem");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		[ExpectedException (typeof (NotSupportedException))] // The type System.Xml.XmlCDataSection may not be serialized with SOAP-encoded messages.
		public void ExportClass_CDataContainer ()
		{
			Export (typeof (CDataContainer), "NSCDataContainer");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		[ExpectedException (typeof (NotSupportedException))] // The type System.Xml.XmlCDataSection may not be serialized with SOAP-encoded messages.
		public void ExportClass_NodeContainer ()
		{
			Export (typeof (NodeContainer), "NSNodeContainer");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_Choices ()
		{
			XmlSchemas schemas = Export (typeof (Choices), "NSChoices");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSChoices\" elementFormDefault=\"qualified\" targetNamespace=\"NSChoices\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSChoices\" targetNamespace=\"NSChoices\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"Choices\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"MyChoice\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"ItemType\" type=\"tns:ItemChoiceType\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"MyChoice\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"ItemType\" type=\"tns:ItemChoiceType\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"ItemChoiceType\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:enumeration value=\"ChoiceZero\" />{0}" +
				"      <xs:enumeration value=\"StrangeOne\" />{0}" +
				"      <xs:enumeration value=\"ChoiceTwo\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_WrongChoices ()
		{
			XmlSchemas schemas = Export (typeof (WrongChoices), "NSWrongChoices");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSWrongChoices\" elementFormDefault=\"qualified\" targetNamespace=\"NSWrongChoices\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSWrongChoices\" targetNamespace=\"NSWrongChoices\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"WrongChoices\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"MyChoice\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"ItemType\" type=\"tns:ItemChoiceType\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"MyChoice\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"ItemType\" type=\"tns:ItemChoiceType\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"ItemChoiceType\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:enumeration value=\"ChoiceZero\" />{0}" +
				"      <xs:enumeration value=\"StrangeOne\" />{0}" +
				"      <xs:enumeration value=\"ChoiceTwo\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_TestSpace ()
		{
			XmlSchemas schemas = Export (typeof (TestSpace), "NSTestSpace");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSTestSpace\" elementFormDefault=\"qualified\" targetNamespace=\"NSTestSpace\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSTestSpace\" targetNamespace=\"NSTestSpace\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"TestSpace\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"elem\" type=\"xs:int\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"elem\" type=\"xs:int\" />{0}" +
#endif
#if NET_2_0
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"attr\" type=\"xs:int\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"attr\" type=\"xs:int\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_ReadOnlyProperties ()
		{
			XmlSchemas schemas = Export (typeof (ReadOnlyProperties), "NSReadOnlyProperties");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSReadOnlyProperties\" elementFormDefault=\"qualified\" targetNamespace=\"NSReadOnlyProperties\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSReadOnlyProperties\" targetNamespace=\"NSReadOnlyProperties\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"ReadOnlyProperties\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_ListDefaults ()
		{
			XmlSchemas schemas = Export (typeof (ListDefaults), "NSListDefaults");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSListDefaults\" elementFormDefault=\"qualified\" targetNamespace=\"NSListDefaults\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSListDefaults\" targetNamespace=\"NSListDefaults\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"ListDefaults\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"list2\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"list3\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"list4\" type=\"tns:ArrayOfString\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"list5\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"ed\" type=\"tns:SimpleClass\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"str\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"list2\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"list3\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"list4\" type=\"tns:ArrayOfString\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"list5\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"ed\" type=\"tns:SimpleClass\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"str\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:anyType[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"something\" type=\"xs:string\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q2=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q2:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:string[]\" ref=\"q2:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_ClsPerson ()
		{
			XmlSchemas schemas = Export (typeof (clsPerson), "NSClsPerson");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSClsPerson\" elementFormDefault=\"qualified\" targetNamespace=\"NSClsPerson\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
 "<xs:schema xmlns:tns=\"NSClsPerson\" targetNamespace=\"NSClsPerson\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
 "  <xs:import namespace=\"http://schemas.xmlsoap.org/soap/encoding/\" />{0}" +
				"  <xs:import namespace=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"  <xs:complexType name=\"clsPerson\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"EmailAccounts\" type=\"tns:ArrayOfAnyType\" />{0}" +
#else
 "      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"EmailAccounts\" type=\"tns:ArrayOfAnyType\" />{0}" +
#endif
 "    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:complexContent mixed=\"false\">{0}" +
				"      <xs:restriction xmlns:q1=\"http://schemas.xmlsoap.org/soap/encoding/\" base=\"q1:Array\">{0}" +
				"        <xs:attribute d5p1:arrayType=\"xs:anyType[]\" ref=\"q1:arrayType\" xmlns:d5p1=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
				"      </xs:restriction>{0}" +
				"    </xs:complexContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_ArrayClass ()
		{
			XmlSchemas schemas = Export (typeof (ArrayClass), "NSArrayClass");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSArrayClass\" elementFormDefault=\"qualified\" targetNamespace=\"NSArrayClass\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSArrayClass\" targetNamespace=\"NSArrayClass\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"ArrayClass\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" form=\"unqualified\" name=\"names\" type=\"xs:anyType\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"names\" type=\"xs:anyType\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportClass_StructContainer ()
		{
			XmlSchemas schemas = Export (typeof (StructContainer), "NSStructContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
#if NET_2_0
				"<xs:schema xmlns:tns=\"NSStructContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSStructContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#else
				"<xs:schema xmlns:tns=\"NSStructContainer\" targetNamespace=\"NSStructContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#endif
				"  <xs:complexType name=\"StructContainer\">{0}" +
				"    <xs:sequence>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" form=\"unqualified\" name=\"Value\" type=\"tns:EnumDefaultValue\" />{0}" +
#else
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Value\" type=\"tns:EnumDefaultValue\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
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
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
/*
#if NET_2_0
		[Category ("NotWorking")] // minOccurs is 1 on Mono
#endif
 */
		public void ExportClass_SimpleClass_Array ()
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
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportEnum ()
		{
			XmlSchemas schemas = Export (typeof (EnumDefaultValue), "NSEnumDefaultValue");
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

			schemas = Export (typeof (EnumDefaultValueNF), "NSEnumDefaultValueNF");
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
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
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
				XmlSchemas schemas = Export (typeDesc.Type, typeDesc.Type.Name);
				Assert.AreEqual (0, schemas.Count, typeDesc.Type.FullName + "#1");
			}
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportXsdPrimitive_ByteArray ()
		{
			XmlSchemas schemas = Export (typeof (byte[]), "ByteArray");
			Assert.AreEqual (0, schemas.Count, "#1");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
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
				XmlSchemas schemas = Export (typeDesc.Type, typeDesc.Type.Name);
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
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportNonXsdPrimitive_Guid ()
		{
			XmlSchemas schemas = Export (typeof (Guid), "NSPrimGuid");
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
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportNonXsdPrimitive_Char ()
		{
			XmlSchemas schemas = Export (typeof (Char), "NSPrimChar");
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

		public class StructContainer
		{
			public EnumDefaultValue Value;
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

#endif