//
// MonoTests.System.Xml.XPathNavigatorCommonTests
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XPathNavigatorCommonTests : Assertion
	{
		XmlDocument document;
		XPathNavigator nav;
		XPathDocument xpathDocument;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}

		private XPathNavigator GetXmlDocumentNavigator (string xml)
		{
			document.LoadXml (xml);
			return document.CreateNavigator ();
		}
		
		private XPathNavigator GetXPathDocumentNavigator (XmlNode node)
		{
			XmlNodeReader xr = new XmlNodeReader (node);
			xpathDocument = new XPathDocument (xr);
			return xpathDocument.CreateNavigator ();
		}

		private void AssertNavigator (XPathNavigator nav, XPathNodeType type, string prefix, string localName, string ns, string name, string value, bool hasAttributes, bool hasChildren, bool isEmptyElement)
		{
			AssertEquals ("NodeType", type, nav.NodeType);
			AssertEquals ("Prefix", prefix, nav.Prefix);
			AssertEquals ("LocalName", localName, nav.LocalName);
			AssertEquals ("Namespace", ns, nav.NamespaceURI);
			AssertEquals ("Name", name, nav.Name);
			AssertEquals ("Value", value, nav.Value);
			AssertEquals ("HasAttributes", hasAttributes, nav.HasAttributes);
			AssertEquals ("HasChildren", hasChildren, nav.HasChildren);
			AssertEquals ("IsEmptyElement", isEmptyElement, nav.IsEmptyElement);
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			string xml = "<?xml version=\"1.0\" standalone=\"yes\"?><foo>bar</foo>";

			nav = GetXmlDocumentNavigator (xml);
			DocumentWithXmlDeclaration (nav);
			nav = GetXPathDocumentNavigator (document);
			DocumentWithXmlDeclaration (nav);
		}

		public void DocumentWithXmlDeclaration (XPathNavigator nav)
		{
			nav.MoveToFirstChild ();
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "bar", false, true, false);
		}

		[Test]
		public void DocumentWithProcessingInstruction ()
		{
			string xml = "<?xml-stylesheet href='foo.xsl' type='text/xsl' ?><foo />";

			nav = GetXmlDocumentNavigator (xml);
			DocumentWithProcessingInstruction (nav);
			nav = GetXPathDocumentNavigator (document);
			DocumentWithProcessingInstruction (nav);
		}

		public void DocumentWithProcessingInstruction (XPathNavigator nav)
		{
			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.ProcessingInstruction, "", "xml-stylesheet", "", "xml-stylesheet", "href='foo.xsl' type='text/xsl' ", false, false, false);
			Assert (!nav.MoveToFirstChild ());
		}

		[Test]
		public void XmlRootElementOnly ()
		{
			string xml = "<foo />";

			nav = GetXmlDocumentNavigator (xml);
			XmlRootElementOnly (nav);
			nav = GetXPathDocumentNavigator (document);
			XmlRootElementOnly (nav);
		}

		private void XmlRootElementOnly (XPathNavigator nav)
		{
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, false, true);
			Assert (!nav.MoveToFirstChild ());
			Assert (!nav.MoveToNext ());
			Assert (!nav.MoveToPrevious ());
			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert (!nav.MoveToNext ());
		}

		[Test]
		public void XmlSimpleTextContent ()
		{
			string xml = "<foo>Test.</foo>";

			nav = GetXmlDocumentNavigator (xml);
			XmlSimpleTextContent (nav);
			nav = GetXPathDocumentNavigator (document);
			XmlSimpleTextContent (nav);
		}

		private void XmlSimpleTextContent (XPathNavigator nav)
		{
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "Test.", false, true, false);
			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "Test.", false, true, false);
			Assert (!nav.MoveToNext ());
			Assert (!nav.MoveToPrevious ());
			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Text, "", "", "", "", "Test.", false, false, false);

			Assert (nav.MoveToParent ());
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "Test.", false, true, false);

			Assert (nav.MoveToParent ());
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "Test.", false, true, false);

			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "Test.", false, true, false);
			Assert (!nav.MoveToNext ());
		}

		[Test]
		public void XmlSimpleElementContent ()
		{
			string xml = "<foo><bar /></foo>";

			nav = GetXmlDocumentNavigator (xml);
			XmlSimpleElementContent (nav);
			nav = GetXPathDocumentNavigator (document);
			XmlSimpleElementContent (nav);
		}

		private void XmlSimpleElementContent (XPathNavigator nav)
		{
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, true, false);
			Assert (!nav.MoveToNext ());
			Assert (!nav.MoveToPrevious ());

			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Element, "", "bar", "", "bar", "", false, false, true);

			Assert (nav.MoveToParent ());
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, true, false);

			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert (!nav.MoveToNext ());
		}

		[Test]
		public void XmlTwoElementsContent ()
		{
			string xml = "<foo><bar /><baz /></foo>";

			nav = GetXmlDocumentNavigator (xml);
			XmlTwoElementsContent (nav);
			nav = GetXPathDocumentNavigator (document);
			XmlTwoElementsContent (nav);
		}

		private void XmlTwoElementsContent (XPathNavigator nav)
		{
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);

			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, true, false);
			Assert (!nav.MoveToNext ());
			Assert (!nav.MoveToPrevious ());

			Assert (nav.MoveToFirstChild ());
			AssertNavigator (nav, XPathNodeType.Element, "", "bar", "", "bar", "", false, false, true);
			Assert (!nav.MoveToFirstChild ());

			Assert (nav.MoveToNext ());
			AssertNavigator (nav, XPathNodeType.Element, "", "baz", "", "baz", "", false, false, true);
			Assert (!nav.MoveToFirstChild ());

			Assert (nav.MoveToPrevious ());
			AssertNavigator (nav, XPathNodeType.Element, "", "bar", "", "bar", "", false, false, true);

			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert (!nav.MoveToNext ());
		}

		[Test]
		public void XmlElementWithAttributes ()
		{
			string xml = "<img src='foo.png' alt='image Fooooooo!' />";

			nav = GetXmlDocumentNavigator (xml);
			XmlElementWithAttributes (nav);
			nav = GetXPathDocumentNavigator (document);
			XmlElementWithAttributes (nav);
		}

		private void XmlElementWithAttributes (XPathNavigator nav)
		{
			nav.MoveToFirstChild ();
			AssertNavigator (nav, XPathNodeType.Element, "", "img", "", "img", "", true, false, true);
			Assert (!nav.MoveToNext ());
			Assert (!nav.MoveToPrevious ());

			Assert (nav.MoveToFirstAttribute ());
			AssertNavigator (nav, XPathNodeType.Attribute, "", "src", "", "src", "foo.png", false, false, false);
			Assert (!nav.MoveToFirstAttribute ());	// On attributes, it fails.

			Assert (nav.MoveToNextAttribute ());
			AssertNavigator (nav, XPathNodeType.Attribute, "", "alt", "", "alt", "image Fooooooo!", false, false, false);
			Assert (!nav.MoveToNextAttribute ());

			Assert (nav.MoveToParent ());
			AssertNavigator (nav, XPathNodeType.Element, "", "img", "", "img", "", true, false, true);

			Assert (nav.MoveToAttribute ("alt", ""));
			AssertNavigator (nav, XPathNodeType.Attribute, "", "alt", "", "alt", "image Fooooooo!", false, false, false);
			Assert (!nav.MoveToAttribute ("src", ""));	// On attributes, it fails.
			Assert (nav.MoveToParent ());
			Assert (nav.MoveToAttribute ("src", ""));
			AssertNavigator (nav, XPathNodeType.Attribute, "", "src", "", "src", "foo.png", false, false, false);

			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
		}

		[Test]
		public void XmlNamespaceNode ()
		{
			string xml = "<html xmlns='http://www.w3.org/1999/xhtml'><body>test.</body></html>";

			nav = GetXmlDocumentNavigator (xml);
			XmlNamespaceNode (nav);
			nav = GetXPathDocumentNavigator (document);
			XmlNamespaceNode (nav);
		}

		private void XmlNamespaceNode (XPathNavigator nav)
		{
			string xhtml = "http://www.w3.org/1999/xhtml";
			string xmlNS = "http://www.w3.org/XML/1998/namespace";
			nav.MoveToFirstChild ();
			AssertNavigator (nav, XPathNodeType.Element,
				"", "html", xhtml, "html", "test.", false, true, false);
			Assert (nav.MoveToFirstNamespace (XPathNamespaceScope.Local));
			AssertNavigator (nav, XPathNodeType.Namespace,
				"", "", "", "", xhtml, false, false, false);

			// Test difference between Local, ExcludeXml and All.
			Assert (!nav.MoveToNextNamespace (XPathNamespaceScope.Local));
			Assert (!nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
			// LAMESPEC: MS.NET 1.0 XmlDocument seems to have some bugs around here.
			// see http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316808
#if true
			Assert (nav.MoveToNextNamespace (XPathNamespaceScope.All));
			AssertNavigator (nav, XPathNodeType.Namespace,
				"", "xml", "", "xml", xmlNS, false, false, false);
			Assert (!nav.MoveToNextNamespace (XPathNamespaceScope.All));
#endif
			// Test to check if MoveToRoot() resets Namespace node status.
			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "test.", false, true, false);
			nav.MoveToFirstChild ();

			// Test without XPathNamespaceScope argument.
			Assert (nav.MoveToFirstNamespace ());
			Assert (nav.MoveToNextNamespace ());
			AssertNavigator (nav, XPathNodeType.Namespace,
				"", "xml", "", "xml", xmlNS, false, false, false);

			// Test MoveToParent()
			Assert (nav.MoveToParent ());
			AssertNavigator (nav, XPathNodeType.Element,
				"", "html", xhtml, "html", "test.", false, true, false);

			nav.MoveToFirstChild ();	// body
			// Test difference between Local and ExcludeXml
			Assert (!nav.MoveToFirstNamespace (XPathNamespaceScope.Local));
			Assert (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml));
			AssertNavigator (nav, XPathNodeType.Namespace,
				"", "", "", "", xhtml, false, false, false);

			Assert (nav.MoveToNextNamespace (XPathNamespaceScope.All));
			AssertNavigator (nav, XPathNodeType.Namespace,
				"", "xml", "", "xml", xmlNS, false, false, false);
			Assert (nav.MoveToParent ());
			AssertNavigator (nav, XPathNodeType.Element,
				"", "body", xhtml, "body", "test.", false, true, false);

			nav.MoveToRoot ();
			AssertNavigator (nav, XPathNodeType.Root, "", "", "", "", "test.", false, true, false);
		}
	}
}
