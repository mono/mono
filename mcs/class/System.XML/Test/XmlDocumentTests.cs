using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlDocumentTests : TestCase
	{
		public XmlDocumentTests () : base ("MonoTests.System.Xml.XmlDocumentTests testsuite") {}
		public XmlDocumentTests (string name) : base (name) {}

		private XmlDocument document;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
		}

		public void TestCreateNodeNodeTypeNameEmptyParams ()
		{
			XmlNode node;

			try {
				node = document.CreateNode (null, null, null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				node = document.CreateNode ("attribute", null, null);
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				node = document.CreateNode ("attribute", "", null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				node = document.CreateNode ("element", null, null);
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				node = document.CreateNode ("element", "", null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			try {
				node = document.CreateNode ("entityreference", null, null);
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}
		}

		public void TestCreateNodeInvalidXmlNodeType ()
		{
			XmlNode node;

			try {
				node = document.CreateNode (XmlNodeType.EndElement, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.EndEntity, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.Entity, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.None, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				node = document.CreateNode (XmlNodeType.Notation, null, null);
				Fail ("Expected an ArgumentOutOfRangeException to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			// TODO:  undocumented allowable type.
			node = document.CreateNode (XmlNodeType.XmlDeclaration, null, null);
			AssertEquals (XmlNodeType.XmlDeclaration, node.NodeType);
		}

		public void TestCreateNodeWhichParamIsUsed ()
		{
			XmlNode node;

			// No constructor params for Document, DocumentFragment.

			node = document.CreateNode (XmlNodeType.CDATA, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlCDataSection)node).Value);

			node = document.CreateNode (XmlNodeType.Comment, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlComment)node).Value);

			node = document.CreateNode (XmlNodeType.DocumentType, "a", "b", "c");
			AssertNull (((XmlDocumentType)node).Value);

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode (XmlNodeType.EntityReference, "a", "b", "c");
//			AssertNull (((XmlEntityReference)node).Value);

			node = document.CreateNode (XmlNodeType.ProcessingInstruction, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlProcessingInstruction)node).Value);

			node = document.CreateNode (XmlNodeType.SignificantWhitespace, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlSignificantWhitespace)node).Value);

			node = document.CreateNode (XmlNodeType.Text, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlText)node).Value);

			node = document.CreateNode (XmlNodeType.Whitespace, "a", "b", "c");
			AssertEquals (String.Empty, ((XmlWhitespace)node).Value);

			node = document.CreateNode (XmlNodeType.XmlDeclaration, "a", "b", "c");
			AssertEquals ("version=\"1.0\"", ((XmlDeclaration)node).Value);
		}

		public void TestCreateNodeNodeTypeName ()
		{
			XmlNode node;

			try {
				node = document.CreateNode ("foo", null, null);
				Fail ("Expected an ArgumentException to be thrown.");
			} catch (ArgumentException) {}

			node = document.CreateNode("attribute", "foo", null);
			AssertEquals (XmlNodeType.Attribute, node.NodeType);

			node = document.CreateNode("cdatasection", null, null);
			AssertEquals (XmlNodeType.CDATA, node.NodeType);

			node = document.CreateNode("comment", null, null);
			AssertEquals (XmlNodeType.Comment, node.NodeType);

			node = document.CreateNode("document", null, null);
			AssertEquals (XmlNodeType.Document, node.NodeType);
			// TODO: test which constructor this ended up calling,
			// i.e. reuse underlying NameTable or not?

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode("documentfragment", null, null);
//			AssertEquals (XmlNodeType.DocumentFragment, node.NodeType);

			node = document.CreateNode("documenttype", null, null);
			AssertEquals (XmlNodeType.DocumentType, node.NodeType);

			node = document.CreateNode("element", "foo", null);
			AssertEquals (XmlNodeType.Element, node.NodeType);

// TODO: add this back in to test when it's implemented.
//			node = document.CreateNode("entityreference", "foo", null);
//			AssertEquals (XmlNodeType.EntityReference, node.NodeType);

			node = document.CreateNode("processinginstruction", null, null);
			AssertEquals (XmlNodeType.ProcessingInstruction, node.NodeType);

			node = document.CreateNode("significantwhitespace", null, null);
			AssertEquals (XmlNodeType.SignificantWhitespace, node.NodeType);

			node = document.CreateNode("text", null, null);
			AssertEquals (XmlNodeType.Text, node.NodeType);

			node = document.CreateNode("whitespace", null, null);
			AssertEquals (XmlNodeType.Whitespace, node.NodeType);
		}

		public void TestDocumentElement ()
		{
			AssertNull (document.DocumentElement);
			XmlElement element = document.CreateElement ("foo", "bar", "http://foo/");
			AssertNotNull (element);

			AssertEquals ("foo", element.Prefix);
			AssertEquals ("bar", element.LocalName);
			AssertEquals ("http://foo/", element.NamespaceURI);

			AssertEquals ("foo:bar", element.Name);

			AssertSame (element, document.AppendChild (element));

			AssertSame (element, document.DocumentElement);
		}

		public void TestDocumentEmpty()
		{
			AssertEquals ("Incorrect output for empty document.", "", document.OuterXml);
		}

		public void TestInnerAndOuterXml ()
		{
			AssertEquals (String.Empty, document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlDeclaration declaration = document.CreateXmlDeclaration ("1.0", null, null);
			document.AppendChild (declaration);
			AssertEquals ("<?xml version=\"1.0\"?>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlElement element = document.CreateElement ("foo");
			document.AppendChild (element);
			AssertEquals ("<?xml version=\"1.0\"?><foo />", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlComment comment = document.CreateComment ("bar");
			document.DocumentElement.AppendChild (comment);
			AssertEquals ("<?xml version=\"1.0\"?><foo><!--bar--></foo>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			XmlText text = document.CreateTextNode ("baz");
			document.DocumentElement.AppendChild (text);
			AssertEquals ("<?xml version=\"1.0\"?><foo><!--bar-->baz</foo>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);

			element = document.CreateElement ("quux");
			element.SetAttribute ("quuux", "squonk");
			document.DocumentElement.AppendChild (element);
			AssertEquals ("<?xml version=\"1.0\"?><foo><!--bar-->baz<quux quuux=\"squonk\" /></foo>", document.InnerXml);
			AssertEquals (document.InnerXml, document.OuterXml);
		}

		public void TestLoadXmlCDATA ()
		{
			document.LoadXml ("<foo><![CDATA[bar]]></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.CDATA);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXMLComment()
		{
// XmlTextReader needs to throw this exception
//			try {
//				document.LoadXml("<!--foo-->");
//				Fail("XmlException should have been thrown.");
//			}
//			catch (XmlException e) {
//				AssertEquals("Exception message doesn't match.", "The root element is missing.", e.Message);
//			}

			document.LoadXml ("<foo><!--Comment--></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Comment);
			AssertEquals ("Comment", document.DocumentElement.FirstChild.Value);

			document.LoadXml (@"<foo><!--bar--></foo>");
			AssertEquals ("Incorrect target.", "bar", ((XmlComment)document.FirstChild.FirstChild).Data);
		}

		public void TestLoadXmlElementSingle ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo/>");

			AssertNotNull (document.DocumentElement);
			AssertSame (document.FirstChild, document.DocumentElement);

			AssertEquals (String.Empty, document.DocumentElement.Prefix);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals (String.Empty, document.DocumentElement.NamespaceURI);
			AssertEquals ("foo", document.DocumentElement.Name);
		}

		public void TestLoadXmlElementWithAttributes ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo bar='baz' quux='quuux'/>");

			XmlElement documentElement = document.DocumentElement;

			AssertEquals ("baz", documentElement.GetAttribute ("bar"));
			AssertEquals ("quuux", documentElement.GetAttribute ("quux"));
		}
		public void TestLoadXmlElementWithChildElement ()
		{
			document.LoadXml ("<foo><bar/></foo>");
			Assert (document.ChildNodes.Count == 1);
			Assert (document.FirstChild.ChildNodes.Count == 1);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals ("bar", document.DocumentElement.FirstChild.LocalName);
		}

		public void TestLoadXmlElementWithTextNode ()
		{
			document.LoadXml ("<foo>bar</foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Text);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXmlExceptionClearsDocument ()
		{
			document.LoadXml ("<foo/>");
			Assert (document.FirstChild != null);
			
			try {
				document.LoadXml ("<123/>");
				Fail ("An XmlException should have been thrown.");
			} catch (XmlException) {}

			Assert (document.FirstChild == null);
		}

		public void TestLoadXmlProcessingInstruction ()
		{
			document.LoadXml (@"<?foo bar='baaz' quux='quuux'?><quuuux></quuuux>");
			AssertEquals ("Incorrect target.", "foo", ((XmlProcessingInstruction)document.FirstChild).Target);
			AssertEquals ("Incorrect data.", "bar='baaz' quux='quuux'", ((XmlProcessingInstruction)document.FirstChild).Data);
		}

		public void TestOuterXml ()
		{
			string xml;
			
			xml = "<root><![CDATA[foo]]></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with cdata OuterXml is incorrect.", xml, document.OuterXml);

			xml = "<root><!--foo--></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with comment OuterXml is incorrect.", xml, document.OuterXml);

			xml = "<root><?foo bar?></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with processing instruction OuterXml is incorrect.", xml, document.OuterXml);
		}

		public void TestParentNodes ()
		{
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XmlNode node = document.FirstChild.FirstChild.FirstChild;
			AssertEquals ("Wrong child found.", "baz", node.LocalName);
			AssertEquals ("Wrong parent.", "bar", node.ParentNode.LocalName);
			AssertEquals ("Wrong parent.", "foo", node.ParentNode.ParentNode.LocalName);
			AssertEquals ("Wrong parent.", "#document", node.ParentNode.ParentNode.ParentNode.LocalName);
			AssertNull ("Expected parent to be null.", node.ParentNode.ParentNode.ParentNode.ParentNode);
		}

		public void TestRemovedElementNextSibling ()
		{
			XmlNode node;
			XmlNode nextSibling;

			document.LoadXml ("<foo><child1/><child2/></foo>");
			node = document.DocumentElement.FirstChild;
			document.DocumentElement.RemoveChild (node);
			nextSibling = node.NextSibling;
			AssertNull ("Expected removed node's next sibling to be null.", nextSibling);
		}
	}
}
