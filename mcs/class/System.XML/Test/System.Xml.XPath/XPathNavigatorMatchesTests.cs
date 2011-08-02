//
// MonoTests.System.Xml.XPathNavigatorMatchesTests
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
	public class XPathNavigatorMatchesTests
	{
		private XPathNavigator CreateNavigator (string xml)
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml (xml);
			return document.CreateNavigator ();
		}

		[Test]
		public void MatchRoot ()
		{
			XPathNavigator navigator = CreateNavigator ("<foo />");
			Assert.IsTrue (navigator.Matches ("/"));
		}

		[Test]
		public void FalseMatchRoot ()
		{
			XPathNavigator navigator = CreateNavigator ("<foo />");
			Assert.IsTrue (!navigator.Matches ("foo"));
		}

		[Test]
		public void MatchDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("foo"));
		}

		[Test]
		public void MatchAbsoluteDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("/foo"));
		}

		[Test]
		public void MatchDocumentElementChild ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("bar"));
			Assert.IsTrue (navigator.Matches ("foo/bar"));
		}

		[Test]
		public void MatchAttribute ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XPathNavigator navigator = document.DocumentElement.Attributes[0].CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("@bar"));
			Assert.IsTrue (navigator.Matches ("foo/@bar"));
		}

		[Test]
		public void SlashSlash ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.FirstChild.CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("foo//baz"));
		}

		[Test]
		public void AbsoluteSlashSlash ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.FirstChild.CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("//baz"));
		}

		[Test]
		public void MatchDocumentElementWithPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert.IsTrue (navigator.Matches ("foo[bar]"));
		}

		[Test]
		public void FalseMatchDocumentElementWithPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert.IsTrue (!navigator.Matches ("foo[baz]"));
		}

		[Test]
		public void MatchesAncestorsButNotCurrent ()
		{
			XPathNavigator nav = CreateNavigator ("<foo><bar><baz/></bar></foo>");
			nav.MoveToFirstChild (); // foo
			nav.MoveToFirstChild (); // bar
			nav.MoveToFirstChild (); // baz
			Assert.IsTrue (nav.Matches ("baz"));
			Assert.IsTrue (nav.Matches ("bar/baz"));
			Assert.IsTrue (!nav.Matches ("foo/bar"));
		}

		[Test]
		[ExpectedException (typeof (XPathException))]
		public void MatchesParentAxis ()
		{
			XPathNavigator nav = CreateNavigator ("<foo/>");
			nav.Matches ("..");
		}

		[Test]
		[ExpectedException (typeof (XPathException))]
		public void MatchesPredicatedParentAxis ()
		{
			XPathNavigator nav = CreateNavigator ("<foo/>");
			nav.Matches ("..[1]");
		}
	}
}

