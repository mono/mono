using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlDocumentTests : TestCase
	{
		public XmlDocumentTests () : base ("Ximian.Mono.Tests.XmlDocumentTests testsuite") {}
		public XmlDocumentTests (string name) : base (name) {}

		private XmlDocument document;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
		}
		
		public void TestLoadXmlSingleElement ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo/>");
			AssertNotNull (document.DocumentElement);

			AssertSame (document.FirstChild, document.DocumentElement);
			AssertSame (document.ChildNodes [0], document.DocumentElement);
		}

		public void TestLoadXmlExceptionClearsDocument ()
		{
			document.LoadXml ("<foo/>");
			Assert (document.ChildNodes.Count > 0);
			
			try {
				document.LoadXml ("<123/>");
				Fail ("An XmlException should have been thrown.");
			} catch (XmlException) {}

			Assert (document.ChildNodes.Count == 0);
		}

		public void TestLoadXmlElementWithChildElement ()
		{
			document.LoadXml ("<foo><bar/></foo>");
			Assert (document.ChildNodes.Count == 1);
			Assert (document.ChildNodes [0].ChildNodes.Count == 1);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals ("bar", document.DocumentElement.ChildNodes [0].LocalName);
		}

		public void TestLoadXmlElementWithTextNode ()
		{
			document.LoadXml ("<foo>bar</foo>");
			Assert (document.DocumentElement.ChildNodes [0].NodeType == XmlNodeType.Text);
			AssertEquals ("bar", document.DocumentElement.ChildNodes [0].Value);
		}

		public void TestDocumentElement ()
		{
			AssertNull (document.DocumentElement);
			XmlElement element = document.CreateElement ("foo", "bar", "http://foo/");
			AssertNotNull (element);

			AssertEquals ("foo", element.Prefix);
			AssertEquals ("bar", element.LocalName);
			AssertEquals ("http://foo/", element.NamespaceURI);

			AssertEquals ("foo:bar", element.Name);

			AssertSame (element, document.AppendChild (element));

			AssertSame (element, document.DocumentElement);
		}

		public void TestLoadXmlElementWithAttributes ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo bar='baz' quux='quuux'/>");

			XmlElement documentElement = document.DocumentElement;

			AssertEquals ("baz", documentElement.GetAttribute ("bar"));
			AssertEquals ("quuux", documentElement.GetAttribute ("quux"));
		}
	}
}
