//
// System.Xml.XmlDocumentFragment.cs
//
// Author: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlDocumentFragmentTests : TestCase
	{
		XmlDocument document;
		XmlDocumentFragment fragment;
		
		public XmlDocumentFragmentTests(string name)
			: base (name)
		{
		}

		protected override void SetUp()
		{
		}

		public void TestConstructor()
		{
			XmlDocument d = new XmlDocument();
			XmlDocumentFragment df = d.CreateDocumentFragment();
			AssertEquals("#Constructor.NodeName", "#document-fragment", df.Name);
			AssertEquals("#Constructor.NodeType", XmlNodeType.DocumentFragment, df.NodeType);
		}

		public void TestAppendChildToFragment()
		{
			document = new XmlDocument();
			fragment = document.CreateDocumentFragment();
			document.LoadXml("<html><head></head><body></body></html>");
			XmlElement el = document.CreateElement("p");
			el.InnerXml = "Test Paragraph";

			// appending element to fragment
			fragment.AppendChild(el);
			AssertNotNull("#AppendChildToFragment.Element", fragment.FirstChild);
			AssertNotNull("#AppendChildToFragment.Element.Children", fragment.FirstChild.FirstChild);
			AssertEquals("#AppendChildToFragment.Element.Child.Text", "Test Paragraph", fragment.FirstChild.FirstChild.Value);
		}

		public void TestAppendFragmentToElement()
		{
			document = new XmlDocument();
			fragment = document.CreateDocumentFragment();
			document.LoadXml("<html><head></head><body></body></html>");
			XmlElement body = document.DocumentElement.LastChild as XmlElement;
			fragment.AppendChild(document.CreateElement("p"));
			fragment.AppendChild(document.CreateElement("div"));

			// appending fragment to element
			body.AppendChild(fragment);
			AssertNotNull("#AppendFragmentToElement.Exist", body.FirstChild);
			AssertEquals("#AppendFragmentToElement.ChildIsElement", XmlNodeType.Element, body.FirstChild.NodeType);
			AssertEquals("#AppendFragmentToElement.FirstChild", "p", body.FirstChild.Name);
			AssertEquals("#AppendFragmentToElement.LastChild", "div", body.LastChild.Name);
		}
	}
}
