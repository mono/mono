using System;
using System.Diagnostics;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlDocumentTests : TestCase
	{
		public XmlDocumentTests() : base("Ximian.Mono.Tests.XmlDocumentTests testsuite") { }
		public XmlDocumentTests(string name) : base(name) { }

		public void TestDocumentElement()
		{
			XmlDocument document = new XmlDocument();
			AssertNull(document.DocumentElement);

			XmlElement element = document.CreateElement("foo", "bar", "http://foo/");
			AssertNotNull(element);

			AssertEquals("foo", element.Prefix);
			AssertEquals("bar", element.LocalName);
			AssertEquals("http://foo/", element.NamespaceURI);

			AssertEquals("foo:bar", element.Name);

			AssertSame(element, document.AppendChild(element));

			AssertSame(element, document.DocumentElement);
		}
	}
}
