//
// MonoTests.System.Xml.SelectNodesTests
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class SelectNodesTests : TestCase
	{
		public SelectNodesTests () : base ("MonoTests.System.Xml.SelectNodesTests testsuite") {}
		public SelectNodesTests (string name) : base (name) {}

		public void TestRoot ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/");
			AssertEquals (1, nodes.Count);
			AssertSame (document, nodes [0]);
		}

		public void TestDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/foo");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		public void TestBadDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/bar");
			AssertEquals (0, nodes.Count);
		}

		public void TestElementWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/*");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [1]);
		}

		public void TestOneChildElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		public void TestOneOtherChildElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/baz");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [0]);
		}

		public void TestTextNode ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo>bar</foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/text()");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		public void TestSplitTextNodes ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo>bar<baz />quux</foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/text()");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [1]);
		}

		public void TestAbbreviatedParentAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/..");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		public void TestFullParentAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/parent::node()");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}

		public void TestAbbreviatedAttributeAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@bar");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.Attributes ["bar"], nodes [0]);
		}

		public void TestFullAttributeAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/attribute::bar");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.Attributes ["bar"], nodes [0]);
		}

		public void TestAbbreviatedAttributeWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@*");
			AssertEquals (2, nodes.Count);
			// are the attributes guanteed to be ordered in the node list?
			AssertSame (document.DocumentElement.Attributes ["bar"], nodes [0]);
			AssertSame (document.DocumentElement.Attributes ["quux"], nodes [1]);
		}

		public void TestAttributeParent ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@bar/..");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}
		
		public void TestUnionOperator ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar|/foo/baz");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [1]);
		}
		
		public void TestNodeWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar />baz<quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/node()");
			AssertEquals (3, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [1]);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [2]);
		}

		public void TestPositionalPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar>1</bar><bar>2</bar></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar[1]");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		public void TestAllFollowingSiblings ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::*");
			AssertEquals (2, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [0]);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [1]);
		}

		public void TestFollowingSiblingBaz ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::baz");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [1], nodes [0]);
		}

		public void TestFollowingSiblingQuux ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::quux");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement.ChildNodes [2], nodes [0]);
		}

		public void TestUnion ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("(/foo) | (/foo)");
			AssertEquals (1, nodes.Count);	// bug #27548
			AssertSame (document.DocumentElement, nodes [0]);
		}

		public void TestAlphabetDigitMixedName ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo1 />");
			XmlNodeList nodes = document.SelectNodes ("/foo1");
			AssertEquals (1, nodes.Count);
			AssertSame (document.DocumentElement, nodes [0]);
		}


		public void TestNamespaceSelect()
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
