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
	public class XmlTextReaderTests : Assertion
	{
		private void AssertStartDocument (XmlReader xmlReader)
		{
			Assert (xmlReader.ReadState == ReadState.Initial);
			Assert (xmlReader.NodeType == XmlNodeType.None);
			Assert (xmlReader.Depth == 0);
			Assert (!xmlReader.EOF);
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
			Assert ("#Read", xmlReader.Read ());
			Assert ("#ReadState", xmlReader.ReadState == ReadState.Interactive);
			Assert (!xmlReader.EOF);
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
			AssertEquals ("NodeType", nodeType, xmlReader.NodeType);
			AssertEquals ("Depth", depth, xmlReader.Depth);
			AssertEquals ("IsEmptyElement", isEmptyElement, xmlReader.IsEmptyElement);

			AssertEquals ("name", name, xmlReader.Name);

			AssertEquals ("prefix", prefix, xmlReader.Prefix);

			AssertEquals ("localName", localName, xmlReader.LocalName);

			AssertEquals ("namespaceURI", namespaceURI, xmlReader.NamespaceURI);

			AssertEquals ("hasValue", (value != String.Empty), xmlReader.HasValue);

			AssertEquals ("Value", value, xmlReader.Value);

			AssertEquals ("hasAttributes", attributeCount > 0, xmlReader.HasAttributes);

			AssertEquals ("attributeCount", attributeCount, xmlReader.AttributeCount);
		}

		private void AssertAttribute (
			XmlReader xmlReader,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value)
		{
			AssertEquals ("value.Indexer", value, xmlReader [name]);

			AssertEquals ("value.GetAttribute", value, xmlReader.GetAttribute (name));

			if (namespaceURI != String.Empty) {
				Assert (xmlReader[localName, namespaceURI] == value);
				Assert (xmlReader.GetAttribute (localName, namespaceURI) == value);
			}
		}

		private void AssertEndDocument (XmlReader xmlReader)
		{
			Assert ("could read", !xmlReader.Read ());
			AssertEquals ("NodeType is not XmlNodeType.None", XmlNodeType.None, xmlReader.NodeType);
			AssertEquals ("Depth is not 0", 0, xmlReader.Depth);
			AssertEquals ("ReadState is not ReadState.EndOfFile",  ReadState.EndOfFile, xmlReader.ReadState);
			Assert ("not EOF", xmlReader.EOF);

			xmlReader.Close ();
			AssertEquals ("ReadState is not ReadState.Cosed", ReadState.Closed, xmlReader.ReadState);
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

			Assert(caughtXmlException);
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

			AssertEquals ("http://foo/", xmlReader.LookupNamespace ("foo"));

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
			Assert (XmlReader.IsName ("foo"));
			Assert (!XmlReader.IsName ("1foo"));
			Assert (!XmlReader.IsName (" foo"));
		}

		[Test]
		public void IsNameToken ()
		{
			Assert (XmlReader.IsNameToken ("foo"));
			Assert (XmlReader.IsNameToken ("1foo"));
			Assert (!XmlReader.IsNameToken (" foo"));
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
			AssertEquals ("hello & world", xmlReader ["value"]);
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
			AssertEquals ("hello ", xmlReader.Value);
			Assert (xmlReader.ReadAttributeValue ());
			AssertEquals (XmlNodeType.EntityReference, xmlReader.NodeType);
			AssertEquals ("ent", xmlReader.Name);
			AssertEquals (XmlNodeType.EntityReference, xmlReader.NodeType);
			Assert (xmlReader.ReadAttributeValue ());
			AssertEquals (" world", xmlReader.Value);
			AssertEquals (XmlNodeType.Text, xmlReader.NodeType);
			Assert (!xmlReader.ReadAttributeValue ());
			AssertEquals (" world", xmlReader.Value); // remains
			AssertEquals (XmlNodeType.Text, xmlReader.NodeType);
			xmlReader.ReadAttributeValue ();
			AssertEquals (XmlNodeType.Text, xmlReader.NodeType);
		}

		[Test]
		public void QuoteChar ()
		{
			string xml = @"<a value='hello &amp; world' value2="""" />";
			XmlReader xmlReader =
				new XmlTextReader (new StringReader (xml));
			xmlReader.Read ();
			xmlReader.MoveToFirstAttribute ();
			AssertEquals ("First", '\'', xmlReader.QuoteChar);
			xmlReader.MoveToNextAttribute ();
			AssertEquals ("Next", '"', xmlReader.QuoteChar);
			xmlReader.MoveToFirstAttribute ();
			AssertEquals ("First.Again", '\'', xmlReader.QuoteChar);
		}

		[Test]
		public void ReadInnerXmlWrongInit ()
		{
			// This behavior is different from XmlNodeReader.
			XmlReader reader = new XmlTextReader (new StringReader ("<root>test of <b>mixed</b> string.</root>"));
			reader.ReadInnerXml ();
			AssertEquals ("initial.ReadState", ReadState.Initial, reader.ReadState);
			AssertEquals ("initial.EOF", false, reader.EOF);
			AssertEquals ("initial.NodeType", XmlNodeType.None, reader.NodeType);
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
		[Ignore ("MS.NET 1.0 fails this test. The related spec is XML rec. 4.1")]
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
		[Ignore ("MS.NET 1.0 fails this test. The related spec is XML rec. 4.1")]
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
			AssertNotNull (foo);
			AssertEquals ("foo-def", foo.InnerText);
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
			AssertEquals (6, xtr.ReadBase64 (bytes2, 0, 10));
			this.AssertNodeValues (xtr, XmlNodeType.EndElement, 0, false, "root", String.Empty,
				"root", String.Empty, String.Empty, 0);
			Assert (!xtr.Read ());
			AssertEquals (4, bytes2 [0]);
			AssertEquals (14, bytes2 [1]);
			AssertEquals (54, bytes2 [2]);
			AssertEquals (114, bytes2 [3]);
			AssertEquals (134, bytes2 [4]);
			AssertEquals (184, bytes2 [5]);
			AssertEquals (0, bytes2 [6]);
			xtr.Close ();

			xtr = new XmlTextReader (base64, XmlNodeType.Document, null);
			bytes2 = new byte [10];
			xtr.Read ();	// root
			xtr.Read ();	// foo
			this.AssertNodeValues (xtr, XmlNodeType.Element, 1, false, "foo", String.Empty,
				"foo", String.Empty, String.Empty, 0);

			// Read less than 4 (i.e. one Base64 block)
			AssertEquals (1, xtr.ReadBase64 (bytes2, 0, 1));
			this.AssertNodeValues (xtr, XmlNodeType.Element, 1, false, "foo", String.Empty,
				"foo", String.Empty, String.Empty, 0);
			AssertEquals (4, bytes2 [0]);

			AssertEquals (5, xtr.ReadBase64 (bytes2, 0, 10));
			this.AssertNodeValues (xtr, XmlNodeType.EndElement, 0, false, "root", String.Empty,
				"root", String.Empty, String.Empty, 0);
			Assert (!xtr.Read ());
			AssertEquals (14, bytes2 [0]);
			AssertEquals (54, bytes2 [1]);
			AssertEquals (114, bytes2 [2]);
			AssertEquals (134, bytes2 [3]);
			AssertEquals (184, bytes2 [4]);
			AssertEquals (0, bytes2 [5]);
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

#if NET_2_0
		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ProhibitDtd ()
		{
			XmlTextReader xtr = new XmlTextReader ("<!DOCTYPE root []><root/>", XmlNodeType.Document, null);
			xtr.ProhibitDtd = true;
			while (!xtr.EOF)
				xtr.Read ();
		}
#endif
	}
}
