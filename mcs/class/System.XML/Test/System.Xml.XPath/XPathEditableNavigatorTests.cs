//
// XPathEditableNavigatorTests.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//

#if NET_2_0

using System;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace MonoTests.System.Xml.XPath
{
	[TestFixture]
	public class XPathEditableNavigatorTests
	{
		private XPathNavigator GetInstance (string xml)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			return doc.CreateNavigator ();
		}

		private static void AssertNavigator (string label, XPathNavigator nav, XPathNodeType type, string prefix, string localName, string ns, string name, string value, bool hasAttributes, bool hasChildren, bool isEmptyElement)
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
		[ExpectedException (typeof (XmlException))]
		public void AppendChildStartDocumentInvalid ()
		{
			XPathNavigator nav = GetInstance (String.Empty);
			XmlWriter w = nav.AppendChild ();
			w.WriteStartDocument ();
			w.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void AppendChildStartAttributeInvalid ()
		{
			XPathNavigator nav = GetInstance (String.Empty);
			XmlWriter w = nav.AppendChild ();
			// Seems like it is just ignored.
			w.WriteStartAttribute ("test");
			w.WriteEndAttribute ();
			w.Close ();
			Assert.AreEqual (XPathNodeType.Root, nav.NodeType, "#1");
			Assert.IsFalse (nav.MoveToFirstChild (), "#2");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void AppendChildElementIncomplete ()
		{
			XPathNavigator nav = GetInstance (String.Empty);
			XmlWriter w = nav.AppendChild ();
			w.WriteStartElement ("foo");
			w.Close ();
		}

		[Test]
		// empty content is allowed.
		public void AppendChildEmptyString ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.MoveToFirstChild (); // root
			nav.AppendChild (String.Empty);
		}

		[Test]
		public void AppendChildElement ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.MoveToFirstChild ();
			XmlWriter w = nav.AppendChild ();
			w.WriteStartElement ("foo");
			w.WriteEndElement ();
			w.Close ();
			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement
		}

		[Test]
		public void AppendChildStringFragment ()
		{
			// check that the input string inherits
			// namespace context.
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.AppendChild ("<child/>fragment<child></child>");

			Assert.IsTrue (nav.MoveToFirstChild (), "#1-1");
			AssertNavigator ("#1-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"child",	// LocalName
				"urn:foo",	// NamespaceURI
				"child",	// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsFalse (nav.MoveToFirstChild (), "#2-1");
			Assert.IsTrue (nav.MoveToNext (), "#2-2");
			AssertNavigator ("#2-3", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"fragment",	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3-1");
			AssertNavigator ("#3-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"child",	// LocalName
				"urn:foo",	// NamespaceURI
				"child",	// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		public void AppendChildStringInvalidFragment ()
		{
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.AppendChild ("<?xml version='1.0'?><root/>");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AppendChildToTextNode ()
		{
			XPathNavigator nav = GetInstance ("<root>text</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			XmlWriter w = nav.AppendChild ();
		}

		[Test]
		public void InsertAfter ()
		{
			XPathNavigator nav = GetInstance ("<root>test</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.InsertAfter ("<blah/><doh>sample</doh>");

			AssertNavigator ("#1", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"test",		// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#2");
			AssertNavigator ("#2-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"blah",		// LocalName
				String.Empty,	// NamespaceURI
				"blah",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3");
			AssertNavigator ("#3-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"doh",		// LocalName
				String.Empty,	// NamespaceURI
				"doh",		// Name
				"sample",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertAfterRoot ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.InsertAfter ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertAfterAttribute ()
		{
			XPathNavigator nav = GetInstance ("<root a='b'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			nav.InsertAfter ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertAfterNamespace ()
		{
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertAfter ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// xmlns:xml='...', which is likely to have XmlElement or XmlDocument as its node.
		public void InsertAfterNamespace2 ()
		{
			XPathNavigator nav = GetInstance ("<root />");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertAfter ();
		}

		[Test]
		// empty content is allowed.
		public void InsertAfterEmptyString ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.MoveToFirstChild (); // root
			nav.InsertAfter (String.Empty);
		}

		[Test]
		public void InsertBefore ()
		{
			XPathNavigator nav = GetInstance ("<root>test</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.InsertBefore ("<blah/><doh>sample</doh>");

			AssertNavigator ("#1", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"test",		// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToFirst (), "#2-1");
			AssertNavigator ("#2-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"blah",		// LocalName
				String.Empty,	// NamespaceURI
				"blah",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3");
			AssertNavigator ("#3-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"doh",		// LocalName
				String.Empty,	// NamespaceURI
				"doh",		// Name
				"sample",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertBeforeRoot ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.InsertBefore ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertBeforeAttribute ()
		{
			XPathNavigator nav = GetInstance ("<root a='b'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			nav.InsertBefore ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertBeforeNamespace ()
		{
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertBefore ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// xmlns:xml='...', which is likely to have XmlElement or XmlDocument as its node.
		public void InsertBeforeNamespace2 ()
		{
			XPathNavigator nav = GetInstance ("<root />");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertBefore ();
		}

		[Test]
		// empty content is allowed.
		public void InsertBeforeEmptyString ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.MoveToFirstChild (); // root
			nav.InsertBefore (String.Empty);
		}

		[Test]
		public void DeleteRange ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>
			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			end.MoveToNext (); // final
			nav.DeleteRange (end);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"root",		// LocalName
				String.Empty,	// NamespaceURI
				"root",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DeleteRangeNullArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>
			nav.DeleteRange (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DeleteRangeInvalidArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>

			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			end.MoveToFirstChild (); // child
			nav.DeleteRange (end);
		}

		[Test]
		public void ReplaceRange ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>

			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			XmlWriter w = nav.ReplaceRange (end);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToParent (), "#1-2");

			w.WriteStartElement ("whoa");
			w.WriteEndElement ();
			w.Close ();

			AssertNavigator ("#2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"whoa",		// LocalName
				String.Empty,	// NamespaceURI
				"whoa",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#2-1");

			AssertNavigator ("#3", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"final",	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReplaceRangeNullArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>
			nav.ReplaceRange (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReplaceRangeInvalidArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>

			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			end.MoveToFirstChild (); // child
			nav.ReplaceRange (end);
		}

		[Test]
		public void PrependChildXmlReader ()
		{
			XPathNavigator nav = GetInstance ("<root><foo>existing_child</foo></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo

			XmlReader reader = new XmlTextReader (
				"<child>text</child><next_sibling/>", 
				XmlNodeType.Element,
				null);

			nav.PrependChild (reader);

			XmlAssert.AssertNode ("#0",
				reader,
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

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				"textexisting_child",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToFirstChild (), "#1-2");

			AssertNavigator ("#2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"child",	// LocalName
				String.Empty,	// NamespaceURI
				"child",	// Name
				"text",		// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#2-2");

			AssertNavigator ("#3", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"next_sibling",	// LocalName
				String.Empty,	// NamespaceURI
				"next_sibling",	// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3-2");

			AssertNavigator ("#4", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"existing_child",// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PrependChildInvalid ()
		{
			XPathNavigator nav = GetInstance ("<root><foo>existing_child</foo></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo

			XmlWriter w = nav.PrependChild ();

			w.WriteStartAttribute ("whoa");
			w.WriteEndAttribute ();
			w.Close ();
		}

		[Test]
		// empty content is allowed.
		public void PrependChildEmptyString ()
		{
			XPathNavigator nav = GetInstance ("<root><foo/><bar/><baz/></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo
			nav.MoveToNext (); // bar
			nav.PrependChild (String.Empty);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"bar",		// LocalName
				String.Empty,	// NamespaceURI
				"bar",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToFirst (), "#1-2");

			AssertNavigator ("#2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement
		}

		[Test]
		public void ReplaceSelf ()
		{
			XPathNavigator nav = GetInstance ("<root><foo>existing_child</foo></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo

			nav.ReplaceSelf ("<hijacker>hah, hah</hijacker><next/>");

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"hijacker",	// LocalName
				String.Empty,	// NamespaceURI
				"hijacker",	// Name
				"hah, hah",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#1-2");

			AssertNavigator ("#2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"next",		// LocalName
				String.Empty,	// NamespaceURI
				"next",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement
		}

		[Test]
		// possible internal behavior difference e.g. due to ReadNode()
		public void ReplaceSelfXmlReaderInteractive ()
		{
			XPathNavigator nav = GetInstance ("<root><foo>existing_child</foo></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo

			XmlReader xr = new XmlTextReader (
				"<hijacker>hah, hah</hijacker><next/>",
				XmlNodeType.Element,
				null);
			xr.MoveToContent ();
			nav.ReplaceSelf (xr);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"hijacker",	// LocalName
				String.Empty,	// NamespaceURI
				"hijacker",	// Name
				"hah, hah",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsFalse (nav.MoveToNext (), "#1-2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// empty content is not allowed
		public void ReplaceSelfEmptyString ()
		{
			XPathNavigator nav = GetInstance ("<root><foo>existing_child</foo></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo

			nav.ReplaceSelf (String.Empty);
		}

		[Test]
		public void SetValueEmptyString ()
		{
			XPathNavigator nav = GetInstance ("<root><foo>existing_child</foo></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // foo

			nav.SetValue (String.Empty);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		public void MoveToFollowing ()
		{
			XPathNavigator end;

			XPathNavigator nav = GetInstance ("<root><bar><foo attr='v1'><baz><foo attr='v2'/></baz></foo></bar><dummy/><foo attr='v3'></foo></root>");
			Assert.IsTrue (nav.MoveToFollowing ("foo", String.Empty), "#1");
			Assert.AreEqual ("v1", nav.GetAttribute ("attr", String.Empty), "#2");
			Assert.IsTrue (nav.MoveToFollowing ("foo", String.Empty), "#3");
			Assert.AreEqual ("v2", nav.GetAttribute ("attr", String.Empty), "#4");
			Assert.IsTrue (nav.MoveToFollowing ("foo", String.Empty), "#5");
			Assert.AreEqual ("v3", nav.GetAttribute ("attr", String.Empty), "#6");

			// round 2
			end = nav.Clone ();

			nav.MoveToRoot ();
			Assert.IsTrue (nav.MoveToFollowing ("foo", String.Empty, end), "#7");
			Assert.AreEqual ("v1", nav.GetAttribute ("attr", String.Empty), "#8");
			Assert.IsTrue (nav.MoveToFollowing ("foo", String.Empty, end), "#9");
			Assert.AreEqual ("v2", nav.GetAttribute ("attr", String.Empty), "#10");
			// end is exclusive
			Assert.IsFalse (nav.MoveToFollowing ("foo", String.Empty, end), "#11");
			// in this case it never moves to somewhere else.
			Assert.AreEqual ("v2", nav.GetAttribute ("attr", String.Empty), "#12");
		}

		[Test]
		public void MoveToFollowingFromAttribute ()
		{
			XPathNavigator nav = GetInstance ("<root a='b'><foo/></root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			// should first move to owner element and go on.
			Assert.IsTrue (nav.MoveToFollowing ("foo", String.Empty));
		}

		[Test]
		public void AppendChildInDocumentFragment ()
		{
			XmlDocumentFragment f = new XmlDocument ().CreateDocumentFragment ();
			XmlWriter w = f.CreateNavigator ().AppendChild ();
			w.WriteStartElement ("foo");
			w.WriteEndElement ();
			w.Close ();
			Assert.IsNotNull (f.FirstChild as XmlElement);
		}

		[Test]
		public void CanEdit ()
		{
			XmlDocument doc = new XmlDocument ();
			Assert.IsTrue (doc.CreateNavigator ().CanEdit);
			Assert.IsTrue (GetInstance ("<root/>").CanEdit);
		}

		[Test]
		public void DeleteSelfAttribute ()
		{
			// bug #376210.
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<test><node date='2000-12-23'>z</node></test>");
			XPathNavigator navigator = document.CreateNavigator ();
			XPathNavigator nodeElement = navigator.SelectSingleNode ("//node");
			nodeElement.MoveToAttribute ("date", String.Empty);
			nodeElement.DeleteSelf ();
			Assert.AreEqual ("<test><node>z</node></test>", document.OuterXml);
		}

		[Test]
		public void WriteAttributeOnAppendedChild ()
		{
			XmlDocument x = new XmlDocument ();
			XmlElement y = x.CreateElement ("test");
			using (XmlWriter w = y.CreateNavigator ().AppendChild ())
				w.WriteAttributeString ("test", "test1");
		}

		[Test] // bug #1558
		public void CreateNavigatorReturnsEdidable ()
		{
			var document = new XmlDocument();
			document.LoadXml ("<div>hello world</div>");
			XPathNavigator navigator = document.CreateNavigator ().CreateNavigator ();
			navigator.SelectSingleNode ("//div").SetValue ("hello world 2");
		}
	}
}

#endif
