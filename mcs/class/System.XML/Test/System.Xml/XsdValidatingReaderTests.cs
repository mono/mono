//
// MonoTests.System.Xml.XsdValidatingReaderTests.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XsdValidatingReaderTests : Assertion
	{
		public XsdValidatingReaderTests ()
		{
		}

		XmlReader xtr;
		XmlValidatingReader xvr;

		private XmlValidatingReader PrepareXmlReader (string xml)
		{
			XmlReader reader = new XmlTextReader (xml, XmlNodeType.Document, null);
//			XmlDocument doc = new XmlDocument ();
//			doc.LoadXml (xml);
//			XmlReader reader = new XmlNodeReader (doc);

			return new XmlValidatingReader (reader);
		}

		[Test]
		public void TestEmptySchema ()
		{
			string xml = "<root/>";
			xvr = PrepareXmlReader (xml);
			xvr.ValidationType = ValidationType.Schema;
			xvr.Read ();	// Is is missing schema component.
		}

		[Test]
		public void TestSimpleValidation ()
		{
			string xml = "<root/>";
			xvr = PrepareXmlReader (xml);
			AssertEquals (ValidationType.Auto, xvr.ValidationType);
			XmlSchema schema = new XmlSchema ();
			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = "root";
			schema.Items.Add (elem);
			xvr.Schemas.Add (schema);
			xvr.Read ();	// root
			AssertEquals (ValidationType.Auto, xvr.ValidationType);
			xvr.Read ();	// EOF

			xml = "<hoge/>";
			xvr = PrepareXmlReader (xml);
			xvr.Schemas.Add (schema);
			try {
				xvr.Read ();
				Fail ("element mismatch is incorrectly allowed");
			} catch (XmlSchemaException) {
			}

			xml = "<hoge xmlns='urn:foo' />";
			xvr = PrepareXmlReader (xml);
			xvr.Schemas.Add (schema);
			try {
				xvr.Read ();
				Fail ("Element in different namespace is incorrectly allowed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestReadTypedValueSimple ()
		{
			string xml = "<root>12</root>";
			XmlSchema schema = new XmlSchema ();
			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = "root";
			elem.SchemaTypeName = new XmlQualifiedName ("integer", XmlSchema.Namespace);
			schema.Items.Add (elem);

			// Lap 1:
			
			xvr = PrepareXmlReader (xml);
			xvr.Schemas.Add (schema);
			// Read directly from root.
			object o = xvr.ReadTypedValue ();
			AssertEquals (ReadState.Initial, xvr.ReadState);
			AssertNull (o);

			xvr.Read ();	// element root
			AssertEquals (XmlNodeType.Element, xvr.NodeType);
			AssertNotNull (xvr.SchemaType);
			Assert (xvr.SchemaType is XmlSchemaDatatype);
			o = xvr.ReadTypedValue ();	// read "12"
			AssertEquals (XmlNodeType.EndElement, xvr.NodeType);
			AssertNotNull (o);
			AssertEquals (typeof (decimal), o.GetType ());
			decimal n = (decimal) o;
			AssertEquals (12, n);
			Assert (!xvr.EOF);
			AssertEquals ("root", xvr.Name);
			AssertNull (xvr.SchemaType);	// EndElement's type

			// Lap 2:

			xvr = PrepareXmlReader (xml);
			xvr.Schemas.Add (schema);
			xvr.Read ();	// root
			XmlSchemaDatatype dt = xvr.SchemaType as XmlSchemaDatatype;
			AssertNotNull (dt);
			AssertEquals (typeof (decimal), dt.ValueType);
			AssertEquals (XmlTokenizedType.None, dt.TokenizedType);
			xvr.Read ();	// text "12"
			AssertNull (xvr.SchemaType);
			o = xvr.ReadTypedValue ();
			// ReadTypedValue is different from ReadString().
			AssertNull (o);
		}

		[Test]
		[Ignore ("XML Schema validator should not be available for validating non namespace-aware XmlReader that handled colon as a name character")]
		public void TestNamespacesFalse ()
		{
			// This tests if Namespaces=false is specified, then
			// the reader's NamespaceURI should be always string.Empty and
			// validation should be done against such schema that has target ns as "".
			string xml = "<x:root xmlns:x='urn:foo' />";
			xvr = PrepareXmlReader (xml);
			xvr.Namespaces = false;
			AssertEquals (ValidationType.Auto, xvr.ValidationType);
			XmlSchema schema = new XmlSchema ();
			schema.TargetNamespace = "urn:foo";
			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = "root";
			schema.Items.Add (elem);
			xvr.Schemas.Add (schema);
			xvr.Read ();	// root
			Assert (!xvr.Namespaces);
			AssertEquals ("x:root", xvr.Name);
			// LocalName may contain colons.
			AssertEquals ("x:root", xvr.LocalName);
			// NamespaceURI is not supplied.
			AssertEquals ("", xvr.NamespaceURI);
		}

		[Test]
		public void TestReadTypedAttributeValue ()
		{
			string xml = "<root attr='12'></root>";
			XmlSchema schema = new XmlSchema ();
			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = "root";
			XmlSchemaComplexType ct = new XmlSchemaComplexType ();
			XmlSchemaAttribute attr = new XmlSchemaAttribute ();
			attr.Name = "attr";
			attr.SchemaTypeName = new XmlQualifiedName ("int", XmlSchema.Namespace);
			ct.Attributes.Add (attr);
			elem.SchemaType = ct;
			schema.Items.Add (elem);

			xvr = PrepareXmlReader (xml);
			xvr.Schemas.Add (schema);
			xvr.Read ();
			AssertEquals ("root", xvr.Name);
			Assert (xvr.MoveToNextAttribute ());	// attr
			AssertEquals ("attr", xvr.Name);
			XmlSchemaDatatype dt = xvr.SchemaType as XmlSchemaDatatype;
			AssertNotNull (dt);
			AssertEquals (typeof (int), dt.ValueType);
			AssertEquals (XmlTokenizedType.None, dt.TokenizedType);
			object o = xvr.ReadTypedValue ();
			AssertEquals (XmlNodeType.Attribute, xvr.NodeType);
			AssertEquals (typeof (int), o.GetType ());
			int n = (int) o;
			AssertEquals (12, n);
			Assert (xvr.ReadAttributeValue ());	// can read = seems not proceed.
		}
	}
}
