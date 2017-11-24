//
// MonoTests.System.Xml.XPathNavigatorReaderTests
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc. http://www.novell.com
//

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
		[Ignore ("Bug in Microsoft reference source")]
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

		[Test]
		public void MixedContentAndDepth ()
		{
			string xml = @"<one>  <two>Some data.<three>more</three> done.</two>  </one>";

			nav = GetXmlDocumentNavigator (xml);
			MixedContentAndDepth (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			MixedContentAndDepth (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			MixedContentAndDepth (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			MixedContentAndDepth (nav, "#4.");
		}

		void MixedContentAndDepth (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();
			r.Read ();
			XmlAssert.AssertNode (label + "#1", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"one", String.Empty, "one", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#2", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"two", String.Empty, "two", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#3", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"Some data.", true, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#4", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Element, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				"three", String.Empty, "three", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#5", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 3, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				"more", true, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#6", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				"three", String.Empty, "three", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#7", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.Text, 2, false,
				// Name, Prefix, LocalName, NamespaceURI
				String.Empty, String.Empty, String.Empty, String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				" done.", true, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#8", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 1, false,
				// Name, Prefix, LocalName, NamespaceURI
				"two", String.Empty, "two", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			r.Read ();
			XmlAssert.AssertNode (label + "#9", r,
				// NodeType, Depth, IsEmptyElement
				XmlNodeType.EndElement, 0, false,
				// Name, Prefix, LocalName, NamespaceURI
				"one", String.Empty, "one", String.Empty,
				// Value, HasValue, AttributeCount, HasAttributes
				String.Empty, false, 0, false);

			Assert.IsFalse (r.Read (), label + "#10");
		}

		[Test]
		public void MoveToFirstAttributeFromAttribute ()
		{
			string xml = @"<one xmlns:foo='urn:foo' a='v' />";

			nav = GetXmlDocumentNavigator (xml);
			MoveToFirstAttributeFromAttribute (nav, "#1.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			MoveToFirstAttributeFromAttribute (nav, "#2.");

			nav = GetXPathDocumentNavigator (document);
			MoveToFirstAttributeFromAttribute (nav, "#3.");

			nav.MoveToRoot ();
			nav.MoveToFirstChild ();
			MoveToFirstAttributeFromAttribute (nav, "#4.");
		}

		void MoveToFirstAttributeFromAttribute (XPathNavigator nav, string label)
		{
			XmlReader r = nav.ReadSubtree ();
			r.MoveToContent ();
			Assert.IsTrue (r.MoveToFirstAttribute (), label + "#1");
			Assert.IsTrue (r.MoveToFirstAttribute (), label + "#2");
			r.ReadAttributeValue ();
			Assert.IsTrue (r.MoveToFirstAttribute (), label + "#3");
			Assert.IsTrue (r.MoveToNextAttribute (), label + "#4");
			Assert.IsTrue (r.MoveToFirstAttribute (), label + "#5");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadSubtreeAttribute ()
		{
			string xml = "<root a='b' />";
			nav = GetXmlDocumentNavigator (xml);
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			nav.ReadSubtree ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadSubtreeNamespace ()
		{
			string xml = "<root xmlns='urn:foo' />";
			nav = GetXmlDocumentNavigator (xml);
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.ReadSubtree ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadSubtreePI ()
		{
			string xml = "<?pi ?><root xmlns='urn:foo' />";
			nav = GetXmlDocumentNavigator (xml);
			nav.MoveToFirstChild ();
			nav.ReadSubtree ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadSubtreeComment ()
		{
			string xml = "<!-- comment --><root xmlns='urn:foo' />";
			nav = GetXmlDocumentNavigator (xml);
			nav.MoveToFirstChild ();
			nav.ReadSubtree ();
		}

		[Test]
		public void ReadSubtreeAttributesByIndex ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<u:Timestamp u:Id='ID1' xmlns:u='urn:foo'></u:Timestamp>");
			XmlReader r = doc.CreateNavigator ().ReadSubtree ();
			r.Read ();
			r.MoveToAttribute (0);
			if (r.LocalName != "Id")
				r.MoveToAttribute (1);
				if (r.LocalName != "Id")
					Assert.Fail ("Should move to the attribute.");
		}
	}
}

