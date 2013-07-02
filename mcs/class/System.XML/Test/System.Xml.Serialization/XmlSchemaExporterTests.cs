//
// System.Xml.Serialization.XmlSchemaExporterTests
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
	public class XmlSchemaExporterTests
	{
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		string Infoset (string source)
		{
			return XmlSerializerTests.Infoset (source);
		}

		private XmlSchemas Export (Type type)
		{
			return Export (type, string.Empty);
		}

		private XmlSchemas Export (Type type, string defaultNamespace)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter (defaultNamespace);
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter sx = new XmlSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (type);
			sx.ExportTypeMapping (tm);
			return schemas;
		}

		private XmlSchemas Export (Type type, XmlAttributeOverrides overrides)
		{
			return Export (type, overrides, string.Empty);
		}

		private XmlSchemas Export (Type type, XmlAttributeOverrides overrides, string defaultNamespace)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter (overrides, defaultNamespace);
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter sx = new XmlSchemaExporter (schemas);
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

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSTimeSpan\" elementFormDefault=\"qualified\" targetNamespace=\"NSTimeSpan\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"TimeSpan\" type=\"tns:TimeSpan\" />{0}" +
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (TimeSpan));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"TimeSpan\" type=\"TimeSpan\" />{0}" +
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportStruct_Array ()
		{
			XmlSchemas schemas = Export (typeof (TimeSpan[]), "NSTimeSpanArray");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSTimeSpanArray\" elementFormDefault=\"qualified\" targetNamespace=\"NSTimeSpanArray\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfTimeSpan\" nillable=\"true\" type=\"tns:ArrayOfTimeSpan\" />{0}" +
				"  <xs:complexType name=\"ArrayOfTimeSpan\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"TimeSpan\" type=\"tns:TimeSpan\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (TimeSpan[]));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfTimeSpan\" nillable=\"true\" type=\"ArrayOfTimeSpan\" />{0}" +
				"  <xs:complexType name=\"ArrayOfTimeSpan\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"TimeSpan\" type=\"TimeSpan\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_SimpleClass ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlElementAttribute element = new XmlElementAttribute ();
			element.ElementName = "saying";
			element.IsNullable = true;
			attr.XmlElements.Add (element);
			overrides.Add (typeof (SimpleClass), "something", attr);

			XmlSchemas schemas = Export (typeof (SimpleClass), overrides, "NSSimpleClass");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSSimpleClass\" elementFormDefault=\"qualified\" targetNamespace=\"NSSimpleClass\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"SimpleClass\" nillable=\"true\" type=\"tns:SimpleClass\" />{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"saying\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (SimpleClass), overrides);
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"SimpleClass\" nillable=\"true\" type=\"SimpleClass\" />{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"saying\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_StringCollection ()
		{
			XmlSchemas schemas = Export (typeof (StringCollection), "NSStringCollection");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSStringCollection\" elementFormDefault=\"qualified\" targetNamespace=\"NSStringCollection\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfString\" nillable=\"true\" type=\"tns:ArrayOfString\" />{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"string\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (StringCollection));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfString\" nillable=\"true\" type=\"ArrayOfString\" />{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"string\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_StringCollectionContainer ()
		{
			XmlSchemas schemas = Export (typeof (StringCollectionContainer), "NSStringCollectionContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSStringCollectionContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSStringCollectionContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"StringCollectionContainer\" nillable=\"true\" type=\"tns:StringCollectionContainer\" />{0}" +
				"  <xs:complexType name=\"StringCollectionContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Messages\" type=\"tns:ArrayOfString\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"string\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (StringCollectionContainer));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"StringCollectionContainer\" nillable=\"true\" type=\"StringCollectionContainer\" />{0}" +
				"  <xs:complexType name=\"StringCollectionContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Messages\" type=\"ArrayOfString\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"string\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_ArrayContainer ()
		{
			XmlSchemas schemas = Export (typeof (ArrayContainer), "NSArrayContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSArrayContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSArrayContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayContainer\" nillable=\"true\" type=\"tns:ArrayContainer\" />{0}" +
				"  <xs:complexType name=\"ArrayContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"items\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (ArrayContainer));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayContainer\" nillable=\"true\" type=\"ArrayContainer\" />{0}" +
				"  <xs:complexType name=\"ArrayContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"items\" type=\"ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_ClassArrayContainer ()
		{
			XmlSchemas schemas = Export (typeof (ClassArrayContainer), "NSClassArrayContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSClassArrayContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSClassArrayContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ClassArrayContainer\" nillable=\"true\" type=\"tns:ClassArrayContainer\" />{0}" +
				"  <xs:complexType name=\"ClassArrayContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"items\" type=\"tns:ArrayOfSimpleClass\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfSimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"SimpleClass\" nillable=\"true\" type=\"tns:SimpleClass\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (ClassArrayContainer));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ClassArrayContainer\" nillable=\"true\" type=\"ClassArrayContainer\" />{0}" +
				"  <xs:complexType name=\"ClassArrayContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"items\" type=\"ArrayOfSimpleClass\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfSimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"SimpleClass\" nillable=\"true\" type=\"SimpleClass\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_SimpleClassWithXmlAttributes ()
		{
			XmlSchemas schemas = Export (typeof (SimpleClassWithXmlAttributes), "NSSimpleClassWithXmlAttributes");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSSimpleClassWithXmlAttributes\" elementFormDefault=\"qualified\" targetNamespace=\"NSSimpleClassWithXmlAttributes\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"simple\" nillable=\"true\" type=\"tns:SimpleClassWithXmlAttributes\" />{0}" +
				"  <xs:complexType name=\"SimpleClassWithXmlAttributes\">{0}" +
				"    <xs:attribute name=\"member\" type=\"xs:string\" />{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (SimpleClassWithXmlAttributes));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"simple\" nillable=\"true\" type=\"SimpleClassWithXmlAttributes\" />{0}" +
				"  <xs:complexType name=\"SimpleClassWithXmlAttributes\">{0}" +
				"    <xs:attribute name=\"member\" type=\"xs:string\" />{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_Field ()
		{
			XmlSchemas schemas = Export (typeof (Field), "NSField");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSField\" elementFormDefault=\"qualified\" targetNamespace=\"NSField\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"field\" nillable=\"true\" type=\"tns:Field\" />{0}" +
				"  <xs:complexType name=\"Field\">{0}" +
				"    <xs:attribute default=\"one\" name=\"flag1\" type=\"tns:FlagEnum\" />{0}" +
				"    <xs:attribute default=\"one\" name=\"flag2\" type=\"tns:FlagEnum\" />{0}" +
				"    <xs:attribute default=\"one two\" form=\"qualified\" name=\"flag3\" type=\"tns:FlagEnum\" />{0}" +
				"    <xs:attribute name=\"flag4\" type=\"tns:FlagEnum\" use=\"required\" />{0}" +
				"    <xs:attribute name=\"modifiers\" type=\"tns:MapModifiers\" use=\"required\" />{0}" +
				"    <xs:attribute form=\"unqualified\" name=\"modifiers2\" type=\"tns:MapModifiers\" use=\"required\" />{0}" +
				"    <xs:attribute default=\"public\" name=\"modifiers3\" type=\"tns:MapModifiers\" />{0}" +
				"    <xs:attribute default=\"protected\" form=\"unqualified\" name=\"modifiers4\" type=\"tns:MapModifiers\" />{0}" +
				"    <xs:attribute default=\"public\" form=\"qualified\" name=\"modifiers5\" type=\"tns:MapModifiers\" />{0}" +
				"    <xs:attribute name=\"names\">{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:list itemType=\"xs:string\" />{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:attribute>{0}" +
				"    <xs:attribute name=\"street\" type=\"xs:string\" />{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"  <xs:simpleType name=\"MapModifiers\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"public\" />{0}" +
				"          <xs:enumeration value=\"protected\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (Field));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"field\" nillable=\"true\" type=\"Field\" />{0}" +
				"  <xs:complexType name=\"Field\">{0}" +
				"    <xs:attribute default=\"one\" name=\"flag1\" type=\"FlagEnum\" />{0}" +
				"    <xs:attribute default=\"one\" name=\"flag2\" type=\"FlagEnum\" />{0}" +
				"    <xs:attribute default=\"one two\" form=\"qualified\" name=\"flag3\" type=\"FlagEnum\" />{0}" +
				"    <xs:attribute name=\"flag4\" type=\"FlagEnum\" use=\"required\" />{0}" +
				"    <xs:attribute name=\"modifiers\" type=\"MapModifiers\" use=\"required\" />{0}" +
				"    <xs:attribute form=\"unqualified\" name=\"modifiers2\" type=\"MapModifiers\" use=\"required\" />{0}" +
				"    <xs:attribute default=\"public\" name=\"modifiers3\" type=\"MapModifiers\" />{0}" +
				"    <xs:attribute default=\"protected\" form=\"unqualified\" name=\"modifiers4\" type=\"MapModifiers\" />{0}" +
				"    <xs:attribute default=\"public\" form=\"qualified\" name=\"modifiers5\" type=\"MapModifiers\" />{0}" +
				"    <xs:attribute name=\"names\">{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:list itemType=\"xs:string\" />{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:attribute>{0}" +
				"    <xs:attribute name=\"street\" type=\"xs:string\" />{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"  <xs:simpleType name=\"MapModifiers\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"public\" />{0}" +
				"          <xs:enumeration value=\"protected\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_MyList ()
		{
			XmlSchemas schemas = Export (typeof (MyList), "NSMyList");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSMyList\" elementFormDefault=\"qualified\" targetNamespace=\"NSMyList\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfAnyType\" nillable=\"true\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (MyList));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfAnyType\" nillable=\"true\" type=\"ArrayOfAnyType\" />{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");
		}

		[Test]
		public void ExportClass_Container ()
		{
			XmlSchemas schemas = Export (typeof (Container), "NSContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Container\" nillable=\"true\" type=\"tns:Container\" />{0}" +
				"  <xs:complexType name=\"Container\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Items\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (Container));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Container\" nillable=\"true\" type=\"Container\" />{0}" +
				"  <xs:complexType name=\"Container\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Items\" type=\"ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_Container2 ()
		{
			XmlSchemas schemas = Export (typeof (Container2), "NSContainer2");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSContainer2\" elementFormDefault=\"qualified\" targetNamespace=\"NSContainer2\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Container2\" nillable=\"true\" type=\"tns:Container2\" />{0}" +
				"  <xs:complexType name=\"Container2\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Items\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (Container2));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Container2\" nillable=\"true\" type=\"Container2\" />{0}" +
				"  <xs:complexType name=\"Container2\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Items\" type=\"ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		[ExpectedException (typeof (InvalidOperationException))] // Cannot use wildcards at the top level of a schema.
		public void ExportClass_MyElem ()
		{
			Export (typeof (MyElem), "NSMyElem");
		}

		[Test]
		public void ExportClass_CDataContainer ()
		{
			XmlSchemas schemas = Export (typeof (CDataContainer), "NSCDataContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSCDataContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSCDataContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"CDataContainer\" nillable=\"true\" type=\"tns:CDataContainer\" />{0}" +
				"  <xs:complexType name=\"CDataContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"cdata\">{0}" +
				"        <xs:complexType mixed=\"true\">{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (CDataContainer));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"CDataContainer\" nillable=\"true\" type=\"CDataContainer\" />{0}" +
				"  <xs:complexType name=\"CDataContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"cdata\">{0}" +
				"        <xs:complexType mixed=\"true\">{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");
		}

		[Test]
		public void ExportClass_NodeContainer ()
		{
			XmlSchemas schemas = Export (typeof (NodeContainer), "NSNodeContainer");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSNodeContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSNodeContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"NodeContainer\" nillable=\"true\" type=\"tns:NodeContainer\" />{0}" +
				"  <xs:complexType name=\"NodeContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"node\">{0}" +
				"        <xs:complexType mixed=\"true\">{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (NodeContainer));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"NodeContainer\" nillable=\"true\" type=\"NodeContainer\" />{0}" +
				"  <xs:complexType name=\"NodeContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"node\">{0}" +
				"        <xs:complexType mixed=\"true\">{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		[Category ("NotWorking")] // Mono does not generate the <xs:choice> node
		[Category ("NotDotNet")] // MS.NET randomly modifies the order of the elements! -> Of course. There is nothing strange. The test is rather strange.
		public void ExportClass_Choices ()
		{
			XmlSchemas schemas = Export (typeof (Choices), "NSChoices");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSChoices\" elementFormDefault=\"qualified\" targetNamespace=\"NSChoices\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Choices\" type=\"tns:Choices\" />{0}" +
				"  <xs:complexType name=\"Choices\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:choice minOccurs=\"1\" maxOccurs=\"1\">{0}" +
				"        <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ChoiceOne\" type=\"xs:string\" />{0}" +
				"        <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ChoiceTwo\" type=\"xs:string\" />{0}" +
				"        <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ChoiceZero\" type=\"xs:string\" />{0}" +
				"      </xs:choice>{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (Choices));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Choices\" type=\"Choices\" />{0}" +
				"  <xs:complexType name=\"Choices\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:choice minOccurs=\"1\" maxOccurs=\"1\">{0}" +
				"        <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ChoiceOne\" type=\"xs:string\" />{0}" +
				"        <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ChoiceTwo\" type=\"xs:string\" />{0}" +
				"        <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ChoiceZero\" type=\"xs:string\" />{0}" +
				"      </xs:choice>{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS.NET 1.x does not escape spaces in a type name, bug is fixed in .NET 2.0
#endif
		public void ExportClass_TestSpace ()
		{
			XmlSchemas schemas = Export (typeof (TestSpace), "NSTestSpace");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSTestSpace\" elementFormDefault=\"qualified\" targetNamespace=\"NSTestSpace\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Type_x0020_with_x0020_space\" nillable=\"true\" type=\"tns:Type_x0020_with_x0020_space\" />{0}" +
				"  <xs:complexType name=\"Type_x0020_with_x0020_space\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Element_x0020_with_x0020_space\" type=\"xs:int\" />{0}" +
				"    </xs:sequence>{0}" +
				"    <xs:attribute name=\"Attribute_x0020_with_x0020_space\" type=\"xs:int\" use=\"required\" />{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (TestSpace));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"Type_x0020_with_x0020_space\" nillable=\"true\" type=\"Type_x0020_with_x0020_space\" />{0}" +
				"  <xs:complexType name=\"Type_x0020_with_x0020_space\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Element_x0020_with_x0020_space\" type=\"xs:int\" />{0}" +
				"    </xs:sequence>{0}" +
				"    <xs:attribute name=\"Attribute_x0020_with_x0020_space\" type=\"xs:int\" use=\"required\" />{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportClass_OptionalValueTypeContainer ()
		{
			XmlAttributeOverrides overrides;
			XmlAttributes attr;

			XmlSchemas schemas = Export (typeof (OptionalValueTypeContainer));
			Assert.AreEqual (2, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"{2}\" />{0}" +
				"  <xs:element name=\"optionalValue\" xmlns:q1=\"{2}\" type=\"q1:optionalValueType\" />{0}" +
				"</xs:schema>", Environment.NewLine, AnotherNamespace, ANamespace),
				sw.ToString (), "#2");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:complexType name=\"optionalValueType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"one four\" name=\"Attributes\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"one\" name=\"Flags\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"false\" name=\"IsEmpty\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"false\" name=\"IsNull\" type=\"xs:boolean\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine, ANamespace), sw.ToString (), "#3");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();

			// remove the DefaultValue attribute on the Flags member
			overrides.Add (typeof (OptionalValueTypeContainer), "Flags", attr);
			// remove the DefaultValue attribute on the Attributes member
			overrides.Add (typeof (OptionalValueTypeContainer), "Attributes", attr);
			// remove the DefaultValue attribute on the IsEmpty member
			overrides.Add (typeof (OptionalValueTypeContainer), "IsEmpty", attr);
			// remove the DefaultValue attribute on the IsNull member
			overrides.Add (typeof (OptionalValueTypeContainer), "IsNull", attr);

			schemas = Export (typeof (OptionalValueTypeContainer), overrides, "urn:myNS");
			Assert.AreEqual (2, schemas.Count, "#4");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"{2}\" />{0}" +
				"  <xs:element name=\"optionalValue\" xmlns:q1=\"{2}\" type=\"q1:optionalValueType\" />{0}" +
				"</xs:schema>", Environment.NewLine, AnotherNamespace, ANamespace),
				sw.ToString (), "#5");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:complexType name=\"optionalValueType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Attributes\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Flags\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"IsEmpty\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"IsNull\" type=\"xs:boolean\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine, ANamespace), sw.ToString (), "#6");
		}

		[Test]
		public void ExportClass_ReadOnlyProperties ()
		{
			XmlSchemas schemas = Export (typeof (ReadOnlyProperties), "NSReadOnlyProperties");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSReadOnlyProperties\" elementFormDefault=\"qualified\" targetNamespace=\"NSReadOnlyProperties\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ReadOnlyProperties\" nillable=\"true\" type=\"tns:ReadOnlyProperties\" />{0}" +
				"  <xs:complexType name=\"ReadOnlyProperties\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (ReadOnlyProperties));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ReadOnlyProperties\" nillable=\"true\" type=\"ReadOnlyProperties\" />{0}" +
				"  <xs:complexType name=\"ReadOnlyProperties\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
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
				"<xs:schema xmlns:tns=\"NSListDefaults\" elementFormDefault=\"qualified\" targetNamespace=\"NSListDefaults\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"root\" nillable=\"true\" type=\"tns:ListDefaults\" />{0}" +
				"  <xs:complexType name=\"ListDefaults\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"list2\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"list3\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"list4\" type=\"tns:ArrayOfString\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"e\" type=\"tns:SimpleClass\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ed\" type=\"tns:SimpleClass\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"str\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"string\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (ListDefaults));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"root\" nillable=\"true\" type=\"ListDefaults\" />{0}" +
				"  <xs:complexType name=\"ListDefaults\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"list2\" type=\"ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"list3\" type=\"ArrayOfAnyType\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"list4\" type=\"ArrayOfString\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"e\" type=\"SimpleClass\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"ed\" type=\"SimpleClass\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"str\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfString\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"string\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"something\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportClass_ClsPerson ()
		{
			XmlSchemas schemas = Export (typeof (clsPerson), "NSClsPerson");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSClsPerson\" elementFormDefault=\"qualified\" targetNamespace=\"NSClsPerson\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"clsPerson\" nillable=\"true\" type=\"tns:clsPerson\" />{0}" +
				"  <xs:complexType name=\"clsPerson\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"EmailAccounts\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (clsPerson));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"clsPerson\" nillable=\"true\" type=\"clsPerson\" />{0}" +
				"  <xs:complexType name=\"clsPerson\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"EmailAccounts\" type=\"ArrayOfAnyType\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportClass_ArrayClass ()
		{
			XmlSchemas schemas = Export (typeof (ArrayClass), "NSArrayClass");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSArrayClass\" elementFormDefault=\"qualified\" targetNamespace=\"NSArrayClass\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayClass\" nillable=\"true\" type=\"tns:ArrayClass\" />{0}" +
				"  <xs:complexType name=\"ArrayClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"names\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (ArrayClass));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayClass\" nillable=\"true\" type=\"ArrayClass\" />{0}" +
				"  <xs:complexType name=\"ArrayClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"names\" />{0}" +
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
				"<xs:schema xmlns:tns=\"NSStructContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSStructContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"StructContainer\" nillable=\"true\" type=\"tns:StructContainer\" />{0}" +
				"  <xs:complexType name=\"StructContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Value\" type=\"tns:EnumDefaultValue\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Times\" type=\"tns:ArrayOfTimeSpan\" />{0}" +
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
				"  <xs:complexType name=\"ArrayOfTimeSpan\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"TimeSpan\" type=\"tns:TimeSpan\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (StructContainer));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"StructContainer\" nillable=\"true\" type=\"StructContainer\" />{0}" +
				"  <xs:complexType name=\"StructContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"Value\" type=\"EnumDefaultValue\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Times\" type=\"ArrayOfTimeSpan\" />{0}" +
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
				"  <xs:complexType name=\"ArrayOfTimeSpan\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"TimeSpan\" type=\"TimeSpan\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"TimeSpan\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportClass_TestDefault ()
		{
			XmlSchemas schemas = Export (typeof (TestDefault));
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas [0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"urn:myNS\" elementFormDefault=\"qualified\" targetNamespace=\"urn:myNS\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"testDefault\" type=\"tns:TestDefault\" />{0}" +
				"  <xs:complexType name=\"TestDefault\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"str\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"Default Value\" name=\"strDefault\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"true\" name=\"boolT\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"false\" name=\"boolF\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"10\" name=\"decimalval\" type=\"xs:decimal\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"one four\" name=\"flag\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"e1 e4\" name=\"flagencoded\" type=\"tns:FlagEnum_Encoded\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"  <xs:simpleType name=\"FlagEnum_Encoded\">{0}" +
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
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (TestDefault), "NSTestDefault");
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas [0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"urn:myNS\" elementFormDefault=\"qualified\" targetNamespace=\"urn:myNS\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"testDefault\" type=\"tns:TestDefault\" />{0}" +
				"  <xs:complexType name=\"TestDefault\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"str\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"Default Value\" name=\"strDefault\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"true\" name=\"boolT\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"false\" name=\"boolF\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"10\" name=\"decimalval\" type=\"xs:decimal\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"one four\" name=\"flag\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"e1 e4\" name=\"flagencoded\" type=\"tns:FlagEnum_Encoded\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"  <xs:simpleType name=\"FlagEnum_Encoded\">{0}" +
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
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportClass_TestDefault_Overrides ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlTypeAttribute xmlType = new XmlTypeAttribute ("flagenum");
			xmlType.Namespace = "yetanother:urn";
			attr.XmlType = xmlType;
			overrides.Add (typeof (FlagEnum_Encoded), attr);

			XmlSchemas schemas = Export (typeof (TestDefault), overrides, "NSTestDefault");
			Assert.AreEqual (2, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas [0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"urn:myNS\" elementFormDefault=\"qualified\" targetNamespace=\"urn:myNS\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"yetanother:urn\" />{0}" +
				"  <xs:element name=\"testDefault\" type=\"tns:TestDefault\" />{0}" +
				"  <xs:complexType name=\"TestDefault\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"str\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"Default Value\" name=\"strDefault\" type=\"xs:string\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"true\" name=\"boolT\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"false\" name=\"boolF\" type=\"xs:boolean\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"10\" name=\"decimalval\" type=\"xs:decimal\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"one four\" name=\"flag\" type=\"tns:FlagEnum\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" default=\"e1 e4\" name=\"flagencoded\" xmlns:q1=\"yetanother:urn\" type=\"q1:flagenum\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:simpleType name=\"FlagEnum\">{0}" +
				"    <xs:list>{0}" +
				"      <xs:simpleType>{0}" +
				"        <xs:restriction base=\"xs:string\">{0}" +
				"          <xs:enumeration value=\"one\" />{0}" +
				"          <xs:enumeration value=\"two\" />{0}" +
				"          <xs:enumeration value=\"four\" />{0}" +
				"        </xs:restriction>{0}" +
				"      </xs:simpleType>{0}" +
				"    </xs:list>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			sw.GetStringBuilder ().Length = 0;
			schemas [1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"yetanother:urn\" elementFormDefault=\"qualified\" targetNamespace=\"yetanother:urn\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:simpleType name=\"flagenum\">{0}" +
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
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		[ExpectedException (typeof (InvalidOperationException))] // Cannot use wildcards at the top level of a schema.
		public void ExportClass_XmlElement ()
		{
			XmlSchemas schemas = Export (typeof (XmlElement), "NS1");
		}

		[Test]
		public void ExportClass_Array ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlElementAttribute element = new XmlElementAttribute ();
			element.ElementName = "saying";
			element.IsNullable = true;
			attr.XmlElements.Add (element);
			overrides.Add (typeof (SimpleClass), "something", attr);

			XmlSchemas schemas = Export (typeof (SimpleClass[]), overrides, "NSSimpleClassArray");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSSimpleClassArray\" elementFormDefault=\"qualified\" targetNamespace=\"NSSimpleClassArray\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfSimpleClass\" nillable=\"true\" type=\"tns:ArrayOfSimpleClass\" />{0}" +
				"  <xs:complexType name=\"ArrayOfSimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"SimpleClass\" nillable=\"true\" type=\"tns:SimpleClass\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"saying\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (SimpleClass[]), overrides);
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfSimpleClass\" nillable=\"true\" type=\"ArrayOfSimpleClass\" />{0}" +
				"  <xs:complexType name=\"ArrayOfSimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"SimpleClass\" nillable=\"true\" type=\"SimpleClass\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:complexType name=\"SimpleClass\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"1\" maxOccurs=\"1\" name=\"saying\" nillable=\"true\" type=\"xs:string\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
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
				"<xs:schema xmlns:tns=\"NSEnumDefaultValue\" elementFormDefault=\"qualified\" targetNamespace=\"NSEnumDefaultValue\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"EnumDefaultValue\" type=\"tns:EnumDefaultValue\" />{0}" +
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

			sw.GetStringBuilder ().Length = 0; 
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSEnumDefaultValueNF\" elementFormDefault=\"qualified\" targetNamespace=\"NSEnumDefaultValueNF\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"EnumDefaultValueNF\" type=\"tns:EnumDefaultValueNF\" />{0}" +
				"  <xs:simpleType name=\"EnumDefaultValueNF\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:enumeration value=\"e1\" />{0}" +
				"      <xs:enumeration value=\"e2\" />{0}" +
				"      <xs:enumeration value=\"e3\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");

			schemas = Export (typeof (EnumDefaultValue));
			Assert.AreEqual (1, schemas.Count, "#5");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"EnumDefaultValue\" type=\"EnumDefaultValue\" />{0}" +
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
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#6");

			schemas = Export (typeof (EnumDefaultValueNF));
			Assert.AreEqual (1, schemas.Count, "#7");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"EnumDefaultValueNF\" type=\"EnumDefaultValueNF\" />{0}" +
				"  <xs:simpleType name=\"EnumDefaultValueNF\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:enumeration value=\"e1\" />{0}" +
				"      <xs:enumeration value=\"e2\" />{0}" +
				"      <xs:enumeration value=\"e3\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#8");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportXmlSerializable ()
		{
			XmlSchemas schemas = Export (typeof (Employee), "NSEmployee");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSEmployee\" elementFormDefault=\"qualified\" targetNamespace=\"NSEmployee\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:element name=\"Employee\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (Employee));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:element name=\"Employee\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportXmlSerializable_Schema ()
		{
			XmlSchemas schemas = Export (typeof (EmployeeSchema), "NSEmployeeSchema");
			Assert.AreEqual (2, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSEmployeeSchema\" elementFormDefault=\"qualified\" targetNamespace=\"NSEmployeeSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#if ONLY_1_1
				"  <xs:import namespace=\"urn:types-devx-com\" />{0}" +
#endif
				"  <xs:element name=\"EmployeeSchema\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"urn:types-devx-com\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"urn:types-devx-com\" targetNamespace=\"urn:types-devx-com\" id=\"EmployeeSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:complexType name=\"employeeRoot\">{0}" +
				"    <xs:attribute name=\"firstName\" />{0}" +
				"    <xs:attribute name=\"lastName\" />{0}" +
				"    <xs:attribute name=\"address\" />{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:element name=\"employee\" type=\"tns:employeeRoot\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#3");

			schemas = Export (typeof (EmployeeSchema));
			Assert.AreEqual (2, schemas.Count, "#4");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#if ONLY_1_1
				"  <xs:import namespace=\"urn:types-devx-com\" />{0}" +
#endif
				"  <xs:element name=\"EmployeeSchema\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"urn:types-devx-com\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#5");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"urn:types-devx-com\" targetNamespace=\"urn:types-devx-com\" id=\"EmployeeSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:complexType name=\"employeeRoot\">{0}" +
				"    <xs:attribute name=\"firstName\" />{0}" +
				"    <xs:attribute name=\"lastName\" />{0}" +
				"    <xs:attribute name=\"address\" />{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:element name=\"employee\" type=\"tns:employeeRoot\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#6");

			schemas = Export (typeof (PrimitiveSchema), "NSPrimitiveSchema");
			Assert.AreEqual (2, schemas.Count, "#7");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSPrimitiveSchema\" elementFormDefault=\"qualified\" targetNamespace=\"NSPrimitiveSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"PrimitiveSchema\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#8");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema id=\"LuckyNumberSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"LuckyNumber\" type=\"xs:int\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#9");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // Schema Id is missing
		public void ExportXmlSerializable_MissingID ()
		{
			Export (typeof (MissingIDSchema));
		}

		[ExpectedException (typeof (InvalidOperationException))]
		public void ExportXmlSerializable_DuplicateNamespace ()
		{
			try {
				Export (typeof (PrimitiveSchema));
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
				// The namespace, , is a duplicate.
			}

			try {
				Export (typeof (XmlSerializableContainer));
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
				// The namespace, , is a duplicate.
			}
		}

		[Test]
#if !NET_2_0
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
#endif
		public void ExportXmlSerializable_SchemaProvider ()
		{
			XmlSchemas schemas = Export (typeof (EmployeeSchemaProvider), "NSEmployeeSchemaProvider");
			//Assert.AreEqual (1, schemas.Count, "#1"); //# of returned schemas is checked in ExportXmlSerializable_SchemaProvider1

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSEmployeeSchemaProvider\" elementFormDefault=\"qualified\" targetNamespace=\"NSEmployeeSchemaProvider\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#if NET_2_0
				"  <xs:import namespace=\"urn:types-devx-com\" />{0}" +
				"  <xs:element name=\"employeeRoot\" nillable=\"true\" xmlns:q1=\"urn:types-devx-com\" type=\"q1:employeeRoot\" />{0}" +
#else
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:element name=\"EmployeeSchemaProvider\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#2");

			schemas = Export (typeof (EmployeeSchemaProvider));
			//Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#if NET_2_0
				"  <xs:import namespace=\"urn:types-devx-com\" />{0}" +
				"  <xs:element name=\"employeeRoot\" nillable=\"true\" xmlns:q1=\"urn:types-devx-com\" type=\"q1:employeeRoot\" />{0}" +
#else
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:element name=\"EmployeeSchemaProvider\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ()), "#4");

			schemas = Export (typeof (PrimitiveSchemaProvider), "NSPrimitiveSchemaProvider");
			//Assert.AreEqual (1, schemas.Count, "#5");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSPrimitiveSchemaProvider\" elementFormDefault=\"qualified\" targetNamespace=\"NSPrimitiveSchemaProvider\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#if NET_2_0
				//"  <xs:import />{0}" +
				"  <xs:element name=\"int\" nillable=\"true\" type=\"xs:int\" />{0}" +
#else
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:element name=\"PrimitiveSchemaProvider\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"</xs:schema>", Environment.NewLine)), Infoset (sw.ToString ().Replace("<xs:import />" + Environment.NewLine, "")), "#6");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")] // support for XmlSchemaProvider is not implemented
#endif
		public void ExportXmlSerializable_SchemaProvider1 () {
			XmlSchemas schemas = Export (typeof (PrimitiveSchemaProvider));
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
#if NET_2_0
				"  <xs:element name=\"int\" nillable=\"true\" type=\"xs:int\" />{0}" +
#else
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:element name=\"PrimitiveSchemaProvider\" nillable=\"true\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#8");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
#if NET_2_0
		[Category ("NotWorking")] // support for XmlSchemaProvider is not implemented
#endif
		public void ExportXmlSerializable_Container ()
		{
			XmlSchemas schemas = Export (typeof (XmlSerializableContainer), "NSXmlSerializableContainer");
			Assert.AreEqual (3, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSXmlSerializableContainer\" elementFormDefault=\"qualified\" targetNamespace=\"NSXmlSerializableContainer\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
				"  <xs:import namespace=\"urn:types-devx-com\" />{0}" +
#if NET_2_0
				"  <xs:import />{0}" +
#endif
				"  <xs:element name=\"XmlSerializableContainer\" nillable=\"true\" type=\"tns:XmlSerializableContainer\" />{0}" +
				"  <xs:complexType name=\"XmlSerializableContainer\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"Slave\">{0}" +
				"        <xs:complexType>{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:element ref=\"xs:schema\" />{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"SlaveSchema\">{0}" +
				"        <xs:complexType>{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:any namespace=\"urn:types-devx-com\" />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"SlaveSchemaProvider\" xmlns:q1=\"urn:types-devx-com\" type=\"q1:employeeRoot\" />{0}" +
#else
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"SlaveSchemaProvider\">{0}" +
				"        <xs:complexType>{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:element ref=\"xs:schema\" />{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
#endif
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"NativeSchema\">{0}" +
				"        <xs:complexType>{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:any namespace=\"\" />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
#if NET_2_0
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"NativeSchemaProvider\" type=\"xs:int\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q2=\"urn:types-devx-com\" ref=\"q2:SlaveNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q3=\"urn:types-devx-com\" ref=\"q3:SlaveSchemaNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q4=\"urn:types-devx-com\" ref=\"q4:SlaveSchemaProviderNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q5=\"urn:types-devx-com\" ref=\"q5:NativeSchemaNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q6=\"urn:types-devx-com\" ref=\"q6:NativeSchemaProviderNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q7=\"urn:types-devx-com\" ref=\"q7:SlaveNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q8=\"urn:types-devx-com\" ref=\"q8:SlaveSchemaNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q9=\"urn:types-devx-com\" ref=\"q9:SlaveSchemaProviderNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q10=\"urn:types-devx-com\" ref=\"q10:NativeSchemaNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q11=\"urn:types-devx-com\" ref=\"q11:NativeSchemaProviderNSOnly\" />{0}" +
#else
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" name=\"NativeSchemaProvider\">{0}" +
				"        <xs:complexType>{0}" +
				"          <xs:sequence>{0}" +
				"            <xs:element ref=\"xs:schema\" />{0}" +
				"            <xs:any />{0}" +
				"          </xs:sequence>{0}" +
				"        </xs:complexType>{0}" +
				"      </xs:element>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q1=\"urn:types-devx-com\" ref=\"q1:SlaveNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q2=\"urn:types-devx-com\" ref=\"q2:SlaveSchemaNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q3=\"urn:types-devx-com\" ref=\"q3:SlaveSchemaProviderNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q4=\"urn:types-devx-com\" ref=\"q4:NativeSchemaNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q5=\"urn:types-devx-com\" ref=\"q5:NativeSchemaProviderNamespace\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q6=\"urn:types-devx-com\" ref=\"q6:SlaveNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q7=\"urn:types-devx-com\" ref=\"q7:SlaveSchemaNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q8=\"urn:types-devx-com\" ref=\"q8:SlaveSchemaProviderNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q9=\"urn:types-devx-com\" ref=\"q9:NativeSchemaNSOnly\" />{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"1\" xmlns:q10=\"urn:types-devx-com\" ref=\"q10:NativeSchemaProviderNSOnly\" />{0}" +
#endif
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"urn:types-devx-com\" targetNamespace=\"urn:types-devx-com\" id=\"EmployeeSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://www.w3.org/2001/XMLSchema\" />{0}" +
#if NET_2_0
				"  <xs:import />{0}" +
#endif
				"  <xs:complexType name=\"employeeRoot\">{0}" +
				"    <xs:attribute name=\"firstName\" />{0}" +
				"    <xs:attribute name=\"lastName\" />{0}" +
				"    <xs:attribute name=\"address\" />{0}" +
				"  </xs:complexType>{0}" +
				"  <xs:element name=\"employee\" type=\"tns:employeeRoot\" />{0}" +
				"  <xs:element name=\"SlaveNamespace\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"  <xs:element name=\"SlaveSchemaNamespace\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"urn:types-devx-com\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#if NET_2_0
				"  <xs:element name=\"SlaveSchemaProviderNamespace\" type=\"tns:employeeRoot\" />{0}" +
#else
				"  <xs:element name=\"SlaveSchemaProviderNamespace\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"  <xs:element name=\"NativeSchemaNamespace\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#if NET_2_0
				"  <xs:element name=\"NativeSchemaProviderNamespace\" type=\"xs:int\" />{0}" +
#else
				"  <xs:element name=\"NativeSchemaProviderNamespace\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"  <xs:element name=\"SlaveNSOnly\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
				"  <xs:element name=\"SlaveSchemaNSOnly\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"urn:types-devx-com\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#if NET_2_0
				 "  <xs:element name=\"SlaveSchemaProviderNSOnly\" type=\"tns:employeeRoot\" />{0}" +
#else
				"  <xs:element name=\"SlaveSchemaProviderNSOnly\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"  <xs:element name=\"NativeSchemaNSOnly\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:any namespace=\"\" />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#if NET_2_0
				"  <xs:element name=\"NativeSchemaProviderNSOnly\" type=\"xs:int\" />{0}" +
#else
				"  <xs:element name=\"NativeSchemaProviderNSOnly\">{0}" +
				"    <xs:complexType>{0}" +
				"      <xs:sequence>{0}" +
				"        <xs:element ref=\"xs:schema\" />{0}" +
				"        <xs:any />{0}" +
				"      </xs:sequence>{0}" +
				"    </xs:complexType>{0}" +
				"  </xs:element>{0}" +
#endif
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[2].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema id=\"LuckyNumberSchema\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"LuckyNumber\" type=\"xs:int\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportXsdPrimitive ()
		{
			ArrayList types = new ArrayList ();
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
				Assert.AreEqual (1, schemas.Count, typeDesc.Type.FullName + "#1");

				StringWriter sw = new StringWriter ();
				schemas[0].Write (sw);

				Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
					"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
					"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
					"  <xs:element name=\"{2}\" {3}type=\"xs:{2}\" />{0}" +
					"</xs:schema>", Environment.NewLine, typeDesc.Type.Name, typeDesc.XmlType, 
					typeDesc.IsNillable ? "nillable=\"true\" " : ""),
					sw.ToString (), typeDesc.Type.FullName + "#2");

				schemas = Export (typeDesc.Type);
				Assert.AreEqual (1, schemas.Count, typeDesc.Type.FullName + "#3");

				sw.GetStringBuilder ().Length = 0;
				schemas[0].Write (sw);

				Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
					"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
					"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
					"  <xs:element name=\"{1}\" {2}type=\"xs:{1}\" />{0}" +
					"</xs:schema>", Environment.NewLine, typeDesc.XmlType, 
					typeDesc.IsNillable ? "nillable=\"true\" " : ""),
					sw.ToString (), typeDesc.Type.FullName + "#4");
			}
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void ExportXsdPrimitive_Object ()
		{
			XmlSchemas schemas = Export (typeof (object), "NSAnyType");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSAnyType\" elementFormDefault=\"qualified\" targetNamespace=\"NSAnyType\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"anyType\" nillable=\"true\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (object));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"anyType\" nillable=\"true\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportXsdPrimitive_ByteArray ()
		{
			XmlSchemas schemas = Export (typeof (byte[]), "NSByteArray");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSByteArray\" elementFormDefault=\"qualified\" targetNamespace=\"NSByteArray\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"base64Binary\" nillable=\"true\" type=\"xs:base64Binary\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (byte[]));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"base64Binary\" nillable=\"true\" type=\"xs:base64Binary\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportXsdPrimitive_Arrays ()
		{
			ArrayList types = new ArrayList ();
			types.Add (new TypeDescription (typeof (sbyte[]), true, "byte", "Byte", true));
			types.Add (new TypeDescription (typeof (bool[]), true, "boolean", "Boolean", true));
			types.Add (new TypeDescription (typeof (short[]), true, "short", "Short", true));
			types.Add (new TypeDescription (typeof (int[]), true, "int", "Int", true));
			types.Add (new TypeDescription (typeof (long[]), true, "long", "Long", true));
			types.Add (new TypeDescription (typeof (float[]), true, "float", "Float", true));
			types.Add (new TypeDescription (typeof (double[]), true, "double", "Double", true));
			types.Add (new TypeDescription (typeof (decimal[]), true, "decimal", "Decimal", true));
			types.Add (new TypeDescription (typeof (ushort[]), true, "unsignedShort", "UnsignedShort", true));
			types.Add (new TypeDescription (typeof (uint[]), true, "unsignedInt", "UnsignedInt", true));
			types.Add (new TypeDescription (typeof (ulong[]), true, "unsignedLong", "UnsignedLong", true));
			types.Add (new TypeDescription (typeof (DateTime[]), true, "dateTime", "DateTime", true));
#if NET_2_0
			types.Add (new TypeDescription (typeof (XmlQualifiedName[]), true, "QName", "QName", true, true));
#else
			types.Add (new TypeDescription (typeof (XmlQualifiedName[]), true, "QName", "QName", true));
#endif
			types.Add (new TypeDescription (typeof (string[]), true, "string", "String", true, true));

			foreach (TypeDescription typeDesc in types) {
				XmlSchemas schemas = Export (typeDesc.Type, typeDesc.Type.Name);
				Assert.AreEqual (1, schemas.Count, typeDesc.Type.FullName + "#1");

				StringWriter sw = new StringWriter ();
				schemas[0].Write (sw);

				Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
					"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
					"<xs:schema xmlns:tns=\"{1}\" elementFormDefault=\"qualified\" targetNamespace=\"{1}\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
					"  <xs:element name=\"ArrayOf{2}\" {5}type=\"tns:ArrayOf{2}\" />{0}" +
					"  <xs:complexType name=\"ArrayOf{2}\">{0}" +
					"    <xs:sequence>{0}" +
					"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"{3}\" {6}type=\"{4}:{3}\" />{0}" +
					"    </xs:sequence>{0}" +
					"  </xs:complexType>{0}" +
					"</xs:schema>", Environment.NewLine, typeDesc.Type.Name, typeDesc.ArrayType, typeDesc.XmlType, 
					typeDesc.XsdType ? "xs" : "tns", typeDesc.IsNillable ? "nillable=\"true\" " : "",
					typeDesc.IsElementNillable ? "nillable=\"true\" " : ""),
					sw.ToString (), typeDesc.Type.FullName + "#2" + "|" + typeDesc.IsNillable);

				schemas = Export (typeDesc.Type);
				Assert.AreEqual (1, schemas.Count, typeDesc.Type.FullName + "#3");

				sw.GetStringBuilder ().Length = 0;
				schemas[0].Write (sw);

				Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
					"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
					"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
					"  <xs:element name=\"ArrayOf{1}\" {4}type=\"ArrayOf{1}\" />{0}" +
					"  <xs:complexType name=\"ArrayOf{1}\">{0}" +
					"    <xs:sequence>{0}" +
					"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"{2}\" {5}type=\"{3}:{2}\" />{0}" +
					"    </xs:sequence>{0}" +
					"  </xs:complexType>{0}" +
					"</xs:schema>", Environment.NewLine, typeDesc.ArrayType, typeDesc.XmlType,
					typeDesc.XsdType ? "xs" : "tns", typeDesc.IsNillable ? "nillable=\"true\" " : "",
					typeDesc.IsElementNillable ? "nillable=\"true\" " : ""),
					sw.ToString (), typeDesc.Type.FullName + "#4");
			}
		}

		[Test]
		public void ExportXsdPrimitive_Object_Arrays ()
		{
			XmlSchemas schemas = Export (typeof (object[]), "NSArrayOfAnyType");
			Assert.AreEqual (1, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSArrayOfAnyType\" elementFormDefault=\"qualified\" targetNamespace=\"NSArrayOfAnyType\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfAnyType\" nillable=\"true\" type=\"tns:ArrayOfAnyType\" />{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			schemas = Export (typeof (object[]));
			Assert.AreEqual (1, schemas.Count, "#3");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:element name=\"ArrayOfAnyType\" nillable=\"true\" type=\"ArrayOfAnyType\" />{0}" +
				"  <xs:complexType name=\"ArrayOfAnyType\">{0}" +
				"    <xs:sequence>{0}" +
				"      <xs:element minOccurs=\"0\" maxOccurs=\"unbounded\" name=\"anyType\" nillable=\"true\" />{0}" +
				"    </xs:sequence>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
		public void ExportNonXsdPrimitive_Guid ()
		{
			XmlSchemas schemas = Export (typeof (Guid), "NSPrimGuid");
			Assert.AreEqual (2, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSPrimGuid\" elementFormDefault=\"qualified\" targetNamespace=\"NSPrimGuid\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://microsoft.com/wsdl/types/\" />{0}" +
				"  <xs:element name=\"guid\" xmlns:q1=\"http://microsoft.com/wsdl/types/\" type=\"q1:guid\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" elementFormDefault=\"qualified\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:simpleType name=\"guid\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:pattern value=\"[0-9a-fA-F]{{8}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{12}}\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#3");

			schemas = Export (typeof (Guid));
			Assert.AreEqual (2, schemas.Count, "#4");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://microsoft.com/wsdl/types/\" />{0}" +
				"  <xs:element name=\"guid\" xmlns:q1=\"http://microsoft.com/wsdl/types/\" type=\"q1:guid\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#5");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" elementFormDefault=\"qualified\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:simpleType name=\"guid\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:pattern value=\"[0-9a-fA-F]{{8}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{12}}\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#6");
		}

		[Test]
		public void ExportNonXsdPrimitive_Char ()
		{
			XmlSchemas schemas = Export (typeof (char), "NSPrimChar");
			Assert.AreEqual (2, schemas.Count, "#1");

			StringWriter sw = new StringWriter ();
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSPrimChar\" elementFormDefault=\"qualified\" targetNamespace=\"NSPrimChar\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://microsoft.com/wsdl/types/\" />{0}" +
				"  <xs:element name=\"char\" xmlns:q1=\"http://microsoft.com/wsdl/types/\" type=\"q1:char\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#2");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" elementFormDefault=\"qualified\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:simpleType name=\"char\">{0}" +
				"    <xs:restriction base=\"xs:unsignedShort\" />{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#3");

			schemas = Export (typeof (char));
			Assert.AreEqual (2, schemas.Count, "#4");

			sw.GetStringBuilder ().Length = 0;
			schemas[0].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"http://microsoft.com/wsdl/types/\" />{0}" +
				"  <xs:element name=\"char\" xmlns:q1=\"http://microsoft.com/wsdl/types/\" type=\"q1:char\" />{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#5");

			sw.GetStringBuilder ().Length = 0;
			schemas[1].Write (sw);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"http://microsoft.com/wsdl/types/\" elementFormDefault=\"qualified\" targetNamespace=\"http://microsoft.com/wsdl/types/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:simpleType name=\"char\">{0}" +
				"    <xs:restriction base=\"xs:unsignedShort\" />{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine), sw.ToString (), "#6");
		}

		public class Employee : IXmlSerializable
		{
			private string _firstName;
			private string _lastName;
			private string _address;

			public virtual XmlSchema GetSchema ()
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

		public class EmployeeSchema : IXmlSerializable
		{
			private string _firstName;
			private string _lastName;
			private string _address;

			public virtual XmlSchema GetSchema ()
			{
				return CreateSchema ();
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

			protected static XmlSchema CreateSchema ()
			{
				XmlSchema schema = new XmlSchema ();
				schema.Id = "EmployeeSchema";
				schema.TargetNamespace = "urn:types-devx-com";

				XmlSchemaComplexType type = new XmlSchemaComplexType ();
				type.Name = "employeeRoot";
				XmlSchemaAttribute firstNameAttr = new XmlSchemaAttribute ();
				firstNameAttr.Name = "firstName";
				type.Attributes.Add (firstNameAttr);

				XmlSchemaAttribute lastNameAttr = new XmlSchemaAttribute ();
				lastNameAttr.Name = "lastName";
				type.Attributes.Add (lastNameAttr);

				XmlSchemaAttribute addressAttr = new XmlSchemaAttribute ();
				addressAttr.Name = "address";
				type.Attributes.Add (addressAttr);

				XmlSchemaElement employeeElement = new XmlSchemaElement ();
				employeeElement.Name = "employee";
				XmlQualifiedName name = new XmlQualifiedName ("employeeRoot", "urn:types-devx-com");
				employeeElement.SchemaTypeName = name;

				schema.Items.Add (type);
				schema.Items.Add (employeeElement);
				return schema;
			}
		}

		public class PrimitiveSchema : IXmlSerializable
		{
			private string _firstName;
			private string _lastName;
			private string _address;

			public virtual XmlSchema GetSchema ()
			{
				XmlSchema schema = new XmlSchema ();
				schema.Id = "LuckyNumberSchema";
				XmlSchemaElement luckyNumberElement = new XmlSchemaElement ();
				luckyNumberElement.Name = "LuckyNumber";
				luckyNumberElement.SchemaTypeName = new XmlQualifiedName ("int", "http://www.w3.org/2001/XMLSchema");
				schema.Items.Add (luckyNumberElement);
				return schema;
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

		public class MissingIDSchema : IXmlSerializable
		{
			private string _firstName;
			private string _lastName;
			private string _address;

			public virtual XmlSchema GetSchema ()
			{
				XmlSchema schema = new XmlSchema ();
				XmlSchemaElement luckyNumberElement = new XmlSchemaElement ();
				luckyNumberElement.Name = "LuckyNumber";
				luckyNumberElement.SchemaTypeName = new XmlQualifiedName ("int", "http://www.w3.org/2001/XMLSchema");
				return schema;
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

#if NET_2_0
		[XmlSchemaProvider ("CreateEmployeeSchema")]
#endif
		public class EmployeeSchemaProvider : EmployeeSchema
		{
#if NET_2_0
			public static XmlQualifiedName CreateEmployeeSchema (XmlSchemaSet schemaSet)
			{
				schemaSet.Add (CreateSchema ());
				return new XmlQualifiedName ("employeeRoot", "urn:types-devx-com");
			}
#else
			public override XmlSchema GetSchema ()
			{
				return null;
			}
#endif
		}

#if NET_2_0
		[XmlSchemaProvider ("CreateLuckyNumberSchema")]
#endif
		public class PrimitiveSchemaProvider : IXmlSerializable
		{
#if NET_2_0
			public static XmlQualifiedName CreateLuckyNumberSchema (XmlSchemaSet schemaSet)
			{
				XmlSchema schema = new XmlSchema ();

				XmlSchemaElement luckyNumberElement = new XmlSchemaElement ();
				luckyNumberElement.Name = "LuckyNumber";

				XmlQualifiedName typeName = new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
				luckyNumberElement.SchemaTypeName = typeName;
				schema.Items.Add (luckyNumberElement);

				schemaSet.Add (schema);
				return typeName;
			}
#endif

			public XmlSchema GetSchema ()
			{
				return null;
			}

			public void WriteXml (XmlWriter writer)
			{
				writer.WriteElementString ("LuckyNumber", "7");
			}

			public void ReadXml (XmlReader reader)
			{
				XmlNodeType type = reader.MoveToContent ();
				if (type == XmlNodeType.Element && reader.LocalName == "LuckyNumber") {
				}
			}
		}

		private class TypeDescription
		{
			public TypeDescription (Type type, bool xsdType, string xmlType, string arrayType) : this (type, xsdType, xmlType, arrayType, false)
			{
			}

			public TypeDescription (Type type, bool xsdType, string xmlType, string arrayType, bool isNillable) :
				this (type, xsdType, xmlType, arrayType, isNillable, false)
			{
			}

			public TypeDescription (Type type, bool xsdType, string xmlType, string arrayType, bool isNillable, bool isElementNillable)
			{
				_type = type;
				_xsdType = xsdType;
				_xmlType = xmlType;
				_arrayType = arrayType;
				_isNillable = isNillable;
				_isElementNillable = isElementNillable;
			}

			public Type Type {
				get { return _type; }
			}

			public string XmlType {
				get { return _xmlType; }
			}

			public string ArrayType {
				get { return _arrayType; }
			}

			public bool XsdType {
				get { return _xsdType; }
			}

			public bool IsNillable {
				get { return _isNillable; }
			}

			public bool IsElementNillable {
				get { return _isElementNillable; }
			}

			private Type _type;
			private bool _xsdType;
			private string _xmlType;
			private string _arrayType;
			private bool _isNillable;
			private bool _isElementNillable;
		}

		public class StructContainer
		{
			public EnumDefaultValue Value;
			public TimeSpan[] Times;
		}

		public class XmlSerializableContainer
		{
			public Employee Slave;
			public EmployeeSchema SlaveSchema;
			public EmployeeSchemaProvider SlaveSchemaProvider;
			public PrimitiveSchema NativeSchema;
			public PrimitiveSchemaProvider NativeSchemaProvider;

			[XmlElement ("SlaveNamespace", Namespace = "urn:types-devx-com")]
			public Employee SlaveNS;
			[XmlElement ("SlaveSchemaNamespace", Namespace = "urn:types-devx-com")]
			public EmployeeSchema SlaveSchemaNS;
			[XmlElement ("SlaveSchemaProviderNamespace", Namespace = "urn:types-devx-com")]
			public EmployeeSchemaProvider SlaveSchemaProviderNS;
			[XmlElement ("NativeSchemaNamespace", Namespace = "urn:types-devx-com")]
			public PrimitiveSchema NativeSchemaNS;
			[XmlElement ("NativeSchemaProviderNamespace", Namespace = "urn:types-devx-com")]
			public PrimitiveSchemaProvider NativeSchemaProviderNS;


			[XmlElement (Namespace = "urn:types-devx-com")]
			public Employee SlaveNSOnly;
			[XmlElement (Namespace = "urn:types-devx-com")]
			public EmployeeSchema SlaveSchemaNSOnly;
			[XmlElement (Namespace = "urn:types-devx-com")]
			public EmployeeSchemaProvider SlaveSchemaProviderNSOnly;
			[XmlElement (Namespace = "urn:types-devx-com")]
			public PrimitiveSchema NativeSchemaNSOnly;
			[XmlElement (Namespace = "urn:types-devx-com")]
			public PrimitiveSchemaProvider NativeSchemaProviderNSOnly;
		}
	}
}
