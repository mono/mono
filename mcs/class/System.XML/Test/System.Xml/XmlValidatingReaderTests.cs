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

		XmlReader xtr;
		XmlValidatingReader dvr;

		private XmlReader PrepareXmlReader (string xml)
		{
			return new XmlTextReader (xml, XmlNodeType.Document, null);
//			XmlDocument doc = new XmlDocument ();
//			doc.LoadXml (xml);
//			return new XmlNodeReader (doc);
		}

		[Test]
		public void TestSingleElement ()
		{
			string intSubset = "<!ELEMENT root EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root />";
			xtr = PrepareXmlReader (xml1);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();

			string xml2 = dtd + "<invalid />";
			xtr = PrepareXmlReader (xml2);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// invalid element.
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			string xml3 = dtd + "<root>invalid PCDATA.</root>";
			xtr = PrepareXmlReader (xml3);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid text
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			string xml4 = dtd + "<root><invalid_child /></root>";
			xtr = PrepareXmlReader (xml4);
			dvr = new XmlValidatingReader (xtr);
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
			string xml1 = dtd + "<root />";
			xtr = PrepareXmlReader (xml1);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// root: invalid end
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			string xml2 = dtd + "<root>Test.</root>";
			xtr = PrepareXmlReader (xml2);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid end
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			string xml3 = dtd + "<root><foo /></root>";
			xtr = PrepareXmlReader (xml3);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo

			string xml4 = dtd + "<root><bar /></root>";
			xtr = PrepareXmlReader (xml4);
			dvr = new XmlValidatingReader (xtr);
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
			string xml1 = dtd + "<root />";
			xtr = PrepareXmlReader (xml1);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end

			string xml2 = dtd + "<root>Test.</root>";
			xtr = PrepareXmlReader (xml2);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid PCDATA
			dvr.Read ();	// endelement root

			string xml3 = dtd + "<root><foo/>Test.<foo></foo></root>";
			xtr = PrepareXmlReader (xml3);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid foo
			dvr.Read ();	// valid #PCDATA
			dvr.Read ();	// valid foo
			dvr.Read ();	// valid endElement foo
			dvr.Read ();	// valid endElement root

			string xml4 = dtd + "<root>Test.<bar /></root>";
			xtr = PrepareXmlReader (xml4);
			dvr = new XmlValidatingReader (xtr);
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
			string xml1 = dtd + "<root><foo/><bar/></root>";
			xtr = PrepareXmlReader (xml1);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			string xml2 = dtd + "<root><foo/></root>";
			xtr = PrepareXmlReader (xml2);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid end root
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			string xml3 = dtd + "<root><bar/></root>";
			xtr = PrepareXmlReader (xml3);
			dvr = new XmlValidatingReader (xtr);
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
			string xml1 = dtd + "<root><foo/><bar/></root>";
			xtr = PrepareXmlReader (xml1);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid element bar
				Fail ("should be failed.");
			} catch (XmlSchemaException) {
			}

			string xml2 = dtd + "<root><foo/></root>";
			xtr = PrepareXmlReader (xml2);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			string xml3 = dtd + "<root><bar/></root>";
			xtr = PrepareXmlReader (xml3);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			string xml4 = dtd + "<root><foo/>text.<bar/></root>";
			xtr = PrepareXmlReader (xml4);
			dvr = new XmlValidatingReader (xtr);
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
			string xml1 = dtd + "<root />";
			xtr = PrepareXmlReader (xml1);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// empty root.
			dvr.Read ();	// end of document.

			string xml2 = dtd + "<root><foo/></root>";
			xtr = PrepareXmlReader (xml2);
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			string xml3 = dtd + "<root><foo /><foo></foo><foo/></root>";
			xtr = PrepareXmlReader (xml3);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// foo
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			string xml4 = dtd + "<root><bar /></root>";
			xtr = PrepareXmlReader (xml4);
			dvr = new XmlValidatingReader (xtr);
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
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			xml = dtd + "<root><foo/><baz/></root>";
			xtr = PrepareXmlReader (xml);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root
		}

		[Test]
		public void TestAttributes ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA #REQUIRED bar CDATA #IMPLIED>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root />";
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// missing attributes
				Fail ("should be failed.");
			} catch (XmlSchemaException ex) {
			}

			xml = dtd + "<root foo='value' />";
			xtr = PrepareXmlReader (xml);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end of document

			xml = dtd + "<root foo='value' bar='2nd' />";
			xtr = PrepareXmlReader (xml);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end of document

			xml = dtd + "<root foo='value' bar='2nd' baz='3rd' />";
			xtr = PrepareXmlReader (xml);
			dvr = new XmlValidatingReader (xtr);
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
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			dvr.ValidationType = ValidationType.DTD;
			dvr.Read ();	// DTD
			dvr.Read ();
			AssertEquals (XmlNodeType.Element, dvr.NodeType);
			AssertEquals ("root", dvr.Name);
			Assert (dvr.MoveToFirstAttribute ());
			AssertEquals ("foo", dvr.Name);
			AssertEquals ("foo-def", dvr.Value);
			Assert (dvr.MoveToNextAttribute ());
			AssertEquals ("bar", dvr.Name);
			AssertEquals ("bar-def", dvr.Value);
		}

		[Test]
		public void TestValidationEvent ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<foo><bar att='val' /></foo>";
			eventFired = false;
			dvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
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
		}

		private bool eventFired;
		private void OnInvalidityFound (object o, ValidationEventArgs e)
		{
			eventFired = true;
		}
	}
}
