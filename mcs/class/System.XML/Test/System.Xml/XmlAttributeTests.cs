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
	public class XmlAttributeTests
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
			Assert.IsNull (attr.Attributes);
		}

		[Test]
		public void AttributeInnerAndOuterXml ()
		{
			attr = doc.CreateAttribute ("foo", "bar", "http://abc.def");
			attr.Value = "baz";
			Assert.AreEqual ("baz", attr.InnerXml, "#1");
			Assert.AreEqual ("foo:bar=\"baz\"", attr.OuterXml, "#2");
		}

		[Test]
		public void AttributeWithNoValue ()
		{
			XmlAttribute attribute = doc.CreateAttribute ("name");
			Assert.AreEqual (String.Empty, attribute.Value, "#1");
			Assert.IsFalse (attribute.HasChildNodes, "#2");
			Assert.IsNull (attribute.FirstChild, "#3");
			Assert.IsNull (attribute.LastChild, "#4");
			Assert.AreEqual (0, attribute.ChildNodes.Count, "#5");
		}

		[Test]
		public void AttributeWithValue ()
		{
			XmlAttribute attribute = doc.CreateAttribute ("name");
			attribute.Value = "value";
			Assert.AreEqual ("value", attribute.Value, "#1");
			Assert.IsTrue (attribute.HasChildNodes, "#2");
			Assert.IsNotNull (attribute.FirstChild, "#3");
			Assert.IsNotNull (attribute.LastChild, "#4");
			Assert.AreEqual (1, attribute.ChildNodes.Count, "#5");
			Assert.AreEqual (XmlNodeType.Text, attribute.ChildNodes [0].NodeType, "#6");
			Assert.AreEqual ("value", attribute.ChildNodes [0].Value, "#7");
		}

		[Test]
		public void CheckPrefixWithNamespace ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:foo' foo='attfoo' foo:foo='attfoofoo' />");
			// hogehoge does not match to any namespace.
			Assert.AreEqual ("xmlns:foo", doc.DocumentElement.Attributes [0].Name);
			try {
				doc.DocumentElement.Attributes [0].Prefix = "hogehoge";
				doc.Save (TextWriter.Null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Cannot bind to the reserved namespace
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void NamespaceAttributes ()
		{
			try {
				doc.CreateAttribute (string.Empty, "xmlns", "urn:foo");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The namespace declaration attribute has an
				// incorrect namespaceURI: urn:foo
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			doc.LoadXml ("<root/>");

			try {
				doc.DocumentElement.SetAttribute ("xmlns", "urn:foo", "urn:bar");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The namespace declaration attribute has an
				// incorrect namespaceURI: urn:foo
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}
		}

		[Test]
		public void HasChildNodes ()
		{
			Assert.IsTrue (attr.HasChildNodes, "#1");
			XmlAttribute attr2 = doc.CreateAttribute ("attr2");
			Assert.IsFalse (attr2.HasChildNodes, "#2");
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("attr1", attr.Name);
		}

		[Test]
		public void NodeType ()
		{
			Assert.AreEqual (XmlNodeType.Attribute, attr.NodeType);
		}

		[Test]
		public void OwnerDocument ()
		{
			Assert.AreSame (doc, attr.OwnerDocument);
		}

		[Test]
		public void ParentNode ()
		{
			Assert.IsNull (attr.ParentNode, "Attr parents not allowed");
		}

		[Test]
		public void Value ()
		{
			Assert.AreEqual ("val1", attr.Value, "#1");
			XmlAttribute attr2 = doc.CreateAttribute ("attr2");
			Assert.AreEqual (string.Empty, attr2.Value, "#2");
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=336180
#endif
		public void SetInnerTextAndXml ()
		{
			string original = doc.OuterXml;
			doc.LoadXml ("<root name='value' />");
			XmlAttribute attr = doc.DocumentElement.Attributes ["name"];
			attr.InnerText = "a&b";
			Assert.AreEqual ("a&b", attr.Value, "setInnerText");
			attr.InnerXml = "a&amp;b";
			Assert.AreEqual ("a&b", attr.Value, "setInnerXml");
			attr.InnerXml = "'a&amp;b'";
			Assert.AreEqual ("'a&amp;b'", attr.InnerXml, "setInnerXml.InnerXml");
			Assert.AreEqual ("'a&b'", attr.Value, "setInnerXml.Value");
			attr.InnerXml = "\"a&amp;b\"";
			Assert.AreEqual ("\"a&amp;b\"", attr.InnerXml, "Double_Quote");
			attr.InnerXml = "\"a&amp;b'";
			Assert.AreEqual ("\"a&amp;b'", attr.InnerXml, "DoubleQuoteStart_SingleQuoteEnd");

			attr.Value = string.Empty;
			XmlNodeChangedEventHandler evInserted = new XmlNodeChangedEventHandler (EventNodeInserted);
			XmlNodeChangedEventHandler evChanged = new XmlNodeChangedEventHandler (EventNodeChanged);
			XmlNodeChangedEventHandler evRemoved = new XmlNodeChangedEventHandler (EventNodeRemoved);
			doc.NodeInserted += evInserted;
			doc.NodeChanged += evChanged;
			doc.NodeRemoved += evRemoved;
			try {
				// set_InnerText event
				attr.InnerText = "fire";
				Assert.IsFalse (inserted, "setInnerText.NodeInserted");
				Assert.IsTrue (changed, "setInnerText.NodeChanged");
				Assert.IsFalse (removed, "setInnerText.NodeRemoved");
				inserted = changed = removed = false;
				// set_InnerXml event
				attr.InnerXml = "fire";
				Assert.IsTrue (inserted, "setInnserXml.NodeInserted");
				Assert.IsFalse (changed, "setInnserXml.NodeChanged");
				Assert.IsTrue (removed, "setInnserXml.NodeRemoved");
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
			doc.DocumentElement.SetAttribute ("attr", string.Empty);
			doc.DocumentElement.Attributes ["attr"].InnerXml = "&ent;";
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.WriteStartElement ("result");
			XmlAttribute attr = doc.DocumentElement.Attributes ["attr"];
			attr.WriteTo (xtw);
			xtw.Close ();
			Assert.AreEqual ("<result attr=\"&ent;\" />", sw.ToString ());
		}

		[Test]
		public void IdentityConstraints ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root (c)+><!ELEMENT c EMPTY><!ATTLIST c foo ID #IMPLIED bar CDATA #IMPLIED>]>";
			string xml = dtd + "<root><c foo='id1' bar='1' /><c foo='id2' bar='2'/></root>";
			XmlValidatingReader vr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			doc.Load (vr);
			Assert.IsNotNull (doc.GetElementById ("id1"), "#1");
			Assert.IsNotNull (doc.GetElementById ("id2"), "#2");
			// MS.NET BUG: Later I try to append it to another element, but
			// it should raise InvalidOperationException.
			// (and if MS.NET conform to DOM 1.0, it should be XmlException.)
//			XmlAttribute attr = doc.DocumentElement.FirstChild.Attributes [0];
			XmlAttribute attr = doc.DocumentElement.FirstChild.Attributes.RemoveAt (0);
			Assert.AreEqual ("id1", attr.Value, "#3");

			doc.DocumentElement.LastChild.Attributes.SetNamedItem (attr);
			Assert.IsNotNull (doc.GetElementById ("id1"), "#4");
			XmlElement elem2 = doc.GetElementById ("id2");
			// MS.NET BUG: it doesn't remove replaced attribute with SetNamedItem!
//			AssertNull (elem2, "#5");
//			AssertEquals ("2", elem2.GetAttribute ("bar"), "#6");
//			elem2.RemoveAttribute ("foo");
//			AssertEquals (string.Empty, elem2.GetAttribute ("foo"), "#7");

			// MS.NET BUG: elem should be the element which has the attribute bar='1'!
			XmlElement elem = doc.GetElementById ("id1");
//			AssertEquals ("2", elem.GetAttribute ("bar"), "#8");

			// Here, required attribute foo is no more required,
			XmlElement elemNew = doc.CreateElement ("c");
			doc.DocumentElement.AppendChild (elemNew);
			// but once attribute is set, document recognizes this ID.
			elemNew.SetAttribute ("foo", "id3");
			Assert.IsNotNull (doc.GetElementById ("id3"), "#9");
			elemNew.RemoveAttribute ("foo");
			Assert.IsNull (doc.GetElementById ("id3"), "#10");

			// MS.NET BUG: multiple IDs are allowed.
			// In such case GetElementById fails.
			elemNew.SetAttribute ("foo", "id2");

			// While XmlValidatingReader validated ID cannot be removed.
			// It is too curious for me.
			elem.RemoveAttribute ("foo");

			// Finally...
			doc.RemoveAll ();
			Assert.IsNull (doc.GetElementById ("id1"), "#11");
			Assert.IsNull (doc.GetElementById ("id2"), "#12");
			Assert.IsNull (doc.GetElementById ("id3"), "#13");
		}

		int removeAllStep;
		[Test]
		public void DefaultAttributeRemoval ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<!DOCTYPE root [<!ELEMENT root (#PCDATA)><!ATTLIST root foo CDATA 'foo-def'>]><root></root>");
			doc.NodeInserted += new XmlNodeChangedEventHandler (OnInsert);
			doc.NodeChanged += new XmlNodeChangedEventHandler (OnChange);
			doc.NodeRemoved += new XmlNodeChangedEventHandler (OnRemove);
			doc.DocumentElement.RemoveAll ();
		}
		
		private void OnInsert (object o, XmlNodeChangedEventArgs e)
		{
			if (removeAllStep == 1)
				Assert.AreEqual (XmlNodeType.Text, e.Node.NodeType);
			else if (removeAllStep == 2) {
				Assert.AreEqual ("foo", e.Node.Name);
				Assert.IsFalse (((XmlAttribute) e.Node).Specified);
			} else
				Assert.Fail ();
			removeAllStep++;
		}

		private void OnChange (object o, XmlNodeChangedEventArgs e)
		{
			Assert.Fail ("Should not be called.");
		}

		private void OnRemove (object o, XmlNodeChangedEventArgs e)
		{
			Assert.AreEqual (0, removeAllStep, "#1");
			Assert.AreEqual ("foo", e.Node.Name, "#2");
			removeAllStep++;
		}

		[Test]
		public void EmptyStringHasTextNode ()
		{
			doc.LoadXml ("<root attr=''/>");
			XmlAttribute attr = doc.DocumentElement.GetAttributeNode ("attr");
			Assert.IsNotNull (attr, "#1");
			Assert.AreEqual (1, attr.ChildNodes.Count, "#2");
			Assert.AreEqual (XmlNodeType.Text, attr.ChildNodes [0].NodeType, "#3");
			Assert.AreEqual (String.Empty, attr.ChildNodes [0].Value, "#4");
		}

		[Test]
		public void CrazyPrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("foo"));
			doc.DocumentElement.SetAttribute ("a", "urn:a", "attr");
			XmlAttribute a = doc.DocumentElement.Attributes [0];
			a.Prefix ="hoge:hoge:hoge";
			// This test is nothing more than ****.
			Assert.AreEqual ("hoge:hoge:hoge", a.Prefix);
			// The resulting string is not XML (so broken), so 
			// it should not be tested.
			// doc.Save (TextWriter.Null);
		}

		[Test]
		public void SetValueAndEntityRefChild ()
		{
			string dtd = @"<!DOCTYPE root [
				<!ELEMENT root EMPTY>
				<!ATTLIST root foo CDATA #IMPLIED>
				<!ENTITY ent 'entityyyy'>
				]>";
			string xml = dtd + "<root foo='&ent;' />";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			doc.DocumentElement.Attributes [0].Value = "replaced";
		}

		[Test] // bug #76311
		public void UpdateIDAttrValueAfterAppend ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<!DOCTYPE USS[<!ELEMENT USS EMPTY><!ATTLIST USS Id ID #REQUIRED>]><USS Id='foo'/>");
			Assert.IsNotNull (doc.SelectSingleNode ("id ('foo')"), "#1");
			doc.DocumentElement.Attributes [0].Value = "bar";
			Assert.IsNull (doc.SelectSingleNode ("id ('foo')"), "#2");
			Assert.IsNotNull (doc.SelectSingleNode ("id ('bar')"), "#3");
			doc.DocumentElement.Attributes [0].ChildNodes [0].Value = "baz";
			// Tests below don't work fine under MS.NET
//			Assert.IsNull (doc.SelectSingleNode ("id ('bar')"), "#4");
//			Assert.IsNotNull (doc.SelectSingleNode ("id ('baz')"), "#5");
			doc.DocumentElement.Attributes [0].AppendChild (doc.CreateTextNode ("baz"));
			Assert.IsNull (doc.SelectSingleNode ("id ('baz')"), "#6");
//			Assert.IsNull (doc.SelectSingleNode ("id ('bar')"), "#7");
//			Assert.IsNotNull (doc.SelectSingleNode ("id ('bazbaz')"), "#7");
		}

		[Test] // http://lists.ximian.com/pipermail/mono-list/2006-May/031557.html
		public void NonEmptyPrefixWithEmptyNS ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.AppendChild (xmlDoc.CreateNode (XmlNodeType.XmlDeclaration,
				string.Empty, string.Empty));

			XmlElement docElement = xmlDoc.CreateElement ("doc");
			docElement.SetAttribute ("xmlns", "http://whatever.org/XMLSchema/foo");
			docElement.SetAttribute ("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
			docElement.SetAttribute ("xsi:schemaLocation", "http://whatever.org/XMLSchema/foo.xsd");
			xmlDoc.AppendChild (docElement);

			XmlElement fooElement = xmlDoc.CreateElement ("foo");
			docElement.AppendChild (fooElement);
			xmlDoc.Save (TextWriter.Null);
		}

		[Test]
		public void NullPrefix ()
		{
			new MyXmlAttribute ("foo", "urn:foo", new XmlDocument ());
		}

		class MyXmlAttribute : XmlAttribute
		{
			public MyXmlAttribute (string localName, string ns, XmlDocument doc)
				: base (null, localName, ns, doc)
			{
			}
		}
	}
}
