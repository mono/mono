//
// MonoTests.System.Xml.SelectNodesTests
//
// Author: Jason Diamond <jason@injektilo.org>
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Jason Diamond
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml.XPath
{
	[TestFixture]
	public class SelectNodesTests : Assertion
	{

		[Test]
		public void Root ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/");
			AssertEquals (1, nodes.Count);
			AssertSame (document, nodes [0]);
		}

		[Test]
		public void DocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/foo");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		[Test]
		public void BadDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/bar");
			AssertEquals (0, nodes.Count);
		}

		[Test]
		public void ElementWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/*");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [1]);
		}

		[Test]
		public void OneChildElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		[Test]
		public void OneOtherChildElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/baz");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [0]);
		}

		[Test]
		public void TextNode ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo>bar</foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/text()");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		[Test]
		public void SplitTextNodes ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo>bar<baz />quux</foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/text()");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [1]);
		}

		[Test]
		public void AbbreviatedParentAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/..");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		[Test]
		public void FullParentAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/parent::node()");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		[Test]
		public void AbbreviatedAttributeAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@bar");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.Attributes ["bar"], nodes [0]);
		}

		[Test]
		public void FullAttributeAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/attribute::bar");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.Attributes ["bar"], nodes [0]);
		}

		[Test]
		public void AbbreviatedAttributeWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@*");
			AssertEquals (2, nodes.Count);
			// are the attributes guanteed to be ordered in the node list?
			AssertSame (document.DocumentElement.Attributes ["bar"], nodes [0]);
			AssertSame (document.DocumentElement.Attributes ["quux"], nodes [1]);
		}

		[Test]
		public void AttributeParent ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@bar/..");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}
		
		[Test]
		public void UnionOperator ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar|/foo/baz");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [1]);
		}
		
		[Test]
		public void NodeWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar />baz<quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/node()");
			AssertEquals (3, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [1]);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [2]);
		}

		[Test]
		public void PositionalPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar>1</bar><bar>2</bar></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar[1]");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		[Test]
		public void AllFollowingSiblings ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::*");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [1]);
		}

		[Test]
		public void FollowingSiblingBaz ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::baz");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [0]);
		}

		[Test]
		public void FollowingSiblingQuux ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::quux");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [0]);
		}

		[Test]
		public void Union ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("(/foo) | (/foo)");
			AssertEquals (1, nodes.Count);	// bug #27548
			AssertSame (document.DocumentElement, nodes [0]);
		}

		[Test]
		public void AlphabetDigitMixedName ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo1 />");
			XmlNodeList nodes = document.SelectNodes ("/foo1");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		[Test]
		public void NamespaceSelect()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<root xmlns=\"urn:foo1:foo2\"/>");
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
			nsmgr.AddNamespace("foons", "urn:foo1:foo2");
			XmlNodeList nodes = document.SelectNodes ("/foons:root", nsmgr);
			AssertEquals (1, nodes.Count);
		}
	}
}
