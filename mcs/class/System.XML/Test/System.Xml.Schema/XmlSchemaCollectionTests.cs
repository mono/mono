//
// System.Xml.XmlSchemaCollectionTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaCollectionTests
	{
		private XmlSchema GetSchema (string path)
		{
			return XmlSchema.Read (new XmlTextReader (path), null);
		}

		private XmlQualifiedName QName (string name, string ns)
		{
			return new XmlQualifiedName (name, ns);
		}

		[Test]
		public void TestAdd ()
		{
			XmlSchemaCollection col = new XmlSchemaCollection ();
			XmlSchema schema = new XmlSchema ();
			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = "foo";
			schema.Items.Add (elem);
			schema.TargetNamespace = "urn:foo";
			col.Add (schema);
			col.Add (schema);	// No problem !?

			XmlSchema schema2 = new XmlSchema ();
			schema2.Items.Add (elem);
			schema2.TargetNamespace = "urn:foo";
			col.Add (schema2);	// No problem !!

			schema.Compile (null);
			col.Add (schema);
			col.Add (schema);	// Still no problem !!!

			schema2.Compile (null);
			col.Add (schema2);

			schema = GetSchema ("Test/XmlFiles/xsd/3.xsd");
			schema.Compile (null);
			col.Add (schema);

			schema2 = GetSchema ("Test/XmlFiles/xsd/3.xsd");
			schema2.Compile (null);
			col.Add (schema2);
		}

		[Test]
		public void TestAddDoesCompilation ()
		{
			XmlSchema schema = new XmlSchema ();
			Assert.IsFalse (schema.IsCompiled);
			XmlSchemaCollection col = new XmlSchemaCollection ();
			col.Add (schema);
			Assert.IsTrue (schema.IsCompiled);
		}

		[Test] // bug #75126
		public void TestGetEnumerator ()
		{
			new XmlSchemaCollection().GetEnumerator();
		}

		[Test] // bug #78220
		public void TestCompile ()
		{
			string schemaFragment1 = string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSDate\" elementFormDefault=\"qualified\" targetNamespace=\"NSDate\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:import namespace=\"NSStatus\" />{0}" +
				"  <xs:element name=\"trans\" type=\"tns:TranslationStatus\" />{0}" +
				"  <xs:complexType name=\"TranslationStatus\">{0}" +
				"    <xs:simpleContent>{0}" +
				"      <xs:extension xmlns:q1=\"NSStatus\" base=\"q1:StatusType\">{0}" +
				"        <xs:attribute name=\"Language\" type=\"xs:int\" use=\"required\" />{0}" +
				"      </xs:extension>{0}" +
				"    </xs:simpleContent>{0}" +
				"  </xs:complexType>{0}" +
				"</xs:schema>", Environment.NewLine);

			string schemaFragment2 = string.Format (CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<xs:schema xmlns:tns=\"NSStatus\" elementFormDefault=\"qualified\" targetNamespace=\"NSStatus\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">{0}" +
				"  <xs:simpleType name=\"StatusType\">{0}" +
				"    <xs:restriction base=\"xs:string\">{0}" +
				"      <xs:enumeration value=\"Untouched\" />{0}" +
				"      <xs:enumeration value=\"Touched\" />{0}" +
				"      <xs:enumeration value=\"Complete\" />{0}" +
				"      <xs:enumeration value=\"None\" />{0}" +
				"    </xs:restriction>{0}" +
				"  </xs:simpleType>{0}" +
				"</xs:schema>", Environment.NewLine);

			XmlSchema schema1 = XmlSchema.Read (new StringReader (schemaFragment1), null);
			XmlSchema schema2 = XmlSchema.Read (new StringReader (schemaFragment2), null);

			XmlSchemaCollection schemas = new XmlSchemaCollection ();
			schemas.Add (schema2);
			schemas.Add (schema1);

			Assert.IsTrue (schema1.IsCompiled, "#1");
			Assert.IsTrue (schema2.IsCompiled, "#2");
		}
	}
}
