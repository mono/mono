//
// System.Xml.XmlNodeTests
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlNodeTests
	{
		XmlDocument document;
		XmlElement element;
		XmlElement element2;
		bool inserted;
		bool inserting;
		bool changed;
		bool changing;
		bool removed;
		bool removing;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.NodeInserted += new XmlNodeChangedEventHandler (this.EventNodeInserted);
			document.NodeInserting += new XmlNodeChangedEventHandler (this.EventNodeInserting);
			document.NodeRemoved += new XmlNodeChangedEventHandler (this.EventNodeRemoved);
			document.NodeRemoving += new XmlNodeChangedEventHandler (this.EventNodeRemoving);
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
		}

		private void EventNodeInserted(Object sender, XmlNodeChangedEventArgs e)
		{
			inserted = true;
		}

		private void EventNodeInserting (Object sender, XmlNodeChangedEventArgs e)
		{
			inserting = true;
		}

		private void EventNodeChanged(Object sender, XmlNodeChangedEventArgs e)
		{
			changed = true;
		}

		private void EventNodeChanging (Object sender, XmlNodeChangedEventArgs e)
		{
			changing = true;
		}

		private void EventNodeRemoved(Object sender, XmlNodeChangedEventArgs e)
		{
			removed = true;
		}

		private void EventNodeRemoving (Object sender, XmlNodeChangedEventArgs e)
		{
			removing = true;
		}

		[Test]
		public void AppendChild ()
		{
			XmlComment comment;

			inserted = false;
			inserting = false;
			element.AppendChild (element2);
			Assert.IsTrue (inserted);
			Assert.IsTrue (inserting);

			// Can only append to elements, documents, and attributes
			try 
			{
				comment = document.CreateComment ("baz");
				comment.AppendChild (element2);
				Assert.Fail ("Expected an InvalidOperationException to be thrown.");
			} 
			catch (InvalidOperationException) {}

			// Can't append a node from one document into another document.
			XmlDocument document2 = new XmlDocument();
			Assert.AreEqual (1, element.ChildNodes.Count);
			try 
			{
				element2 = document2.CreateElement ("qux");
				element.AppendChild (element2);
				Assert.Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
			Assert.AreEqual (1, element.ChildNodes.Count);

			// Can't append to a readonly node.
/* TODO put this in when I figure out how to create a read-only node.
			try 
			{
				XmlElement element3 = (XmlElement)element.CloneNode (false);
				Assert.IsTrue (!element.IsReadOnly);
				Assert.IsTrue (element3.IsReadOnly);
				element2 = document.CreateElement ("quux");
				element3.AppendChild (element2);
				Assert.Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
*/
		}

		[Test]
		public void GetNamespaceOfPrefix ()
		{
			document.LoadXml ("<root xmlns='urn:default' attr='value' "
				+ "xml:lang='en' xmlns:foo='urn:foo' foo:att='fooatt'>text node</root>");
			XmlNode n = document.DocumentElement;
			Assert.AreEqual ("urn:default", n.GetNamespaceOfPrefix (String.Empty), "#1");
			Assert.AreEqual ("urn:foo", n.GetNamespaceOfPrefix ("foo"), "#2");
			Assert.AreEqual (String.Empty, n.GetNamespaceOfPrefix ("bar"), "#3");
			Assert.AreEqual ("http://www.w3.org/XML/1998/namespace", n.GetNamespaceOfPrefix ("xml"), "#4");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", n.GetNamespaceOfPrefix ("xmlns"), "#5");

			n = document.DocumentElement.FirstChild;
			Assert.AreEqual ("urn:default", n.GetNamespaceOfPrefix (String.Empty), "#6");
			Assert.AreEqual ("urn:foo", n.GetNamespaceOfPrefix ("foo"), "#7");
			Assert.AreEqual (String.Empty, n.GetNamespaceOfPrefix ("bar"), "#8");
			Assert.AreEqual ("http://www.w3.org/XML/1998/namespace", n.GetNamespaceOfPrefix ("xml"), "#9");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", n.GetNamespaceOfPrefix ("xmlns"), "#10");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetNamespaceOfPrefixNullArg ()
		{
			new XmlDocument ().GetNamespaceOfPrefix (null);
		}

		[Test]
		public void InsertBefore()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub /></root>");
			XmlElement docelem = document.DocumentElement;
			docelem.InsertBefore(document.CreateElement("good_child"), docelem.FirstChild);
			Assert.AreEqual ("good_child", docelem.FirstChild.Name, "InsertBefore.Normal");
			// These are required for .NET 1.0 but not for .NET 1.1.
			try {
				document.InsertBefore (document.CreateElement ("BAD_MAN"), docelem);
				Assert.Fail ("#InsertBefore.BadPositionButNoError.1");
			}
			catch (Exception) {}
		}

		[Test]
		public void InsertAfter()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub1 /><sub2 /></root>");
			XmlElement docelem = document.DocumentElement;
			XmlElement newelem = document.CreateElement("good_child");
			docelem.InsertAfter(newelem, docelem.FirstChild);
			Assert.AreEqual (3, docelem.ChildNodes.Count, "InsertAfter.Normal");
			Assert.AreEqual ("sub1", docelem.FirstChild.Name, "InsertAfter.First");
			Assert.AreEqual ("sub2", docelem.LastChild.Name, "InsertAfter.Last");
			Assert.AreEqual ("good_child", docelem.FirstChild.NextSibling.Name, "InsertAfter.Prev");
			Assert.AreEqual ("good_child", docelem.LastChild.PreviousSibling.Name, "InsertAfter.Next");
			// this doesn't throw any exception *only on .NET 1.1*
			// .NET 1.0 throws an exception.
			try {
				document.InsertAfter(document.CreateElement("BAD_MAN"), docelem);
#if USE_VERSION_1_1
				Assert.AreEqual ("<root><sub1 /><good_child /><sub2 /></root><BAD_MAN />", document.InnerXml, "InsertAfter with bad location");
			} catch (XmlException ex) {
				throw ex;
			}
#else
			} catch (Exception) {}
