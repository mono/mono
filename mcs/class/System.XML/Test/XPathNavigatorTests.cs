//
// MonoTests.System.Xml.XPathNavigatorTests
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Jason Diamond
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XPathNavigatorTests
	{
		XmlDocument document;
		XPathNavigator navigator;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}
		
		[Test]
		public void CreateNavigator ()
		{
			document.LoadXml ("<foo />");
			navigator = document.CreateNavigator ();
			Assertion.AssertNotNull (navigator);
		}

		[Test]
		public void PropertiesOnDocument ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.CreateNavigator ();
			
			Assertion.AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assertion.AssertEquals (String.Empty, navigator.Name);
			Assertion.AssertEquals (String.Empty, navigator.LocalName);
			Assertion.AssertEquals (String.Empty, navigator.NamespaceURI);
			Assertion.AssertEquals (String.Empty, navigator.Prefix);
			Assertion.Assert (!navigator.HasAttributes);
			Assertion.Assert (navigator.HasChildren);
			Assertion.Assert (!navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnElement ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.DocumentElement.CreateNavigator ();
			
			Assertion.AssertEquals (XPathNodeType.Element, navigator.NodeType);
			Assertion.AssertEquals ("foo:bar", navigator.Name);
			Assertion.AssertEquals ("bar", navigator.LocalName);
			Assertion.AssertEquals ("#foo", navigator.NamespaceURI);
			Assertion.AssertEquals ("foo", navigator.Prefix);
			Assertion.Assert (!navigator.HasAttributes);
			Assertion.Assert (!navigator.HasChildren);
			Assertion.Assert (navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnAttribute ()
		{
			document.LoadXml ("<foo bar:baz='quux' xmlns:bar='#bar' />");
			navigator = document.DocumentElement.GetAttributeNode("baz", "#bar").CreateNavigator ();
			
			Assertion.AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			Assertion.AssertEquals ("bar:baz", navigator.Name);
			Assertion.AssertEquals ("baz", navigator.LocalName);
			Assertion.AssertEquals ("#bar", navigator.NamespaceURI);
			Assertion.AssertEquals ("bar", navigator.Prefix);
			Assertion.Assert (!navigator.HasAttributes);
			Assertion.Assert (!navigator.HasChildren);
			Assertion.Assert (!navigator.IsEmptyElement);
		}

		[Test]
		public void Navigation ()
		{
			document.LoadXml ("<foo><bar /><baz /></foo>");
			navigator = document.DocumentElement.CreateNavigator ();
			
			Assertion.AssertEquals ("foo", navigator.Name);
			Assertion.Assert (navigator.MoveToFirstChild ());
			Assertion.AssertEquals ("bar", navigator.Name);
			Assertion.Assert (navigator.MoveToNext ());
			Assertion.AssertEquals ("baz", navigator.Name);
			Assertion.Assert (!navigator.MoveToNext ());
			Assertion.AssertEquals ("baz", navigator.Name);
			Assertion.Assert (navigator.MoveToPrevious ());
			Assertion.AssertEquals ("bar", navigator.Name);
			Assertion.Assert (!navigator.MoveToPrevious ());
			Assertion.Assert (navigator.MoveToParent ());
			Assertion.AssertEquals ("foo", navigator.Name);
			navigator.MoveToRoot ();
			Assertion.AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assertion.Assert (!navigator.MoveToParent ());
			Assertion.AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assertion.Assert (navigator.MoveToFirstChild ());
			Assertion.AssertEquals ("foo", navigator.Name);
			Assertion.Assert (navigator.MoveToFirst ());
			Assertion.AssertEquals ("foo", navigator.Name);
			Assertion.Assert (navigator.MoveToFirstChild ());
			Assertion.AssertEquals ("bar", navigator.Name);
			Assertion.Assert (navigator.MoveToNext ());
			Assertion.AssertEquals ("baz", navigator.Name);
			Assertion.Assert (navigator.MoveToFirst ());
			Assertion.AssertEquals ("bar", navigator.Name);
		}

		[Test]
		public void MoveToAndIsSamePosition ()
		{
			XmlDocument document1 = new XmlDocument ();
			document1.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator1a = document1.DocumentElement.CreateNavigator ();
			XPathNavigator navigator1b = document1.DocumentElement.CreateNavigator ();

			XmlDocument document2 = new XmlDocument ();
			document2.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator2 = document2.DocumentElement.CreateNavigator ();

			Assertion.AssertEquals ("foo", navigator1a.Name);
			Assertion.Assert (navigator1a.MoveToFirstChild ());
			Assertion.AssertEquals ("bar", navigator1a.Name);

			Assertion.Assert (!navigator1b.IsSamePosition (navigator1a));
			Assertion.AssertEquals ("foo", navigator1b.Name);
			Assertion.Assert (navigator1b.MoveTo (navigator1a));
			Assertion.Assert (navigator1b.IsSamePosition (navigator1a));
			Assertion.AssertEquals ("bar", navigator1b.Name);

			Assertion.Assert (!navigator2.IsSamePosition (navigator1a));
			Assertion.AssertEquals ("foo", navigator2.Name);
			Assertion.Assert (!navigator2.MoveTo (navigator1a));
			Assertion.AssertEquals ("foo", navigator2.Name);
		}

		[Test]
		public void AttributeNavigation ()
		{
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			navigator = document.DocumentElement.CreateNavigator ();

			Assertion.AssertEquals (XPathNodeType.Element, navigator.NodeType);
			Assertion.AssertEquals ("foo", navigator.Name);
			Assertion.Assert (navigator.MoveToFirstAttribute ());
			Assertion.AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			Assertion.AssertEquals ("bar", navigator.Name);
			Assertion.AssertEquals ("baz", navigator.Value);
			Assertion.Assert (navigator.MoveToNextAttribute ());
			Assertion.AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			Assertion.AssertEquals ("quux", navigator.Name);
			Assertion.AssertEquals ("quuux", navigator.Value);
		}

		[Test]
		public void ElementAndRootValues()
		{
			document.LoadXml ("<foo><bar>baz</bar><quux>quuux</quux></foo>");
			navigator = document.DocumentElement.CreateNavigator ();

			Assertion.AssertEquals (XPathNodeType.Element, navigator.NodeType);
			Assertion.AssertEquals ("foo", navigator.Name);
			//Assertion.AssertEquals ("bazquuux", navigator.Value);

			navigator.MoveToRoot ();
			//Assertion.AssertEquals ("bazquuux", navigator.Value);
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			document.LoadXml ("<?xml version=\"1.0\" standalone=\"yes\"?>\"<Root><foo>bar</foo></Root>");
			navigator = document.CreateNavigator ();

			navigator.MoveToRoot ();
			navigator.MoveToFirstChild ();
			Assertion.AssertEquals (XPathNodeType.Element, navigator.NodeType);
			Assertion.AssertEquals ("Root", navigator.Name);
		}
	}
}
