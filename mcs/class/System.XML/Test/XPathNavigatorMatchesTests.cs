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
		[Test]
		public void MatchRoot ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("/"));
		}

		[Test]
		public void FalseMatchRoot ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.CreateNavigator ();

			Assertion.Assert (!navigator.Matches ("foo"));
		}

		[Test]
		public void MatchDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("foo"));
		}

		[Test]
		public void MatchAbsoluteDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("/foo"));
		}

		[Test]
		public void MatchDocumentElementChild ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("bar"));
			Assertion.Assert (navigator.Matches ("foo/bar"));
		}

		[Test]
		public void MatchAttribute ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XPathNavigator navigator = document.DocumentElement.Attributes[0].CreateNavigator ();

			Assertion.Assert (navigator.Matches ("@bar"));
			Assertion.Assert (navigator.Matches ("foo/@bar"));
		}

		[Test]
		public void SlashSlash ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.FirstChild.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("foo//baz"));
		}

		[Test]
		public void AbsoluteSlashSlash ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.FirstChild.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("//baz"));
		}

		[Test]
		public void MatchDocumentElementWithPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assertion.Assert (navigator.Matches ("foo[bar]"));
		}

		[Test]
		public void FalseMatchDocumentElementWithPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assertion.Assert (!navigator.Matches ("foo[baz]"));
		}
	}
}

