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
using System.Xml.Schema;
using System.Xml.XPath;
#if NET_4_5
using System.Threading;
using System.Threading.Tasks;
#endif

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
			AssertNode ("", xmlReader, nodeType, depth,
				isEmptyElement, name, prefix, localName,
				namespaceURI, value, attributeCount);
		}

		private void AssertNode (
			string label,
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
			Assert.IsTrue (xmlReader.Read (), label + " Read() return value");
			Assert.IsTrue (xmlReader.ReadState == ReadState.Interactive, label + " ReadState");
			Assert.IsTrue (!xmlReader.EOF, label + " !EOF");
			AssertNodeValues (label, xmlReader, nodeType, depth,
				isEmptyElement, name, prefix, localName,
				namespaceURI, value, value != String.Empty,
				attributeCount, attributeCount > 0);
		}

		private void AssertNodeValues (
			string label,
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
			AssertNodeValues (label, xmlReader, nodeType, depth,
				isEmptyElement, name, prefix, localName,
				namespaceURI, value, value != String.Empty,
				attributeCount, attributeCount > 0);
		}

		private void AssertNodeValues (
			string label,
			XmlReader xmlReader,
			XmlNodeType nodeType,
			int depth,
			bool isEmptyElement,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value,
			bool hasValue,
			int attributeCount,
			bool hasAttributes)
		{
			label = String.Concat (label, "(", xmlReader.GetType ().Name, ")");
			Assert.AreEqual (nodeType, xmlReader.NodeType, label + ": NodeType");
			Assert.AreEqual (isEmptyElement, xmlReader.IsEmptyElement, label + ": IsEmptyElement");

			Assert.AreEqual (name, xmlReader.Name, label + ": name");

			Assert.AreEqual (prefix, xmlReader.Prefix, label + ": prefix");

			Assert.AreEqual (localName, xmlReader.LocalName, label + ": localName");

			Assert.AreEqual (namespaceURI, xmlReader.NamespaceURI, label + ": namespaceURI");

			Assert.AreEqual (depth, xmlReader.Depth, label + ": Depth");

			Assert.AreEqual (hasValue, xmlReader.HasValue, label + ": hasValue");

			Assert.AreEqual (value, xmlReader.Value, label + ": Value");

			Assert.AreEqual (hasAttributes, xmlReader.HasAttributes, label + ": hasAttributes");

			Assert.AreEqual (attributeCount, xmlReader.AttributeCount, label + ": attributeCount");
		}

		private void AssertAttribute (
			XmlReader xmlReader,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value)
		{
			Assert.AreEqual (value, xmlReader [name], "value");

			Assert.IsTrue (xmlReader.GetAttribute (name) == value);

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

		private delegate void TestMethod (XmlReader reader);

		private void RunTest (string xml, TestMethod method)
		{
			xtr = new XmlTextReader (new StringReader (xml));
			method (xtr);

			// DTD validation
			xtr = new XmlTextReader (new StringReader (xml));
			XmlValidatingReader xvr = new XmlValidatingReader (xtr);
			xvr.ValidationType = ValidationType.DTD;
			xvr.EntityHandling = EntityHandling.ExpandCharEntities;
			method (xvr);

			// XSD validation
			xtr = new XmlTextReader (new StringReader (xml));
			xvr = new XmlValidatingReader (xtr);
			xvr.EntityHandling = EntityHandling.ExpandCharEntities;
			method (xvr);

			document.XmlResolver = null;
			document.LoadXml (xml);
			xnr = new XmlNodeReader (document);
			method (xnr);
#if NET_2_0
/*
			// XPathNavigatorReader tests
			System.Xml.XPath.XPathDocument doc = new System.Xml.XPath.XPathDocument (new StringReader (xml));
			XmlReader xpr = doc.CreateNavigator ().ReadSubtree ();
			method (xpr);
*/
#endif
		}





		[Test]
		public void InitialState ()
		{
			RunTest (xml1, new TestMethod (InitialState));
		}

		private void InitialState (XmlReader reader)
		{
			Assert.AreEqual (0, reader.Depth, "Depth");
			Assert.AreEqual (false, reader.EOF, "EOF");
			Assert.AreEqual (false, reader.HasValue, "HasValue");
			Assert.AreEqual (false, reader.IsEmptyElement, "IsEmptyElement");
			Assert.AreEqual (String.Empty, reader.LocalName, "LocalName");
			Assert.AreEqual (XmlNodeType.None, reader.NodeType, "NodeType");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "ReadState");
		}

		[Test]
		public void Read ()
		{
			RunTest (xml1, new TestMethod (Read));
		}

		public void Read (XmlReader reader)
		{
			reader.Read ();
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "<root>.NodeType");
			Assert.AreEqual ("root", reader.Name, "<root>.Name");
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "<root>.ReadState");
			Assert.AreEqual (0, reader.Depth, "<root>.Depth");

			// move to 'child'
			reader.Read ();
			Assert.AreEqual (1, reader.Depth, "<child/>.Depth");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "<child/>.NodeType");
			Assert.AreEqual ("child", reader.Name, "<child/>.Name");

			reader.Read ();
			Assert.AreEqual (0, reader.Depth, "</root>.Depth");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "</root>.NodeType");
			Assert.AreEqual ("root", reader.Name, "</root>.Name");

			reader.Read ();
			Assert.AreEqual (true, reader.EOF, "end.EOF");
			Assert.AreEqual (XmlNodeType.None, reader.NodeType, "end.NodeType");
		}

		[Test]
		[Category ("NotDotNet")]
		public void ReadAttributeValue ()
		{
			RunTest ("<root attr=''/>", new TestMethod (ReadAttributeValue));
		}

		public void ReadAttributeValue (XmlReader reader)
		{
			reader.Read ();	// root
			Assert.IsTrue (reader.MoveToFirstAttribute ());
			// It looks like that MS.NET shows AttributeCount and
			// HasAttributes as the same as element node!
			this.AssertNodeValues ("#1",
				reader, XmlNodeType.Attribute,
				1, false, "attr", "", "attr", "", "", true, 1, true);
			Assert.IsTrue (reader.ReadAttributeValue ());
			// MS.NET XmlTextReader fails. Its Prefix returns 
			// null instead of "". It is fixed in MS.NET 2.0.
			this.AssertNodeValues ("#2",
				reader, XmlNodeType.Text,
				2, false, "", "", "", "", "", true, 1, true);
			Assert.IsTrue (reader.MoveToElement ());
			this.AssertNodeValues ("#3",
				reader, XmlNodeType.Element,
				0, true, "root", "", "root", "", "", false, 1, true);
		}

		[Test]
		public void ReadEmptyElement ()
		{
			RunTest (xml2, new TestMethod (ReadEmptyElement));
		}

		public void ReadEmptyElement (XmlReader reader)
		{
			reader.Read ();	// root
			Assert.AreEqual (false, reader.IsEmptyElement);
			reader.Read ();	// foo
			Assert.AreEqual ("foo", reader.Name);
			Assert.AreEqual (true, reader.IsEmptyElement);
			reader.Read ();	// bar
			Assert.AreEqual ("bar", reader.Name);
			Assert.AreEqual (false, reader.IsEmptyElement);
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
			Assert.AreEqual ("  test of ", s, "readString.1.ret_val");
			Assert.AreEqual ("b", reader.Name, "readString.1.Name");
			s = reader.ReadString ();
			Assert.AreEqual ("mixed", s, "readString.2.ret_val");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "readString.2.NodeType");
			s = reader.ReadString ();	// never proceeds.
			Assert.AreEqual (String.Empty, s, "readString.3.ret_val");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "readString.3.NodeType");
			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "readString.4.NodeType");
			Assert.AreEqual (" string.", reader.Value, "readString.4.Value");
			s = reader.ReadString ();	// reads the same Text node.
			Assert.AreEqual (" string. cdata string.", s, "readString.5.ret_val");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "readString.5.NodeType");
		}

		[Test]
		public void ReadInnerXml ()
		{
			const string xml = "<root><foo>test of <b>mixed</b> string.</foo><bar /></root>";
			RunTest (xml, new TestMethod (ReadInnerXml));
		}

		public void ReadInnerXml (XmlReader reader)
		{
			reader.Read ();
			reader.Read ();
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "initial.ReadState");
			Assert.AreEqual (false, reader.EOF, "initial.EOF");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "initial.NodeType");
			string s = reader.ReadInnerXml ();
			Assert.AreEqual ("test of <b>mixed</b> string.", s, "read_all");
			Assert.AreEqual ("bar", reader.Name, "after.Name");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "after.NodeType");
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
		public void NestedEmptyTag ()
		{
			string xml = "<foo><bar/></foo>";
			RunTest (xml, new TestMethod (NestedEmptyTag));
		}

		public void NestedEmptyTag (XmlReader xmlReader)
		{
			AssertStartDocument (xmlReader);

			AssertNode (
				"#1",
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
				"#2",
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
				"#3",
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
		public void EmptyElementWithAttributes ()
		{
			string xml = @"<foo bar=""baz"" quux='quuux' x:foo='x-foo' xmlns:x = 'urn:xfoo' />";
			RunTest (xml, new TestMethod (EmptyElementWithAttributes ));
		}

		public void EmptyElementWithAttributes (XmlReader xmlReader)
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
				4 // attributeCount
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

			AssertAttribute (
				xmlReader, // xmlReader
				"notexist", // name
				String.Empty, // prefix
				"notexist", // localName
				String.Empty, // namespaceURI
				null // value
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"x:foo", // name
				"x", // prefix
				"foo", // localName
				"urn:xfoo", // namespaceURI
				"x-foo" // value
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"x:bar", // name
				"x", // prefix
				"bar", // localName
				"urn:xfoo", // namespaceURI
				null // value
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

		[Test]
		public void PredefinedEntitiesInAttribute ()
		{
			string xml = "<foo bar='&lt;&gt;&amp;&apos;&quot;'/>";
			RunTest (xml, new TestMethod (PredefinedEntitiesInAttribute ));
		}

		public void PredefinedEntitiesInAttribute (XmlReader xmlReader)
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
				"<>&'\"" // value
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void CharacterReferencesInAttribute ()
		{
			string xml = "<foo bar='&#70;&#x4F;&#x4f;'/>";
			RunTest (xml, new TestMethod (CharacterReferencesInAttribute));
		}

		public void CharacterReferencesInAttribute (XmlReader xmlReader)
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
				"FOO" // value
			);

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void CDATA ()
		{
			string xml = "<foo><![CDATA[<>&]]></foo>";
			RunTest (xml, new TestMethod (CDATA));
		}

		public void CDATA (XmlReader xmlReader)
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
				XmlNodeType.CDATA, // nodeType
				1, //depth
				false, // isEmptyElement
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				String.Empty, // namespaceURI
				"<>&", // value
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
		public void EmptyElementInDefaultNamespace ()
		{
			string xml = @"<foo xmlns='http://foo/' />";
			RunTest (xml, new TestMethod (EmptyElementInDefaultNamespace));
		}

		public void EmptyElementInDefaultNamespace (XmlReader xmlReader)
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
				"http://foo/", // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"xmlns", // name
				String.Empty, // prefix
				"xmlns", // localName
				"http://www.w3.org/2000/xmlns/", // namespaceURI
				"http://foo/" // value
			);

			Assert.AreEqual ("http://foo/", xmlReader.LookupNamespace (String.Empty));

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void ChildElementInNamespace ()
		{
			string xml = @"<foo:bar xmlns:foo='http://foo/'><baz:quux xmlns:baz='http://baz/' /></foo:bar>";
			RunTest (xml, new TestMethod (ChildElementInNamespace));
		}

		public void ChildElementInNamespace (XmlReader xmlReader)
		{
			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, // depth
				false, // isEmptyElement
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

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				1, // depth
				true, // isEmptyElement
				"baz:quux", // name
				"baz", // prefix
				"quux", // localName
				"http://baz/", // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"xmlns:baz", // name
				"xmlns", // prefix
				"baz", // localName
				"http://www.w3.org/2000/xmlns/", // namespaceURI
				"http://baz/" // value
			);

			Assert.AreEqual ("http://foo/", xmlReader.LookupNamespace ("foo"));
			Assert.AreEqual ("http://baz/", xmlReader.LookupNamespace ("baz"));

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, // depth
				false, // isEmptyElement
				"foo:bar", // name
				"foo", // prefix
				"bar", // localName
				"http://foo/", // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			Assert.AreEqual ("http://foo/", xmlReader.LookupNamespace ("foo"));
			Assert.IsNull (xmlReader.LookupNamespace ("baz"));

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void ChildElementInDefaultNamespace ()
		{
			string xml = @"<foo:bar xmlns:foo='http://foo/'><baz xmlns='http://baz/' /></foo:bar>";
			RunTest (xml, new TestMethod (ChildElementInDefaultNamespace));
		}

		public void ChildElementInDefaultNamespace (XmlReader xmlReader)
		{
			AssertStartDocument (xmlReader);

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				0, // depth
				false, // isEmptyElement
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

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.Element, // nodeType
				1, // depth
				true, // isEmptyElement
				"baz", // name
				String.Empty, // prefix
				"baz", // localName
				"http://baz/", // namespaceURI
				String.Empty, // value
				1 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"xmlns", // name
				String.Empty, // prefix
				"xmlns", // localName
				"http://www.w3.org/2000/xmlns/", // namespaceURI
				"http://baz/" // value
			);

			Assert.AreEqual ("http://foo/", xmlReader.LookupNamespace ("foo"));
			Assert.AreEqual ("http://baz/", xmlReader.LookupNamespace (String.Empty));

			AssertNode (
				xmlReader, // xmlReader
				XmlNodeType.EndElement, // nodeType
				0, // depth
				false, // isEmptyElement
				"foo:bar", // name
				"foo", // prefix
				"bar", // localName
				"http://foo/", // namespaceURI
				String.Empty, // value
				0 // attributeCount
			);

			Assert.AreEqual ("http://foo/", xmlReader.LookupNamespace ("foo"));

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void AttributeInNamespace ()
		{
			string xml = @"<foo bar:baz='quux' xmlns:bar='http://bar/' />";
			RunTest (xml, new TestMethod (AttributeInNamespace));
		}

		public void AttributeInNamespace (XmlReader xmlReader)
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
				2 // attributeCount
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"bar:baz", // name
				"bar", // prefix
				"baz", // localName
				"http://bar/", // namespaceURI
				"quux" // value
			);

			AssertAttribute (
				xmlReader, // xmlReader
				"xmlns:bar", // name
				"xmlns", // prefix
				"bar", // localName
				"http://www.w3.org/2000/xmlns/", // namespaceURI
				"http://bar/" // value
			);

			Assert.AreEqual ("http://bar/", xmlReader.LookupNamespace ("bar"));

			AssertEndDocument (xmlReader);
		}

		[Test]
		public void MoveToElementFromAttribute ()
		{
			string xml = @"<foo bar=""baz"" />";
			RunTest (xml, new TestMethod (MoveToElementFromAttribute));
		}

		public void MoveToElementFromAttribute (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
			Assert.IsTrue (xmlReader.MoveToFirstAttribute ());
			Assert.AreEqual (XmlNodeType.Attribute, xmlReader.NodeType);
			Assert.IsTrue (xmlReader.MoveToElement ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
		}

		[Test]
		public void MoveToElementFromElement ()
		{
			string xml = @"<foo bar=""baz"" />";
			RunTest (xml, new TestMethod (MoveToElementFromElement));
		}

		public void MoveToElementFromElement (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
			Assert.IsTrue (!xmlReader.MoveToElement ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
		}

		[Test]
		public void MoveToFirstAttributeWithNoAttributes ()
		{
			string xml = @"<foo />";
			RunTest (xml, new TestMethod (MoveToFirstAttributeWithNoAttributes));
		}

		public void MoveToFirstAttributeWithNoAttributes (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
			Assert.IsTrue (!xmlReader.MoveToFirstAttribute ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
		}

		[Test]
		public void MoveToNextAttributeWithNoAttributes ()
		{
			string xml = @"<foo />";
			RunTest (xml, new TestMethod (MoveToNextAttributeWithNoAttributes));
		}

		public void MoveToNextAttributeWithNoAttributes (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
			Assert.IsTrue (!xmlReader.MoveToNextAttribute ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
		}

		[Test]
		public void MoveToNextAttribute()
		{
			string xml = @"<foo bar=""baz"" quux='quuux'/>";
			RunTest (xml, new TestMethod (MoveToNextAttribute));
		}

		public void MoveToNextAttribute (XmlReader xmlReader)
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

			Assert.IsTrue (xmlReader.MoveToNextAttribute ());
			Assert.AreEqual ("bar", xmlReader.Name);
			Assert.AreEqual ("baz", xmlReader.Value);

			Assert.IsTrue (xmlReader.MoveToNextAttribute ());
			Assert.AreEqual ("quux", xmlReader.Name);
			Assert.AreEqual ("quuux", xmlReader.Value);

			Assert.IsTrue (!xmlReader.MoveToNextAttribute ());

			Assert.IsTrue (xmlReader.MoveToElement ());

			AssertNodeValues (
				"#1",
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

			AssertEndDocument (xmlReader);
		}

		[Test]
//		[Category ("NotDotNet")] // MS XmlNodeReader never moves to xml declaration.
		[Ignore ("Too inconsistent reference implementations to determine which is correct behavior.")]
		public void MoveToXmlDeclAttributes ()
		{
			string xml = "<?xml version=\"1.0\" standalone=\"yes\"?><root/>";
			RunTest (xml, new TestMethod (MoveToXmlDeclAttributes));
		}

		public void MoveToXmlDeclAttributes (XmlReader xmlReader)
		{
			xmlReader.Read ();
			this.AssertNodeValues ("#1", xmlReader, 
				XmlNodeType.XmlDeclaration,
				0,
				false,
				"xml",
				String.Empty,
				"xml",
				String.Empty,
				"version=\"1.0\" standalone=\"yes\"",
				2);
			Assert.IsTrue (xmlReader.MoveToFirstAttribute (), "MoveToFirstAttribute");
			this.AssertNodeValues ("#2", xmlReader, 
				XmlNodeType.Attribute,
				0, // FIXME: might be 1
				false,
				"version",
				String.Empty,
				"version",
				String.Empty,
				"1.0",
				2);
			xmlReader.ReadAttributeValue ();
			this.AssertNodeValues ("#3", xmlReader, 
				XmlNodeType.Text,
				1, // FIXME might be 2
				false,
				String.Empty,
				null, // FIXME: should be String.Empty,
				String.Empty,
				null, // FIXME: should be String.Empty,
				"1.0",
				2);
			xmlReader.MoveToNextAttribute ();
			this.AssertNodeValues ("#4", xmlReader, 
				XmlNodeType.Attribute,
				0, // FIXME: might be 1
				false,
				"standalone",
				String.Empty,
				"standalone",
				String.Empty,
				"yes",
				2);
			xmlReader.ReadAttributeValue ();
			this.AssertNodeValues ("#5", xmlReader, 
				XmlNodeType.Text,
				1, // FIXME: might be 2
				false,
				String.Empty,
				null, // FIXME: should be String.Empty,
				String.Empty,
				null, // FIXME: should be String.Empty,
				"yes",
				2);
		}

		[Test]
		public void AttributeOrder ()
		{
			string xml = @"<foo _1='1' _2='2' _3='3' />";
			RunTest (xml, new TestMethod (AttributeOrder));
		}

		public void AttributeOrder (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);

			Assert.IsTrue (xmlReader.MoveToFirstAttribute ());
			Assert.AreEqual ("_1", xmlReader.Name);
			Assert.IsTrue (xmlReader.MoveToNextAttribute ());
			Assert.AreEqual ("_2", xmlReader.Name);
			Assert.IsTrue (xmlReader.MoveToNextAttribute ());
			Assert.AreEqual ("_3", xmlReader.Name);

			Assert.IsTrue (!xmlReader.MoveToNextAttribute ());
		}

		[Test]
		[Category ("NotDotNet")]
		public void IndexerAndAttributes ()
		{
			string xml = @"<?xml version='1.0' standalone='no'?><foo _1='1' _2='2' _3='3' />";
			RunTest (xml, new TestMethod (IndexerAndAttributes));
		}

		public void IndexerAndAttributes (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual ("1.0", xmlReader ["version"]);
			Assert.AreEqual ("1.0", xmlReader.GetAttribute ("version"));
			// .NET 1.1 BUG. XmlTextReader returns null, while XmlNodeReader returns "".
			Assert.AreEqual (null, xmlReader ["encoding"]);
			Assert.AreEqual (null, xmlReader.GetAttribute ("encoding"));
			Assert.AreEqual ("no", xmlReader ["standalone"]);
			Assert.AreEqual ("no", xmlReader.GetAttribute ("standalone"));
			Assert.AreEqual ("1.0", xmlReader [0]);
			Assert.AreEqual ("1.0", xmlReader.GetAttribute (0));
			Assert.AreEqual ("no", xmlReader [1]);
			Assert.AreEqual ("no", xmlReader.GetAttribute (1));

			Assert.IsTrue (xmlReader.Read ());
			Assert.AreEqual (XmlNodeType.Element, xmlReader.NodeType);
			Assert.AreEqual ("1", xmlReader ["_1"]);

			Assert.IsTrue (xmlReader.MoveToFirstAttribute ());
			Assert.AreEqual ("_1", xmlReader.Name);
			Assert.AreEqual ("1", xmlReader ["_1"]);
			Assert.IsTrue (xmlReader.MoveToNextAttribute ());
			Assert.AreEqual ("_2", xmlReader.Name);
			Assert.AreEqual ("1", xmlReader ["_1"]);
			Assert.IsTrue (xmlReader.MoveToNextAttribute ());
			Assert.AreEqual ("_3", xmlReader.Name);
			Assert.AreEqual ("1", xmlReader ["_1"]);

			Assert.IsTrue (!xmlReader.MoveToNextAttribute ());
		}

		[Test]
		public void ProhibitedMultipleAttributes ()
		{
			string xml = @"<foo _1='1' _1='1' />";
			try {
				RunTest (xml, new TestMethod (ReadAll));
			} catch (XmlException) {
			}
			xml = @"<foo _1='1' _1='2' />";
			try {
				RunTest (xml, new TestMethod (ReadAll));
			} catch (XmlException) {
			}
		}

		public void ReadAll (XmlReader xmlReader)
		{
			while (!xmlReader.EOF)
				xmlReader.Read ();
		}

		[Test]
		public void SurrogatePairContent ()
		{
			string xml = "<root xmlns='&#x10100;'/>";
			RunTest (xml, new TestMethod (SurrogatePairContent));
		}

		public void SurrogatePairContent (XmlReader xmlReader)
		{
			xmlReader.Read ();
			Assert.AreEqual (true, xmlReader.MoveToAttribute ("xmlns"));
			Assert.AreEqual ("xmlns", xmlReader.Name);
			Assert.AreEqual (2, xmlReader.Value.Length);
			Assert.AreEqual (0xD800, (int) xmlReader.Value [0]);
			Assert.AreEqual (0xDD00, (int) xmlReader.Value [1]);
		}

		[Test]
		public void ReadOuterXmlOnEndElement ()
		{
			string xml = "<root><foo></foo></root>";
			RunTest (xml, new TestMethod (ReadOuterXmlOnEndElement));
		}

		public void ReadOuterXmlOnEndElement (XmlReader xmlReader)
		{
			xmlReader.Read ();
			xmlReader.Read ();
			xmlReader.Read ();
			Assert.AreEqual (String.Empty, xmlReader.ReadOuterXml ());
		}

		[Test]
		public void ReadInnerXmlOnEndElement ()
		{
			string xml = "<root><foo></foo></root>";
			RunTest (xml, new TestMethod (ReadInnerXmlOnEndElement));
		}

		private void ReadInnerXmlOnEndElement (XmlReader xmlReader)
		{
			xmlReader.Read ();
			xmlReader.Read ();
			xmlReader.Read ();
			Assert.AreEqual (String.Empty, xmlReader.ReadInnerXml ());
		}

		[Test]
		public void LookupEmptyPrefix ()
		{
			string xml = "<root><foo></foo></root>";
			RunTest (xml, new TestMethod (LookupEmptyPrefix));
		}

		void LookupEmptyPrefix (XmlReader xmlReader)
		{
			xmlReader.Read ();
			Assert.IsNull (xmlReader.LookupNamespace (String.Empty));
		}

		[Test]
		public void ReadStartElement ()
		{
			string xml = "<root>test</root>";
			RunTest (xml, new TestMethod (ReadStartElement));
		}

		void ReadStartElement (XmlReader xr)
		{
			xr.Read ();
			xr.ReadStartElement ();
			// consume Element node.
			Assert.AreEqual (XmlNodeType.Text, xr.NodeType);
		}

		[Test]
		public void LookupNamespaceAtEndElement ()
		{
			string xml = "<root xmlns:x='urn:foo'><foo/></root>";
			RunTest (xml, new TestMethod (LookupNamespaceAtEndElement));
		}

		void LookupNamespaceAtEndElement (XmlReader reader)
		{
			reader.Read ();
			Assert.AreEqual ("urn:foo", reader.LookupNamespace ("x"), "#1");
			reader.Read ();
			Assert.AreEqual ("urn:foo", reader.LookupNamespace ("x"), "#2");
			reader.Read ();
			Assert.AreEqual ("urn:foo", reader.LookupNamespace ("x"), "#3");
		}

		[Test]
		public void ReadClosedReader ()
		{
			string xml = "<fin>aaa</fin>";
			RunTest (xml, new TestMethod (ReadClosedReader));
		}

		void ReadClosedReader (XmlReader reader)
		{
			reader.Read ();
			reader.Close();
			reader.Read (); // silently returns false
		}

#if NET_2_0
		[Test]
		public void CreateSimple ()
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ProhibitDtd = false;
			XmlReader xr = XmlReader.Create ("Test/XmlFiles/nested-dtd-test.xml", s);
			xr.Read ();
			Assert.AreEqual (XmlNodeType.DocumentType, xr.NodeType, "#1");
			xr.Read ();
			Assert.AreEqual (XmlNodeType.Whitespace, xr.NodeType, "#2");
			xr.Read ();
			Assert.AreEqual (XmlNodeType.Element, xr.NodeType, "#3");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void CreateSimpleProhibitDtd ()
		{
			XmlReader xr = XmlReader.Create ("Test/XmlFiles/nested-dtd-test.xml");
			xr.Read ();
		}

		[Test]
		// a bit revised version of bug #78706
		public void CreateFromUrlClose ()
		{
			string file = "Test/XmlFiles/78706.xml";
			try {
				if (!File.Exists (file))
					File.Create (file).Close ();
				XmlReaderSettings s = new XmlReaderSettings ();
				s.CloseInput = false; // explicitly
				XmlReader r = XmlReader.Create (file, s);
				r.Close ();
				XmlTextWriter w = new XmlTextWriter (file, null);
				w.Close ();
			} finally {
				if (File.Exists (file))
					File.Delete (file);
			}
		}

		[Test]
		// a bit revised version of bug #385638
		public void CreateFromUrlClose2 ()
		{
			string file = "Test/XmlFiles/385638.xml";
			try {
				if (File.Exists (file))
					File.Delete (file);
				using (TextWriter tw = File.CreateText (file))
					tw.Write ("<xml />");
				XmlReaderSettings s = new XmlReaderSettings ();
				s.IgnoreWhitespace = true; // this results in XmlFilterReader, which is the key for this bug.
				XmlReader r = XmlReader.Create (file, s);
				r.Close ();
				XmlTextWriter w = new XmlTextWriter (file, null);
				w.Close ();
			} finally {
				if (File.Exists (file))
					File.Delete (file);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Create_String_Empty ()
		{
			XmlReader.Create (String.Empty);
		}

		[Test]
		public void ReadToDescendant ()
		{
			string xml = @"<root><foo/><bar/><foo> test text <bar><bar></bar></bar></foo></root>";
			RunTest (xml, new TestMethod (ReadToDescendant));
		}

		void ReadToDescendant (XmlReader xmlReader)
		{
			// move to first <bar/>
			Assert.IsTrue (xmlReader.ReadToDescendant ("bar"), "#1");
			// no children in <bar/>. It is empty.
			Assert.IsTrue (!xmlReader.ReadToDescendant ("bar"), "#2");
			Assert.AreEqual ("bar", xmlReader.Name, "#2-2");

			// move to the second <foo>
			xmlReader.Read ();
			// move to the second <bar>
			Assert.IsTrue (xmlReader.ReadToDescendant ("bar"), "#3");
			// move to <bar> inside <bar>...</bar>
			Assert.IsTrue (xmlReader.ReadToDescendant ("bar"), "#4");
			// the next is EndElement of </bar>, so no move.
			Assert.IsTrue (!xmlReader.ReadToDescendant ("bar"), "#5");
			Assert.AreEqual (XmlNodeType.EndElement, xmlReader.NodeType, "#5-2");
		}

		[Test]
		public void ReadToDescepdant2 ()
		{
			string xml = "<root/>";
			RunTest (xml, new TestMethod (ReadToDescendant2));
		}

		void ReadToDescendant2 (XmlReader xmlReader)
		{
			// make sure that it works when the reader is at Initial state.
			Assert.IsTrue (xmlReader.ReadToDescendant ("root"));
		}

		[Test]
		public void ReadToFollowing ()
		{
			string xml = @"<root><foo/><bar/><foo><bar><bar></bar></bar></foo></root>";
			RunTest (xml, new TestMethod (ReadToFollowing));
		}

		public void ReadToFollowing (XmlReader xmlReader)
		{
			Assert.IsTrue (xmlReader.ReadToFollowing ("bar"), "#1");
			Assert.IsTrue (xmlReader.ReadToFollowing ("bar"), "#2");
			Assert.AreEqual (2, xmlReader.Depth, "#2-2");
			Assert.IsTrue (xmlReader.ReadToFollowing ("bar"), "#3");
			Assert.AreEqual (3, xmlReader.Depth, "#3-2");
			Assert.IsTrue (!xmlReader.ReadToFollowing ("bar"), "#4");
		}

		[Test]
		[Category ("NotDotNet")]
		public void ReadToNextSiblingAtInitialState ()
		{
			string xml = @"<root></root>";
			RunTest (xml, new TestMethod (ReadToNextSiblingAtInitialState ));
		}

		void ReadToNextSiblingAtInitialState (XmlReader xmlReader)
		{
			Assert.IsTrue (!xmlReader.ReadToNextSibling ("bar"), "#1");
			Assert.IsTrue (!xmlReader.ReadToNextSibling ("root"), "#2");
		}

		[Test]
		public void ReadToNextSibling ()
		{
			string xml = @"<root><foo/><bar attr='value'/><foo><pooh/><bar></bar><foo></foo><bar/></foo></root>";
			RunTest (xml, new TestMethod (ReadToNextSibling));
		}

		void ReadToNextSibling (XmlReader xmlReader)
		{
			// It is funky, but without it MS.NET results in an infinite loop.
			xmlReader.Read (); // root

			xmlReader.Read (); // foo
			Assert.IsTrue (xmlReader.ReadToNextSibling ("bar"), "#3");

			Assert.AreEqual ("value", xmlReader.GetAttribute ("attr"), "#3-2");
			xmlReader.Read (); // foo
			xmlReader.Read (); // pooh
			Assert.IsTrue (xmlReader.ReadToNextSibling ("bar"), "#4");
			Assert.IsTrue (!xmlReader.IsEmptyElement, "#4-2");
			Assert.IsTrue (xmlReader.ReadToNextSibling ("bar"), "#5");
			Assert.IsTrue (xmlReader.IsEmptyElement, "#5-2");
			Assert.IsTrue (xmlReader.Read (), "#6"); // /foo

			AssertNodeValues ("#7", xmlReader,
				XmlNodeType.EndElement,
				1,		// Depth
				false,		// IsEmptyElement
				"foo",		// Name
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes
		}

		// bug #81451
		[Test]
		public void ReadToNextSibling2 ()
		{
			string xml = @"<root><baz><bar><foo attr='value'/></bar><foo attr='value2'><bar><foo /></bar></foo></baz></root>";
			RunTest (xml, new TestMethod (ReadToNextSibling2));
		}

		void ReadToNextSibling2 (XmlReader r)
		{
			r.MoveToContent (); // ->root
			r.Read (); // root->baz
			r.Read (); // baz->bar
			Assert.IsTrue (r.ReadToNextSibling ("foo"), "#1");
			Assert.AreEqual ("value2", r.GetAttribute ("attr"), "#2");
			r.Read (); // foo[@value='value2']->bar
			Assert.IsTrue (!r.ReadToNextSibling ("foo"), "#3");
			Assert.AreEqual (XmlNodeType.EndElement, r.NodeType, "#4");
			Assert.AreEqual ("foo", r.LocalName, "#5");
		}

		// bug #347768
		[Test]
		public void ReadToNextSibling3 ()
		{
			string xml = @" <books> <book> <name>Happy C Sharp</name> </book> </books>";
			XmlReader reader = XmlReader.Create (new StringReader (xml));

			reader.MoveToContent ();

			while (reader.Read ())
				reader.ReadToNextSibling ("book"); // should not result in an infinite loop
		}

		// bug #676020
		[Test]
		public void ReadToNextSibling4 ()
		{
			string xml = @"<SerializableStringDictionary>
<SerializableStringDictionary>
<DictionaryEntry Key=""Key1"" Value=""Value1""/>
<DictionaryEntry Key=""Key2"" Value=""Value2""/>
<DictionaryEntry Key=""Key3"" Value=""Value3""/>
</SerializableStringDictionary>
</SerializableStringDictionary>";

			var reader = XmlReader.Create (new StringReader (xml));

			Assert.IsTrue (reader.ReadToDescendant ("SerializableStringDictionary"), "#1");
			Assert.IsTrue (reader.ReadToDescendant ("DictionaryEntry"), "#2");

			int count = 0;
			do {
				reader.MoveToAttribute ("Key");
				var key = reader.ReadContentAsString ();
				reader.MoveToAttribute ("Value");
				var value = reader.ReadContentAsString ();
				count++;
			}
			while (reader.ReadToNextSibling ("DictionaryEntry"));
			Assert.AreEqual (3, count, "#3");
		}

		[Test]
		public void ReadSubtree ()
		{
			string xml = @"<root><foo/><bar attr='value'></bar></root>";
			RunTest (xml, new TestMethod (ReadSubtree));
		}

		void ReadSubtree (XmlReader reader)
		{
			reader.MoveToContent (); // root
			reader.Read (); // foo
			XmlReader st = reader.ReadSubtree (); // <foo/>

			// MS bug: IsEmptyElement should be false here.
			/*
			AssertNodeValues ("#1", st,
				XmlNodeType.None,
				0,		// Depth
				false,		// IsEmptyElement
				String.Empty,	// Name
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes
			*/
			Assert.AreEqual (XmlNodeType.None, st.NodeType, "#1");

			st.Read ();
			AssertNodeValues ("#2", st,
				XmlNodeType.Element,
				0,
				true,		// IsEmptyElement
				"foo",		// Name
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes

			Assert.IsTrue (!st.Read (), "#3");

			// At this state, reader is not positioned on <bar> yet
			AssertNodeValues ("#3-2", reader,
				XmlNodeType.Element,
				1,		// Depth. It is 1 for main tree.
				true,		// IsEmptyElement
				"foo",		// Name
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes

			reader.Read ();

			AssertNodeValues ("#4", reader,
				XmlNodeType.Element,
				1,		// Depth. It is 1 for main tree.
				false,		// IsEmptyElement
				"bar",		// Name
				String.Empty,	// Prefix
				"bar",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				1,		// AttributeCount
				true);		// HasAttributes

			st = reader.ReadSubtree ();
			st.Read (); // Initial -> Interactive
			AssertNodeValues ("#5", st,
				XmlNodeType.Element,
				0,		// Depth. It is 0 for subtree.
				false,		// IsEmptyElement
				"bar",		// Name
				String.Empty,	// Prefix
				"bar",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				1,		// AttributeCount
				true);		// HasAttributes

			st.Read ();
			AssertNodeValues ("#6-1", st,
				XmlNodeType.EndElement,
				0,		// Depth. It is 0 for subtree.
				false,		// IsEmptyElement
				"bar",		// Name
				String.Empty,	// Prefix
				"bar",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes

			AssertNodeValues ("#6-2", st,
				XmlNodeType.EndElement,
				0,		// Depth. It is 0 for subtree.
				false,		// IsEmptyElement
				"bar",		// Name
				String.Empty,	// Prefix
				"bar",		// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes

			Assert.IsTrue (!st.Read (), "#7");
		}

		[Test]
		public void ReadInteger ()
		{
			string xml1 = "<root>1</root>";
			XmlReader xr;
			
			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			Assert.AreEqual ("1", xr.ReadElementContentAsString (), "#1");

			AssertNodeValues ("#1-2", xr,
				XmlNodeType.None,
				0,		// Depth. It is 0 for subtree.
				false,		// IsEmptyElement
				String.Empty,	// Name
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Value
				false,		// HasValue
				0,		// AttributeCount
				false);		// HasAttributes

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			// this XmlReader has no schema, thus the value is untyped
			Assert.AreEqual ("1", xr.ReadElementContentAsObject (), "#2");

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			xr.Read ();
			Assert.AreEqual ("1", xr.ReadContentAsString (), "#3");

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			Assert.AreEqual (1, xr.ReadElementContentAsInt (), "#4");

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			Assert.AreEqual (1, xr.ReadElementContentAs (typeof (int), null), "#5");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadContentAsIntFail ()
		{
			XmlReader xr = XmlReader.Create (
				new StringReader ("<doc>1.0</doc>"));
			xr.Read ();
			xr.ReadElementContentAsInt ();
		}

		[Test]
		public void ReadDateTime ()
		{
			DateTime time = new DateTime (2006, 1, 2, 3, 4, 56);
			string xml1 = "<root>2006-01-02T03:04:56</root>";
			XmlReader xr;

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			// this XmlReader has no schema, thus the value is untyped
			Assert.AreEqual ("2006-01-02T03:04:56", xr.ReadElementContentAsString (), "#1");

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			xr.Read ();
			Assert.AreEqual (time, xr.ReadContentAsDateTime (), "#2");

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			Assert.AreEqual (time, xr.ReadElementContentAsDateTime (), "#3");

			xr = XmlReader.Create (new StringReader (xml1));
			xr.Read ();
			Assert.AreEqual (time, xr.ReadElementContentAs (typeof (DateTime), null), "#4");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadContentAsDateTimeFail ()
		{
			XmlReader xr = XmlReader.Create (
				new StringReader ("<doc>P1Y2M3D</doc>"));
			xr.Read ();
			xr.ReadElementContentAsDateTime ();
		}

		[Test]
		public void ReadContentAs_QNameEmptyNSResolver ()
		{
			XmlReader xr = XmlReader.Create (
				new StringReader ("<doc xmlns:x='urn:foo'>x:el</doc>"));
			xr.Read ();
			object o = xr.ReadElementContentAs (
				typeof (XmlQualifiedName), null);
			// without IXmlNamespaceResolver, it still resolves
			// x:el as valid QName.
			Assert.IsNotNull (o, "#1");
			XmlQualifiedName q = o as XmlQualifiedName;
			Assert.AreEqual (new XmlQualifiedName ("el", "urn:foo"), q, "#2 : " + o.GetType ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadContentStringOnElementFail ()
		{
			XmlReader xr = XmlReader.Create (new StringReader ("<a>test</a>"));
			xr.Read ();
			xr.ReadContentAsString ();
		}

		[Test]
		public void ReadContentStringOnEndElement ()
		{
			XmlReader xr = XmlReader.Create (new StringReader ("<a>test</a>"));
			xr.Read ();
			xr.Read ();
			xr.Read ();
			Assert.AreEqual (String.Empty, xr.ReadContentAsString ()); // does not fail, unlike at Element!
		}

		[Test]
		public void ReadContentStringOnPI ()
		{
			XmlReader xr = XmlReader.Create (new StringReader ("<?pi ?><a>test</a>"));
			xr.Read ();
			Assert.AreEqual (String.Empty, xr.ReadContentAsString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // unlike ReadContentAsString()
		public void ReadElementContentStringOnPI ()
		{
			XmlReader xr = XmlReader.Create (new StringReader ("<?pi ?><a>test</a>"));
			xr.Read ();
			Assert.AreEqual (XmlNodeType.ProcessingInstruction, xr.NodeType);
			xr.ReadElementContentAsString ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadElementContentStringMixedContent ()
		{
			XmlReader xr = XmlReader.Create (
				new StringReader ("<doc>123<child>456</child>789</doc>"));
			xr.Read ();
			// "child" is regarded as an invalid node.
			string s = xr.ReadElementContentAsString ();
		}

		[Test]
		public void ReadContentStringMixedContent ()
		{
			XmlReader xr = XmlReader.Create (
				new StringReader ("<doc>123<child>456</child>789</doc>"));
			xr.Read ();
			xr.Read (); // from Text "123"
			string s = xr.ReadContentAsString ();
			Assert.AreEqual ("123", s, "#1");
			Assert.AreEqual (XmlNodeType.Element, xr.NodeType, "#2");
		}

		[Test]
		public void ReadElementContentAsString ()
		{
			XmlTextReader r = new XmlTextReader (
				"<root/>", XmlNodeType.Document, null);
			r.Read ();
			Assert.AreEqual (String.Empty, r.ReadElementContentAsString (), "#1");
			Assert.AreEqual (XmlNodeType.None, r.NodeType, "#2");
		}

		[Test]
		public void ReadElementContentAs ()
		{
			// as System.Object

			XmlTextReader r = new XmlTextReader (
				"<root/>", XmlNodeType.Document, null);
			r.Read ();
			Assert.AreEqual (String.Empty, r.ReadElementContentAs (typeof (object), null), "#1");
			Assert.AreEqual (XmlNodeType.None, r.NodeType, "#2");

			// regardless of its value, the return value is string.
			r = new XmlTextReader ("<root>1</root>", XmlNodeType.Document, null);
			r.Read ();
			Assert.AreEqual ("1", r.ReadElementContentAs (typeof (object), null), "#3");
			Assert.AreEqual (XmlNodeType.None, r.NodeType, "#4");
		}

		[Test]
		public void ReadContentStringOnAttribute ()
		{
			string xml = @"<root id='myId'><child /></root>";
			RunTest (xml, new TestMethod (ReadContentStringOnAttribute));
		}

		void ReadContentStringOnAttribute (XmlReader reader)
		{
			reader.Read ();
			Assert.IsTrue (reader.MoveToAttribute ("id"));
			Assert.AreEqual ("myId", reader.ReadContentAsString ());
		}

		[Test]
		public void ReadElementContentAsStringEmpty ()
		{
			string xml = "<root><sample/></root>";
			RunTest (xml, new TestMethod (ReadElementContentAsStringEmpty));
		}

		void ReadElementContentAsStringEmpty (XmlReader reader)
		{
			reader.MoveToContent ();
			reader.Read ();
			Assert.AreEqual (String.Empty, reader.ReadElementContentAsString ("sample", ""));
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType);
		}

		[Test]
		public void ReadSubtreeClose ()
		{
			// bug #334752
			string xml = @"<root><item-list><item id='a'/><item id='b'/></item-list></root>";
			RunTest (xml, new TestMethod (ReadSubtreeClose));
		}

		void ReadSubtreeClose (XmlReader reader)
		{
			reader.ReadToFollowing ("item-list");
			XmlReader sub = reader.ReadSubtree ();
			sub.ReadToDescendant ("item");
			sub.Close ();
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#1");
			Assert.AreEqual ("item-list", reader.Name, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadSubtreeOnNonElement ()
		{
			string xml = @"<x> <y/></x>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			r.Read (); // x
			r.Read (); // ws
			r.ReadSubtree ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadSubtreeOnNonElement2 ()
		{
			string xml = @"<x> <y/></x>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			r.ReadSubtree ();
		}

		[Test]
		public void ReadSubtreeEmptyElement ()
		{
			string xml = @"<x/>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			r.Read ();
			XmlReader s = r.ReadSubtree ();
			Assert.IsTrue (s.Read (), "#1");
			Assert.AreEqual (XmlNodeType.Element, s.NodeType, "#2");
			Assert.IsTrue (!s.Read (), "#3");
			Assert.AreEqual (XmlNodeType.None, s.NodeType, "#4");
		}

		[Test]
		public void ReadSubtreeEmptyElementWithAttribute ()
		{
			string xml = @"<root><x a='b'/></root>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			r.Read ();
			r.Read ();
			XmlReader r2 = r.ReadSubtree ();
			Console.WriteLine ("X");
			r2.Read ();
			XmlReader r3 = r2.ReadSubtree ();
			r2.MoveToFirstAttribute ();
			Assert.IsTrue (!r.IsEmptyElement, "#1");
			Assert.IsTrue (!r2.IsEmptyElement, "#2");
			r3.Close ();
			Assert.IsTrue (r.IsEmptyElement, "#3");
			Assert.IsTrue (r2.IsEmptyElement, "#4");
			r2.Close ();
			Assert.IsTrue (r.IsEmptyElement, "#5");
			Assert.IsTrue (r2.IsEmptyElement, "#6");
		}

		[Test]
		public void ReadContentAsBase64 ()
		{
			byte[] randomData = new byte[24];
			for (int i = 0; i < randomData.Length; i++)
				randomData [i] = (byte) i;

			string xmlString = "<?xml version=\"1.0\"?><data>" +
			Convert.ToBase64String (randomData) + "</data>";
			TextReader textReader = new StringReader (xmlString);
			XmlReader xmlReader = XmlReader.Create (textReader);
			xmlReader.ReadToFollowing ("data");

			int readBytes = 0;
			byte[] buffer = new byte [24];

			xmlReader.ReadStartElement ();
			readBytes = xmlReader.ReadContentAsBase64 (buffer, 0, buffer.Length);
			Assert.AreEqual (24, readBytes, "#1");
			Assert.AreEqual (0, xmlReader.ReadContentAsBase64 (buffer, 0, buffer.Length), "#2");
			StringWriter sw = new StringWriter ();
			foreach (byte b in buffer) sw.Write ("{0:X02}", b);
			Assert.AreEqual ("000102030405060708090A0B0C0D0E0F1011121314151617", sw.ToString (), "#3");
		}

		[Test]
		public void ReadContentAsBase64_2 () // bug #480066
		{
			StringReader readerString = new StringReader ("<root><b64>TWFu</b64><b64>TWFu</b64>\r\n\t<b64>TWFu</b64><b64>TWFu</b64></root>");
			XmlReaderSettings settingsXml = new XmlReaderSettings ();
			settingsXml.XmlResolver = null;
			using (var readerXml = XmlReader.Create (readerString, settingsXml)) {
				readerXml.MoveToContent ();
				readerXml.Read ();
				readerXml.ReadStartElement ("b64");
				const int bufferLength = 1024;
				byte [] buffer = new byte [bufferLength];
				readerXml.ReadContentAsBase64 (buffer, 0, bufferLength);
				Assert.AreEqual (XmlNodeType.EndElement, readerXml.NodeType, "#1");
				readerXml.Read ();
				Assert.AreEqual (XmlNodeType.Element, readerXml.NodeType, "#2");
			}
		}
		
		[Test]
		public void ReadContentAsBase64_3 () // bug #543332			
		{
			byte [] fakeState = new byte[25];
			byte [] fixedSizeBuffer = new byte [25];
			byte [] readDataBuffer = new byte [25];
			var ms = new MemoryStream ();
			var xw = XmlWriter.Create (ms);
			xw.WriteStartElement ("root");
			xw.WriteBase64 (fakeState, 0, fakeState.Length);
			xw.WriteEndElement ();
			xw.Close ();
			var reader = XmlReader.Create (new MemoryStream (ms.ToArray ()));
			reader.MoveToContent ();
			// we cannot completely trust the length read to indicate the end.
			int bytesRead;
			bytesRead = reader.ReadElementContentAsBase64 (fixedSizeBuffer, 0, fixedSizeBuffer.Length);
			Assert.AreEqual (25, bytesRead, "#1");
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#2");
			bytesRead = reader.ReadElementContentAsBase64 (fixedSizeBuffer, 0, fixedSizeBuffer.Length);
			Assert.AreEqual (0, bytesRead, "#3");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4");
		}

		[Test]
		public void ReadElementContentAsQNameDefaultNS ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw);
			xw.WriteStartElement ("", "foo", "urn:foo");
			xw.WriteValue (new XmlQualifiedName ("x", "urn:foo"));
			xw.WriteEndElement ();
			xw.Close ();
			var xr = XmlReader.Create (new StringReader (sw.ToString ()));
			xr.MoveToContent ();
			var q = (XmlQualifiedName) xr.ReadElementContentAs (typeof (XmlQualifiedName), xr as IXmlNamespaceResolver);
			Assert.AreEqual ("urn:foo", q.Namespace, "#1");
		}

		[Test]
		public void ReadElementContentAsArray ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw);
			xw.WriteStartElement ("root");
			xw.WriteAttributeString ("xmlns", "b", "http://www.w3.org/2000/xmlns/", "urn:bar");
			var arr = new XmlQualifiedName [] { new XmlQualifiedName ("foo"), new XmlQualifiedName ("bar", "urn:bar") };
			xw.WriteValue (arr);
			xw.Close ();
			var xr = XmlReader.Create (new StringReader (sw.ToString ()));
			xr.MoveToContent ();
			var ret = xr.ReadElementContentAs (typeof (XmlQualifiedName []), null) as XmlQualifiedName [];
			Assert.IsNotNull (ret, "#1");
			Assert.AreEqual (arr [0], ret [0], "#2");
			Assert.AreEqual (arr [1], ret [1], "#3");
		}

#if NET_4_5
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MustSetAsyncFlag ()
		{
			var r = XmlReader.Create (new StringReader ("<root/>"));
			r.ReadAsync ();
		}

		Exception RunAsync (Action action)
		{
			var task = Task<Exception>.Run (async () => {
				try {
					action ();
					return null;
				} catch (Exception ex) {
					return ex;
				}
			});
			task.Wait ();
			Assert.That (task.IsCompleted);
			return task.Result;
		}

		[Test]
		public void SimpleAsync ()
		{
			var xml = "<root test=\"monkey\"/>";
			var task = Task<Exception>.Run (async () => {
				try {
					var s = new XmlReaderSettings ();
					s.Async = true;
					var r = XmlReader.Create (new StringReader (xml), s);

					Assert.That (await r.ReadAsync ());
					Assert.That (r.MoveToFirstAttribute ());

					Assert.AreEqual (await r.GetValueAsync (), "monkey");
					r.Close ();
					return null;
				} catch (Exception ex) {
					return ex;
				}
			});
			task.Wait ();
			Assert.That (task.IsCompleted);
			if (task.Result != null)
				throw task.Result;
		}
#endif
#endif
	}
}
