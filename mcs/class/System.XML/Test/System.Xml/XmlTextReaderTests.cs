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
			AssertEquals ("value", value, xmlReader [name]);

			Assert (xmlReader.GetAttribute (name) == value);

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
		public void ExternalDocument ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load ("XmlFiles/nested-dtd-test.xml");
		}
	}
}
