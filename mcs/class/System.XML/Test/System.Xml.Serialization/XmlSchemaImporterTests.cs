//
// System.Xml.Serialization.XmlSchemaImporterTests
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2005 Gert Driesen
// Copyright (C) 2006-2007 Novell, Inc.
// 

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CSharp;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSchemaImporterTests
	{
		private const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";

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
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
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
		public void ImportTypeMapping_XsdPrimitive_AnyURI ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSAnyURI\" elementFormDefault=\"qualified\" targetNamespace=\"NSAnyURI\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"anyURI\" type=\"xs:anyURI\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("anyURI", qname.Name, "#2");
			Assert.AreEqual ("NSAnyURI", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("anyURI", map.ElementName, "#5");
			Assert.AreEqual ("NSAnyURI", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Base64 ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSBase64\" elementFormDefault=\"qualified\" targetNamespace=\"NSBase64\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"base64\" type=\"xs:base64\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));
			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping (new XmlQualifiedName ("base64", "NSBase64"));

			Assert.IsNotNull (map, "#1");
			Assert.AreEqual ("base64", map.ElementName, "#2");
			Assert.AreEqual ("NSBase64", map.Namespace, "#3");
			Assert.AreEqual ("System.String", map.TypeFullName, "#4");
			Assert.AreEqual ("String", map.TypeName, "#5");
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
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSChar\" elementFormDefault=\"qualified\" targetNamespace=\"NSChar\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"char\" type=\"xs:char\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));
			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping (new XmlQualifiedName ("char", "NSChar"));

			Assert.IsNotNull (map, "#A1");
			Assert.AreEqual ("char", map.ElementName, "#A2");
			Assert.AreEqual ("NSChar", map.Namespace, "#A3");
			Assert.AreEqual ("System.String", map.TypeFullName, "#A4");
			Assert.AreEqual ("String", map.TypeName, "#A5");

#if ONLY_1_1
			schemas = ExportType (typeof (char));
			importer = new XmlSchemaImporter (schemas);
			map = importer.ImportTypeMapping (new XmlQualifiedName ("char", "NSChar"));

			Assert.IsNotNull (map, "#B1");
			Assert.AreEqual ("char", map.ElementName, "#B2");
			Assert.AreEqual ("NSChar", map.Namespace, "#B3");
			Assert.AreEqual ("System.Char", map.TypeFullName, "#B4");
			Assert.AreEqual ("Char", map.TypeName, "#B5");
#endif
		}

		[Test]
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
		public void ImportTypeMapping_XsdPrimitive_ENTITIES ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSENTITIES\" elementFormDefault=\"qualified\" targetNamespace=\"NSENTITIES\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"ENTITIES\" type=\"xs:ENTITIES\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("ENTITIES", qname.Name, "#2");
			Assert.AreEqual ("NSENTITIES", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("ENTITIES", map.ElementName, "#5");
			Assert.AreEqual ("NSENTITIES", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_ENTITY ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSENTITY\" elementFormDefault=\"qualified\" targetNamespace=\"NSENTITY\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"ENTITY\" type=\"xs:ENTITY\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("ENTITY", qname.Name, "#2");
			Assert.AreEqual ("NSENTITY", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("ENTITY", map.ElementName, "#5");
			Assert.AreEqual ("NSENTITY", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
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
		public void ImportTypeMapping_XsdPrimitive_GDay ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSGDay\" elementFormDefault=\"qualified\" targetNamespace=\"NSGDay\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"gDay\" type=\"xs:gDay\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("gDay", qname.Name, "#2");
			Assert.AreEqual ("NSGDay", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("gDay", map.ElementName, "#5");
			Assert.AreEqual ("NSGDay", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_GMonthDay ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSGMonthDay\" elementFormDefault=\"qualified\" targetNamespace=\"NSGMonthDay\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"gMonthDay\" type=\"xs:gMonthDay\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("gMonthDay", qname.Name, "#2");
			Assert.AreEqual ("NSGMonthDay", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("gMonthDay", map.ElementName, "#5");
			Assert.AreEqual ("NSGMonthDay", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Guid ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSGuid\" elementFormDefault=\"qualified\" targetNamespace=\"NSGuid\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"guid\" type=\"xs:guid\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));
			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping (new XmlQualifiedName ("guid", "NSGuid"));

			Assert.IsNotNull (map, "#A1");
			Assert.AreEqual ("guid", map.ElementName, "#A2");
			Assert.AreEqual ("NSGuid", map.Namespace, "#A3");
			Assert.AreEqual ("System.String", map.TypeFullName, "#A4");
			Assert.AreEqual ("String", map.TypeName, "#A5");

#if ONLY_1_1
			schemas = ExportType (typeof (Guid));
			importer = new XmlSchemaImporter (schemas);
			map = importer.ImportTypeMapping (new XmlQualifiedName ("guid", "NSGuid"));

			Assert.IsNotNull (map, "#B1");
			Assert.AreEqual ("guid", map.ElementName, "#B2");
			Assert.AreEqual ("NSGuid", map.Namespace, "#B3");
			Assert.AreEqual ("System.Guid", map.TypeFullName, "#B4");
			Assert.AreEqual ("Guid", map.TypeName, "#B5");
#endif
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_GYear ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSGYear\" elementFormDefault=\"qualified\" targetNamespace=\"NSGYear\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"gYear\" type=\"xs:gYear\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("gYear", qname.Name, "#2");
			Assert.AreEqual ("NSGYear", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("gYear", map.ElementName, "#5");
			Assert.AreEqual ("NSGYear", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_GYearMonth ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSGYearMonth\" elementFormDefault=\"qualified\" targetNamespace=\"NSGYearMonth\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"gYearMonth\" type=\"xs:gYearMonth\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("gYearMonth", qname.Name, "#2");
			Assert.AreEqual ("NSGYearMonth", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("gYearMonth", map.ElementName, "#5");
			Assert.AreEqual ("NSGYearMonth", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_HexBinary ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSHexBinary\" elementFormDefault=\"qualified\" targetNamespace=\"NSHexBinary\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"hexBinary\" type=\"xs:hexBinary\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("hexBinary", qname.Name, "#2");
			Assert.AreEqual ("NSHexBinary", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("hexBinary", map.ElementName, "#5");
			Assert.AreEqual ("NSHexBinary", map.Namespace, "#6");
			Assert.AreEqual ("System.Byte[]", map.TypeFullName, "#7");
			Assert.AreEqual ("Byte[]", map.TypeName, "#8");
		}

		[Test]
		public void ImportTypeMapping_XsdPrimitive_IDREFS ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSIDREFS\" elementFormDefault=\"qualified\" targetNamespace=\"NSIDREFS\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"IDREFS\" type=\"xs:IDREFS\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("IDREFS", qname.Name, "#2");
			Assert.AreEqual ("NSIDREFS", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("IDREFS", map.ElementName, "#5");
			Assert.AreEqual ("NSIDREFS", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
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
		public void ImportTypeMapping_XsdPrimitive_Integer ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSInteger\" elementFormDefault=\"qualified\" targetNamespace=\"NSInteger\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"integer\" type=\"xs:integer\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("integer", qname.Name, "#2");
			Assert.AreEqual ("NSInteger", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("integer", map.ElementName, "#5");
			Assert.AreEqual ("NSInteger", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
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
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_Month ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSMonth\" elementFormDefault=\"qualified\" targetNamespace=\"NSMonth\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"month\" type=\"xs:month\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping (new XmlQualifiedName ("month", "NSMonth"));

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("month", map.ElementName, "#5");
			Assert.AreEqual ("NSMonth", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
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
		public void ImportTypeMapping_XsdPrimitive_Time ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSTime\" elementFormDefault=\"qualified\" targetNamespace=\"NSTime\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"time\" type=\"xs:time\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("time", qname.Name, "#2");
			Assert.AreEqual ("NSTime", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("time", map.ElementName, "#5");
			Assert.AreEqual ("NSTime", map.Namespace, "#6");
			Assert.AreEqual ("System.DateTime", map.TypeFullName, "#7");
			Assert.AreEqual ("DateTime", map.TypeName, "#8");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_TimeInstant ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSTimeInstant\" elementFormDefault=\"qualified\" targetNamespace=\"NSTimeInstant\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"timeInstant\" type=\"xs:timeInstant\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping (new XmlQualifiedName ("timeInstant", "NSTimeInstant"));

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("timeInstant", map.ElementName, "#5");
			Assert.AreEqual ("NSTimeInstant", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTypeMapping_XsdPrimitive_TimePeriod ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<xs:schema xmlns:tns=\"NSTimePeriod\" elementFormDefault=\"qualified\" targetNamespace=\"NSTimePeriod\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
				"  <xs:element name=\"timePeriod\" type=\"xs:timePeriod\" />" +
				"</xs:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping (new XmlQualifiedName ("timePeriod", "NSTimePeriod"));

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("timePeriod", map.ElementName, "#5");
			Assert.AreEqual ("NSTimePeriod", map.Namespace, "#6");
			Assert.AreEqual ("System.String", map.TypeFullName, "#7");
			Assert.AreEqual ("String", map.TypeName, "#8");
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
		public void ImportTypeMapping_EnumSimpleContent ()
		{
			string schemaFragment = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<s:schema xmlns:tns=\"NSDate\" elementFormDefault=\"qualified\" targetNamespace=\"NSDate\" xmlns:s=\"http://www.w3.org/2001/XMLSchema\">" +
				"      <s:element name=\"trans\" type=\"tns:TranslationStatus\" />" +
				"      <s:complexType name=\"TranslationStatus\">" +
				"        <s:simpleContent>" +
				"          <s:extension base=\"tns:StatusType\">" +
				"            <s:attribute name=\"Language\" type=\"s:int\" use=\"required\" />" +
				"          </s:extension>" +
				"        </s:simpleContent>" +
				"      </s:complexType>" +
				"      <s:simpleType name=\"StatusType\">" +
				"        <s:restriction base=\"s:string\">" +
				"          <s:enumeration value=\"Untouched\" />" +
				"          <s:enumeration value=\"Touched\" />" +
				"          <s:enumeration value=\"Complete\" />" +
				"          <s:enumeration value=\"None\" />" +
				"        </s:restriction>" +
				"      </s:simpleType>" +
				"</s:schema>";

			XmlSchemas schemas = new XmlSchemas ();
			schemas.Add (XmlSchema.Read (new StringReader (schemaFragment), null));

			ArrayList qnames = GetXmlQualifiedNames (schemas);
			Assert.AreEqual (1, qnames.Count, "#1");

			XmlQualifiedName qname = (XmlQualifiedName) qnames[0];

			Assert.AreEqual ("trans", qname.Name, "#2");
			Assert.AreEqual ("NSDate", qname.Namespace, "#3");

			XmlSchemaImporter importer = new XmlSchemaImporter (schemas);
			XmlTypeMapping map = importer.ImportTypeMapping ((XmlQualifiedName) qnames[0]);

			Assert.IsNotNull (map, "#4");
			Assert.AreEqual ("trans", map.ElementName, "#5");
			Assert.AreEqual ("NSDate", map.Namespace, "#6");
			Assert.AreEqual ("TranslationStatus", map.TypeFullName, "#7");
			Assert.AreEqual ("TranslationStatus", map.TypeName, "#8");
			
			CodeNamespace codeNamespace = ExportCode (map);
			Assert.IsNotNull (codeNamespace, "#9");
			
			CodeTypeDeclaration type = FindType (codeNamespace, "TranslationStatus");
			Assert.IsNotNull (type, "#10");

#if NET_2_0
			CodeMemberProperty property = FindMember (type, "Value") as CodeMemberProperty;
			Assert.IsNotNull (property, "#A1");
			Assert.IsTrue (property.HasGet, "#A2");
			Assert.IsTrue (property.HasSet, "#A3");
			Assert.AreEqual ("StatusType", property.Type.BaseType, "#A4");

			CodeMemberField field = FindMember (type, "valueField") as CodeMemberField;
			Assert.IsNotNull (field, "#A5");
			Assert.AreEqual ("StatusType", field.Type.BaseType, "#A6");

			property = FindMember (type, "Language") as CodeMemberProperty;
			Assert.IsNotNull (property, "#B1");
			Assert.IsTrue (property.HasGet, "#B2");
			Assert.IsTrue (property.HasSet, "#B3");
			Assert.AreEqual ("System.Int32", property.Type.BaseType, "#B4");

			field = FindMember (type, "languageField") as CodeMemberField;
			Assert.IsNotNull (field, "#B5");
			Assert.AreEqual ("System.Int32", field.Type.BaseType, "#B6");
#else
			CodeMemberField field = FindMember (type, "Value") as CodeMemberField;
			Assert.IsNotNull (field, "#A1");
			Assert.AreEqual ("StatusType", field.Type.BaseType, "#A2");

			field = FindMember (type, "Language") as CodeMemberField;
			Assert.IsNotNull (field, "#B1");
			Assert.AreEqual ("System.Int32", field.Type.BaseType, "#B2");
#endif
		}

		XmlSchemaImporter CreateSchemaImporter (string xsd)
		{
			XmlSchemas s = new XmlSchemas ();
			XmlReader r = new XmlTextReader (xsd, XmlNodeType.Document, null);
			s.Add (XmlSchema.Read (r, null));
			return new XmlSchemaImporter (s);
		}

		[Test]
		public void ImportTypeMapping_NullableField ()
		{
			string xsd = @"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Root'>
    <xs:complexType>
      <xs:sequence>
        <xs:element name='Bar' nillable='true' type='xs:int' />
      </xs:sequence>
      <xs:attribute name='A' use='optional' type='xs:int' />
    </xs:complexType>
  </xs:element>
</xs:schema>";
			XmlSchemaImporter imp = CreateSchemaImporter (xsd);
			XmlTypeMapping map = imp.ImportTypeMapping (new XmlQualifiedName ("Root"));
			CodeNamespace cns = ExportCode (map);
#if NET_2_0
			CodeMemberProperty p = (CodeMemberProperty) FindMember (FindType (cns, "Root"), "Bar");
			Assert.AreEqual (1, p.Type.TypeArguments.Count, "2.0 #1");
			Assert.AreEqual ("System.Int32", p.Type.TypeArguments [0].BaseType, "2.0 #2");
#else
			CodeMemberField f = (CodeMemberField) FindMember (FindType (cns, "Root"), "Bar");
			Assert.AreEqual ("System.Int32", f.Type.BaseType, "1.x #1");
#endif
		}

		[Test]
		public void ImportMembersMapping_NullableField ()
		{
			string xsd = @"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Root'>
    <xs:complexType>
      <xs:sequence>
        <xs:element name='Bar' nillable='true' type='xs:int' />
        <xs:element name='Baz' type='xs:int' />
      </xs:sequence>
      <xs:attribute name='A' use='optional' type='xs:int' />
    </xs:complexType>
  </xs:element>
</xs:schema>";
			XmlSchemaImporter imp = CreateSchemaImporter (xsd);
			XmlMembersMapping map = imp.ImportMembersMapping (new XmlQualifiedName ("Root"));
			Assert.AreEqual (3, map.Count, "#1");
			XmlMemberMapping bar = map [0];
			Assert.AreEqual ("Bar", bar.ElementName, "#2-1");
			Assert.IsFalse (bar.CheckSpecified, "#2-2");
			XmlMemberMapping baz = map [1];
			Assert.AreEqual ("Baz", baz.ElementName, "#3-1");
			Assert.IsFalse (baz.CheckSpecified, "#3-2");
			XmlMemberMapping a = map [2];
			Assert.AreEqual ("A", a.ElementName, "#4-1"); // ... element name?
			Assert.IsTrue (a.CheckSpecified, "#4-2");

#if NET_2_0
			CodeDomProvider p = new Microsoft.CSharp.CSharpCodeProvider ();
			Assert.AreEqual ("System.Nullable`1[System.Int32]", bar.GenerateTypeName (p), "#5-1");
			Assert.AreEqual ("System.Int32", baz.GenerateTypeName (p), "#5-2");
#endif
		}
		
		CodeNamespace ExportCode (XmlTypeMapping map)
		{
			CodeNamespace codeNamespace = new CodeNamespace ();
			XmlCodeExporter exp = new XmlCodeExporter (codeNamespace);
			exp.ExportTypeMapping (map);
			return codeNamespace;
		}
		
		CodeTypeDeclaration FindType (CodeNamespace codeNamespace, string name)
		{
			foreach (CodeTypeDeclaration t in codeNamespace.Types)
				if (t.Name == name)
					return t;
			return null;
		}
		
		CodeTypeMember FindMember (CodeTypeDeclaration type, string name)
		{
			foreach (CodeTypeMember m in type.Members)
				if (m.Name == name)
					return m;
			return null;
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

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ImportTypeMappingNonExistent ()
		{
			XmlSchemas xss = new XmlSchemas ();
			xss.Add (new XmlSchema ());
			XmlSchemaImporter imp = new XmlSchemaImporter (xss);
			imp.ImportTypeMapping (new XmlQualifiedName ("foo"));
		}

		[Test]
		public void AnyTypeTopLevelElementImportsAllComplexTypes ()
		{
			string xsd = @"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Root' type='xs:anyType' />
  <xs:complexType name='FooType'>
    <xs:sequence>
      <xs:element name='Child1' type='xs:string' />
      <xs:element name='Child2' type='xs:string' />
      <xs:element name='Child3' type='xs:string' />
    </xs:sequence>
  </xs:complexType>
  <xs:complexType name='BarType' />
</xs:schema>";
			XmlSchemas xss = new XmlSchemas ();
			xss.Add (XmlSchema.Read (new XmlTextReader (new StringReader (xsd)), null));
			XmlSchemaImporter imp = new XmlSchemaImporter (xss);
			CodeNamespace cns = new CodeNamespace ();
			XmlCodeExporter exp = new XmlCodeExporter (cns);
			exp.ExportTypeMapping (imp.ImportTypeMapping (new XmlQualifiedName ("Root")));
			bool foo = false, bar = false;
			foreach (CodeTypeDeclaration td in cns.Types) {
				if (td.Name == "FooType")
					foo = true;
				else if (td.Name == "BarType")
					bar = true;
			}
			Assert.IsTrue (foo, "FooType not found");
			Assert.IsTrue (bar, "BarType not found");
		}

		[Test]
		public void DefaultTypeTopLevelElementImportsAllComplexTypes ()
		{
			string xsd = @"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Root' />
  <xs:complexType name='FooType'>
    <xs:sequence>
      <xs:element name='Child1' type='xs:string' />
      <xs:element name='Child2' type='xs:string' />
      <xs:element name='Child3' type='xs:string' />
    </xs:sequence>
  </xs:complexType>
  <xs:complexType name='BarType' />
</xs:schema>";
			XmlSchemas xss = new XmlSchemas ();
			xss.Add (XmlSchema.Read (new XmlTextReader (new StringReader (xsd)), null));
			XmlSchemaImporter imp = new XmlSchemaImporter (xss);
			CodeNamespace cns = new CodeNamespace ();
			XmlCodeExporter exp = new XmlCodeExporter (cns);
			exp.ExportTypeMapping (imp.ImportTypeMapping (new XmlQualifiedName ("Root")));
			bool foo = false, bar = false;
			foreach (CodeTypeDeclaration td in cns.Types) {
				if (td.Name == "FooType")
					foo = true;
				else if (td.Name == "BarType")
					bar = true;
			}
			Assert.IsTrue (foo, "FooType not found");
			Assert.IsTrue (bar, "BarType not found");
		}

		[Test]
		public void ImportComplexDerivationByExtension ()
		{
			string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Root' type='DerivedType' />
  <xs:complexType name='DerivedType'>
    <xs:complexContent>
      <xs:extension base='BaseType'>
        <xs:attribute name='Foo' type='xs:string' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:complexType name='BaseType'>
    <xs:attribute name='Foo' type='xs:string' />
  </xs:complexType>
</xs:schema>";
			XmlSchemaImporter imp = CreateImporter (xsd);
			CodeNamespace cns = new CodeNamespace ();
			XmlCodeExporter exp = new XmlCodeExporter (cns);
			exp.ExportTypeMapping (imp.ImportTypeMapping (new XmlQualifiedName ("Root")));
		}

		[Test]
		public void ImportSimpleSchemaType ()
		{
			string xsd = @"
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='a' type='b' />
  <xs:simpleType name='b'>
    <xs:restriction base='xs:string'>
      <xs:enumeration value='v1'/>
      <xs:enumeration value='v2'/>
      <xs:enumeration value='v3'/>
    </xs:restriction>
  </xs:simpleType>
</xs:schema>";
			XmlSchemaImporter imp = CreateImporter (xsd);
			XmlTypeMapping tm = imp.ImportTypeMapping (new XmlQualifiedName ("a"));
			Assert.AreEqual ("a", tm.ElementName, "#1");
			Assert.AreEqual ("b", tm.TypeName, "#2");
		}

		[Test]
		public void ImportWildcardElementAsClass ()
		{
			var xss = new XmlSchemas ();
			xss.Add (XmlSchema.Read (XmlReader.Create ("Test/XmlFiles/xsd/670945-1.xsd"), null));
			xss.Add (XmlSchema.Read (XmlReader.Create ("Test/XmlFiles/xsd/670945-2.xsd"), null));
			var imp = new XmlSchemaImporter (xss);
			var xtm = imp.ImportSchemaType (new XmlQualifiedName ("SystemDateTime", "http://www.onvif.org/ver10/schema"));
			var cns = new CodeNamespace ();
			var exp = new XmlCodeExporter (cns);
			exp.ExportTypeMapping (xtm);
			var sw = new StringWriter ();
			new CSharpCodeProvider ().GenerateCodeFromNamespace (cns, sw, null);
			Assert.IsTrue (sw.ToString ().IndexOf ("class SystemDateTimeExtension") > 0, "#1");
		}

		XmlSchemaImporter CreateImporter (params string [] schemaXmlStrings)
		{
			XmlSchemas xss = new XmlSchemas ();
			foreach (string xsd in schemaXmlStrings)
				xss.Add (XmlSchema.Read (new XmlTextReader (new StringReader (xsd)), null));
			return new XmlSchemaImporter (xss);
		}
	}
}
