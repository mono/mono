//
// MonoTests.System.Xml.XPathNavigatorMatchesTests
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond
//

using System;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XPathNavigatorMatchesTests : TestCase
	{
		public XPathNavigatorMatchesTests () : base ("MonoTests.System.Xml.XPathNavigatorMatchesTests testsuite") {}
		public XPathNavigatorMatchesTests (string name) : base (name) {}

		public void TestMatchRoot ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.CreateNavigator ();

			Assert (navigator.Matches ("/"));
		}

		public void TestFalseMatchRoot ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.CreateNavigator ();

			Assert (!navigator.Matches ("foo"));
		}

		public void TestMatchDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert (navigator.Matches ("foo"));
		}

		public void TestMatchAbsoluteDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert (navigator.Matches ("/foo"));
		}

		public void TestMatchDocumentElementChild ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.CreateNavigator ();

			Assert (navigator.Matches ("bar"));
			Assert (navigator.Matches ("foo/bar"));
		}

		public void TestMatchAttribute ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo bar='baz' />");
			XPathNavigator navigator = document.DocumentElement.Attributes[0].CreateNavigator ();

			Assert (navigator.Matches ("@bar"));
			Assert (navigator.Matches ("foo/@bar"));
		}

		public void TestSlashSlash ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.FirstChild.CreateNavigator ();

			Assert (navigator.Matches ("foo//baz"));
		}

		public void TestAbsoluteSlashSlash ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XPathNavigator navigator = document.DocumentElement.FirstChild.FirstChild.CreateNavigator ();

			Assert (navigator.Matches ("//baz"));
		}

		public void TestMatchDocumentElementWithPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert (navigator.Matches ("foo[bar]"));
		}

		public void TestFalseMatchDocumentElementWithPredicate ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert (!navigator.Matches ("foo[baz]"));
		}
	}
}

