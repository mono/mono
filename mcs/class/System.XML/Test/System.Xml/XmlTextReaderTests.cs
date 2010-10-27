//
// XmlTextReaderTests.cs
//
// Authors:
//   Jason Diamond (jason@injektilo.org)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.IO;
using System.Xml;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlTextReaderTests
	{
		private void AssertStartDocument (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.ReadState == ReadState.Initial);
			Assert.IsTrue (xmlReader.NodeType == XmlNodeType.None);
			Assert.IsTrue (xmlReader.Depth == 0);
			Assert.IsTrue (!xmlReader.EOF);
		}

		private void AssertNode (
			XmlReader xmlReader,
			XmlNodeType nodeType,
			int depth,
			bool isEmptyElement,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value,
			int attributeCount)
		{
			Assert.IsTrue (xmlReader.Read (), "#Read");
			Assert.IsTrue (xmlReader.ReadState == ReadState.Interactive, "#ReadState");
			Assert.IsTrue (!xmlReader.EOF);
			AssertNodeValues (xmlReader, nodeType, depth, isEmptyElement, name, prefix, localName, namespaceURI, value, attributeCount);
		}

		private void AssertNodeValues (
			XmlReader xmlReader,
			XmlNodeType nodeType,
			int depth,
			bool isEmptyElement,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value,
			int attributeCount)
		{
			Assert.AreEqual (nodeType, xmlReader.NodeType, "NodeType");
			Assert.AreEqual (depth, xmlReader.Depth, "Depth");
			Assert.AreEqual (isEmptyElement, xmlReader.IsEmptyElement, "IsEmptyElement");

			Assert.AreEqual (name, xmlReader.Name, "name");

			Assert.AreEqual (prefix, xmlReader.Prefix, "prefix");

			Assert.AreEqual (localName, xmlReader.LocalName, "localName");

			Assert.AreEqual (namespaceURI, xmlReader.NamespaceURI, "namespaceURI");

			Assert.AreEqual ((value != String.Empty), xmlReader.HasValue, "hasValue");

			Assert.AreEqual (value, xmlReader.Value, "Value");

			Assert.AreEqual (attributeCount > 0, xmlReader.HasAttributes, "hasAttributes");

			Assert.AreEqual (attributeCount, xmlReader.AttributeCount, "attributeCount");
		}

		private void AssertAttribute (
			XmlReader xmlReader,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value)
		{
			Assert.AreEqual (value, xmlReader [name], "value.Indexer");

			Assert.AreEqual (value, xmlReader.GetAttribute (name), "value.GetAttribute");

			if (namespaceURI != String.Empty) {
				Assert.IsTrue (xmlReader[localName, namespaceURI] == value);
				Assert.IsTrue (xmlReader.GetAttribute (localName, namespaceURI) == value);
			}
		}

		private void AssertEndDocument (XmlReader xmlReader)
		{
			Assert.IsTrue (!xmlReader.Read (), "could read");
			Assert.AreEqual (XmlNodeType.None, xmlReader.NodeType, "NodeType is not XmlNodeType.None");
			Assert.AreEqual (0, xmlReader.Depth, "Depth is not 0");
			Assert.AreEqual (ReadState.EndOfFile, xmlReader.ReadState, "ReadState is not ReadState.EndOfFile");
			Assert.IsTrue (xmlReader.EOF, "not EOF");

			xmlReader.Close ();
			Assert.AreEqual (ReadState.Closed, xmlReader.ReadState, "ReadState is not ReadState.Cosed");
		}

		[Test]
		public void StartAndEndTagWithAttribute ()
		{
			string xml = @"<foo bar='baz'></foo>";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"bar", // name
				String.Empty, // prefix
				"bar", // localName
				String.Empty, // namespaceURI
				"baz" // value
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		// expecting parser error
		[Test]
		public void EmptyElementWithBadName ()
		{
			string xml = "<1foo/>";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			bool caughtXmlException = false;

			try {
				xmlReader.Read();
			} catch (XmlException) {
				caughtXmlException = true;
			}

			Assert.IsTrue (caughtXmlException);
		}

		[Test]
		public void EmptyElementWithStartAndEndTag ()
		{
			string xml = "<foo></foo>";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		// checking parser
		[Test]
		public void EmptyElementWithStartAndEndTagWithWhitespace ()
		{
			string xml = "<foo ></foo >";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void EmptyElementWithAttribute ()
		{
			string xml = @"<foo bar=""baz""/>";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				true, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"bar", // name
				String.Empty, // prefix
				"bar", // localName
				String.Empty, // namespaceURI
				"baz" // value
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void EmptyElementInNamespace ()
		{
			string xml = @"<foo:bar xmlns:foo='http://foo/' />";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, // depth
				true, // isEmptyElement
				"foo:bar", // name
				"foo", // prefix
				"bar", // localName
				"http://foo/", // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"xmlns:foo", // name
				"xmlns", // prefix
				"foo", // localName
				"http://www.w3.org/2000/xmlns/", // namespaceURI
				"http://foo/" // value
			);

			Assert.AreEqual ("http://foo/", xmlReader.LookupNamespace ("foo"));

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void EntityReferenceInAttribute ()
		{
			string xml = "<foo bar='&baz;'/>";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				true, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"bar", // name
				String.Empty, // prefix
				"bar", // localName
				String.Empty, // namespaceURI
				"&baz;" // value
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void IsName ()
		{
			Assert.IsTrue (XmlReader.IsName ("foo"));
			Assert.IsTrue (!XmlReader.IsName ("1foo"));
			Assert.IsTrue (!XmlReader.IsName (" foo"));
		}

		[Test]
		public void IsNameToken ()
		{
			Assert.IsTrue (XmlReader.IsNameToken ("foo"));
			Assert.IsTrue (XmlReader.IsNameToken ("1foo"));
			Assert.IsTrue (!XmlReader.IsNameToken (" foo"));
		}

		[Test]
		public void FragmentConstructor()
		{
			XmlDocument doc = new XmlDocument();
//			doc.LoadXml("<root/>");

			string xml = @"<foo><bar xmlns=""NSURI"">TEXT NODE</bar></foo>";
			MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(xml));

			XmlParserContext ctx = new XmlParserContext(doc.NameTable, new XmlNamespaceManager(doc.NameTable), "", "", "", "",
				doc.BaseURI, "", XmlSpace.Default, Encoding.Default);

			XmlTextReader xmlReader = new XmlTextReader(ms, XmlNodeType.Element, ctx);
			AssertNode(xmlReader, XmlNodeType.Element, 0, false, "foo", "", "foo", "", "", 0);

			AssertNode(xmlReader, XmlNodeType.Element, 1, false, "bar", "", "bar", "NSURI", "", 1);

			AssertNode(xmlReader, XmlNodeType.Text, 2, false, "", "", "", "", "TEXT NODE", 0);

			AssertNode(xmlReader, XmlNodeType.EndElement, 1, false, "bar", "", "bar", "NSURI", "", 0);

			AssertNode(xmlReader, XmlNodeType.EndElement, 0, false, "foo", "", "foo", "", "", 0);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void AttributeWithCharacterReference ()
		{
			string xml = @"<a value='hello &amp; world' />";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));
			xmlReader.Read ();
			Assert.AreEqual ("hello & world", xmlReader ["value"]);
		}

		[Test]
		public void AttributeWithEntityReference ()
		{
			string xml = @"<a value='hello &ent; world' />";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));
			xmlReader.Read ();
			xmlReader.MoveToFirstAttribute ();
			xmlReader.ReadAttributeValue ();
			Assert.AreEqual ("hello ", xmlReader.Value);
			Assert.IsTrue (xmlReader.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, xmlReader.NodeType);
			Assert.AreEqual ("ent", xmlReader.Name);
			Assert.AreEqual (XmlNodeType.EntityReference, xmlReader.NodeType);
			Assert.IsTrue (xmlReader.ReadAttributeValue ());
			Assert.AreEqual (" world", xmlReader.Value);
			Assert.AreEqual (XmlNodeType.Text, xmlReader.NodeType);
			Assert.IsTrue (!xmlReader.ReadAttributeValue ());
			Assert.AreEqual (" world", xmlReader.Value); // remains
			Assert.AreEqual (XmlNodeType.Text, xmlReader.NodeType);
			xmlReader.ReadAttributeValue ();
			Assert.AreEqual (XmlNodeType.Text, xmlReader.NodeType);
		}

		[Test]
		public void QuoteChar ()
		{
			string xml = @"<a value='hello &amp; world' value2="""" />";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));
			xmlReader.Read ();
			xmlReader.MoveToFirstAttribute ();
			Assert.AreEqual ('\'', xmlReader.QuoteChar, "First");
			xmlReader.MoveToNextAttribute ();
			Assert.AreEqual ('"', xmlReader.QuoteChar, "Next");
			xmlReader.MoveToFirstAttribute ();
			Assert.AreEqual ('\'', xmlReader.QuoteChar, "First.Again");
		}

		[Test]
		public void ReadInnerXmlWrongInit ()
		{
			// This behavior is different from XmlNodeReader.
			XmlReader reader = new XmlTextReader (new StringReader ("<root>test of <b>mixed</b> string.</root>"));
			reader.ReadInnerXml ();
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "initial.ReadState");
			Assert.AreEqual (false, reader.EOF, "initial.EOF");
			Assert.AreEqual (XmlNodeType.None, reader.NodeType, "initial.NodeType");
		}

		[Test]
		public void EntityReference ()
		{
			string xml = "<foo>&bar;</foo>";
			XmlReader xmlReader = new XmlTextReader (new StringReader (xml));
			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EntityReference, // nodeType
				1, //depth
				false, // isEmptyElement
				"bar", // name
				String.Empty, // prefix
				"bar", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void EntityReferenceInsideText ()
		{
			string xml = "<foo>bar&baz;quux</foo>";
			XmlReader xmlReader = new XmlTextReader (new StringReader (xml));
			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Text, // nodeType
				1, //depth
				false, // isEmptyElement
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				String.Empty, // namespaceURI
				"bar", // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EntityReference, // nodeType
				1, //depth
				false, // isEmptyElement
				"baz", // name
				String.Empty, // prefix
				"baz", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Text, // nodeType
				1, //depth
				false, // isEmptyElement
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				String.Empty, // namespaceURI
				"quux", // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void XmlDeclAfterWhitespace ()
		{
			XmlTextReader xtr = new XmlTextReader (
				" <?xml version='1.0' ?><root />",
				XmlNodeType.Document,
				null);
			xtr.Read ();	// ws
			xtr.Read ();	// not-wf xmldecl
			xtr.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void XmlDeclAfterComment ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"<!-- comment --><?xml version='1.0' ?><root />",
				XmlNodeType.Document,
				null);
			xtr.Read ();	// comment
			xtr.Read ();	// not-wf xmldecl
			xtr.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void XmlDeclAfterProcessingInstruction ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"<?myPI let it go ?><?xml version='1.0' ?><root />",
				XmlNodeType.Document,
				null);
			xtr.Read ();	// PI
			xtr.Read ();	// not-wf xmldecl
			xtr.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void StartsFromEndElement ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"</root>",
				XmlNodeType.Document,
				null);
			xtr.Read ();
			xtr.Close ();
		}

		[Test]
		public void ReadAsElementContent ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"<foo /><bar />", XmlNodeType.Element, null);
			xtr.Read ();
			xtr.Close ();
		}

		[Test]
		public void ReadAsAttributeContent ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"test", XmlNodeType.Attribute, null);
			xtr.Read ();
			xtr.Close ();
		}

		[Test] 
		public void ExternalDocument ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load ("Test/XmlFiles/nested-dtd-test.xml");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void NotAllowedCharRef ()
		{
			string xml = "<root>&#0;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Normalization = true;
			xtr.Read ();
			xtr.Read ();
			xtr.Close ();
		}

		[Test]
		public void NotAllowedCharRefButPassNormalizationFalse ()
		{
			string xml = "<root>&#0;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Read ();
			xtr.Read ();
			xtr.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Ignore ("MS.NET 1.0 does not pass this test. The related spec is XML rec. 4.1")]
		public void UndeclaredEntityInIntSubsetOnlyXml ()
		{
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent2;
			string xml = dtd + "<root>&ent;&ent2;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			while (!xtr.EOF)
				xtr.Read ();
			xtr.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Ignore ("MS.NET 1.0 does not pass this test. The related spec is XML rec. 4.1")]
		public void UndeclaredEntityInStandaloneXml ()
		{
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent2;
			string xml = "<?xml version='1.0' standalone='yes' ?>" 
				+ dtd + "<root>&ent;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			while (!xtr.EOF)
				xtr.Read ();
			xtr.Close ();
		}

		[Test]
		public void ExpandParameterEntity ()
		{
			string ent = "<!ENTITY foo \"foo-def\">";
			string pe = "<!ENTITY % pe '" + ent + "'>";
			string eldecl = "<!ELEMENT root ANY>";
			string dtd = "<!DOCTYPE root[" + eldecl + pe + "%pe;]>";
			string xml = dtd + "<root/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlEntity foo = doc.DocumentType.Entities.GetNamedItem ("foo") as XmlEntity;
			Assert.IsNotNull (foo);
			Assert.AreEqual ("foo-def", foo.InnerText);
		}

		[Test]
		public void IfNamespacesThenProhibitedAttributes ()
		{
			string xml = @"<foo _1='1' xmlns:x='urn:x' x:_1='1' />";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
		}

		[Test]
		public void ReadBase64 ()
		{
			byte [] bytes = new byte [] {4,14,54,114,134,184,254,255};
			
			string base64 = "<root><foo>BA42coa44</foo></root>";
			XmlTextReader xtr = new XmlTextReader (base64, XmlNodeType.Document, null);
			byte [] bytes2 = new byte [10];
			xtr.Read ();	// root
			xtr.Read ();	// foo
			this.AssertNodeValues (xtr, XmlNodeType.Element, 1, false, "foo", String.Empty,
				"foo", String.Empty, String.Empty, 0);
			Assert.AreEqual (6, xtr.ReadBase64 (bytes2, 0, 10));
			this.AssertNodeValues (xtr, XmlNodeType.EndElement, 0, false, "root", String.Empty,
				"root", String.Empty, String.Empty, 0);
			Assert.IsTrue (!xtr.Read ());
			Assert.AreEqual (4, bytes2 [0]);
			Assert.AreEqual (14, bytes2 [1]);
			Assert.AreEqual (54, bytes2 [2]);
			Assert.AreEqual (114, bytes2 [3]);
			Assert.AreEqual (134, bytes2 [4]);
			Assert.AreEqual (184, bytes2 [5]);
			Assert.AreEqual (0, bytes2 [6]);
			xtr.Close ();

			xtr = new XmlTextReader (base64, XmlNodeType.Document, null);
			bytes2 = new byte [10];
			xtr.Read ();	// root
			xtr.Read ();	// foo
			this.AssertNodeValues (xtr, XmlNodeType.Element, 1, false, "foo", String.Empty,
				"foo", String.Empty, String.Empty, 0);

			// Read less than 4 (i.e. one Base64 block)
			Assert.AreEqual (1, xtr.ReadBase64 (bytes2, 0, 1));
			this.AssertNodeValues (xtr, XmlNodeType.Element, 1, false, "foo", String.Empty,
				"foo", String.Empty, String.Empty, 0);
			Assert.AreEqual (4, bytes2 [0]);

			Assert.AreEqual (5, xtr.ReadBase64 (bytes2, 0, 10));
			this.AssertNodeValues (xtr, XmlNodeType.EndElement, 0, false, "root", String.Empty,
				"root", String.Empty, String.Empty, 0);
			Assert.IsTrue (!xtr.Read ());
			Assert.AreEqual (14, bytes2 [0]);
			Assert.AreEqual (54, bytes2 [1]);
			Assert.AreEqual (114, bytes2 [2]);
			Assert.AreEqual (134, bytes2 [3]);
			Assert.AreEqual (184, bytes2 [4]);
			Assert.AreEqual (0, bytes2 [5]);
			while (!xtr.EOF)
				xtr.Read ();
			xtr.Close ();
		}

		[Test]
		public void ReadBase64Test2 ()
		{
			string xml = "<root/>";
			XmlTextReader xtr = new XmlTextReader (new StringReader (xml));
			xtr.Read ();
			byte [] data = new byte [1];
			xtr.ReadBase64 (data, 0, 1);
			while (!xtr.EOF)
				xtr.Read ();

			xml = "<root></root>";
			xtr = new XmlTextReader (new StringReader (xml));
			xtr.Read ();
			xtr.ReadBase64 (data, 0, 1);
			while (!xtr.EOF)
				xtr.Read ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void CheckNamespaceValidity1 ()
		{
			string xml = "<x:root />";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Read ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void CheckNamespaceValidity2 ()
		{
			string xml = "<root x:attr='val' />";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Read ();
		}

		[Test]
		public void NamespaceFalse ()
		{
			string xml = "<x:root />";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Namespaces = false;
			xtr.Read ();
		}

		[Test]
		public void NormalizationLineEnd ()
		{
			string s = "One\rtwo\nthree\r\nfour";
			string t = "<hi><![CDATA[" + s + "]]></hi>";

			XmlTextReader r = new XmlTextReader (new StringReader (t));
			r.WhitespaceHandling = WhitespaceHandling.Significant;

			r.Normalization = true;

			s = r.ReadElementString ("hi");
			Assert.AreEqual ("One\ntwo\nthree\nfour", s);
		}

		[Test]
		public void NormalizationAttributes ()
		{
			// does not normalize attribute values.
			StringReader sr = new StringReader ("<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root attr ID #IMPLIED>]><root attr='   value   '/>");
			XmlTextReader xtr = new XmlTextReader (sr);
			xtr.Normalization = true;
			xtr.Read ();
			xtr.Read ();
			xtr.MoveToFirstAttribute ();
			Assert.AreEqual ("   value   ", xtr.Value);
		}

		[Test]
		public void CloseIsNotAlwaysEOF ()
		{
			// See bug #63505
			XmlTextReader xtr = new XmlTextReader (
				new StringReader ("<a></a><b></b>"));
			xtr.Close ();
			Assert.IsTrue (!xtr.EOF); // Close() != EOF
		}

		[Test]
		public void CloseIsNotAlwaysEOF2 ()
		{
			XmlTextReader xtr = new XmlTextReader ("Test/XmlFiles/simple.xml");
			xtr.Close ();
			Assert.IsTrue (!xtr.EOF); // Close() != EOF
		}

		[Test]
		public void IXmlLineInfo ()
		{
			// See bug #63507
			XmlTextReader aux = new XmlTextReader (
				new StringReader ("<all><hello></hello><bug></bug></all>"));
			Assert.AreEqual (0, aux.LineNumber);
			Assert.AreEqual (0, aux.LinePosition);
			aux.MoveToContent();
			Assert.AreEqual (1, aux.LineNumber);
			Assert.AreEqual (2, aux.LinePosition);
			aux.Read();
			Assert.AreEqual (1, aux.LineNumber);
			Assert.AreEqual (7, aux.LinePosition);
			aux.ReadOuterXml();
			Assert.AreEqual (1, aux.LineNumber);
			Assert.AreEqual (22, aux.LinePosition);
			aux.ReadInnerXml();
			Assert.AreEqual (1, aux.LineNumber);
			Assert.AreEqual (34, aux.LinePosition);
			aux.Read();
			Assert.AreEqual (1, aux.LineNumber);
			Assert.AreEqual (38, aux.LinePosition);
			aux.Close();
			Assert.AreEqual (0, aux.LineNumber);
			Assert.AreEqual (0, aux.LinePosition);
		}

		[Test]
		public void AttributeNormalizationWrapped ()
		{
			// When XmlValidatingReader there used to be a problem.
			string xml = "<root attr=' value\nstring' />";
			XmlTextReader xtr = new XmlTextReader (xml,
				XmlNodeType.Document, null);
			xtr.Normalization = true;
			XmlValidatingReader xvr = new XmlValidatingReader (xtr);
			xvr.Read ();
			xvr.MoveToFirstAttribute ();
			Assert.AreEqual (" value string", xvr.Value);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ProhibitDtd ()
		{
			XmlTextReader xtr = new XmlTextReader ("<!DOCTYPE root []><root/>", XmlNodeType.Document, null);
			xtr.ProhibitDtd = true;
			while (!xtr.EOF)
				xtr.Read ();
		}

#if NET_2_0
		[Test]
		public void Settings ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root/>", XmlNodeType.Document, null);
			Assert.IsNull (xtr.Settings);
		}

		// Copied from XmlValidatingReaderTests.cs
		[Test]
		public void ExpandEntity ()
		{
			string intSubset = "<!ELEMENT root (#PCDATA)><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'><!ENTITY ent 'entity string'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root foo='&ent;' bar='internal &ent; value'>&ent;</root>";
			XmlTextReader dvr = new XmlTextReader (xml, XmlNodeType.Document, null);
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
		}

		[Test]
		public void PreserveEntity ()
		{
			string intSubset = "<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'foo-def' bar CDATA 'bar-def'><!ENTITY ent 'entity string'>";
			string dtd = "<!DOCTYPE root [" + intSubset + "]>";
			string xml = dtd + "<root foo='&ent;' bar='internal &ent; value' />";
			XmlTextReader dvr = new XmlTextReader (xml, XmlNodeType.Document, null);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.Read ();	// DTD
			dvr.Read ();
			Assert.AreEqual (XmlNodeType.Element, dvr.NodeType);
			Assert.AreEqual ("root", dvr.Name);
			Assert.IsTrue (dvr.MoveToFirstAttribute ());
			Assert.AreEqual ("foo", dvr.Name);
			// MS BUG: it returns "entity string", however, entity should not be exanded.
			Assert.AreEqual ("&ent;", dvr.Value);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual ("", dvr.Value);
			Assert.IsTrue (!dvr.ReadAttributeValue ());

			// bar
			Assert.IsTrue (dvr.MoveToNextAttribute ());
			Assert.AreEqual ("bar", dvr.Name);
			Assert.AreEqual ("internal &ent; value", dvr.Value);
			//  ReadAttributeValue()
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual ("", dvr.Name);
			Assert.AreEqual ("internal ", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			Assert.AreEqual ("ent", dvr.Name);
			Assert.AreEqual ("", dvr.Value);
			Assert.IsTrue (dvr.ReadAttributeValue ());
			Assert.AreEqual (XmlNodeType.Text, dvr.NodeType);
			Assert.AreEqual ("", dvr.Name);
			Assert.AreEqual (" value", dvr.Value);

		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ExpandEntityRejectsUndeclaredEntityAttr ()
		{
			XmlTextReader xtr = new XmlTextReader ("<!DOCTYPE root SYSTEM 'foo.dtd'><root attr='&rnt;'>&rnt;</root>", XmlNodeType.Document, null);
			xtr.EntityHandling = EntityHandling.ExpandEntities;
			xtr.XmlResolver = null;
			xtr.Read ();
			xtr.Read (); // attribute entity 'rnt' is undeclared
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ExpandEntityRejectsUndeclaredEntityContent ()
		{
			XmlTextReader xtr = new XmlTextReader ("<!DOCTYPE root SYSTEM 'foo.dtd'><root>&rnt;</root>", XmlNodeType.Document, null);
			xtr.EntityHandling = EntityHandling.ExpandEntities;
			xtr.XmlResolver = null;
			xtr.Read ();
			xtr.Read ();
			xtr.Read (); // content entity 'rnt' is undeclared
		}

		// mostly copied from XmlValidatingReaderTests.
		[Test]
		public void ResolveEntity ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent;&ent2;</root>";
			XmlTextReader dvr = new XmlTextReader (xml, XmlNodeType.Document, null);
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
			Assert.AreEqual ("", dvr.Value);

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

		// mostly copied from XmlValidatingReaderTests.
		[Test]
		public void ResolveEntity2 ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent3;&ent2;</root>";
			XmlTextReader dvr = new XmlTextReader (xml, XmlNodeType.Document, null);
			dvr.EntityHandling = EntityHandling.ExpandCharEntities;
			dvr.Read ();	// DTD
			dvr.Read ();	// root
			dvr.Read ();	// &ent3;
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			// ent3 does not exists in this dtd.
			Assert.AreEqual (XmlNodeType.EntityReference, dvr.NodeType);
			try {
				dvr.ResolveEntity ();
				Assert.Fail ("Attempt to resolve undeclared entity should fail.");
			} catch (XmlException) {
			}
		}
#endif

		[Test]
		public void SurrogatePair ()
		{
			string xml = @"<!DOCTYPE test [<!ELEMENT test ANY>
		<!ENTITY % a '<!ENTITY ref " + "\"\uF090\u8080\"" + @">'>
		%a;
	]>
	<test>&ref;</test>";
			XmlValidatingReader r = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			r.Read ();
			r.Read ();
			r.Read ();
			r.Read ();
			Assert.AreEqual (0xf090, (int) r.Value [0], "#1");
			Assert.AreEqual (0x8080, (int) r.Value [1], "#1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void EntityDeclarationNotWF ()
		{
			string xml = @"<!DOCTYPE doc [
				<!ELEMENT doc (#PCDATA)>
				<!ENTITY e ''>
				<!ENTITY e '<foo&>'>
				]>
				<doc>&e;</doc> ";
			XmlTextReader xtr = new XmlTextReader (xml,
				XmlNodeType.Document, null);
			xtr.Read ();
		}

		[Test] // bug #76102
		public void SurrogateAtReaderByteCache ()
		{
			XmlTextReader xtr = null;
			try {
				xtr = new XmlTextReader (File.OpenText ("Test/XmlFiles/76102.xml"));
				while (!xtr.EOF)
					xtr.Read ();
			} finally {
				if (xtr != null)
					xtr.Close ();
			}
		}

		[Test] // bug #76247
		public void SurrogateRoundtrip ()
		{
			byte [] data = new byte [] {0x3c, 0x61, 0x3e, 0xf0,
				0xa8, 0xa7, 0x80, 0x3c, 0x2f, 0x61, 0x3e};
			XmlTextReader xtr = new XmlTextReader (
				new MemoryStream (data));
			xtr.Read ();
			string line = xtr.ReadString ();
			int [] arr = new int [line.Length];
			for (int i = 0; i < line.Length; i++)
				arr [i] = (int) line [i];
			Assert.AreEqual (new int [] {0xd862, 0xddc0}, arr);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void RejectEmptyNamespaceWithNonEmptyPrefix ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root xmlns:my='' />",
				XmlNodeType.Document, null);
			xtr.Read ();
		}

		[Test]
		public void EncodingProperty ()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n<root>\n<node>\nvalue\n</node>\n</root>";
			XmlTextReader xr = new XmlTextReader (xml, XmlNodeType.Document, null);
			Assert.IsNull (xr.Encoding, "#1");
			xr.Read ();
			Assert.AreEqual (Encoding.Unicode, xr.Encoding, "#2");
		}

		[Test]
		public void WhitespaceHandlingSignificant ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root>  <child xml:space='preserve'>    <descendant xml:space='default'>    </descendant>   </child><child xml:space='default'>   </child>  </root>",
				XmlNodeType.Document, null);
			xtr.WhitespaceHandling = WhitespaceHandling.Significant;

			xtr.Read (); // root
			xtr.Read (); // child. skip whitespaces
			Assert.AreEqual (XmlNodeType.Element, xtr.NodeType, "#1");
			xtr.Read (); // significant whitespaces
			Assert.AreEqual (XmlNodeType.SignificantWhitespace, xtr.NodeType, "#2");
			xtr.Read ();
			Assert.AreEqual ("descendant", xtr.LocalName, "#3");
			xtr.Read (); // end of descendant. skip whitespaces
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType, "#4");
			xtr.Read (); // significant whitespaces
			Assert.AreEqual (XmlNodeType.SignificantWhitespace, xtr.NodeType, "#5");
			xtr.Read (); // end of child
			xtr.Read (); // child
			xtr.Read (); // end of child. skip whitespaces
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType, "#6");
			xtr.Read (); // end of root. skip whitespaces
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType, "#7");
		}

		[Test]
		public void WhitespaceHandlingNone ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root>  <child xml:space='preserve'>    <descendant xml:space='default'>    </descendant>   </child><child xml:space='default'>   </child>  </root>",
				XmlNodeType.Document, null);
			xtr.WhitespaceHandling = WhitespaceHandling.None;

			xtr.Read (); // root
			xtr.Read (); // child. skip whitespaces
			Assert.AreEqual (XmlNodeType.Element, xtr.NodeType, "#1");
			xtr.Read (); // descendant. skip significant whitespaces
			Assert.AreEqual ("descendant", xtr.LocalName, "#2");
			xtr.Read (); // end of descendant. skip whitespaces
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType, "#3");
			xtr.Read (); // end of child. skip significant whitespaces
			xtr.Read (); // child
			xtr.Read (); // end of child. skip whitespaces
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType, "#6");
			xtr.Read (); // end of root. skip whitespaces
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType, "#7");
		}

		[Test]
		public void WhitespacesAfterTextDeclaration ()
		{
			XmlTextReader xtr = new XmlTextReader (
				"<?xml version='1.0' encoding='utf-8' ?> <x/>",
				XmlNodeType.Element,
				null);
			xtr.Read ();
			Assert.AreEqual (XmlNodeType.Whitespace, xtr.NodeType, "#1");
			Assert.AreEqual (" ", xtr.Value, "#2");
		}

		// bug #79683
		[Test]
		public void NotationPERef ()
		{
			string xml = "<!DOCTYPE root SYSTEM 'Test/XmlFiles/79683.dtd'><root/>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			while (!xtr.EOF)
				xtr.Read ();
		}

		[Test] // bug #80308
		public void ReadCharsNested ()
		{
			char[] buf = new char [4];

			string xml = "<root><text>AAAA</text></root>";
			string [] strings = new string [] {
				"<tex", "t>AA", "AA</", "text", ">"};
			XmlTextReader r = new XmlTextReader (
				xml, XmlNodeType.Document, null);
			int c, n = 0;
			while (r.Read ())
				if (r.NodeType == XmlNodeType.Element)
					while ((c = r.ReadChars (buf, 0, buf.Length)) > 0)
						Assert.AreEqual (strings [n++], new string (buf, 0, c), "at " + n);
			Assert.AreEqual (5, n, "total lines");
		}

		[Test] // bug #81294
		public void DtdCommentContainsCloseBracket ()
		{
			string xml = @"<!DOCTYPE kanjidic2 [<!ELEMENT kanjidic2 EMPTY> <!-- ] --> ]><kanjidic2 />";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			while (!xtr.EOF)
				xtr.Read ();
		}

		[Test]
		public void CloseTagAfterTextWithTrailingCRNormalized () // bug #398374
		{
			string xml = "<root><foo>some text\r</foo></root>";
			XmlTextReader r = new XmlTextReader (xml, XmlNodeType.Document, null);
			r.Normalization = true;
			while (!r.EOF)
				r.Read ();
		}

		[Test]
		public void Bug412657 ()
		{
			string s = "<Verifier id='SimpleIntVerifier'/>";
			MemoryStream stream = new MemoryStream (Encoding.UTF8.GetBytes(s));
			XmlParserContext ctx = new XmlParserContext (null, null, null, XmlSpace.Default);
			Assert.IsNull (ctx.NamespaceManager, "#1");
			Assert.IsNull (ctx.NameTable, "#2");
			XmlReader reader = new XmlTextReader (stream, XmlNodeType.Element, ctx);
			Assert.IsNull (ctx.NamespaceManager, "#1");
			reader.Read (); // should not raise NRE.
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidUTF ()
		{
			byte [] data = new byte [] {0x4d, 0x53, 0x43, 0x46,
				0x00, 0x00, 0x00, 0x00, 0xab, 0x0a};
			XmlTextReader xtr = new XmlTextReader (
				new MemoryStream (data));
			xtr.Read ();
		}

		[Test]
		public void ParserContextNullNameTable ()
		{
			string input = "<?xml version='1.0' encoding='UTF-8'?><plist version='1.0'></plist>";
			XmlParserContext context = new XmlParserContext (null, null, null, XmlSpace.None); // null NameTable
			XmlTextReader xtr = new XmlTextReader (input, XmlNodeType.Document, context);
			while (!xtr.EOF)
				xtr.Read ();
		}

		[Test]
		public void ParsingWithNSMgrSubclass ()
		{
			XmlNamespaceManager nsMgr = new XmlNamespaceManager (new NameTable ());
			nsMgr.AddNamespace ("foo", "bar");
			XmlParserContext inputContext = new XmlParserContext (null, nsMgr, null, XmlSpace.None);
			XmlReader xr = XmlReader.Create (new StringReader ("<empty/>"), new XmlReaderSettings (), inputContext);

			XmlNamespaceManager aMgr = new MyNS (xr);
			XmlParserContext inputContext2 = new XmlParserContext(null, aMgr, null, XmlSpace.None);
			XmlReader xr2 = XmlReader.Create (new StringReader ("<foo:haha>namespace test</foo:haha>"), new XmlReaderSettings (), inputContext2);

			while (xr2.Read ()) {}

		}


		// The MyNS subclass chains namespace lookups
		class MyNS : XmlNamespaceManager {
			private XmlReader xr;


			public MyNS (XmlReader xr)
				: base (xr.NameTable) {
				this.xr = xr;
			}

			public override string LookupNamespace (string prefix) {
				string str = base.LookupNamespace (prefix);
				if (!string.IsNullOrEmpty (str))
					return str;
				if (xr != null)
					return xr.LookupNamespace (prefix);
				return String.Empty;
			}
		}

		[Test]
		public void EmptyXmlBase ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xml:base='' />");
		}

		[Test]
		public void GetAttribute ()
		{
			StringReader sr = new StringReader("<rootElement myAttribute=\"the value\"></rootElement>");
			using (XmlReader reader = XmlReader.Create(sr)) {
				reader.Read ();
				Assert.AreEqual (reader.GetAttribute("myAttribute", null), "the value", "#1");
			}
		}
	}
}
