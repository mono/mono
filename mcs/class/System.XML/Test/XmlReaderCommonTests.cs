//
// System.Xml.XmlReaderCommonTests
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//  Note: Most of testcases are moved from XmlTextReaderTests.cs and
//  XmlNodeReaderTests.cs.
//

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlReaderTests
	{
		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.LoadXml (xml1);
		}

		XmlDocument document;
		const string xml1 = "<root attr1='value1'><child /></root>";
		const string xml2 = "<root><foo/><bar>test.</bar></root>";
		const string xml3 = "<root>  test of <b>mixed</b> string.<![CDATA[ cdata string.]]></root>";
		const string xml4 = "<root>test of <b>mixed</b> string.</root>";
		XmlTextReader xtr;
		XmlNodeReader xnr;

		// copy from XmlTextReaderTests
		private void AssertStartDocument (XmlReader xmlReader)
		{
			Assertion.Assert (xmlReader.ReadState == ReadState.Initial);
			Assertion.Assert (xmlReader.NodeType == XmlNodeType.None);
			Assertion.Assert (xmlReader.Depth == 0);
			Assertion.Assert (!xmlReader.EOF);
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
			Assertion.Assert ("Read() return value", xmlReader.Read ());
			Assertion.Assert ("ReadState", xmlReader.ReadState == ReadState.Interactive);
			Assertion.Assert ("!EOF", !xmlReader.EOF);
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
			Assertion.AssertEquals ("NodeType", nodeType, xmlReader.NodeType);
			Assertion.AssertEquals ("Depth", depth, xmlReader.Depth);
			Assertion.AssertEquals ("IsEmptyElement", isEmptyElement, xmlReader.IsEmptyElement);

			Assertion.AssertEquals ("name", name, xmlReader.Name);

			Assertion.AssertEquals ("prefix", prefix, xmlReader.Prefix);

			Assertion.AssertEquals ("localName", localName, xmlReader.LocalName);

			Assertion.AssertEquals ("namespaceURI", namespaceURI, xmlReader.NamespaceURI);

			Assertion.AssertEquals ("hasValue", (value != String.Empty), xmlReader.HasValue);

			Assertion.AssertEquals ("Value", value, xmlReader.Value);

			Assertion.AssertEquals ("hasAttributes", attributeCount > 0, xmlReader.HasAttributes);

			Assertion.AssertEquals ("attributeCount", attributeCount, xmlReader.AttributeCount);
		}

		private void AssertAttribute (
			XmlReader xmlReader,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value)
		{
			Assertion.AssertEquals ("value", value, xmlReader [name]);

			Assertion.Assert (xmlReader.GetAttribute (name) == value);

			if (namespaceURI != String.Empty) {
				Assertion.Assert (xmlReader[localName, namespaceURI] == value);
				Assertion.Assert (xmlReader.GetAttribute (localName, namespaceURI) == value);
			}
		}

		private void AssertEndDocument (XmlReader xmlReader)
		{
			Assertion.Assert ("could read", !xmlReader.Read ());
			Assertion.AssertEquals ("NodeType is not XmlNodeType.None", XmlNodeType.None, xmlReader.NodeType);
			Assertion.AssertEquals ("Depth is not 0", 0, xmlReader.Depth);
			Assertion.AssertEquals ("ReadState is not ReadState.EndOfFile",  ReadState.EndOfFile, xmlReader.ReadState);
			Assertion.Assert ("not EOF", xmlReader.EOF);

			xmlReader.Close ();
			Assertion.AssertEquals ("ReadState is not ReadState.Cosed", ReadState.Closed, xmlReader.ReadState);
		}

		private delegate void TestMethod (XmlReader reader);

		private void RunTest (string xml, TestMethod method)
		{
			xtr = new XmlTextReader (new StringReader (xml));
			try {
				method (xtr);
			} catch (AssertionException ex) {
				throw new AssertionException ("XmlTextReader failed:  " + ex.Message, ex);
			}

			document.LoadXml (xml);
			xnr = new XmlNodeReader (document);
			try {
				method (xnr);
			} catch (AssertionException ex) {
				throw new AssertionException ("XmlNodeReader failed:  " + ex.Message, ex);
			}
		}





		[Test]
		public void InitialState ()
		{
			RunTest (xml1, new TestMethod (InitialState));
		}

		private void InitialState (XmlReader reader)
		{
			Assertion.AssertEquals ("Depth", 0, reader.Depth);
			Assertion.AssertEquals ("EOF", false, reader.EOF);
			Assertion.AssertEquals ("HasValue", false, reader.HasValue);
			Assertion.AssertEquals ("IsEmptyElement", false, reader.IsEmptyElement);
			Assertion.AssertEquals ("LocalName", String.Empty, reader.LocalName);
			Assertion.AssertEquals ("NodeType", XmlNodeType.None, reader.NodeType);
			Assertion.AssertEquals ("ReadState", ReadState.Initial, reader.ReadState);
		}

		[Test]
		public void Read ()
		{
			RunTest (xml1, new TestMethod (Read));
		}

		public void Read (XmlReader reader)
		{
			reader.Read ();
			Assertion.AssertEquals ("<root>.NodeType", XmlNodeType.Element, reader.NodeType);
			Assertion.AssertEquals ("<root>.Name", "root", reader.Name);
			Assertion.AssertEquals ("<root>.ReadState", ReadState.Interactive, reader.ReadState);
			Assertion.AssertEquals ("<root>.Depth", 0, reader.Depth);

			// move to 'child'
			reader.Read ();
			Assertion.AssertEquals ("<child/>.Depth", 1, reader.Depth);
			Assertion.AssertEquals ("<child/>.NodeType", XmlNodeType.Element, reader.NodeType);
			Assertion.AssertEquals ("<child/>.Name", "child", reader.Name);

			reader.Read ();
			Assertion.AssertEquals ("</root>.Depth", 0, reader.Depth);
			Assertion.AssertEquals ("</root>.NodeType", XmlNodeType.EndElement, reader.NodeType);
			Assertion.AssertEquals ("</root>.Name", "root", reader.Name);

			reader.Read ();
			Assertion.AssertEquals ("end.EOF", true, reader.EOF);
			Assertion.AssertEquals ("end.NodeType", XmlNodeType.None, reader.NodeType);
		}

		[Test]
		public void ReadEmptyElement ()
		{
			RunTest (xml2, new TestMethod (ReadEmptyElement));
		}

		public void ReadEmptyElement (XmlReader reader)
		{
			reader.Read ();	// root
			Assertion.AssertEquals (false, reader.IsEmptyElement);
			reader.Read ();	// foo
			Assertion.AssertEquals ("foo", reader.Name);
			Assertion.AssertEquals (true, reader.IsEmptyElement);
			reader.Read ();	// bar
			Assertion.AssertEquals ("bar", reader.Name);
			Assertion.AssertEquals (false, reader.IsEmptyElement);
		}

		[Test]
		public void ReadStringFromElement ()
		{
			RunTest (xml3, new TestMethod (ReadStringFromElement));
		}

		public void ReadStringFromElement (XmlReader reader)
		{
			// Note: ReadString() test works only when the reader is
			// positioned at the container element.
			// In case the reader is positioned at the first 
			// character node, XmlTextReader and XmlNodeReader works
			// different!!

			reader.Read ();
			string s = reader.ReadString ();
			Assertion.AssertEquals ("readString.1.ret_val", "  test of ", s);
			Assertion.AssertEquals ("readString.1.Name", "b", reader.Name);
			s = reader.ReadString ();
			Assertion.AssertEquals ("readString.2.ret_val", "mixed", s);
			Assertion.AssertEquals ("readString.2.NodeType", XmlNodeType.EndElement, reader.NodeType);
			s = reader.ReadString ();	// never proceeds.
			Assertion.AssertEquals ("readString.3.ret_val", String.Empty, s);
			Assertion.AssertEquals ("readString.3.NodeType", XmlNodeType.EndElement, reader.NodeType);
			reader.Read ();
			Assertion.AssertEquals ("readString.4.NodeType", XmlNodeType.Text, reader.NodeType);
			Assertion.AssertEquals ("readString.4.Value", " string.", reader.Value);
			s = reader.ReadString ();	// reads the same Text node.
			Assertion.AssertEquals ("readString.5.ret_val", " string. cdata string.", s);
			Assertion.AssertEquals ("readString.5.NodeType", XmlNodeType.EndElement, reader.NodeType);
		}

		[Test]
		public void ReadInnerXml ()
		{
			RunTest (xml4, new TestMethod (ReadInnerXml));
		}

		public void ReadInnerXml (XmlReader reader)
		{
			reader.Read ();
			Assertion.AssertEquals ("initial.ReadState", ReadState.Interactive, reader.ReadState);
			Assertion.AssertEquals ("initial.EOF", false, reader.EOF);
			Assertion.AssertEquals ("initial.NodeType", XmlNodeType.Element, reader.NodeType);
		}


		[Test]
		public void EmptyElement ()
		{
			RunTest ("<foo/>", new TestMethod (EmptyElement));
		}
		
		public void EmptyElement (XmlReader xmlReader)
		{

			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, // depth
				true, // isEmptyElement
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
		public void EmptyElementWithStartAndEndTag ()
		{
			string xml = "<foo></foo>";
			RunTest (xml,
				new TestMethod (EmptyElementWithStartAndEndTag));
		}

		public void EmptyElementWithStartAndEndTag (XmlReader xmlReader)
		{
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
		public void NestedEmptyTag ()
		{
			string xml = "<foo><bar/></foo>";
			RunTest (xml, new TestMethod (NestedEmptyTag));
		}

		public void NestedEmptyTag (XmlReader xmlReader)
		{
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
				XmlNodeType.Element, // nodeType
				1, //depth
				true, // isEmptyElement
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
		public void NestedText ()
		{
			string xml = "<foo>bar</foo>";
			RunTest (xml, new TestMethod (NestedText));
		}

		public void NestedText (XmlReader xmlReader)
		{
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
			RunTest (xml, new TestMethod (EmptyElementWithAttribute));
		}

		public void EmptyElementWithAttribute (XmlReader xmlReader)
		{
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
		public void StartAndEndTagWithAttribute ()
		{
			string xml = @"<foo bar='baz'></foo>";
			RunTest (xml, new TestMethod (StartAndEndTagWithAttribute));
		}

		public void StartAndEndTagWithAttribute (XmlReader xmlReader)
		{
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

		[Test]
		public void EmptyElementWithTwoAttributes ()
		{
			string xml = @"<foo bar=""baz"" quux='quuux'/>";
			RunTest (xml, new TestMethod (EmptyElementWithTwoAttributes ));
		}

		public void EmptyElementWithTwoAttributes (XmlReader xmlReader)
		{

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
				2 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"bar", // name
				String.Empty, // prefix
				"bar", // localName
				String.Empty, // namespaceURI
				"baz" // value
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"quux", // name
				String.Empty, // prefix
				"quux", // localName
				String.Empty, // namespaceURI
				"quuux" // value
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void ProcessingInstructionBeforeDocumentElement ()
		{
			string xml = "<?foo bar?><baz/>";
			RunTest (xml, new TestMethod (ProcessingInstructionBeforeDocumentElement));
		}

		public void ProcessingInstructionBeforeDocumentElement (XmlReader xmlReader)
		{
			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.ProcessingInstruction, // nodeType
				0, //depth
				false, // isEmptyElement
				"foo", // name
				String.Empty, // prefix
				"foo", // localName
				String.Empty, // namespaceURI
				"bar", // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				true, // isEmptyElement
				"baz", // name
				String.Empty, // prefix
				"baz", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void CommentBeforeDocumentElement ()
		{
			string xml = "<!--foo--><bar/>";
			RunTest (xml, new TestMethod (CommentBeforeDocumentElement));
		}

		public void CommentBeforeDocumentElement (XmlReader xmlReader)
		{
			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Comment, // nodeType
				0, //depth
				false, // isEmptyElement
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				String.Empty, // namespaceURI
				"foo", // value
				0 // attributeCount
			);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, //depth
				true, // isEmptyElement
				"bar", // name
				String.Empty, // prefix
				"bar", // localName
				String.Empty, // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void PredefinedEntities ()
		{
			string xml = "<foo>&lt;&gt;&amp;&apos;&quot;</foo>";
			RunTest (xml, new TestMethod (PredefinedEntities));
		}

		public void PredefinedEntities (XmlReader xmlReader)
		{
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
				XmlNodeType.Text, // nodeType
				1, //depth
				false, // isEmptyElement
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				String.Empty, // namespaceURI
				"<>&'\"", // value
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
		public void EntityReference ()
		{
			string xml = "<foo>&bar;</foo>";
			RunTest (xml, new TestMethod (EntityReference));
		}

		public void EntityReference (XmlReader xmlReader)
		{
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
			RunTest (xml, new TestMethod (EntityReferenceInsideText));
		}

		public void EntityReferenceInsideText (XmlReader xmlReader)
		{
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
		public void CharacterReferences ()
		{
			string xml = "<foo>&#70;&#x4F;&#x4f;</foo>";
			RunTest (xml, new TestMethod (CharacterReferences));
		}

		public void CharacterReferences (XmlReader xmlReader)
		{
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
				XmlNodeType.Text, // nodeType
				1, //depth
				false, // isEmptyElement
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				String.Empty, // namespaceURI
				"FOO", // value
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

	}
}
