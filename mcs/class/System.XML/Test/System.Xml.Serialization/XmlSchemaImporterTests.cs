//
// System.Xml.Serialization.XmlSchemaImporterTests
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
	public class XmlSchemaImporterTests
	{
		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_Struct ()
		{
			XmlSchemas schemas = ExportType (typeof (TimeSpan));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("TimeSpan", map.ElementName, "#3");
			Assert.AreEqual ("NSTimeSpan", map.Namespace, "#4");
			Assert.AreEqual ("TimeSpan", map.TypeFullName, "#5");
			Assert.AreEqual ("TimeSpan", map.TypeName, "#6");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_AnyType ()
		{
			XmlSchemas schemas = ExportType (typeof (object));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("anyType", map.ElementName, "#3");
			Assert.AreEqual ("NSObject", map.Namespace, "#4");
			Assert.AreEqual ("System.Object", map.TypeFullName, "#5");
			Assert.AreEqual ("Object", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Boolean ()
		{
			XmlSchemas schemas = ExportType (typeof (bool));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("boolean", map.ElementName, "#3");
			Assert.AreEqual ("NSBoolean", map.Namespace, "#4");
			Assert.AreEqual ("System.Boolean", map.TypeFullName, "#5");
			Assert.AreEqual ("Boolean", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Short ()
		{
			XmlSchemas schemas = ExportType (typeof (short));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("short", map.ElementName, "#3");
			Assert.AreEqual ("NSInt16", map.Namespace, "#4");
			Assert.AreEqual ("System.Int16", map.TypeFullName, "#5");
			Assert.AreEqual ("Int16", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_UnsignedShort ()
		{
			XmlSchemas schemas = ExportType (typeof (ushort));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("unsignedShort", map.ElementName, "#3");
			Assert.AreEqual ("NSUInt16", map.Namespace, "#4");
			Assert.AreEqual ("System.UInt16", map.TypeFullName, "#5");
			Assert.AreEqual ("UInt16", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Int ()
		{
			XmlSchemas schemas = ExportType (typeof (int));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("int", map.ElementName, "#3");
			Assert.AreEqual ("NSInt32", map.Namespace, "#4");
			Assert.AreEqual ("System.Int32", map.TypeFullName, "#5");
			Assert.AreEqual ("Int32", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_UnsignedInt ()
		{
			XmlSchemas schemas = ExportType (typeof (uint));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("unsignedInt", map.ElementName, "#3");
			Assert.AreEqual ("NSUInt32", map.Namespace, "#4");
			Assert.AreEqual ("System.UInt32", map.TypeFullName, "#5");
			Assert.AreEqual ("UInt32", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Long ()
		{
			XmlSchemas schemas = ExportType (typeof (long));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("long", map.ElementName, "#3");
			Assert.AreEqual ("NSInt64", map.Namespace, "#4");
			Assert.AreEqual ("System.Int64", map.TypeFullName, "#5");
			Assert.AreEqual ("Int64", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_UnsignedLong ()
		{
			XmlSchemas schemas = ExportType (typeof (ulong));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("unsignedLong", map.ElementName, "#3");
			Assert.AreEqual ("NSUInt64", map.Namespace, "#4");
			Assert.AreEqual ("System.UInt64", map.TypeFullName, "#5");
			Assert.AreEqual ("UInt64", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Float ()
		{
			XmlSchemas schemas = ExportType (typeof (float));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("float", map.ElementName, "#3");
			Assert.AreEqual ("NSSingle", map.Namespace, "#4");
			Assert.AreEqual ("System.Single", map.TypeFullName, "#5");
			Assert.AreEqual ("Single", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Double ()
		{
			XmlSchemas schemas = ExportType (typeof (double));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("double", map.ElementName, "#3");
			Assert.AreEqual ("NSDouble", map.Namespace, "#4");
			Assert.AreEqual ("System.Double", map.TypeFullName, "#5");
			Assert.AreEqual ("Double", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_DateTime ()
		{
			XmlSchemas schemas = ExportType (typeof (DateTime));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("dateTime", map.ElementName, "#3");
			Assert.AreEqual ("NSDateTime", map.Namespace, "#4");
			Assert.AreEqual ("System.DateTime", map.TypeFullName, "#5");
			Assert.AreEqual ("DateTime", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Decimal ()
		{
			XmlSchemas schemas = ExportType (typeof (decimal));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("decimal", map.ElementName, "#3");
			Assert.AreEqual ("NSDecimal", map.Namespace, "#4");
			Assert.AreEqual ("System.Decimal", map.TypeFullName, "#5");
			Assert.AreEqual ("Decimal", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_QName ()
		{
			XmlSchemas schemas = ExportType (typeof (XmlQualifiedName));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("QName", map.ElementName, "#3");
			Assert.AreEqual ("NSXmlQualifiedName", map.Namespace, "#4");
			Assert.AreEqual ("System.Xml.XmlQualifiedName", map.TypeFullName, "#5");
			Assert.AreEqual ("XmlQualifiedName", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_String ()
		{
			XmlSchemas schemas = ExportType (typeof (string));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("string", map.ElementName, "#3");
			Assert.AreEqual ("NSString", map.Namespace, "#4");
			Assert.AreEqual ("System.String", map.TypeFullName, "#5");
			Assert.AreEqual ("String", map.TypeName, "#6");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Guid ()
		{
			XmlSchemas schemas = ExportType (typeof (Guid));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("int", map.ElementName, "#3");
			Assert.AreEqual ("NSInt32", map.Namespace, "#4");
			Assert.AreEqual ("System.Int32", map.TypeFullName, "#5");
			Assert.AreEqual ("Int32", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_UnsignedByte ()
		{
			XmlSchemas schemas = ExportType (typeof (byte));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("unsignedByte", map.ElementName, "#3");
			Assert.AreEqual ("NSByte", map.Namespace, "#4");
			Assert.AreEqual ("System.Byte", map.TypeFullName, "#5");
			Assert.AreEqual ("Byte", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Byte ()
		{
			XmlSchemas schemas = ExportType (typeof (sbyte));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("byte", map.ElementName, "#3");
			Assert.AreEqual ("NSSByte", map.Namespace, "#4");
			Assert.AreEqual ("System.SByte", map.TypeFullName, "#5");
			Assert.AreEqual ("SByte", map.TypeName, "#6");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Char ()
		{
			XmlSchemas schemas = ExportType (typeof (char));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("int", map.ElementName, "#3");
			Assert.AreEqual ("NSChar", map.Namespace, "#4");
			Assert.AreEqual ("System.Int32", map.TypeFullName, "#5");
			Assert.AreEqual ("Int32", map.TypeName, "#6");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_Base64Binary ()
		{
			XmlSchemas schemas = ExportType (typeof (byte[]));
			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#2");
			Assert.AreEqual ("base64Binary", map.ElementName, "#3");
			Assert.AreEqual ("NSByte[]", map.Namespace, "#4");
			Assert.AreEqual ("System.Byte[]", map.TypeFullName, "#5");
			Assert.AreEqual ("Byte[]", map.TypeName, "#6");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Duration ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSDuration\" elementFormDefault=\"qualified\" targetNamespace=\"NSDuration\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"duration\" type=\"xs:duration\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("duration", qname.Name, "#2");
			Assert.AreEqual ("NSDuration", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("duration", map.ElementName, "#5");
			Assert.AreEqual ("NSDuration", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Date ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSDate\" elementFormDefault=\"qualified\" targetNamespace=\"NSDate\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"date\" type=\"xs:date\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("date", qname.Name, "#2");
			Assert.AreEqual ("NSDate", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("date", map.ElementName, "#5");
			Assert.AreEqual ("NSDate", map.Namespace, "#6");
			Assert.AreEqual ("System.DateTime", map.TypeFullName, "#7");
			Assert.AreEqual ("DateTime", map.TypeName, "#8");
		}

		private static XmlSchemas ExportType (Type type)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter ("NS" + type.Name);
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter sx = new XmlSchemaExporter (schemas);
			XmlTypeMapping tm = ri.ImportTypeMapping (type);
			sx.ExportTypeMapping (tm);
			return schemas;
		}

		private static ArrayList GetXmlQualifiedNames (XmlSchemas schemas)
		{
			ArrayList qnames = new ArrayList ();

			foreach (XmlSchema schema in schemas) {
				if (!schema.IsCompiled) schema.Compile (null);
				foreach (XmlSchemaObject ob in schema.Items)
					if (ob is XmlSchemaElement)
						qnames.Add (((XmlSchemaElement) ob).QualifiedName);
			}

			return qnames;
		}
	}
}
