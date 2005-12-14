//
// MonoTests.System.Xml.XPathNavigatorReaderTests
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc. http://www.novell.com
//
#if NET_2_0

using System;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

using MonoTests.System.Xml; // XmlAssert

namespace MonoTests.System.Xml.XPath
{
	[TestFixture]
	public class XPathNavigatorReaderTests
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

		[Test]
		public void ReadSubtree1 ()
		{
			string xml = "<root/>";

			nav = GetXmlDocumentNavigator (xml);
			ReadSubtree1 (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			ReadSubtree1 (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			ReadSubtree1 (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			ReadSubtree1 (nav, "#4.");
		}

		void ReadSubtree1 (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();

			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.None, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#2");
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, true,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsFalse (r.Read (), label + "#4");
		}

		[Test]
		public void ReadSubtree2 ()
		{
			string xml = "<root></root>";

			nav = GetXmlDocumentNavigator (xml);
			ReadSubtree2 (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			ReadSubtree2 (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			ReadSubtree2 (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			ReadSubtree2 (nav, "#4.");
		}

		void ReadSubtree2 (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();

			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.None, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#2");
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#4");
			XmlAssert.AssertNode (label + "#5", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsFalse (r.Read (), label + "#6");
		}

		[Test]
		public void ReadSubtree3 ()
		{
			string xml = "<root attr='value'/>";

			nav = GetXmlDocumentNavigator (xml);
			ReadSubtree3 (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			ReadSubtree3 (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			ReadSubtree3 (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			ReadSubtree3 (nav, "#4.");
		}

		void ReadSubtree3 (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();

			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.None, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#2");
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, true,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 1, true);

			Assert.IsTrue (r.MoveToFirstAttribute (), label + "#4");
			XmlAssert.AssertNode (label + "#5", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Attribute, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"attr", String.Empty, "attr", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"value", true, 1, true);

			Assert.IsTrue (r.ReadAttributeValue (), label + "#6");
			XmlAssert.AssertNode (label + "#7", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"value", true, 1, true);

			Assert.IsFalse (r.ReadAttributeValue (), label + "#8");
			Assert.IsFalse (r.MoveToNextAttribute (), label + "#9");
			Assert.IsTrue (r.MoveToElement (), label + "#10");

			Assert.IsFalse (r.Read (), label + "#11");
		}

		[Test]
		public void DocElem_OpenClose_Attribute ()
		{
			string xml = "<root attr='value'></root>";

			nav = GetXmlDocumentNavigator (xml);
			DocElem_OpenClose_Attribute (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			DocElem_OpenClose_Attribute (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			DocElem_OpenClose_Attribute (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			DocElem_OpenClose_Attribute (nav, "#4.");
		}

		void DocElem_OpenClose_Attribute (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();

			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.None, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#2");
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 1, true);

			Assert.IsTrue (r.MoveToFirstAttribute (), label + "#4");
			XmlAssert.AssertNode (label + "#5", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Attribute, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"attr", String.Empty, "attr", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"value", true, 1, true);

			Assert.IsTrue (r.ReadAttributeValue (), label + "#6");
			XmlAssert.AssertNode (label + "#7", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"value", true, 1, true);

			Assert.IsFalse (r.ReadAttributeValue (), label + "#8");
			Assert.IsFalse (r.MoveToNextAttribute (), label + "#9");
			Assert.IsTrue (r.MoveToElement (), label + "#10");

			Assert.IsTrue (r.Read (), label + "#11");
			XmlAssert.AssertNode (label + "#12", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsFalse (r.Read (), label + "#13");
		}

		[Test]
		public void FromChildElement ()
		{
			string xml = "<root><foo attr='value'>test</foo><bar/></root>";

			nav = GetXmlDocumentNavigator (xml);
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo
			FromChildElement (nav, "#1.");

			nav = GetXPathDocumentNavigator (document);
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo
			FromChildElement (nav, "#2.");
		}

		void FromChildElement (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();

			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.None, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#2");
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"foo", String.Empty, "foo", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 1, true);

			Assert.IsTrue (r.Read (), label + "#4");
			XmlAssert.AssertNode (label + "#5", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"test", true, 0, false);

			Assert.IsTrue (r.Read (), label + "#6");
			XmlAssert.AssertNode (label + "#7", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"foo", String.Empty, "foo", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			// end at </foo>, without moving toward <bar>.
			Assert.IsFalse (r.Read (), label + "#8");
		}

		[Test]
		[Category ("NotDotNet")] // MS bug
		public void AttributesAndNamespaces ()
		{
			string xml = "<root attr='value' x:a2='v2' xmlns:x='urn:foo' xmlns='urn:default'></root>";

			nav = GetXmlDocumentNavigator (xml);
			AttributesAndNamespaces (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			AttributesAndNamespaces (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			AttributesAndNamespaces (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			AttributesAndNamespaces (nav, "#4.");
		}

		void AttributesAndNamespaces (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();

			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.None, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsTrue (r.Read (), label + "#2");
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", "urn:default",
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 4, true);

			// Namespaces

			Assert.IsTrue (r.MoveToAttribute ("xmlns:x"), label + "#4");
			XmlAssert.AssertNode (label + "#5", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Attribute, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"xmlns:x", "xmlns", "x",
				"http://www.w3.org/2000/xmlns/",
				// Value, HasValue, AttributeCount, HasAttributes
				"urn:foo", true, 4, true);

			Assert.IsTrue (r.ReadAttributeValue (), label + "#6");
///* MS.NET has a bug here
			XmlAssert.AssertNode (label + "#7", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"urn:foo", true, 4, true);
//*/

			Assert.IsFalse (r.ReadAttributeValue (), label + "#8");

			Assert.IsTrue (r.MoveToAttribute ("xmlns"), label + "#9");
			XmlAssert.AssertNode (label + "#10", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Attribute, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"xmlns", String.Empty, "xmlns",
				"http://www.w3.org/2000/xmlns/",
				// Value, HasValue, AttributeCount, HasAttributes
				"urn:default", true, 4, true);

			Assert.IsTrue (r.ReadAttributeValue (), label + "#11");
///* MS.NET has a bug here
			XmlAssert.AssertNode (label + "#12", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"urn:default", true, 4, true);
//*/

			Assert.IsFalse (r.ReadAttributeValue (), label + "#13");

			// Attributes

			Assert.IsTrue (r.MoveToAttribute ("attr"), label + "#14");
			XmlAssert.AssertNode (label + "#15", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Attribute, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"attr", String.Empty, "attr", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"value", true, 4, true);

			Assert.IsTrue (r.ReadAttributeValue (), label + "#16");
			XmlAssert.AssertNode (label + "#17", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"value", true, 4, true);

			Assert.IsFalse (r.ReadAttributeValue (), label + "#18");

			Assert.IsTrue (r.MoveToAttribute ("x:a2"), label + "#19");
			XmlAssert.AssertNode (label + "#20", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Attribute, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"x:a2", "x", "a2", "urn:foo",
				// Value, HasValue, AttributeCount, HasAttributes
				"v2", true, 4, true);

			Assert.IsTrue (r.ReadAttributeValue (), label + "#21");
			XmlAssert.AssertNode (label + "#22", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"v2", true, 4, true);

			Assert.IsTrue (r.MoveToElement (), label + "#24");

			Assert.IsTrue (r.Read (), label + "#25");
			XmlAssert.AssertNode (label + "#26", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"root", String.Empty, "root", "urn:default",
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsFalse (r.Read (), label + "#27");
		}
	}
}

#endif
