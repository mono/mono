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

		public void TestCreateProcessingInstructionInvalid()
		{
			XmlProcessingInstruction processingInstruction;
//			string outerXml;

			// A newly created node should/shouldn't? have a parent or a documentelement?
			// need to make a test to find out.


			// Invalid contents doesn't fail on the create but will on methods
			// like OuterXml.

//			processingInstruction = null;
//			processingInstruction = document.CreateProcessingInstruction("foo", "bar?>baz");
//			Assert(processingInstruction != null);
//			document.AppendChild(processingInstruction);
//			try {
//				outerXml = document.OuterXml;
//				Fail("Should have thrown an ArgumentException.");
//			} catch (ArgumentException) { }
			
			processingInstruction = null;
			processingInstruction = document.CreateProcessingInstruction("foo", "bar?>baz");
			Assert(processingInstruction != null);
			
			processingInstruction = null;
			processingInstruction = document.CreateProcessingInstruction("XML", "bar");
			Assert(processingInstruction != null);
			
			processingInstruction = null;
			processingInstruction = document.CreateProcessingInstruction("xml", "bar");
			Assert(processingInstruction != null);

			try {
				Fail("Should have thrown an Exception.");
			}
			catch (Exception e) {
				string billy = e.Message;
			}

		}


		public void TestLoadProcessingInstruction ()
		{
			document.LoadXml (@"<?foo bar='baaz' quux='quuux'?><quuuux></quuuux>");
			// Not sure where this goes in a doc yet...
		}

		public void TestLoadCDATA ()
		{
			document.LoadXml ("<foo><![CDATA[bar]]></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.CDATA);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadComment()
		{
			document.LoadXml ("<foo><!--Comment--></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Comment);
			AssertEquals ("Comment", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXmlSingleElement ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo/>");
			AssertNotNull (document.DocumentElement);

			AssertSame (document.FirstChild, document.DocumentElement);
			AssertSame (document.FirstChild, document.DocumentElement);
		}

		public void TestLoadXmlExceptionClearsDocument ()
		{
			document.LoadXml ("<foo/>");
			Assert (document.FirstChild != null);
			
			try {
				document.LoadXml ("<123/>");
				Fail ("An XmlException should have been thrown.");
			} catch (XmlException) {}

			Assert (document.FirstChild == null);
		}

		public void TestLoadXmlElementWithChildElement ()
		{
			document.LoadXml ("<foo><bar/></foo>");
			Assert (document.ChildNodes.Count == 1);
			Assert (document.FirstChild.ChildNodes.Count == 1);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals ("bar", document.DocumentElement.FirstChild.LocalName);
		}

		public void TestLoadXmlElementWithTextNode ()
		{
			document.LoadXml ("<foo>bar</foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Text);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
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
