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
	}
}

#endif
