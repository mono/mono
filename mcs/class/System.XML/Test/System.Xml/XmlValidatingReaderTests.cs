//
// MonoTests.System.Xml.XmlValidatingReaderTests.cs
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
	public class XmlValidatingReaderTests : Assertion
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
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root>invalid PCDATA.</root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid text
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><invalid_child /></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid child
				Fail ("should be failed.");
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
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root>Test.</root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid end
				Fail ("should be failed.");
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
				Fail ("should be failed.");
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
				Fail ("should be failed.");
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
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><bar/></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid element bar
				Fail ("should be failed.");
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
				Fail ("should be failed.");
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
				Fail ("should be failed.");
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
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}
		}

		[Test]
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
				Fail ("should be failed."); // MS.NET fails to fail this test.
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
				Fail ("should be failed.");
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
				Fail ("should be failed.");
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
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);
			AssertEquals (2, dvr.AttributeCount);
			// foo
			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			AssertEquals ("foo-def", dvr ["foo"]);
			AssertNotNull (dvr ["bar"]);
			AssertEquals ("foo-def", dvr.Value);
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals ("", dvr.Name);
			AssertEquals ("foo-def", dvr.Value);
			// bar
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("foo-def", dvr ["foo"]);
			AssertNotNull (dvr ["bar"]);
			AssertEquals ("bar-def", dvr.Value);
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals ("", dvr.Name);
			AssertEquals ("bar-def", dvr.Value);
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
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);
			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			AssertEquals ("entity string", dvr.Value);
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("internal entity string value", dvr.Value);
			AssertEquals ("entity string", dvr.ReadString ());

			// ValidationType = None

			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandEntities;
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			dvr.Read ();
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);

			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			AssertEquals ("entity string", dvr.Value);

			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("internal entity string value", dvr.Value);
			AssertEquals ("entity string", dvr.ReadString ());
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
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);
			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			// MS BUG: it returns "entity string", however, entity should not be exanded.
			AssertEquals ("&ent;", dvr.Value);
			//  ReadAttributeValue()
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			AssertEquals ("ent", dvr.Name);
			AssertEquals ("", dvr.Value);
			Assert (!dvr.ReadAttributeValue ());

			// bar
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("internal &ent; value", dvr.Value);
			//  ReadAttributeValue()
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals ("", dvr.Name);
			AssertEquals ("internal ", dvr.Value);
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			AssertEquals ("ent", dvr.Name);
			AssertEquals ("", dvr.Value);
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals ("", dvr.Name);
			AssertEquals (" value", dvr.Value);

			// ValidationType = None

			dvr = PrepareXmlReader (xml);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			dvr.Read ();
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);

			// foo
			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			AssertEquals ("&ent;", dvr.Value);
			//  ReadAttributeValue()
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			AssertEquals ("ent", dvr.Name);
			AssertEquals ("", dvr.Value);
			Assert (!dvr.ReadAttributeValue ());

			// bar
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("internal &ent; value", dvr.Value);
			//  ReadAttributeValue()
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals ("", dvr.Name);
			AssertEquals ("internal ", dvr.Value);
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			AssertEquals ("ent", dvr.Name);
			AssertEquals ("", dvr.Value);
			Assert (dvr.ReadAttributeValue ());
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals ("", dvr.Name);
			AssertEquals (" value", dvr.Value);
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
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);
			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			AssertEquals ("id1", dvr.Value);
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("nameToken", dvr.Value);
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("baz", dvr.Name);
			AssertEquals ("list of name token", dvr.Value);
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("quux", dvr.Name);
			AssertEquals (" quuux quuux  ", dvr.Value);
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
			Assert (dvr.Read ());	// invalid foo
			Assert (eventFired);
			AssertEquals ("foo", dvr.Name);
			Assert (dvr.Read ());	// invalid bar
			AssertEquals ("bar", dvr.Name);
			Assert (dvr.MoveToFirstAttribute ());	// att
			AssertEquals ("att", dvr.Name);
			Assert (dvr.Read ());	// invalid end foo
			AssertEquals ("foo", dvr.Name);
			AssertEquals (XmlNodeType.EndElement, dvr.NodeType);
			Assert (!dvr.Read ());

			// When ValidationType is None, event should not be fired,
			eventFired = false;
			dvr = PrepareXmlReader (xml);
			dvr.ValidationEventHandler += new ValidationEventHandler (OnInvalidityFound);
			dvr.ValidationType = ValidationType.None;
			dvr.Read ();	// DTD
			Assert (dvr.Read ());	// invalid foo
			Assert (!eventFired);
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
				Fail ();
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
				Fail ();
			} catch (XmlSchemaException) {
			}

			xml = dtd + "<root><c foo='val' /><c baz='val val val 1 2 3'></root>";
			dvr = PrepareXmlReader (xml);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// c[1]
			try {
				dvr.Read ();	// c[2]
				Fail ();
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
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			AssertEquals (1, dvr.Depth);
			dvr.ResolveEntity ();
			// It is still entity reference.
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			dvr.Read ();
			AssertEquals (XmlNodeType.Text, dvr.NodeType);
			AssertEquals (2, dvr.Depth);
			AssertEquals ("entity string", dvr.Value);
			dvr.Read ();
			AssertEquals (XmlNodeType.EndEntity, dvr.NodeType);
			AssertEquals (1, dvr.Depth);
			AssertEquals ("", dvr.Value);

			dvr.Read ();	// &ent2;
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			AssertEquals (1, dvr.Depth);
			dvr.ResolveEntity ();
			// It is still entity reference.
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			// It now became element node.
			dvr.Read ();
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals (2, dvr.Depth);
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
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
#if NET_2_0
			// under .NET 2.0, an error is raised here.
			// under .NET 1.1, the error is thrown on the next read.
			try {
				dvr.ResolveEntity ();
				Fail ("Attempt to resolve undeclared entity should fail.");
			} catch (XmlException) {
			}
#else
			// ent3 does not exist in this dtd.
			dvr.ResolveEntity ();
			AssertEquals (XmlNodeType.EntityReference, dvr.NodeType);
			try {
				dvr.Read ();
				Fail ("Attempt to resolve undeclared entity should fail.");
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
	}
}
