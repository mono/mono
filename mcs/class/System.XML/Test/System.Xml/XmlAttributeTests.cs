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
		bool inserted;
		bool changed;
		bool removed;

		[SetUp]
		public void GetReady()
		{
			doc = new XmlDocument ();
			attr = doc.CreateAttribute ("attr1");
			attr.Value = "val1";
		}

		private void EventNodeInserted(Object sender, XmlNodeChangedEventArgs e)
		{
			inserted = true;
		}

		private void EventNodeChanged(Object sender, XmlNodeChangedEventArgs e)
		{
			changed = true;
		}

		private void EventNodeRemoved(Object sender, XmlNodeChangedEventArgs e)
		{
			removed = true;
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
			XmlAttribute attr = doc.DocumentElement.Attributes ["name"];
			attr.InnerText = "a&b";
			AssertEquals ("setInnerText", "a&b", attr.Value);
			attr.InnerXml = "a&amp;b";
			AssertEquals ("setInnerXml", "a&b", attr.Value);
			attr.InnerXml = "'a&amp;b'";
			AssertEquals ("setInnerXml.InnerXml", "'a&amp;b'", attr.InnerXml);
			AssertEquals ("setInnerXml.Value", "'a&b'", attr.Value);
			attr.InnerXml = "\"a&amp;b\"";
			AssertEquals ("\"a&amp;b\"", attr.InnerXml);
			attr.InnerXml = "\"a&amp;b'";
			AssertEquals ("\"a&amp;b'", attr.InnerXml);

			attr.Value = "";
			XmlNodeChangedEventHandler evInserted = new XmlNodeChangedEventHandler (EventNodeInserted);
			XmlNodeChangedEventHandler evChanged = new XmlNodeChangedEventHandler (EventNodeChanged);
			XmlNodeChangedEventHandler evRemoved = new XmlNodeChangedEventHandler (EventNodeRemoved);
			doc.NodeInserted += evInserted;
			doc.NodeChanged += evChanged;
			doc.NodeRemoved += evRemoved;
			try {
				// set_InnerText event
				attr.InnerText = "fire";
				AssertEquals ("setInnerText.NodeInserted", false, inserted);
				AssertEquals ("setInnerText.NodeChanged", true, changed);
				AssertEquals ("setInnerText.NodeRemoved", false, removed);
				inserted = changed = removed = false;
				// set_InnerXml event
				attr.InnerXml = "fire";
				AssertEquals ("setInnserXml.NodeInserted", true, inserted);
				AssertEquals ("setInnserXml.NodeChanged", false, changed);
				AssertEquals ("setInnserXml.NodeRemoved", true, removed);
				inserted = changed = removed = false;
			} finally {
				doc.NodeInserted -= evInserted;
				doc.NodeChanged -= evChanged;
				doc.NodeRemoved -= evRemoved;
			}
		}



		private void OnSetInnerText (object o, XmlNodeChangedEventArgs e)
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
