//
// System.Xml.XmlSchemaCollectionTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaCollectionTests : Assertion
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
			Assert (!schema.IsCompiled);
			XmlSchemaCollection col = new XmlSchemaCollection ();
			col.Add (schema);
			Assert (schema.IsCompiled);
		}
	}
}
