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
	public class XPathNavigatorTests : Assertion
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
			AssertNotNull (navigator);
		}

		[Test]
		public void PropertiesOnDocument ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.CreateNavigator ();
			
			AssertEquals (XPathNodeType.Root, navigator.NodeType);
			AssertEquals (String.Empty, navigator.Name);
			AssertEquals (String.Empty, navigator.LocalName);
			AssertEquals (String.Empty, navigator.NamespaceURI);
			AssertEquals (String.Empty, navigator.Prefix);
			Assert (!navigator.HasAttributes);
			Assert (navigator.HasChildren);
			Assert (!navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnElement ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.DocumentElement.CreateNavigator ();
			
			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("foo:bar", navigator.Name);
			AssertEquals ("bar", navigator.LocalName);
			AssertEquals ("#foo", navigator.NamespaceURI);
			AssertEquals ("foo", navigator.Prefix);
			Assert (!navigator.HasAttributes);
			Assert (!navigator.HasChildren);
			Assert (navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnAttribute ()
		{
			document.LoadXml ("<foo bar:baz='quux' xmlns:bar='#bar' />");
			navigator = document.DocumentElement.GetAttributeNode("baz", "#bar").CreateNavigator ();
			
			AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			AssertEquals ("bar:baz", navigator.Name);
			AssertEquals ("baz", navigator.LocalName);
			AssertEquals ("#bar", navigator.NamespaceURI);
			AssertEquals ("bar", navigator.Prefix);
			Assert (!navigator.HasAttributes);
			Assert (!navigator.HasChildren);
			Assert (!navigator.IsEmptyElement);
		}

		[Test]
		public void Navigation ()
		{
			document.LoadXml ("<foo><bar /><baz /></foo>");
			navigator = document.DocumentElement.CreateNavigator ();
			
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirstChild ());
			AssertEquals ("bar", navigator.Name);
			Assert (navigator.MoveToNext ());
			AssertEquals ("baz", navigator.Name);
			Assert (!navigator.MoveToNext ());
			AssertEquals ("baz", navigator.Name);
			Assert (navigator.MoveToPrevious ());
			AssertEquals ("bar", navigator.Name);
			Assert (!navigator.MoveToPrevious ());
			Assert (navigator.MoveToParent ());
			AssertEquals ("foo", navigator.Name);
			navigator.MoveToRoot ();
			AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assert (!navigator.MoveToParent ());
			AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assert (navigator.MoveToFirstChild ());
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirst ());
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirstChild ());
			AssertEquals ("bar", navigator.Name);
			Assert (navigator.MoveToNext ());
			AssertEquals ("baz", navigator.Name);
			Assert (navigator.MoveToFirst ());
			AssertEquals ("bar", navigator.Name);
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

			AssertEquals ("foo", navigator1a.Name);
			Assert (navigator1a.MoveToFirstChild ());
			AssertEquals ("bar", navigator1a.Name);

			Assert (!navigator1b.IsSamePosition (navigator1a));
			AssertEquals ("foo", navigator1b.Name);
			Assert (navigator1b.MoveTo (navigator1a));
			Assert (navigator1b.IsSamePosition (navigator1a));
			AssertEquals ("bar", navigator1b.Name);

			Assert (!navigator2.IsSamePosition (navigator1a));
			AssertEquals ("foo", navigator2.Name);
			Assert (!navigator2.MoveTo (navigator1a));
			AssertEquals ("foo", navigator2.Name);
		}

		[Test]
		public void AttributeNavigation ()
		{
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			navigator = document.DocumentElement.CreateNavigator ();

			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirstAttribute ());
			AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			AssertEquals ("bar", navigator.Name);
			AssertEquals ("baz", navigator.Value);
			Assert (navigator.MoveToNextAttribute ());
			AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			AssertEquals ("quux", navigator.Name);
			AssertEquals ("quuux", navigator.Value);
		}

		[Test]
		public void ElementAndRootValues()
		{
			document.LoadXml ("<foo><bar>baz</bar><quux>quuux</quux></foo>");
			navigator = document.DocumentElement.CreateNavigator ();

			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("foo", navigator.Name);
			//AssertEquals ("bazquuux", navigator.Value);

			navigator.MoveToRoot ();
			//AssertEquals ("bazquuux", navigator.Value);
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			document.LoadXml ("<?xml version=\"1.0\" standalone=\"yes\"?><Root><foo>bar</foo></Root>");
			navigator = document.CreateNavigator ();

			navigator.MoveToRoot ();
			navigator.MoveToFirstChild ();
			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("Root", navigator.Name);
		}

		[Test]
		public void DocumentWithProcessingInstruction ()
		{
			document.LoadXml ("<?xml-stylesheet href='foo.xsl' type='text/xsl' ?><foo />");
			navigator = document.CreateNavigator ();

			Assert (navigator.MoveToFirstChild ());
			AssertEquals (XPathNodeType.ProcessingInstruction, navigator.NodeType);
			AssertEquals ("xml-stylesheet", navigator.Name);

			XPathNodeIterator iter = navigator.SelectChildren (XPathNodeType.Element);
			AssertEquals (0, iter.Count);
		}
	}
}
