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

		public void TestMatchDocumentElement ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<foo />");
			XPathNavigator navigator = document.DocumentElement.CreateNavigator ();

			Assert (navigator.Matches ("foo"));
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
	}
}

