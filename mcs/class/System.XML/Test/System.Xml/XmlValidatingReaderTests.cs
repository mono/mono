//
// MonoTests.System.Xml.XmlValidatingReaderTests.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlValidatingReaderTests
	{
		public XmlValidatingReaderTests ()
		{
		}

		XmlValidatingReader dvr;

		private XmlValidatingReader PrepareXmlReader (string xml)
		{
			XmlReader reader = new XmlTextReader (xml, XmlNodeType.Document, null);
//			XmlDocument doc = new XmlDocument ();
//			doc.LoadXml (xml);
//			XmlReader reader = new XmlNodeReader (doc);

			return new XmlValidatingReader (reader);
		}

		[Test]
		public void TestSingleElement ()
		{
			string intSubset = "<!ELEMENT root EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();

			xml = dtd + "<invalid />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// invalid element.
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root>invalid PCDATA.</root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid text
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><invalid_child /></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid child
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestElementContent ()
		{
			string intSubset = "<!ELEMENT root (foo)><!ELEMENT foo EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// root: invalid end
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root>Test.</root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid end
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><foo /></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo

			xml = dtd + "<root><bar /></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid element
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestMixedContent ()
		{
			string intSubset = "<!ELEMENT root (#PCDATA | foo)*><!ELEMENT foo EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end

			xml = dtd + "<root>Test.</root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid PCDATA
			dvr.Read ();	// endelement root

			xml = dtd + "<root><foo/>Test.<foo></foo></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid foo
			dvr.Read ();	// valid #PCDATA
			dvr.Read ();	// valid foo
			dvr.Read ();	// valid endElement foo
			dvr.Read ();	// valid endElement root

			xml = dtd + "<root>Test.<bar /></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid #PCDATA
			try {
				dvr.Read ();	// invalid element
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestSequence ()
		{
			string intSubset = "<!ELEMENT root (foo, bar)><!ELEMENT foo EMPTY><!ELEMENT bar EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root><foo/><bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			xml = dtd + "<root><foo/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid end root
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid element bar
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestChoice ()
		{
			string intSubset = "<!ELEMENT root (foo|bar)><!ELEMENT foo EMPTY><!ELEMENT bar EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root><foo/><bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid element bar
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><foo/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			xml = dtd + "<root><bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			xml = dtd + "<root><foo/>text.<bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid text
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestAny ()
		{
			string intSubset = "<!ELEMENT root ANY><!ELEMENT foo EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// empty root.
			dvr.Read ();	// end of document.

			xml = dtd + "<root><foo/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			xml = dtd + "<root><foo /><foo></foo><foo/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// foo
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			xml = dtd + "<root><bar /></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// bar: invalid (undeclared) element
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		[Category ("NotDotNet")]
		// MS fails to validate nondeterministic content validation.
		public void TestNonDeterministicContent ()
		{
			string intSubset = "<!ELEMENT root ((foo, bar)|(foo,baz))><!ELEMENT foo EMPTY><!ELEMENT bar EMPTY><!ELEMENT baz EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root><foo/><bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			xml = dtd + "<root><foo/><baz/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestAttributes ()
		{
			// simple content and attributes are required
			string intSubset = "<!ELEMENT root (foo)><!ELEMENT foo EMPTY><!ATTLIST root foo CDATA #REQUIRED bar CDATA #IMPLIED>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root><foo/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// missing attributes
				Assert.Fail ("should be failed."); // MS.NET fails to fail this test.
			} catch (XmlSchemaException) {
			}

			// empty element but attributes are required
			intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA #REQUIRED bar CDATA #IMPLIED>";
			dtd = "<!DOCTYPE root [" + intSubset + "]>";
			xml = dtd + "<root />";
			dvr = PrepareXmlReader (xml);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// missing attributes
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root foo='value' />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end of document

			xml = dtd + "<root foo='value' bar='2nd' />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end of document

			xml = dtd + "<root foo='value' bar='2nd' baz='3rd' />";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// undeclared attribute baz
				Assert.Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
		public void TestAttributeDefaultContribution ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root />";

			dvr = PrepareXmlReader (xml);
			dvr.ValidationType = ValidationType.DTD;
			this.TestAttributeDefaultContributionInternal (dvr);

			dvr = PrepareXmlReader (xml);
			dvr.ValidationType = ValidationType.None;
			this.TestAttributeDefaultContributionInternal (dvr);
		}

		private void TestAttributeDefaultContributionInternal (XmlReader dvr)
		{
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);
			Assert.AreEqual (2, dvr.AttributeCount);
			// foo
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual ("foo-def", dvr ["foo"]);
			Assert.IsNotNull (dvr ["bar"]);
			Assert.AreEqual ("foo-def", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual ("foo-def", dvr.Value);
			// bar
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("foo-def", dvr ["foo"]);
			Assert.IsNotNull (dvr ["bar"]);
			Assert.AreEqual ("bar-def", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual ("bar-def", dvr.Value);
		}

		[Test]
		public void TestExpandEntity ()
		{
			string intSubset = "<!ELEMENT root (#PCDATA)><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'><!ENTITY ent 'entity string'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root foo='&ent;' bar='internal &ent; value'>&ent;</root>";
			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandEntities;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual ("entity string", dvr.Value);
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("internal entity string value", dvr.Value);
			Assert.AreEqual ("entity string", dvr.ReadString ());

			// ValidationType = None

			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandEntities;
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);

			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual ("entity string", dvr.Value);

			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("internal entity string value", dvr.Value);
			Assert.AreEqual ("entity string", dvr.ReadString ());
		}

		[Test]
		public void TestPreserveEntity ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'><!ENTITY ent 'entity string'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root foo='&ent;' bar='internal &ent; value' />";
			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			// MS BUG: it returns "entity string", however, entity should not be exanded.
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (!dvr.ReadAttributeValue ());

			// bar
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual ("internal ", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual (" value", dvr.Value);

			// ValidationType = None

			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);

			// foo
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (!dvr.ReadAttributeValue ());

			// bar
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual ("internal ", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual (" value", dvr.Value);
		}

		[Test]
		// it used to be regarded as MS bug but it was not really.
		public void TestPreserveEntityNotOnDotNet ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'><!ENTITY ent 'entity string'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root foo='&ent;' bar='internal &ent; value' />";
			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual ("entity string", dvr.Value);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (!dvr.ReadAttributeValue ());

			// bar
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("internal entity string value", dvr.Value);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual ("internal ", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual (" value", dvr.Value);

			// ValidationType = None

			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);

			// foo
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual ("entity string", dvr.Value);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (!dvr.ReadAttributeValue ());

			// bar
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("internal entity string value", dvr.Value);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual ("internal ", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual (String.Empty, dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (String.Empty, dvr.Name);
			Assert.AreEqual (" value", dvr.Value);
		}

		[Test]
		public void TestNormalization ()
		{
			string intSubset = "<!ELEMENT root EMPTY>"
				+ "<!ATTLIST root foo ID #REQUIRED"
				+ " bar NMTOKEN #IMPLIED "
				+ " baz NMTOKENS #IMPLIED "
				+ " quux CDATA #IMPLIED >";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root foo=' id1 ' bar='  nameToken  ' baz=' list  of\r\nname token' quux=' quuux\tquuux\t ' />";
			dvr = PrepareXmlReader (xml);
			((XmlTextReader) dvr.Reader).Normalization = true;
			dvr.EntityHandling = EntityHandling.ExpandEntities;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual ("id1", dvr.Value);
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("nameToken", dvr.Value);
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("baz", dvr.Name);
			Assert.AreEqual ("list of name token", dvr.Value);
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("quux", dvr.Name);
			Assert.AreEqual (" quuux quuux  ", dvr.Value);
		}

		[Test]
		public void TestValidationEvent ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<foo><bar att='val' /></foo>";
			eventFired = false;
			dvr = PrepareXmlReader (xml);
			dvr.ValidationEventHandler += new ValidationEventHandler (OnInvalidityFound);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			Assert.IsTrue (dvr.Read ());	// invalid foo
			Assert.IsTrue (eventFired);
			Assert.AreEqual ("foo", dvr.Name);
			Assert.IsTrue (dvr.Read ());	// invalid bar
			Assert.AreEqual ("bar", dvr.Name);
			Assert.IsTrue (dvr.MoveToFirstAttribute ());	// att
			Assert.AreEqual ("att", dvr.Name);
			Assert.IsTrue (dvr.Read ());	// invalid end foo
			Assert.AreEqual ("foo", dvr.Name);
			Assert.AreEqual (XmlNodeType.EndElement, dvr.NodeType);
			Assert.IsTrue (!dvr.Read ());

			// When ValidationType is None, event should not be fired,
			eventFired = false;
			dvr = PrepareXmlReader (xml);
			dvr.ValidationEventHandler += new ValidationEventHandler (OnInvalidityFound);
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			Assert.IsTrue (dvr.Read ());	// invalid foo
			Assert.IsTrue (!eventFired);
		}

		private bool eventFired;
		private void OnInvalidityFound (object o, ValidationEventArgs e)
		{
			eventFired = true;
		}

		[Test]
		public void TestIdentityConstraints ()
		{
			string intSubset = "<!ELEMENT root (c)+><!ELEMENT c EMPTY><!ATTLIST root foo ID #REQUIRED>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root><c foo='val' /><c foo='val'></root>";
			dvr = PrepareXmlReader (xml);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// root misses attribute foo
				Assert.Fail ();
			} catch (XmlSchemaException) {
			}

			intSubset = "<!ELEMENT root (c)+><!ELEMENT c EMPTY><!ATTLIST c foo ID #REQUIRED bar IDREF #IMPLIED baz IDREFS #IMPLIED>";
			dtd = "<!DOCTYPE root [" + intSubset + "]>";
			xml = dtd + "<root><c foo='val' /><c foo='val'></root>";
			dvr = PrepareXmlReader (xml);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// c[1]
			try {
				dvr.Read ();	// c[2]
				Assert.Fail ();
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><c foo='val' /><c baz='val val val 1 2 3'></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// c[1]
			try {
				dvr.Read ();	// c[2]
				Assert.Fail ();
			} catch (XmlSchemaException) {
			}
		}

		// Entity tests are almost copied from XmlNodeReaderTests.
		[Test]
		public void ResolveEntity ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent;&ent2;</root>";
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			dvr.ValidationType = ValidationType.None;
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// &ent;
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual (1, dvr.Depth);
			dvr.ResolveEntity ();
			// It is still entity reference.
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual (2, dvr.Depth);
			Assert.AreEqual ("entity string", dvr.Value);
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.EndEntity, dvr.NodeType);
			Assert.AreEqual (1, dvr.Depth);
			Assert.AreEqual (String.Empty, dvr.Value);

			dvr.Read ();	// &ent2;
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual (1, dvr.Depth);
			dvr.ResolveEntity ();
			// It is still entity reference.
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			// It now became element node.
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual (2, dvr.Depth);
		}

		[Test]
		public void ResolveEntity2 ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent3;&ent2;</root>";
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			dvr.ValidationType = ValidationType.None;
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// &ent3;
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
#if NET_2_0
			// under .NET 2.0, an error is raised here.
			// under .NET 1.1, the error is thrown on the next read.
			try {
				dvr.ResolveEntity ();
				Assert.Fail ("Attempt to resolve undeclared entity should fail.");
			} catch (XmlException) {
			}
#else
			// ent3 does not exist in this dtd.
			dvr.ResolveEntity ();
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			try {
				dvr.Read ();
				Assert.Fail ("Attempt to resolve undeclared entity should fail.");
			} catch (XmlException) {
			}
#endif
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ResolveEntityWithoutDTD ()
		{
			string xml = "<root>&ent;&ent2;</root>";
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			dvr.Read ();	// root
			dvr.Read ();	// &ent;
		}

		[Test]
		public void ResolveEntityReadAttributeValue ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root (#PCDATA)*><!ATTLIST root attr CDATA #REQUIRED><!ENTITY ent 'entity string'>]>";
			string xml = dtd + "<root attr='&ent; text'>&ent;</root>";
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			dvr.Read (); // doctype
			dvr.Read (); // root
			dvr.MoveToAttribute (0); // attr
			Assert.IsTrue (dvr.ReadAttributeValue ()); // Should read expanded text
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType); // not EntityReference
			Assert.AreEqual ("entity string text", dvr.Value);
			Assert.IsTrue (!dvr.ReadAttributeValue ());
		}

		[Test]
		public void ResolveEntitySequentialText ()
		{
			string xml = @"<!DOCTYPE doc [
				<!ELEMENT doc ANY>
				<!ELEMENT foo  ANY>
				<!ENTITY ref1 '<![CDATA[cdata]]>test'>
				]>
				<doc><foo>&ref1; test </foo></doc>";
			string refOut = "<doc><foo><![CDATA[cdata]]>test test </foo></doc>";

			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			XmlValidatingReader r = new XmlValidatingReader (xtr);
			r.Read ();
			r.Read ();
			r.Read ();
			Assert.AreEqual (refOut, r.ReadOuterXml ());
		}

		[Test]
		// imported testcase from sys.security which had regression.
		public void ResolveEntityAndBaseURI ()
		{
			string world = Path.Combine (Path.GetTempPath (), "world.txt");
			string dtd = Path.Combine (Path.GetTempPath (), "doc.dtd");
			try {
				using (TextWriter w = File.CreateText (world)) {
					w.WriteLine ("world");
				}
				using (TextWriter w = File.CreateText (dtd)) {
					w.WriteLine ("<!-- dummy -->");
				}

				string xml = String.Format ("<!DOCTYPE doc SYSTEM \"{1}\" [\n" +
					"<!ATTLIST doc attrExtEnt ENTITY #IMPLIED>\n" +
					"<!ENTITY ent1 \"Hello\">\n" +
					"<!ENTITY ent2 SYSTEM \"{0}\">\n" +
					"<!ENTITY entExt SYSTEM \"earth.gif\" NDATA gif>\n" +
					"<!NOTATION gif SYSTEM \"viewgif.exe\">\n" +
					"]>\n" +
					"<doc attrExtEnt=\"entExt\">\n" +
					"   &ent1;, &ent2;!\n" +
					"</doc>\n" +
					"\n" +
					"<!-- Let world.txt contain \"world\" (excluding the quotes) -->\n",
					world, dtd);

				XmlValidatingReader xvr =
					new XmlValidatingReader (
					xml, XmlNodeType.Document, null);
				xvr.ValidationType = ValidationType.None;
				xvr.EntityHandling =
					EntityHandling.ExpandCharEntities;
				XmlDocument doc = new XmlDocument ();
				doc.Load (xvr);

			} finally {
				if (File.Exists (world))
					File.Delete (world);
				if (File.Exists (dtd))
					File.Delete (dtd);
			}
		}

		[Test]
		//[NotWorking ("default namespace seems null, not String.Empty")]
#if NET_2_0
#else
		// MS.NET 1.x does not consider cases that xmlns* attributes
		// could be declared as default.
		[Category ("NotDotNet")]
#endif
		public void DefaultXmlnsAttributeLookup ()
		{
			string xml = @"<!DOCTYPE X [
			<!ELEMENT X (Y)+>
			<!ENTITY baz 'urn:baz'>
			<!ATTLIST X
				xmlns CDATA 'urn:foo'
				xmlns:bar CDATA 'urn:bar'
				xmlns:baz CDATA #IMPLIED
				dummy CDATA 'dummy'
				baz:dummy CDATA 'dummy'>
			<!ELEMENT Y (#PCDATA)*>
			<!ATTLIST Y
				xmlns CDATA #IMPLIED
				xmlns:bar CDATA #IMPLIED>
			]>
			<X xmlns:baz='&baz;'><Y/><Y>text.</Y><Y xmlns='' xmlns:bar='urn:hoge'>text.</Y></X>";
			XmlValidatingReader xvr = new XmlValidatingReader (
				xml, XmlNodeType.Document, null);
			xvr.Read (); // DTD
			xvr.Read (); // whitespace
			xvr.Read ();
			Assert.AreEqual ("urn:foo", xvr.LookupNamespace (String.Empty), "#1-1");
			Assert.AreEqual ("urn:bar", xvr.LookupNamespace ("bar"), "#1-2");

			Assert.AreEqual ("urn:baz", xvr.LookupNamespace ("baz"), "#1-a");
			Assert.IsTrue (xvr.MoveToAttribute ("baz:dummy"), "#1-b");
			Assert.AreEqual ("urn:baz", xvr.NamespaceURI, "#1-c");

			Assert.IsTrue (xvr.MoveToAttribute ("dummy"), "#1-d");
			Assert.AreEqual (String.Empty, xvr.NamespaceURI, "#1-e");

			xvr.Read (); // first Y, empty element
			Assert.AreEqual ("urn:foo", xvr.LookupNamespace (String.Empty), "#2-1");
			Assert.AreEqual ("urn:bar", xvr.LookupNamespace ("bar"), "#2-2");
			xvr.Read (); // second Y, start element
			Assert.AreEqual ("urn:foo", xvr.LookupNamespace (String.Empty), "#3-1");
			Assert.AreEqual ("urn:bar", xvr.LookupNamespace ("bar"), "#3-2");
			xvr.Read (); // inside normal Y. Check inheritance
			Assert.AreEqual ("urn:foo", xvr.LookupNamespace (String.Empty), "#4-1");
			Assert.AreEqual ("urn:bar", xvr.LookupNamespace ("bar"), "#4-2");
			xvr.Read (); // second Y, end element
			Assert.AreEqual ("urn:foo", xvr.LookupNamespace (String.Empty), "#5-1");
			Assert.AreEqual ("urn:bar", xvr.LookupNamespace ("bar"), "#5-2");
			xvr.Read (); // third Y, suppresses default namespaces
			Assert.AreEqual (null, xvr.LookupNamespace (String.Empty), "#6-1");
			Assert.AreEqual ("urn:hoge", xvr.LookupNamespace ("bar"), "#6-2");
			xvr.Read (); // inside suppressing Y. Check inheritance
			Assert.AreEqual (null, xvr.LookupNamespace (String.Empty), "#7-1");
			Assert.AreEqual ("urn:hoge", xvr.LookupNamespace ("bar"), "#7-2");
			xvr.Read (); // end of suppressing element
			Assert.AreEqual (null, xvr.LookupNamespace (String.Empty), "#8-1");
			Assert.AreEqual ("urn:hoge", xvr.LookupNamespace ("bar"), "#8-2");
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (XmlSchemaException))]
		public void Bug80231 ()
		{
			string xml = "<!DOCTYPE file [<!ELEMENT file EMPTY><!ATTLIST file name CDATA #REQUIRED>]><file name=\"foo\" bar=\"baz\" />";
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ProhibitDtd = false;
			settings.ValidationType = ValidationType.DTD;
			XmlReader r = XmlReader.Create (new StringReader (xml), settings);
			while (!r.EOF)
				r.Read ();
		}
#endif

#if NET_2_0		
		[Test]		
		public void Bug501814 ()
		{
			string xsd = @"
			<xs:schema id='Layout'
				targetNamespace='foo'
				elementFormDefault='qualified'
				xmlns='foo'                  
				xmlns:xs='http://www.w3.org/2001/XMLSchema'>

				<xs:element name='Layout' type='Layout' />

				<xs:complexType name='Layout'>
					<xs:group ref='AnyLayoutElement' minOccurs='0' maxOccurs='unbounded' />
				</xs:complexType>

				<xs:group name='AnyLayoutElement'>
					<xs:choice>			
						<xs:element name='Label' type='Label' />			
					</xs:choice>
				</xs:group>
	
				<xs:complexType name='LayoutElement' abstract='true'>
					<xs:attribute name='id' type='xs:ID' use='optional' />
					<xs:attribute name='visible' type='xs:boolean' use='optional' default='true' />
				</xs:complexType>
	
				<xs:complexType name='Label'>
					<xs:complexContent mixed='true'>
						<xs:extension base='LayoutElement'>
						<xs:attribute name='bold' type='xs:boolean' use='required'/>
						</xs:extension>
					</xs:complexContent>
					</xs:complexType>
			</xs:schema>";
			
			XmlDocument doc = new XmlDocument ();
			
			XmlSchema schema = XmlSchema.Read (XmlReader.Create (new StringReader (xsd)), null);			
			
			doc.LoadXml (@"
				<Layout xmlns='foo'>
	            <Label bold='false'>Text inside label</Label>
                </Layout>");
			doc.Schemas.Add (schema);
			doc.Validate (null);
		}
#endif
		
#if NET_2_0
		[Test]		
		public void Bug502168 ()
		{
			string xsd = @"<xs:schema id='Layout'
				targetNamespace='foo'
				elementFormDefault='qualified'
				xmlns='foo'                  
				xmlns:xs='http://www.w3.org/2001/XMLSchema'>

				<xs:element name='Layout' type='Layout' />
				
				 <xs:complexType name='Layout'>
				  <xs:group ref='AnyLayoutElement' minOccurs='0' maxOccurs='unbounded' />
				 </xs:complexType>
				
				 <xs:group name='AnyLayoutElement'>
				  <xs:choice>
				   <xs:element name='Layout' type='Layout' />   
				   <xs:element name='ImageContainer' type='ImageContainer' />
				   <xs:element name='VideoInstance' type='VideoInstance'/>
				  </xs:choice>
				 </xs:group>
				
				 <xs:complexType name='ImageDummy'>
				 </xs:complexType>
				
				 <xs:complexType name='LayoutElement' abstract='true'>  
				 </xs:complexType>
				
				 <xs:group name='AnyImageElement'>
				  <xs:choice>
				   <xs:element name='ImageDummy' type='ImageDummy' />
				  </xs:choice>
				 </xs:group>
				
				 <xs:complexType name='ImageContainer'>
				  <xs:complexContent>
				   <xs:extension base='LayoutElement'>
				    <xs:choice minOccurs='1' maxOccurs='1'>
				     <xs:element name='Content' type='SingleImage' minOccurs='1' maxOccurs='1'
				nillable='false'/>
				    </xs:choice>    
				   </xs:extension>
				  </xs:complexContent>
				 </xs:complexType>
				
				 <xs:complexType name='SingleImage'>
				  <xs:group ref='AnyImageElement' minOccurs='1' maxOccurs='1'/>
				 </xs:complexType>
				
				 <xs:complexType name='VideoApplicationFile'>
				  <xs:complexContent>
				   <xs:extension base='VideoInstance'>
				    <xs:attribute name='fileName' type='xs:string' use='optional'/>
				   </xs:extension>
				  </xs:complexContent>
				 </xs:complexType>
				
				 <xs:complexType abstract='true' name='Video'>
				  <xs:complexContent>
				   <xs:extension base='LayoutElement'>
				    <xs:group ref='AnyImageElement' minOccurs='0' maxOccurs='1'/>    
				   </xs:extension>
				  </xs:complexContent>
				 </xs:complexType>
				
				 <xs:complexType abstract='true' name='VideoInstance'>
				  <xs:complexContent>
				   <xs:extension base='Video'>
				    <xs:attribute name='name' type='xs:string' use='optional'/>
				   </xs:extension>
				  </xs:complexContent>
				 </xs:complexType>
				</xs:schema>";


			XmlDocument doc = new XmlDocument ();
			XmlSchema schema = XmlSchema.Read (XmlReader.Create (new StringReader (xsd)), null);
			doc.LoadXml (@"<Layout xmlns='foo' />");
			doc.Schemas.Add(schema);
			doc.Validate(null);
		}
#endif		
	}
}