#endif
}

		[Test]
		public void Normalize ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root>This is the <b>hardest</b> one.</root>");
			doc.NodeInserted += new XmlNodeChangedEventHandler (EventNodeInserted);
			doc.NodeChanged += new XmlNodeChangedEventHandler (EventNodeChanged);
			doc.NodeRemoved += new XmlNodeChangedEventHandler (EventNodeRemoved);

			Assert.AreEqual (3, doc.DocumentElement.ChildNodes.Count);

			doc.DocumentElement.Normalize ();
			Assert.AreEqual (3, doc.DocumentElement.ChildNodes.Count);
			Assert.IsTrue (changed);
			inserted = changed = removed = false;

			doc.DocumentElement.AppendChild (doc.CreateTextNode ("Addendum."));
			Assert.AreEqual (4, doc.DocumentElement.ChildNodes.Count);
			inserted = changed = removed = false;

			doc.DocumentElement.Normalize ();
			Assert.AreEqual (3, doc.DocumentElement.ChildNodes.Count);
			Assert.IsTrue (changed);
			Assert.IsTrue (removed);
			inserted = changed = removed = false;

			doc.DocumentElement.SetAttribute ("attr", "");
			XmlAttribute attr = doc.DocumentElement.Attributes [0] as XmlAttribute;
			Assert.AreEqual (1, attr.ChildNodes.Count);
			inserted = changed = removed = false;
			attr.Normalize ();
			// Such behavior violates DOM Level 2 Node#normalize(),
			// but MS DOM is designed as such.
			Assert.AreEqual (1, attr.ChildNodes.Count);
		}

		[Test]
		public void Normalize2 ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.LoadXml ("<root>  </root>");
			XmlElement root = doc.DocumentElement;
			root.AppendChild (doc.CreateTextNode ("foo"));
			root.AppendChild (doc.CreateTextNode ("bar"));
			root.AppendChild (doc.CreateWhitespace ("   "));
			root.AppendChild (doc.CreateTextNode ("baz"));
			doc.NodeInserted += new XmlNodeChangedEventHandler (OnChange);
			doc.NodeChanged += new XmlNodeChangedEventHandler (OnChange);
			doc.NodeRemoved += new XmlNodeChangedEventHandler (OnChange);
			Assert.AreEqual (5, root.ChildNodes.Count, "Before Normalize()");
			root.Normalize ();
			Assert.AreEqual ("<root>  foobar   baz</root>", root.OuterXml);
			Assert.AreEqual (1, root.ChildNodes.Count, "After Normalize()");
		}

		int normalize2Count;

		private void OnChange (object o, XmlNodeChangedEventArgs e)
		{
			switch (normalize2Count) {
			case 0:
				Assert.AreEqual (XmlNodeChangedAction.Remove, e.Action, "Action0");
				Assert.AreEqual ("  ", e.Node.Value, "Value0");
				break;
			case 1:
				Assert.AreEqual (XmlNodeChangedAction.Remove, e.Action, "Action1");
				Assert.AreEqual ("bar", e.Node.Value, "Value1");
				break;
			case 2:
				Assert.AreEqual (XmlNodeChangedAction.Remove, e.Action, "Action2");
				Assert.AreEqual ("   ", e.Node.Value, "Value2");
				break;
			case 3:
				Assert.AreEqual (XmlNodeChangedAction.Remove, e.Action, "Action3");
				Assert.AreEqual ("baz", e.Node.Value, "Value3");
				break;
			case 4:
				Assert.AreEqual (XmlNodeChangedAction.Change, e.Action, "Action4");
				Assert.AreEqual ("  foobar   baz", e.Node.Value, "Value4");
				break;
			default:
				Assert.Fail (String.Format ("Unexpected event. Action = {0}, node type = {1}, node name = {2}, node value = {3}", e.Action, e.Node.NodeType, e.Node.Name, e.Node.Value));
				break;
			}
			normalize2Count++;
		}

		[Test]
		public void PrependChild()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub1 /><sub2 /></root>");
			XmlElement docelem = document.DocumentElement;
			docelem.PrependChild(document.CreateElement("prepender"));
			Assert.AreEqual ("prepender", docelem.FirstChild.Name, "PrependChild");
		}

		public void saveTestRemoveAll ()
		{
			// TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveAll ();
			Assert.IsTrue (removed);
			Assert.IsTrue (removing);
		}

		[Test]
		public void RemoveChild ()
		{
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveChild (element2);
			Assert.IsTrue (removed);
			Assert.IsTrue (removing);
		}
		
		[Test]
		public void RemoveLastChild ()
		{
			element.InnerXml = "<foo/><bar/><baz/>";
			element.RemoveChild (element.LastChild);
			Assert.IsNotNull (element.FirstChild);
		}
		
		[Test]
		public void GetPrefixOfNamespace ()
		{
			document.LoadXml ("<root><c1 xmlns='urn:foo'><c2 xmlns:foo='urn:foo' xmlns='urn:bar'><c3 xmlns=''/></c2></c1></root>");
			Assert.AreEqual (String.Empty, document.DocumentElement.GetPrefixOfNamespace ("urn:foo"), "root");
			Assert.AreEqual (String.Empty, document.DocumentElement.GetPrefixOfNamespace ("urn:foo"), "c1");
			Assert.AreEqual (String.Empty, document.DocumentElement.FirstChild.GetPrefixOfNamespace ("urn:foo"), "c2");
			Assert.AreEqual ("foo", document.DocumentElement.FirstChild.FirstChild.GetPrefixOfNamespace ("urn:foo"), "c3");

			// disconnected nodes.
			XmlNode n = document.CreateElement ("foo");
			Assert.AreEqual (String.Empty, n.GetPrefixOfNamespace ("foo"));
			n = document.CreateTextNode ("text"); // does not have Attributes
			Assert.AreEqual (String.Empty, n.GetPrefixOfNamespace ("foo"));
			n = document.CreateXmlDeclaration ("1.0", null, null); // does not have Attributes
			Assert.AreEqual (String.Empty, n.GetPrefixOfNamespace ("foo"));
		}

		[Test]
		public void GetPrefixOfNamespace2 ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("foo"));
			doc.DocumentElement.SetAttributeNode (
			doc.CreateAttribute ("xmlns", "u", "http://www.w3.org/2000/xmlns/"));
			doc.DocumentElement.Attributes [0].Value = "urn:foo";
			XmlElement el = doc.CreateElement ("bar");
			doc.DocumentElement.AppendChild (el);
			Assert.AreEqual ("u", el.GetPrefixOfNamespace ("urn:foo"));
		}

		[Test]
		public void ReplaceChild ()
		{
			document.LoadXml ("<root/>");
			document.NodeInserted += new XmlNodeChangedEventHandler (this.EventNodeInserted);
			document.NodeChanged += new XmlNodeChangedEventHandler (this.EventNodeChanged);
			document.NodeRemoved += new XmlNodeChangedEventHandler (this.EventNodeRemoved);
			inserted = changed = removed = false;
			XmlElement el = document.CreateElement("root2");
			document.ReplaceChild (el, document.DocumentElement);
			Assert.AreEqual ("root2", document.DocumentElement.Name);
			Assert.AreEqual (1, document.ChildNodes.Count);
			Assert.IsTrue (inserted && removed && !changed);
		}

		[Test]
		public void InnerText ()
		{
			document.LoadXml ("<root>This is <b>mixed</b> content. Also includes <![CDATA[CDATA section]]>.<!-- Should be ignored --></root>");
			string total = "This is mixed content. Also includes CDATA section.";
			XmlNode elemB = document.DocumentElement.ChildNodes [1];
			Assert.AreEqual ("mixed", elemB.FirstChild.InnerText);	// text node
			Assert.AreEqual ("mixed", elemB.InnerText);	// element b
			Assert.AreEqual (total, document.DocumentElement.InnerText);	// element root
			Assert.AreEqual (total, document.InnerText);	// whole document
		}

		[Test]
		public void InnerXmlWithXmlns ()
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement ("KeyValue", "http://www.w3.org/2000/09/xmldsig#");
			xel.SetAttribute ("xmlns", "http://www.w3.org/2000/09/xmldsig#");
			xel.InnerXml = "<DSAKeyValue>blablabla</DSAKeyValue>";
			string expected = "<KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><DSAKeyValue>blablabla</DSAKeyValue></KeyValue>";
			Assert.AreEqual (expected, xel.OuterXml);
		}

		[Test]
		public void SelectNodes ()
		{
			// This test is done in this class since it tests only XmlDocumentNavigator.
			string xpath = "//@*|//namespace::*";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			XmlNodeList nl = doc.SelectNodes (xpath);
			Assert.AreEqual (6, nl.Count);
			// BTW, as for namespace nodes, Node does not exist
			// in the tree, so the return value should be
			// implementation dependent.
			Assert.AreEqual (XmlNodeType.Attribute, nl [0].NodeType, "#1");
			Assert.AreEqual (XmlNodeType.Attribute, nl [1].NodeType, "#2");
			Assert.AreEqual (XmlNodeType.Attribute, nl [2].NodeType, "#3");
			Assert.AreEqual (XmlNodeType.Attribute, nl [3].NodeType, "#4");
			Assert.AreEqual (XmlNodeType.Attribute, nl [4].NodeType, "#5");
			Assert.AreEqual (XmlNodeType.Attribute, nl [5].NodeType, "#6");
			Assert.AreEqual ("xmlns", nl [0].LocalName);
			Assert.AreEqual ("xml", nl [1].LocalName);
			Assert.AreEqual ("xmlns", nl [2].LocalName);
			Assert.AreEqual ("xml", nl [3].LocalName);
			Assert.AreEqual ("xmlns", nl [4].LocalName);
			Assert.AreEqual ("xml", nl [5].LocalName);
		}

		[Test]
		[Ignore ("MS.NET has a bug; it does not return nodes in document order.")]
		public void SelectNodes2 ()
		{
			// This test is done in this class since it tests only XmlDocumentNavigator.
			string xpath = "//*|//@*|//namespace::*";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			XmlNodeList nl = doc.SelectNodes (xpath);
			Assert.AreEqual (9, nl.Count);
			Assert.AreEqual (XmlNodeType.Element, nl [0].NodeType);
			Assert.AreEqual (XmlNodeType.Attribute, nl [1].NodeType);
			Assert.AreEqual (XmlNodeType.Attribute, nl [2].NodeType);
			Assert.AreEqual (XmlNodeType.Element, nl [3].NodeType);
			Assert.AreEqual (XmlNodeType.Attribute, nl [4].NodeType);
			Assert.AreEqual (XmlNodeType.Attribute, nl [5].NodeType);
			Assert.AreEqual (XmlNodeType.Element, nl [6].NodeType);
			Assert.AreEqual (XmlNodeType.Attribute, nl [7].NodeType);
			Assert.AreEqual (XmlNodeType.Attribute, nl [8].NodeType);
			Assert.AreEqual ("element", nl [0].LocalName);
			Assert.AreEqual ("xmlns", nl [1].LocalName);
			Assert.AreEqual ("xml", nl [2].LocalName);
			Assert.AreEqual ("foo", nl [3].LocalName);
			Assert.AreEqual ("xmlns", nl [4].LocalName);
			Assert.AreEqual ("xml", nl [5].LocalName);
			Assert.AreEqual ("bar", nl [6].LocalName);
			Assert.AreEqual ("xmlns", nl [7].LocalName);
			Assert.AreEqual ("xml", nl [8].LocalName);
		}

		[Test]
		public void BaseURI ()
		{
			// See bug #64120.
			XmlDocument doc = new XmlDocument ();
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/simple.xml"));
			XmlElement el = doc.CreateElement ("foo");
			Assert.AreEqual (String.Empty, el.BaseURI);
			doc.DocumentElement.AppendChild (el);
			Assert.IsTrue (String.Empty != el.BaseURI);
			XmlAttribute attr = doc.CreateAttribute ("attr");
			Assert.AreEqual (String.Empty, attr.BaseURI);
		}

		[Test]
		public void CloneReadonlyNode ()
		{
			// Clone() should return such node that is not readonly
			string dtd = "<!DOCTYPE root ["
				+ "<!ELEMENT root (#PCDATA|foo)*>"
				+ "<!ELEMENT foo EMPTY>"
				+ "<!ENTITY ent1 '<foo /><![CDATA[cdata]]>'>]>";
			string xml = dtd + "<root>&ent1;</root>";

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNode n = doc.DocumentElement.FirstChild.FirstChild;
			Assert.IsTrue (n.IsReadOnly, "#1");
			Assert.IsTrue (!n.CloneNode (true).IsReadOnly, "#2");
		}

		[Test] // bug #80233
		public void InnerTextComment ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<a><!--xx--></a>");
			Assert.AreEqual (String.Empty, doc.InnerText);
		}

		[Test] // part of bug #80331
		public void AppendReferenceChildAsNewChild ()
		{
			XmlDocument d = new XmlDocument ();
			XmlElement r = d.CreateElement ("Docs");
			d.AppendChild (r);

			XmlElement s = Create (d, "param", "pattern");
			s.AppendChild (Create (d, "para", "insert text here"));

			r.AppendChild (s);

			r.AppendChild (Create (d, "param", "pattern"));
			r.AppendChild (Create (d, "param", "pattern"));

			r.InsertBefore (s, r.FirstChild);
		}

		XmlElement Create (XmlDocument d, string name, string param)
		{
			XmlElement e = d.CreateElement (name);
			e.SetAttribute ("name", param);
			return e;
		}

		[Test] // bug #80331
		public void PrependChild2 ()
		{
			XmlDocument d = new XmlDocument ();
			XmlElement r = d.CreateElement ("Docs");
			d.AppendChild (r);

			XmlElement s = Create (d, "param", "pattern");
			s.AppendChild (Create (d, "para", "insert text here"));

			r.AppendChild (s);

			r.AppendChild (Create (d, "param", "pattern"));
			r.AppendChild (Create (d, "param", "pattern"));

			r.PrependChild (s);
		}

	}
}
