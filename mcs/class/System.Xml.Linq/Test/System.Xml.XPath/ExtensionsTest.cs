//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.
// (C) 2003 Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//
// (based on XPathNavigatorCommonTests)
//

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XPathNavigatorCommonTests
	{
		XPathNavigator nav;

		private void AssertNavigator (string label, XPathNavigator nav, XPathNodeType type, string prefix, string localName, string ns, string name, string value, bool hasAttributes, bool hasChildren, bool isEmptyElement)
		{
			label += nav.GetType ();
			Assert.AreEqual (type, nav.NodeType, label + "NodeType");
			Assert.AreEqual (prefix, nav.Prefix, label + "Prefix");
			Assert.AreEqual (localName, nav.LocalName, label + "LocalName");
			Assert.AreEqual (ns, nav.NamespaceURI, label + "Namespace");
			Assert.AreEqual (name, nav.Name, label + "Name");
			Assert.AreEqual (value, nav.Value, label + "Value");
			Assert.AreEqual (hasAttributes, nav.HasAttributes, label + "HasAttributes");
			Assert.AreEqual (hasChildren, nav.HasChildren, label + "HasChildren");
			Assert.AreEqual (isEmptyElement, nav.IsEmptyElement, label + "IsEmptyElement");
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			string xml = "<?xml version=\"1.0\" standalone=\"yes\"?><foo>bar</foo>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			DocumentWithXmlDeclaration (nav);
		}

		public void DocumentWithXmlDeclaration (XPathNavigator nav)
		{
			nav.MoveToFirstChild ();
			AssertNavigator ("#1", nav, XPathNodeType.Element, "", "foo", "", "foo", "bar", false, true, false);
		}

		[Test]
		public void DocumentWithProcessingInstruction ()
		{
			string xml = "<?xml-stylesheet href='foo.xsl' type='text/xsl' ?><foo />";

			nav = XDocument.Parse (xml).CreateNavigator ();
			DocumentWithProcessingInstruction (nav);
		}

		public void DocumentWithProcessingInstruction (XPathNavigator nav)
		{
			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#1", nav, XPathNodeType.ProcessingInstruction, "", "xml-stylesheet", "", "xml-stylesheet", "href='foo.xsl' type='text/xsl' ", false, false, false);
			Assert.IsTrue (!nav.MoveToFirstChild ());
		}

		[Test]
		public void XmlRootElementOnly ()
		{
			string xml = "<foo />";

			nav = XDocument.Parse (xml).CreateNavigator ();
			XmlRootElementOnly (nav);
		}

		private void XmlRootElementOnly (XPathNavigator nav)
		{
			AssertNavigator ("#1", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#2", nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, false, true);
			Assert.IsTrue (!nav.MoveToFirstChild ());
			Assert.IsTrue (!nav.MoveToNext ());
			Assert.IsTrue (!nav.MoveToPrevious ());
			nav.MoveToRoot ();
			AssertNavigator ("#3", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert.IsTrue (!nav.MoveToNext ());
		}

		[Test]
		public void XmlSimpleTextContent ()
		{
			string xml = "<foo>Test.</foo>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			XmlSimpleTextContent (nav);
		}

		private void XmlSimpleTextContent (XPathNavigator nav)
		{
			AssertNavigator ("#1", nav, XPathNodeType.Root, "", "", "", "", "Test.", false, true, false);
			Assert.IsTrue (nav.MoveToFirstChild (), "#x1");
			AssertNavigator ("#2", nav, XPathNodeType.Element, "", "foo", "", "foo", "Test.", false, true, false);
			Assert.IsTrue (!nav.MoveToNext (), "#x2");
			Assert.IsTrue (!nav.MoveToPrevious (), "#x3");
			Assert.IsTrue (nav.MoveToFirstChild (), "#x4");
			AssertNavigator ("#3", nav, XPathNodeType.Text, "", "", "", "", "Test.", false, false, false);

			Assert.IsTrue (nav.MoveToParent (), "#x5");
			AssertNavigator ("#4", nav, XPathNodeType.Element, "", "foo", "", "foo", "Test.", false, true, false);

			Assert.IsTrue (nav.MoveToParent (), "#x6");
			AssertNavigator ("#5", nav, XPathNodeType.Root, "", "", "", "", "Test.", false, true, false);

			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.MoveToRoot ();
			AssertNavigator ("#6", nav, XPathNodeType.Root, "", "", "", "", "Test.", false, true, false);
			Assert.IsTrue (!nav.MoveToNext (), "#x7");
		}

		[Test]
		public void XmlSimpleElementContent ()
		{
			string xml = "<foo><bar /></foo>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			XmlSimpleElementContent (nav);
		}

		private void XmlSimpleElementContent (XPathNavigator nav)
		{
			AssertNavigator ("#1", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#2", nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, true, false);
			Assert.IsTrue (!nav.MoveToNext ());
			Assert.IsTrue (!nav.MoveToPrevious ());

			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#3", nav, XPathNodeType.Element, "", "bar", "", "bar", "", false, false, true);

			Assert.IsTrue (nav.MoveToParent ());
			AssertNavigator ("#4", nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, true, false);

			nav.MoveToRoot ();
			AssertNavigator ("#5", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert.IsTrue (!nav.MoveToNext ());
		}

		[Test]
		public void XmlTwoElementsContent ()
		{
			string xml = "<foo><bar /><baz /></foo>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			XmlTwoElementsContent (nav);
		}

		private void XmlTwoElementsContent (XPathNavigator nav)
		{
			AssertNavigator ("#1", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);

			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#2", nav, XPathNodeType.Element, "", "foo", "", "foo", "", false, true, false);
			Assert.IsTrue (!nav.MoveToNext ());
			Assert.IsTrue (!nav.MoveToPrevious ());

			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#3", nav, XPathNodeType.Element, "", "bar", "", "bar", "", false, false, true);
			Assert.IsTrue (!nav.MoveToFirstChild ());

			Assert.IsTrue (nav.MoveToNext ());
			AssertNavigator ("#4", nav, XPathNodeType.Element, "", "baz", "", "baz", "", false, false, true);
			Assert.IsTrue (!nav.MoveToFirstChild ());

			Assert.IsTrue (nav.MoveToPrevious ());
			AssertNavigator ("#5", nav, XPathNodeType.Element, "", "bar", "", "bar", "", false, false, true);

			nav.MoveToRoot ();
			AssertNavigator ("#6", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
			Assert.IsTrue (!nav.MoveToNext ());
		}

		[Test]
		public void XmlElementWithAttributes ()
		{
			string xml = "<img src='foo.png' alt='image Fooooooo!' />";

			nav = XDocument.Parse (xml).CreateNavigator ();
			XmlElementWithAttributes (nav);
		}

		private void XmlElementWithAttributes (XPathNavigator nav)
		{
			nav.MoveToFirstChild ();
			AssertNavigator ("#1", nav, XPathNodeType.Element, "", "img", "", "img", "", true, false, true);
			Assert.IsTrue (!nav.MoveToNext (), "#x1");
			Assert.IsTrue (!nav.MoveToPrevious (), "#x2");

			Assert.IsTrue (nav.MoveToFirstAttribute (), "#x3");
			AssertNavigator ("#2", nav, XPathNodeType.Attribute, "", "src", "", "src", "foo.png", false, false, false);
			Assert.IsTrue (!nav.MoveToFirstAttribute (), "#x4");	// On attributes, it fails.

			Assert.IsTrue (nav.MoveToNextAttribute (), "#x5");
			AssertNavigator ("#3", nav, XPathNodeType.Attribute, "", "alt", "", "alt", "image Fooooooo!", false, false, false);
			Assert.IsTrue (!nav.MoveToNextAttribute (), "#x6");

			Assert.IsTrue (nav.MoveToParent (), "#x7");
			AssertNavigator ("#4", nav, XPathNodeType.Element, "", "img", "", "img", "", true, false, true);

			Assert.IsTrue (nav.MoveToAttribute ("alt", ""), "#x8");
			AssertNavigator ("#5", nav, XPathNodeType.Attribute, "", "alt", "", "alt", "image Fooooooo!", false, false, false);
			Assert.IsTrue (!nav.MoveToAttribute ("src", ""), "#x9");	// On attributes, it fails.
			Assert.IsTrue (nav.MoveToParent (), "#x10");
			Assert.IsTrue (nav.MoveToAttribute ("src", ""), "#x11");
			AssertNavigator ("#6", nav, XPathNodeType.Attribute, "", "src", "", "src", "foo.png", false, false, false);

			nav.MoveToRoot ();
			AssertNavigator ("#7", nav, XPathNodeType.Root, "", "", "", "", "", false, true, false);
		}

		[Test]
		// seems like MS does not want to fix their long-time-known
		// XPathNavigator bug, so just set it as NotDotNet.
		// We are better.
		[Category ("NotDotNet")]
		public void XmlNamespaceNode ()
		{
			string xml = "<html xmlns='http://www.w3.org/1999/xhtml'><body>test.</body></html>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			XmlNamespaceNode (nav);
		}

		private void XmlNamespaceNode (XPathNavigator nav)
		{
			string xhtml = "http://www.w3.org/1999/xhtml";
			string xmlNS = "http://www.w3.org/XML/1998/namespace";
			nav.MoveToFirstChild ();
			AssertNavigator ("#1", nav, XPathNodeType.Element,
				"", "html", xhtml, "html", "test.", false, true, false);
			Assert.IsTrue (nav.MoveToFirstNamespace (XPathNamespaceScope.Local));
			AssertNavigator ("#2", nav, XPathNodeType.Namespace,
				"", "", "", "", xhtml, false, false, false);

			// Test difference between Local, ExcludeXml and All.
			Assert.IsTrue (!nav.MoveToNextNamespace (XPathNamespaceScope.Local));
			Assert.IsTrue (!nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
			// LAMESPEC: MS.NET 1.0 XmlDocument seems to have some bugs around here.
			// see http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316808
#if true
			Assert.IsTrue (nav.MoveToNextNamespace (XPathNamespaceScope.All));
			AssertNavigator ("#3", nav, XPathNodeType.Namespace,
				"", "xml", "", "xml", xmlNS, false, false, false);
			Assert.IsTrue (!nav.MoveToNextNamespace (XPathNamespaceScope.All));
#endif
			// Test to check if MoveToRoot() resets Namespace node status.
			nav.MoveToRoot ();
			AssertNavigator ("#4", nav, XPathNodeType.Root, "", "", "", "", "test.", false, true, false);
			nav.MoveToFirstChild ();

			// Test without XPathNamespaceScope argument.
			Assert.IsTrue (nav.MoveToFirstNamespace ());
			Assert.IsTrue (nav.MoveToNextNamespace ());
			AssertNavigator ("#5", nav, XPathNodeType.Namespace,
				"", "xml", "", "xml", xmlNS, false, false, false);

			// Test MoveToParent()
			Assert.IsTrue (nav.MoveToParent ());
			AssertNavigator ("#6", nav, XPathNodeType.Element,
				"", "html", xhtml, "html", "test.", false, true, false);

			nav.MoveToFirstChild ();	// body
			// Test difference between Local and ExcludeXml
			Assert.IsTrue (!nav.MoveToFirstNamespace (XPathNamespaceScope.Local), "Local should fail");
			Assert.IsTrue (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml), "ExcludeXml should succeed");
			AssertNavigator ("#7", nav, XPathNodeType.Namespace,
				"", "", "", "", xhtml, false, false, false);

			Assert.IsTrue (nav.MoveToNextNamespace (XPathNamespaceScope.All));
			AssertNavigator ("#8", nav, XPathNodeType.Namespace,
				"", "xml", "", "xml", xmlNS, false, false, false);
			Assert.IsTrue (nav.MoveToParent ());
			AssertNavigator ("#9", nav, XPathNodeType.Element,
				"", "body", xhtml, "body", "test.", false, true, false);

			nav.MoveToRoot ();
			AssertNavigator ("#10", nav, XPathNodeType.Root, "", "", "", "", "test.", false, true, false);
		}

		[Test]
		public void MoveToNamespaces ()
		{
			string xml = "<a xmlns:x='urn:x'><b xmlns:y='urn:y'/><c/><d><e attr='a'/></d></a>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			MoveToNamespaces (nav);
		}

		private void MoveToNamespaces (XPathNavigator nav)
		{
			XPathNodeIterator iter = nav.Select ("//e");
			iter.MoveNext ();
			nav.MoveTo (iter.Current);
			Assert.AreEqual ("e", nav.Name, "#1");
			nav.MoveToFirstNamespace ();
			Assert.AreEqual ("x", nav.Name, "#2");
			nav.MoveToNextNamespace ();
			Assert.AreEqual ("xml", nav.Name, "#3");
		}

		[Test]
		public void IsDescendant ()
		{
			string xml = "<a><b/><c/><d><e attr='a'/></d></a>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			IsDescendant (nav);
		}

		private void IsDescendant (XPathNavigator nav)
		{
			XPathNavigator tmp = nav.Clone ();
			XPathNodeIterator iter = nav.Select ("//e");
			iter.MoveNext ();
			Assert.IsTrue (nav.MoveTo (iter.Current), "#1");
			Assert.IsTrue (nav.MoveToFirstAttribute (), "#2");
			Assert.AreEqual ("attr", nav.Name, "#3");
			Assert.AreEqual ("", tmp.Name, "#4");
			Assert.IsTrue (tmp.IsDescendant (nav), "#5");
			Assert.IsTrue (!nav.IsDescendant (tmp), "#6");
			tmp.MoveToFirstChild ();
			Assert.AreEqual ("a", tmp.Name, "#7");
			Assert.IsTrue (tmp.IsDescendant (nav), "#8");
			Assert.IsTrue (!nav.IsDescendant (tmp), "#9");
			tmp.MoveTo (iter.Current);
			Assert.AreEqual ("e", tmp.Name, "#10");
			Assert.IsTrue (tmp.IsDescendant (nav), "#11");
			Assert.IsTrue (!nav.IsDescendant (tmp), "#12");
		}

		[Test]
		public void LiterallySplitText ()
		{
			string xml = "<root><![CDATA[test]]> string</root>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			LiterallySplitText (nav);
		}

		private void LiterallySplitText (XPathNavigator nav)
		{
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.Text, nav.NodeType, "#1");
			Assert.AreEqual ("test string", nav.Value, "#2");
		}

		// bug #75609
		[Test]
		public void SelectChildren ()
		{
			string xml = "<root><foo xmlns='urn:foo' /><ns:foo xmlns:ns='urn:foo' /></root>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			SelectChildrenNS (nav);
		}

		private void SelectChildrenNS (XPathNavigator nav)
		{
			nav.MoveToFirstChild (); // root
			XPathNodeIterator iter = nav.SelectChildren ("foo", "urn:foo");
			Assert.AreEqual (2, iter.Count, "#1");
		}

#if NET_2_0

		[Test]
		// bug #78067
		public void OuterXml ()
		{
			string xml = @"<?xml version=""1.0""?>
<one>
        <two>Some data.</two>
</one>";

			nav = XDocument.Parse (xml).CreateNavigator ();
			OuterXml (nav);
		}

		private void OuterXml (XPathNavigator nav)
		{
			string ret = @"<one>
  <two>Some data.</two>
</one>";
			Assert.AreEqual (ret, nav.OuterXml.Replace ("\r\n", "\n"), "#1");
		}

		[Test]
		public void ReadSubtreeLookupNamespace ()
		{
			string xml = "<x:foo xmlns:x='urn:x'><bar>x:val</bar></x:foo>";
			var doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.LastChild.LastChild.CreateNavigator ();
			var xr = nav.ReadSubtree ();
			xr.MoveToContent ();
			xr.Read (); // should be at x:val
			Assert.AreEqual ("urn:x", xr.LookupNamespace ("x"), "#1");
		}
#endif

		[Test]
		public void GetNamespaceConsistentTree ()
		{
			string xml = "<x:root xmlns:x='urn:x'>  <x:foo xmlns='ns1'> <x:bar /> </x:foo>  <x:foo xmlns:y='ns2'> <x:bar /> </x:foo></x:root>";
			nav = XDocument.Parse (xml).CreateNavigator ();
			GetNamespaceConsistentTree (nav);
		}

		private void GetNamespaceConsistentTree (XPathNavigator nav)
		{
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.MoveToNext ();
			Assert.AreEqual (String.Empty, nav.GetNamespace (""), "#1." + nav.GetType ());
			nav.MoveToNext ();
			nav.MoveToNext ();
			Assert.AreEqual ("", nav.GetNamespace (""), "#2." + nav.GetType ());
		}
	}
}