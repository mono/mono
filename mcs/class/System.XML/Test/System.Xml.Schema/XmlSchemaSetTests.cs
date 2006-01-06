//
// System.Xml.XmlSchemaSetTests.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//
#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaSetTests
	{
		[Test]
		public void Add ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' />");
			ss.Add (null, new XmlNodeReader (doc)); // null targetNamespace
			ss.Compile ();

			// same document, different targetNamespace
			ss.Add ("ab", new XmlNodeReader (doc));

			// Add(null, xmlReader) -> targetNamespace in the schema
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:foo' />");
			ss.Add (null, new XmlNodeReader (doc));

			Assert.AreEqual (3, ss.Count);

			bool chameleon = false;
			bool ab = false;
			bool urnfoo = false;

			foreach (XmlSchema schema in ss.Schemas ()) {
				if (schema.TargetNamespace == null)
					chameleon = true;
				else if (schema.TargetNamespace == "ab")
					ab = true;
				else if (schema.TargetNamespace == "urn:foo")
					urnfoo = true;
			}
			Assert.IsTrue (chameleon, "chameleon schema missing");
			Assert.IsTrue (ab, "target-remapped schema missing");
			Assert.IsTrue (urnfoo, "target specified in the schema ignored");
		}

		[Test]
		[Ignore ("This behavior might be changed, since Add(XmlSchema) does not throw any exceptions, while this does.")]
		[ExpectedException (typeof (ArgumentException))]
		public void AddTwice ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' />");
			ss.Add ("ab", new XmlNodeReader (doc));
			ss.Add ("ab", new XmlNodeReader (doc));
		}

		[Test]
		public void CompilationSettings ()
		{
			Assert.IsNotNull (new XmlSchemaSet ().CompilationSettings);
			new XmlSchemaSet ().CompilationSettings = null;
		}

		[Test]
		public void DisableUpaCheck ()
		{
			string schema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:complexType name='Foo'>
    <xs:sequence>
      <xs:choice minOccurs='0'>
        <xs:element name='el'/>
      </xs:choice>
      <xs:element name='el' />
    </xs:sequence>
  </xs:complexType>
</xs:schema>";
			XmlSchema xs = XmlSchema.Read (new XmlTextReader (
				schema, XmlNodeType.Document, null), null);
			XmlSchemaSet xss = new XmlSchemaSet ();
			xss.Add (xs);
			xss.CompilationSettings.EnableUpaCheck = false;

			xss.Compile ();
		}

		[Test]
		public void AddRollbackIsCompiled ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			ss.Add (new XmlSchema ());
			ss.Compile ();
			Assert.IsTrue (ss.IsCompiled, "#1");
			XmlSchema sc = new XmlSchema (); // compiled one
			sc.Compile (null);
			ss.Add (sc);
			Assert.IsFalse (ss.IsCompiled, "#2");
			ss.Add (new XmlSchema ()); // not-compiled one
			Assert.IsFalse (ss.IsCompiled, "#3");
			XmlSchema s;

			s = new XmlSchema ();
			s.TargetNamespace = "urn:foo";
			XmlSchemaElement el;
			el = new XmlSchemaElement ();
			el.Name = "root";
			s.Items.Add (el);
			ss.Add (s);

			s = new XmlSchema ();
			s.TargetNamespace = "urn:foo";
			el = new XmlSchemaElement ();
			el.Name = "foo";
			s.Items.Add (el);
			ss.Add (s);
			ss.Compile ();
			Assert.IsTrue (ss.IsCompiled, "#4");
			ss.RemoveRecursive (s);
			Assert.IsTrue (ss.IsCompiled, "#5");
		}
	}
}
#endif
