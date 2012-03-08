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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml.XPath
{
	[TestFixture]
	public class SelectNodesTests
	{

		[Test]
		public void Root ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document, nodes [0], "#2");
		}

		[Test]
		public void DocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/foo");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement, nodes [0], "#2");
		}

		[Test]
		public void BadDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("/bar");
			Assert.AreEqual (0, nodes.Count, "#1");
		}

		[Test]
		public void ElementWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/*");
			Assert.AreEqual (2, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0], "#2");
			Assert.AreSame (document.DocumentElement.ChildNodes [1], nodes [1], "#3");
		}

		[Test]
		public void OneChildElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0], "#2");
		}

		[Test]
		public void OneOtherChildElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/baz");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [1], nodes [0]);
		}

		[Test]
		public void TextNode ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo>bar</foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/text()");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0]);
		}

		[Test]
		public void SplitTextNodes ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo>bar<baz />quux</foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/text()");
			Assert.AreEqual (2, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0], "#2");
			Assert.AreSame (document.DocumentElement.ChildNodes [2], nodes [1], "#3");
		}

		[Test]
		public void AbbreviatedParentAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/..");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement, nodes [0], "#2");
		}

		[Test]
		public void FullParentAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/parent::node()");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement, nodes [0], "#2");
		}

		[Test]
		public void AbbreviatedAttributeAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@bar");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.Attributes ["bar"], nodes [0], "#2");
		}

		[Test]
		public void FullAttributeAxis ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/attribute::bar");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.Attributes ["bar"], nodes [0], "#2");
		}

		[Test]
		public void AbbreviatedAttributeWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@*");
			Assert.AreEqual (2, nodes.Count, "#1");
			// are the attributes guanteed to be ordered in the node list?
			Assert.AreSame (document.DocumentElement.Attributes ["bar"], nodes [0], "#2");
			Assert.AreSame (document.DocumentElement.Attributes ["quux"], nodes [1], "#3");
		}

		[Test]
		public void AttributeParent ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XmlNodeList nodes = document.SelectNodes ("/foo/@bar/..");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement, nodes [0], "#2");
		}
		
		[Test]
		public void UnionOperator ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar|/foo/baz");
			Assert.AreEqual (2, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0], "#2");
			Assert.AreSame (document.DocumentElement.ChildNodes [1], nodes [1], "#3");
		}
		
		[Test]
		public void NodeWildcard ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar />baz<quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/node()");
			Assert.AreEqual (3, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0], "#2");
			Assert.AreSame (document.DocumentElement.ChildNodes [1], nodes [1], "#3");
			Assert.AreSame (document.DocumentElement.ChildNodes [2], nodes [2], "#4");
		}

		[Test]
		public void PositionalPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar>1</bar><bar>2</bar></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar[1]");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [0], nodes [0], "#2");
		}

		[Test]
		public void AllFollowingSiblings ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::*");
			Assert.AreEqual (2, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [1], nodes [0], "#2");
			Assert.AreSame (document.DocumentElement.ChildNodes [2], nodes [1], "#3");
		}

		[Test]
		public void FollowingSiblingBaz ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::baz");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [1], nodes [0], "#2");
		}

		[Test]
		public void FollowingSiblingQuux ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /><baz /><quux /></foo>");
			XmlNodeList nodes = document.SelectNodes ("/foo/bar/following-sibling::quux");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement.ChildNodes [2], nodes [0], "#2");
		}

		[Test]
		public void Union ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XmlNodeList nodes = document.SelectNodes ("(/foo) | (/foo)");
			Assert.AreEqual (1, nodes.Count);	// bug #27548, "#1");
			Assert.AreSame (document.DocumentElement, nodes [0], "#1");
		}

		[Test]
		public void AlphabetDigitMixedName ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo1 />");
			XmlNodeList nodes = document.SelectNodes ("/foo1");
			Assert.AreEqual (1, nodes.Count, "#1");
			Assert.AreSame (document.DocumentElement, nodes [0], "#2");
		}

		[Test]
		public void NamespaceSelect ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<root xmlns=\"urn:foo1:foo2\"/>");
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
			nsmgr.AddNamespace("foons", "urn:foo1:foo2");
			XmlNodeList nodes = document.SelectNodes ("/foons:root", nsmgr);
			Assert.AreEqual (1, nodes.Count, "#1");
		}

		[Test]
		public void NamespaceSelectWithNsElasure ()
		{
			XmlDocument doc = new XmlDocument ();

			doc.LoadXml ("<root xmlns='urn:root' xmlns:hoge='urn:hoge'><foo xmlns='urn:foo'><bar xmlns=''><baz/></bar></foo></root>");
			XmlNode n = doc.FirstChild.FirstChild.FirstChild.FirstChild; //baz
			XmlNodeList nl = n.SelectNodes ("namespace::*");
			Assert.AreEqual ("hoge", nl [0].LocalName, "#1");
			Assert.AreEqual ("xml", nl [1].LocalName, "#2");
			Assert.AreEqual (2, nl.Count, "#3");

			n = doc.FirstChild.FirstChild; // foo
			nl = n.SelectNodes ("namespace::*");
			Console.WriteLine ("at foo::");
			Assert.AreEqual ("xmlns", nl [0].LocalName, "#1");
			Assert.AreEqual ("hoge", nl [1].LocalName, "#2");
			Assert.AreEqual ("xml", nl [2].LocalName, "#3");
			Assert.AreEqual (3, nl.Count, "#4");
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

			Assert.AreEqual (2, ans.Count, "#1");
			Assert.AreEqual ("bar", ans [0].Name, "#2");
			Assert.AreEqual ("baz", ans [1].Name, "#3");

			/* Should include the root node
			   see http://www.w3.org/TR/xpath#axes
			*/
			bax = doc.GetElementsByTagName ("bax")[0];
			ans = bax.SelectNodes ("ancestor::*");

			Assert.AreEqual (3, ans.Count, "doc#1");
			Assert.AreEqual ("foo", ans [0].Name, "doc#2");
			Assert.AreEqual ("bar", ans [1].Name, "doc#3");
			Assert.AreEqual ("baz", ans [2].Name, "doc#4");
			//*/
		}

		[Test]
		public void AncestorAxisOrder () {
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><bar><baz><bax /></baz></bar></foo>");

			/* This order, though we are going through in increasing
			   value of the "position" query parameter, is not the same
			   as the order obtained by iterating the node set, which is
			   the document order.
			   see http://www.w3.org/TR/xpath#node-sets
			 */
			XmlNode i, bax = doc.SelectSingleNode ("//bax");
			i = bax.SelectSingleNode ("ancestor::*[1]");
			Assert.AreEqual ("baz", i.Name, "#1");
			i = bax.SelectSingleNode ("ancestor::*[2]");
			Assert.AreEqual ("bar", i.Name, "#2");
			i = bax.SelectSingleNode ("ancestor::*[3]");
			Assert.AreEqual ("foo", i.Name, "#3");
			i = bax.SelectSingleNode ("ancestor::*[4]");
			Assert.AreEqual (null, i, "#3");
			i = bax.SelectSingleNode ("ancestor::*[position()=1]");
			Assert.AreEqual ("baz", i.Name, "#1fx");
			i = bax.SelectSingleNode ("ancestor::*[position()=2]");
			Assert.AreEqual ("bar", i.Name, "#2fx");
			i = bax.SelectSingleNode ("ancestor::*[position()=3]");
			Assert.AreEqual ("foo", i.Name, "#3fx");
			i = bax.SelectSingleNode ("ancestor::*[position()=4]");
			Assert.AreEqual (null, i, "#3");
		}

		[Test]
		public void AncestorOrSelfAxis () {
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><bar><baz><bax /></baz></bar></foo>");

			XmlNode bar = doc.GetElementsByTagName ("bar") [0];
			XmlElement barClone = (XmlElement) bar.CloneNode (true);
			XmlNodeList baxs = barClone.GetElementsByTagName ("bax");

			XmlNode bax = baxs [0];
			XmlNodeList ans = bax.SelectNodes ("ancestor-or-self::*");

			Assert.AreEqual (3, ans.Count, "#1");
			Assert.AreEqual ("bar", ans [0].Name, "#2");
			Assert.AreEqual ("baz", ans [1].Name, "#3");
			Assert.AreEqual ("bax", ans [2].Name, "#4");

			/*  Should include the root node
			   see http://www.w3.org/TR/xpath#axes
			*/
			bax = doc.GetElementsByTagName ("bax")[0];
			ans = bax.SelectNodes ("ancestor-or-self::*");

			Assert.AreEqual (4, ans.Count, "#1");
			Assert.AreEqual ("foo", ans [0].Name, "#2");
			Assert.AreEqual ("bar", ans [1].Name, "#3");
			Assert.AreEqual ("baz", ans [2].Name, "#4");
			Assert.AreEqual ("bax", ans [3].Name, "#5");
			//*/
		}

		[Test]
		public void AncestorOrSelfAxisOrder () {
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><bar><baz><bax /></baz></bar></foo>");

			XmlNode i, bax = doc.SelectSingleNode ("//bax");
			i = bax.SelectSingleNode ("ancestor-or-self::*[1]");
			Assert.AreEqual ("bax", i.Name, "#1");
			i = bax.SelectSingleNode ("ancestor-or-self::*[2]");
			Assert.AreEqual ("baz", i.Name, "#2");
			i = bax.SelectSingleNode ("ancestor-or-self::*[3]");
			Assert.AreEqual ("bar", i.Name, "#3");
			i = bax.SelectSingleNode ("ancestor-or-self::*[4]");
			Assert.AreEqual ("foo", i.Name, "#4");
			i = bax.SelectSingleNode ("ancestor-or-self::*[5]");
			Assert.AreEqual (null, i, "#5");
			i = bax.SelectSingleNode ("ancestor-or-self::*[position()=1]");
			Assert.AreEqual ("bax", i.Name, "#1fx");
			i = bax.SelectSingleNode ("ancestor-or-self::*[position()=2]");
			Assert.AreEqual ("baz", i.Name, "#2fx");
			i = bax.SelectSingleNode ("ancestor-or-self::*[position()=3]");
			Assert.AreEqual ("bar", i.Name, "#3fx");
			i = bax.SelectSingleNode ("ancestor-or-self::*[position()=4]");
			Assert.AreEqual ("foo", i.Name, "#4fx");
			i = bax.SelectSingleNode ("ancestor-or-self::*[position()=5]");
			Assert.AreEqual (null, i, "#5");
		}

		[Test] // bug #497017
		public void AncestorAxisOrder2 () {
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<item id=\"1\"><item id=\"2\"><n/></item></item>");
			XmlNode id, n = doc.SelectSingleNode ("/item/item/n");

			id = n.SelectSingleNode ("ancestor::item[@id][position()=1]/@id");
			Assert.AreEqual ("2", id.InnerText, "Nearest ancestor is 2");

			id = n.SelectSingleNode ("ancestor::item[@id][2]/@id");
			Assert.AreEqual ("1", id.InnerText, "Second ancestor is 1");
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
			Assert.AreEqual (4, nodes.Count, "#1");
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
			Assert.IsTrue (n != null, "#1");
			XmlNodeList nodes = n.SelectNodes (".//_:div//_:div", ns);
			Assert.AreEqual (5, nodes.Count, "#2");
		}

		// This test requires Linq.
		[Test] // xamarin bug #3705
		public void ReturnedNavigatorInstancesUnique ()
		{
			string testDocument = 
				@"<Notes>
					<Note><Reference>a</Reference>Some text1</Note>
					<Note><Reference>b</Reference>Some text2</Note>
					<Note><Reference>a</Reference>Some text3</Note>
				</Notes>";
			
			StringReader r = new StringReader (testDocument);
			var data = new XPathDocument (r);
			var l = new List<XPathNavigator> ();

			// test PredicateIterator wrapped by XPathNodeIteratorEnumerator.
			var parent = data.CreateNavigator ().SelectSingleNode ("/Notes");
			foreach (XPathNavigator n in parent.Select ("Note[Reference]")) {
				Assert.IsFalse (l.Contains (n), "should not iterate the same XPathNavigator instance twice, regardless of whether position is same or not");
				l.Add (n);
			}

			// Same test for SimpleSlashIterator wrapped by XPathNodeIteratorEnumerator.
			l.Clear ();
			foreach (XPathNavigator n in data.CreateNavigator ().Select ("/Notes/Note[Reference]")) {
				Assert.IsFalse (l.Contains (n), "should not iterate the same XPathNavigator instance twice, regardless of whether position is same or not");
				l.Add (n);
			}

			// test orderby too. In short, XPathNodeIteratorEnumerator is the key player that assures XPathNavigator uniqueness.
			foreach (XPathNavigator n in data.CreateNavigator ()
				.Select ("/Notes/Note[Reference]")
				.Cast<XPathNavigator> ()
				.OrderBy (nav => nav.SelectSingleNode ("Reference").Value)) {
				Assert.IsFalse (l.Contains (n), "should not iterate the same XPathNavigator instance twice, regardless of whether position is same or not");
				l.Add (n);
			}
		}
	}
}
