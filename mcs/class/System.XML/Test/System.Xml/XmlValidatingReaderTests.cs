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
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlValidatingReaderTests : Assertion
	{
		public XmlValidatingReaderTests ()
		{
		}

		XmlTextReader xtr;
		XmlValidatingReader dvr;

		[Test]
		public void TestSingleElement ()
		{
			string intSubset = "<!ELEMENT root EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root />";
			xtr = new XmlTextReader (xml1, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();

			string xml2 = dtd + "<invalid />";
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// invalid element.
				Fail ("should be failed.");
			} catch (XmlException ex) {
				if (!ex.Message.StartsWith ("Invalid start element"))
					throw ex;
			}

			string xml3 = dtd + "<root>invalid PCDATA.</root>";
			xtr = new XmlTextReader (xml3, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid text
				Fail ("should be failed.");
			} catch (XmlException ex) {
				if (!ex.Message.StartsWith ("Current element root does not allow"))
					throw ex;
			}

			string xml4 = dtd + "<root><invalid_child /></root>";
			xtr = new XmlTextReader (xml4, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid child
				Fail ("should be failed.");
			} catch (XmlException ex) {
				if (!ex.Message.StartsWith ("Invalid start element"))
					throw ex;
			}
		}

		[Test]
		public void TestElementContent ()
		{
			string intSubset = "<!ELEMENT root (foo)><!ELEMENT foo EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root />";
			xtr = new XmlTextReader (xml1, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// root: invalid end
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Invalid end element"))
					throw ex;
			}

			string xml2 = dtd + "<root>Test.</root>";
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid end
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Current element root"))
					throw ex;
			}

			string xml3 = dtd + "<root><foo /></root>";
			xtr = new XmlTextReader (xml3, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo

			string xml4 = dtd + "<root><bar /></root>";
			xtr = new XmlTextReader (xml4, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid element
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Invalid start element"))
					throw ex;
			}
		}

		[Test]
		public void TestMixedContent ()
		{
			string intSubset = "<!ELEMENT root (#PCDATA | foo)*><!ELEMENT foo EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root />";
			xtr = new XmlTextReader (xml1, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end

			string xml2 = dtd + "<root>Test.</root>";
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid PCDATA
			dvr.Read ();	// endelement root

			string xml3 = dtd + "<root><foo/>Test.<foo></foo></root>";
			xtr = new XmlTextReader (xml3, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid foo
			dvr.Read ();	// valid #PCDATA
			dvr.Read ();	// valid foo
			dvr.Read ();	// valid endElement foo
			dvr.Read ();	// valid endElement root

			string xml4 = dtd + "<root>Test.<bar /></root>";
			xtr = new XmlTextReader (xml4, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// valid #PCDATA
			try {
				dvr.Read ();	// invalid element
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Invalid start element"))
					throw ex;
			}
		}

		[Test]
		public void TestSequence ()
		{
			string intSubset = "<!ELEMENT root (foo, bar)><!ELEMENT foo EMPTY><!ELEMENT bar EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root><foo/><bar/></root>";
			xtr = new XmlTextReader (xml1, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			string xml2 = dtd + "<root><foo/></root>";
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid end root
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Invalid end element"))
					throw ex;
			}

			string xml3 = dtd + "<root><bar/></root>";
			xtr = new XmlTextReader (xml3, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// invalid element bar
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Invalid start element"))
					throw ex;
			}
		}

		[Test]
		public void TestChoice ()
		{
			string intSubset = "<!ELEMENT root (foo|bar)><!ELEMENT foo EMPTY><!ELEMENT bar EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root><foo/><bar/></root>";
			xtr = new XmlTextReader (xml1, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid element bar
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Invalid start element"))
					throw ex;
			}

			string xml2 = dtd + "<root><foo/></root>";
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			string xml3 = dtd + "<root><bar/></root>";
			xtr = new XmlTextReader (xml3, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// bar
			dvr.Read ();	// end root

			string xml4 = dtd + "<root><foo/>text.<bar/></root>";
			xtr = new XmlTextReader (xml4, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			try {
				dvr.Read ();	// invalid text
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Current element root"))
					throw ex;
			}
		}

		[Test]
		public void TestAny ()
		{
			string intSubset = "<!ELEMENT root ANY><!ELEMENT foo EMPTY>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml1 = dtd + "<root />";
			xtr = new XmlTextReader (xml1, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// empty root.
			dvr.Read ();	// end of document.

			string xml2 = dtd + "<root><foo/></root>";
			xtr = new XmlTextReader (xml2, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			string xml3 = dtd + "<root><foo /><foo></foo><foo/></root>";
			xtr = new XmlTextReader (xml3, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// foo
			dvr.Read ();	// foo
			dvr.Read ();	// foo
			dvr.Read ();	// end root

			string xml4 = dtd + "<root><bar /></root>";
			xtr = new XmlTextReader (xml4, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			try {
				dvr.Read ();	// bar: invalid (undeclared) element
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Element bar is not declared"))
					throw ex;
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
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
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
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Required attribute root"))
					throw ex;
			}

			xml = dtd + "<root foo='value' />";
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end of document

			xml = dtd + "<root foo='value' bar='2nd' />";
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// end of document

			xml = dtd + "<root foo='value' bar='2nd' baz='3rd' />";
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			dvr = new XmlValidatingReader (xtr);
			dvr.Read ();	// DTD
			try {
				dvr.Read ();	// undeclared attribute baz
				Fail ("should be failed.");
			} catch (Exception ex) {
				if (!ex.Message.StartsWith ("Attribute baz is not declared"))
					throw ex;
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
	}
}
