// XmlAttributeTests.cs : Tests for the XmlAttribute class
//
// Author: Mike Kestner <mkestner@speakeasy.net>
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Mike Kestner
// (C) 2003 Martin Willemoes Hansen

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlAttributeTests
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
			Assertion.AssertNull (attr.Attributes);
		}

		[Test]
		public void AttributeInnerAndOuterXml ()
		{
			attr = doc.CreateAttribute ("foo", "bar", "http://abc.def");
			attr.Value = "baz";
			Assertion.AssertEquals ("baz", attr.InnerXml);
			Assertion.AssertEquals ("foo:bar=\"baz\"", attr.OuterXml);
		}

		[Test]
		public void AttributeWithNoValue ()
		{
			XmlAttribute attribute = doc.CreateAttribute ("name");
			Assertion.AssertEquals (String.Empty, attribute.Value);
			Assertion.Assert (!attribute.HasChildNodes);
			Assertion.AssertNull (attribute.FirstChild);
			Assertion.AssertNull (attribute.LastChild);
			Assertion.AssertEquals (0, attribute.ChildNodes.Count);
		}

		[Test]
		public void AttributeWithValue ()
		{
			XmlAttribute attribute = doc.CreateAttribute ("name");
			attribute.Value = "value";
			Assertion.AssertEquals ("value", attribute.Value);
			Assertion.Assert (attribute.HasChildNodes);
			Assertion.AssertNotNull (attribute.FirstChild);
			Assertion.AssertNotNull (attribute.LastChild);
			Assertion.AssertEquals (1, attribute.ChildNodes.Count);
			Assertion.AssertEquals (XmlNodeType.Text, attribute.ChildNodes [0].NodeType);
			Assertion.AssertEquals ("value", attribute.ChildNodes [0].Value);
		}

		[Test]
		public void HasChildNodes ()
		{
			Assertion.Assert (attr.HasChildNodes);
		}

		[Test]
		public void Name ()
		{
			Assertion.AssertEquals ("attr1", attr.Name);
		}

		[Test]
		public void NodeType ()
		{
			Assertion.AssertEquals (XmlNodeType.Attribute, attr.NodeType);
		}

		[Test]
		public void OwnerDocument ()
		{
			Assertion.AssertSame (doc, attr.OwnerDocument);
		}

		[Test]
		public void ParentNode ()
		{
			Assertion.AssertNull ("Attr parents not allowed", attr.ParentNode);
		}

		[Test]
		public void Value ()
		{
			Assertion.AssertEquals ("val1", attr.Value);
		}

		[Test]
		public void SetInnerTextAndXml ()
		{
			string original = doc.OuterXml;
			doc.LoadXml ("<root name='value' />");
			XmlNodeChangedEventHandler eh = new XmlNodeChangedEventHandler (OnSetInnerText);
			try {
				doc.DocumentElement.Attributes ["name"].InnerText = "a&b";
				Assertion.AssertEquals ("setInnerText", "a&b", doc.DocumentElement.Attributes ["name"].Value);
				doc.DocumentElement.Attributes ["name"].InnerXml = "a&amp;b";
				Assertion.AssertEquals ("setInnerXml", "a&b", doc.DocumentElement.Attributes ["name"].Value);

				doc.NodeChanged += eh;
				doc.DocumentElement.Attributes ["name"].InnerText = "fire";
				// If you failed to pass it, then the reason may be loop of event.
				Assertion.AssertEquals ("setInnerText.Event", "event was fired", doc.DocumentElement.GetAttribute ("appended"));
			} catch(Exception ex) {
				Assertion.Fail(ex.Message);
			} finally {
				doc.LoadXml (original);
				doc.NodeChanged -= eh;
			}
		}

		public void OnSetInnerText (object o, XmlNodeChangedEventArgs e)
		{
			if(e.NewParent.Value == "fire")
				doc.DocumentElement.SetAttribute ("appended", "event was fired");
		}
	}
}
