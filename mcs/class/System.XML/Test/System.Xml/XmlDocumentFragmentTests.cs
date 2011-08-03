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
			Assert.AreEqual ("#document-fragment", df.Name, "#Constructor.NodeName");
			Assert.AreEqual (XmlNodeType.DocumentFragment, df.NodeType, "#Constructor.NodeType");
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
			Assert.IsNotNull (fragment.FirstChild, "#AppendChildToFragment.Element");
			Assert.IsNotNull (fragment.FirstChild.FirstChild, "#AppendChildToFragment.Element.Children");
			Assert.AreEqual ("Test Paragraph", fragment.FirstChild.FirstChild.Value, "#AppendChildToFragment.Element.Child.Text");
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
			XmlNode ret = body.AppendChild (fragment);
			Assert.IsNotNull (body.FirstChild, "#AppendFragmentToElement.Exist");
			Assert.AreEqual (XmlNodeType.Element, body.FirstChild.NodeType, "#AppendFragmentToElement.ChildIsElement");
			Assert.AreEqual ("p", body.FirstChild.Name, "#AppendFragmentToElement.FirstChild");
			Assert.AreEqual ("div", body.LastChild.Name, "#AppendFragmentToElement.LastChild");
			Assert.AreEqual ("p", ret.LocalName, "#AppendFragmentToElement.ReturnValue");
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
			Assert.AreEqual ("<foo /><bar /><baz />", fragment.InnerXml, "#Simple");
		}

		[Test]
		public void SetInnerXml ()
		{
			document = new XmlDocument ();
			fragment = document.CreateDocumentFragment ();
			fragment.InnerXml = "<foo /><bar><child /></bar><baz />";
			Assert.AreEqual ("foo", fragment.FirstChild.Name);
			Assert.AreEqual ("bar", fragment.FirstChild.NextSibling.Name);
			Assert.AreEqual ("child", fragment.FirstChild.NextSibling.FirstChild.Name);
			Assert.AreEqual ("baz", fragment.LastChild.Name);
		}

		[Test]
		public void InnerText ()
		{
			document = new XmlDocument ();
			fragment = document.CreateDocumentFragment ();
			string text = "<foo /><bar><child /></bar><baz />";
			fragment.InnerText = text;
			Assert.AreEqual (text, fragment.InnerText);
		}
	}
}
