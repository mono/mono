// XmlAttributeTests.cs : Tests for the XmlAttribute class
//
// Author: Mike Kestner <mkestner@speakeasy.net>
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Mike Kestner
// (C) 2003 Martin Willemoes Hansen

using System;
using System.IO;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlAttributeTests : Assertion
	{
		XmlDocument doc;
		XmlAttribute attr;

		[SetUp]
		public void GetReady()
		{
			doc = new XmlDocument ();
			attr = doc.CreateAttribute ("attr1");
			attr.Value = "val1";
		}

		[Test]
		public void Attributes ()
		{
			AssertNull (attr.Attributes);
		}

		[Test]
		public void AttributeInnerAndOuterXml ()
		{
			attr = doc.CreateAttribute ("foo", "bar", "http://abc.def");
			attr.Value = "baz";
			AssertEquals ("baz", attr.InnerXml);
			AssertEquals ("foo:bar=\"baz\"", attr.OuterXml);
		}

		[Test]
		public void AttributeWithNoValue ()
		{
			XmlAttribute attribute = doc.CreateAttribute ("name");
			AssertEquals (String.Empty, attribute.Value);
			Assert (!attribute.HasChildNodes);
			AssertNull (attribute.FirstChild);
			AssertNull (attribute.LastChild);
			AssertEquals (0, attribute.ChildNodes.Count);
		}

		[Test]
		public void AttributeWithValue ()
		{
			XmlAttribute attribute = doc.CreateAttribute ("name");
			attribute.Value = "value";
			AssertEquals ("value", attribute.Value);
			Assert (attribute.HasChildNodes);
			AssertNotNull (attribute.FirstChild);
			AssertNotNull (attribute.LastChild);
			AssertEquals (1, attribute.ChildNodes.Count);
			AssertEquals (XmlNodeType.Text, attribute.ChildNodes [0].NodeType);
			AssertEquals ("value", attribute.ChildNodes [0].Value);
		}

		[Test]
		public void NamespaceAttributes ()
		{
			try {
				doc.CreateAttribute ("", "xmlns", "urn:foo");
				Assertion.Fail ("Creating xmlns attribute with invalid nsuri should be error.");
			} catch (Exception) {
			}
			doc.LoadXml ("<root/>");
			try {
				doc.DocumentElement.SetAttribute ("xmlns", "urn:foo", "urn:bar");
				Assertion.Fail ("SetAttribute for xmlns with invalid nsuri should be error.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void HasChildNodes ()
		{
			Assert (attr.HasChildNodes);
		}

		[Test]
		public void Name ()
		{
			AssertEquals ("attr1", attr.Name);
		}

		[Test]
		public void NodeType ()
		{
			AssertEquals (XmlNodeType.Attribute, attr.NodeType);
		}

		[Test]
		public void OwnerDocument ()
		{
			AssertSame (doc, attr.OwnerDocument);
		}

		[Test]
		public void ParentNode ()
		{
			AssertNull ("Attr parents not allowed", attr.ParentNode);
		}

		[Test]
		public void Value ()
		{
			AssertEquals ("val1", attr.Value);
		}

		[Test]
		public void SetInnerTextAndXml ()
		{
			string original = doc.OuterXml;
			doc.LoadXml ("<root name='value' />");
			XmlNodeChangedEventHandler eh = new XmlNodeChangedEventHandler (OnSetInnerText);
			try {
				doc.DocumentElement.Attributes ["name"].InnerText = "a&b";
				AssertEquals ("setInnerText", "a&b", doc.DocumentElement.Attributes ["name"].Value);
				doc.DocumentElement.Attributes ["name"].InnerXml = "a&amp;b";
				AssertEquals ("setInnerXml", "a&b", doc.DocumentElement.Attributes ["name"].Value);

				doc.NodeChanged += eh;
				doc.DocumentElement.Attributes ["name"].InnerText = "fire";
				// If you failed to pass it, then the reason may be loop of event.
				AssertEquals ("setInnerText.Event", "event was fired", doc.DocumentElement.GetAttribute ("appended"));
			} catch(Exception ex) {
				Fail(ex.Message);
			} finally {
				doc.NodeChanged -= eh;
			}
		}

		public void OnSetInnerText (object o, XmlNodeChangedEventArgs e)
		{
			if(e.NewParent.Value == "fire")
				doc.DocumentElement.SetAttribute ("appended", "event was fired");
		}

		[Test]
		public void WriteTo ()
		{
			doc.AppendChild (doc.CreateElement ("root"));
			doc.DocumentElement.SetAttribute ("attr","");
			doc.DocumentElement.Attributes ["attr"].InnerXml = "&ent;";
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.WriteStartElement ("result");
			XmlAttribute attr = doc.DocumentElement.Attributes ["attr"];
			attr.WriteTo (xtw);
			xtw.Close ();
			Assertion.AssertEquals ("<result attr=\"&ent;\" />", sw.ToString ());
		}
	}
}
