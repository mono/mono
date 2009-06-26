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
		public void NamespaceSelect ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<root xmlns=\"urn:foo1:foo2\"/>");
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
			nsmgr.AddNamespace("foons", "urn:foo1:foo2");
			XmlNodeList nodes = document.SelectNodes ("/foons:root", nsmgr);
			AssertEquals (1, nodes.Count);
		}

		[Test]
		public void NamespaceSelectWithNsElasure ()
		{
			XmlDocument doc = new XmlDocument ();

			doc.LoadXml ("<root xmlns='urn:root' xmlns:hoge='urn:hoge'><foo xmlns='urn:foo'><bar xmlns=''><baz/></bar></foo></root>");
			XmlNode n = doc.FirstChild.FirstChild.FirstChild.FirstChild; //baz
			XmlNodeList nl = n.SelectNodes ("namespace::*");
			AssertEquals ("hoge", nl [0].LocalName);
			AssertEquals ("xml", nl [1].LocalName);
			AssertEquals (2, nl.Count);

			n = doc.FirstChild.FirstChild; // foo
			nl = n.SelectNodes ("namespace::*");
			Console.WriteLine ("at foo::");
			AssertEquals ("xmlns", nl [0].LocalName);
			AssertEquals ("hoge", nl [1].LocalName);
			AssertEquals ("xml", nl [2].LocalName);
			AssertEquals (3, nl.Count);
		}

		[Test]
		public void AncestorAxis () {
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><bar><baz><bax /></baz></bar></foo>");

			XmlNode bar = doc.GetElementsByTagName ("bar") [0];
			XmlElement barClone = (XmlElement) bar.CloneNode (true);
			XmlNodeList baxs = barClone.GetElementsByTagName ("bax");

			XmlNode bax = baxs [0];
			XmlNodeList ans = bax.SelectNodes ("ancestor::*");
			AssertEquals (2, ans.Count);
			AssertEquals ("bar", ans [0].Name);
			AssertEquals ("baz", ans [1].Name);
		}

		[Test] // bug #458245
		public void SelectFromDetachedAttribute ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<a></a>");
			XmlNode attr = doc.CreateAttribute ("b");
			attr.SelectSingleNode ("//*[@id='foo']");
		}

		[Test]
		public void Bug443490 ()
		{
			string xml = "<foo xmlns='urn:foo'><bar><div id='e1'> <div id='e1.1'> <div id='e1.1.1'> <div id='e1.1.1.1'> <div id='e1.1.1.1.1'/> </div> <div id='e1.1.1.2'/> </div> </div> </div></bar></foo>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNamespaceManager ns = new XmlNamespaceManager (doc.NameTable);
			ns.AddNamespace ("_", "urn:foo");
			string xpath = "//_:div//_:div//_:div";
			var nodes = doc.SelectNodes (xpath, ns);
			AssertEquals (4, nodes.Count);
		}

		[Test]
		public void Bug443090_2 ()
		{
			string xml = @"
<html xmlns='http://www.w3.org/1999/xhtml'>
<body>
<div id='e1'>
    <div id='e1.1'>
        <div id='e1.1.1'/>
        <div id='e1.1.2'>
          <div id='e1.1.2.1'>
              <div id='e1.1.2.1.1'>e1.1.2.1.1</div>
          </div>
        </div>
    </div>
</div>
</body>
</html>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNamespaceManager ns = new XmlNamespaceManager (doc.NameTable);
			ns.AddNamespace ("_", "http://www.w3.org/1999/xhtml");

			XmlNode n = doc.SelectSingleNode ("//_:html", ns);
			Assert ("#1", n != null);
			XmlNodeList nodes = n.SelectNodes (".//_:div//_:div", ns);
			AssertEquals ("#2", 5, nodes.Count);
		}
	}
}
