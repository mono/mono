//
// System.Xml.XmlDocumentFragment.cs
//
// Author: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Atsushi Enomoto
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDocumentFragmentTests
	{
		XmlDocument document;
		XmlDocumentFragment fragment;

		[Test]
		public void Constructor ()
		{
			XmlDocument d = new XmlDocument ();
			XmlDocumentFragment df = d.CreateDocumentFragment ();
			Assertion.AssertEquals ("#Constructor.NodeName", "#document-fragment", df.Name);
			Assertion.AssertEquals ("#Constructor.NodeType", XmlNodeType.DocumentFragment, df.NodeType);
		}

		[Test]
		public void AppendChildToFragment ()
		{
			document = new XmlDocument ();
			fragment = document.CreateDocumentFragment ();
			document.LoadXml ("<html><head></head><body></body></html>");
			XmlElement el = document.CreateElement ("p");
			el.InnerXml = "Test Paragraph";

			// appending element to fragment
			fragment.AppendChild (el);
			Assertion.AssertNotNull ("#AppendChildToFragment.Element", fragment.FirstChild);
			Assertion.AssertNotNull ("#AppendChildToFragment.Element.Children", fragment.FirstChild.FirstChild);
			Assertion.AssertEquals ("#AppendChildToFragment.Element.Child.Text", "Test Paragraph", fragment.FirstChild.FirstChild.Value);
		}

		[Test]
		public void AppendFragmentToElement ()
		{
			document = new XmlDocument ();
			fragment = document.CreateDocumentFragment ();
			document.LoadXml ("<html><head></head><body></body></html>");
			XmlElement body = document.DocumentElement.LastChild as XmlElement;
			fragment.AppendChild (document.CreateElement ("p"));
			fragment.AppendChild (document.CreateElement ("div"));

			// appending fragment to element
			body.AppendChild (fragment);
			Assertion.AssertNotNull ("#AppendFragmentToElement.Exist", body.FirstChild);
			Assertion.AssertEquals ("#AppendFragmentToElement.ChildIsElement", XmlNodeType.Element, body.FirstChild.NodeType);
			Assertion.AssertEquals ("#AppendFragmentToElement.FirstChild", "p", body.FirstChild.Name);
			Assertion.AssertEquals ("#AppendFragmentToElement.LastChild", "div", body.LastChild.Name);
		}

		[Test]
		public void GetInnerXml ()
		{
			// this will be also tests of TestWriteTo()/TestWriteContentTo()

			document = new XmlDocument ();
			fragment = document.CreateDocumentFragment ();
			fragment.AppendChild (document.CreateElement ("foo"));
			fragment.AppendChild (document.CreateElement ("bar"));
			fragment.AppendChild (document.CreateElement ("baz"));
			Assertion.AssertEquals ("#Simple", "<foo /><bar /><baz />", fragment.InnerXml);
		}

		[Test]
		public void SetInnerXml ()
		{
			document = new XmlDocument ();
			fragment = document.CreateDocumentFragment ();
			fragment.InnerXml = "<foo /><bar><child /></bar><baz />";
			Assertion.AssertEquals ("foo", fragment.FirstChild.Name);
			Assertion.AssertEquals ("bar", fragment.FirstChild.NextSibling.Name);
			Assertion.AssertEquals ("child", fragment.FirstChild.NextSibling.FirstChild.Name);
			Assertion.AssertEquals ("baz", fragment.LastChild.Name);
		}
	}
}
