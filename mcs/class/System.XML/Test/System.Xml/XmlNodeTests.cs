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

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlNodeTests : Assertion
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
			Assert (inserted);
			Assert (inserting);

			// Can only append to elements, documents, and attributes
			try 
			{
				comment = document.CreateComment ("baz");
				comment.AppendChild (element2);
				Fail ("Expected an InvalidOperationException to be thrown.");
			} 
			catch (InvalidOperationException) {}

			// Can't append a node from one document into another document.
			XmlDocument document2 = new XmlDocument();
			AssertEquals (1, element.ChildNodes.Count);
			try 
			{
				element2 = document2.CreateElement ("qux");
				element.AppendChild (element2);
				Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
			AssertEquals (1, element.ChildNodes.Count);

			// Can't append to a readonly node.
/* TODO put this in when I figure out how to create a read-only node.
			try 
			{
				XmlElement element3 = (XmlElement)element.CloneNode (false);
				Assert (!element.IsReadOnly);
				Assert (element3.IsReadOnly);
				element2 = document.CreateElement ("quux");
				element3.AppendChild (element2);
				Fail ("Expected an ArgumentException to be thrown.");
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
			AssertEquals ("urn:default", n.GetNamespaceOfPrefix (String.Empty));
			AssertEquals ("urn:foo", n.GetNamespaceOfPrefix ("foo"));
			AssertEquals (String.Empty, n.GetNamespaceOfPrefix ("bar"));
			AssertEquals (String.Empty, n.GetNamespaceOfPrefix ("xml"));
			AssertEquals (String.Empty, n.GetNamespaceOfPrefix ("xmlns"));

			n = document.DocumentElement.FirstChild;
			AssertEquals ("urn:default", n.GetNamespaceOfPrefix (String.Empty));
			AssertEquals ("urn:foo", n.GetNamespaceOfPrefix ("foo"));
			AssertEquals (String.Empty, n.GetNamespaceOfPrefix ("bar"));
			AssertEquals (String.Empty, n.GetNamespaceOfPrefix ("xml"));
			AssertEquals (String.Empty, n.GetNamespaceOfPrefix ("xmlns"));
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
			AssertEquals("InsertBefore.Normal", "good_child", docelem.FirstChild.Name);
			// These are required for .NET 1.0 but not for .NET 1.1.
			try {
				document.InsertBefore (document.CreateElement ("BAD_MAN"), docelem);
				Fail ("#InsertBefore.BadPositionButNoError.1");
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
			AssertEquals("InsertAfter.Normal", 3, docelem.ChildNodes.Count);
			AssertEquals("InsertAfter.First", "sub1", docelem.FirstChild.Name);
			AssertEquals("InsertAfter.Last", "sub2", docelem.LastChild.Name);
			AssertEquals("InsertAfter.Prev", "good_child", docelem.FirstChild.NextSibling.Name);
			AssertEquals("InsertAfter.Next", "good_child", docelem.LastChild.PreviousSibling.Name);
			// this doesn't throw any exception *only on .NET 1.1*
			// .NET 1.0 throws an exception.
			try {
				document.InsertAfter(document.CreateElement("BAD_MAN"), docelem);
#if USE_VERSION_1_1
				AssertEquals("InsertAfter with bad location", 
				"<root><sub1 /><good_child /><sub2 /></root><BAD_MAN />",
					document.InnerXml);
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

			AssertEquals (3, doc.DocumentElement.ChildNodes.Count);

			doc.DocumentElement.Normalize ();
			AssertEquals (3, doc.DocumentElement.ChildNodes.Count);
			Assert (changed);
			inserted = changed = removed = false;

			doc.DocumentElement.AppendChild (doc.CreateTextNode ("Addendum."));
			AssertEquals (4, doc.DocumentElement.ChildNodes.Count);
			inserted = changed = removed = false;

			doc.DocumentElement.Normalize ();
			AssertEquals (3, doc.DocumentElement.ChildNodes.Count);
			Assert (changed);
			Assert (removed);
			inserted = changed = removed = false;

			doc.DocumentElement.SetAttribute ("attr", "");
			XmlAttribute attr = doc.DocumentElement.Attributes [0] as XmlAttribute;
			AssertEquals (1, attr.ChildNodes.Count);
			inserted = changed = removed = false;
			attr.Normalize ();
			// Such behavior violates DOM Level 2 Node#normalize(),
			// but MS DOM is designed as such.
			AssertEquals (1, attr.ChildNodes.Count);
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
			AssertEquals ("Before Normalize()", 5, root.ChildNodes.Count);
			root.Normalize ();
			AssertEquals ("<root>  foobar   baz</root>", root.OuterXml);
			AssertEquals ("After Normalize()", 1, root.ChildNodes.Count);
		}

		int normalize2Count;

		private void OnChange (object o, XmlNodeChangedEventArgs e)
		{
			switch (normalize2Count) {
			case 0:
				AssertEquals ("Action0", XmlNodeChangedAction.Remove, e.Action);
				AssertEquals ("Value0", "  ", e.Node.Value);
				break;
			case 1:
				AssertEquals ("Action1", XmlNodeChangedAction.Remove, e.Action);
				AssertEquals ("Value1", "bar", e.Node.Value);
				break;
			case 2:
				AssertEquals ("Action2", XmlNodeChangedAction.Remove, e.Action);
				AssertEquals ("Value2", "   ", e.Node.Value);
				break;
			case 3:
				AssertEquals ("Action3", XmlNodeChangedAction.Remove, e.Action);
				AssertEquals ("Value3", "baz", e.Node.Value);
				break;
			case 4:
				AssertEquals ("Action4", XmlNodeChangedAction.Change, e.Action);
				AssertEquals ("Value4", "  foobar   baz", e.Node.Value);
				break;
			default:
				Fail (String.Format ("Unexpected event. Action = {0}, node type = {1}, node name = {2}, node value = {3}", e.Action, e.Node.NodeType, e.Node.Name, e.Node.Value));
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
			AssertEquals("PrependChild", "prepender", docelem.FirstChild.Name);
		}

		public void saveTestRemoveAll ()
		{
			// TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveAll ();
			Assert (removed);
			Assert (removing);
		}

		[Test]
		public void RemoveChild ()
		{
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveChild (element2);
			Assert (removed);
			Assert (removing);
		}
		
		[Test]
		public void RemoveLastChild ()
		{
			element.InnerXml = "<foo/><bar/><baz/>";
			element.RemoveChild (element.LastChild);
			AssertNotNull (element.FirstChild);
		}
		
		[Test]
		public void GetPrefixOfNamespace ()
		{
			document.LoadXml ("<root><c1 xmlns='urn:foo'><c2 xmlns:foo='urn:foo' xmlns='urn:bar'><c3 xmlns=''/></c2></c1></root>");
			AssertEquals ("root", String.Empty, document.DocumentElement.GetPrefixOfNamespace ("urn:foo"));
			AssertEquals ("c1", String.Empty, document.DocumentElement.GetPrefixOfNamespace ("urn:foo"));
			AssertEquals ("c2", String.Empty, document.DocumentElement.FirstChild.GetPrefixOfNamespace ("urn:foo"));
			AssertEquals ("c3", "foo", document.DocumentElement.FirstChild.FirstChild.GetPrefixOfNamespace ("urn:foo"));

			// disconnected nodes.
			XmlNode n = document.CreateElement ("foo");
			AssertEquals (String.Empty, n.GetPrefixOfNamespace ("foo"));
			n = document.CreateTextNode ("text"); // does not have Attributes
			AssertEquals (String.Empty, n.GetPrefixOfNamespace ("foo"));
			n = document.CreateXmlDeclaration ("1.0", null, null); // does not have Attributes
			AssertEquals (String.Empty, n.GetPrefixOfNamespace ("foo"));
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
			AssertEquals ("root2", document.DocumentElement.Name);
			AssertEquals (1, document.ChildNodes.Count);
			Assert (inserted && removed && !changed);
		}

		[Test]
		public void InnerText ()
		{
			document.LoadXml ("<root>This is <b>mixed</b> content. Also includes <![CDATA[CDATA section]]>.<!-- Should be ignored --></root>");
			string total = "This is mixed content. Also includes CDATA section.";
			XmlNode elemB = document.DocumentElement.ChildNodes [1];
			AssertEquals ("mixed", elemB.FirstChild.InnerText);	// text node
			AssertEquals ("mixed", elemB.InnerText);	// element b
			AssertEquals (total, document.DocumentElement.InnerText);	// element root
			AssertEquals (total, document.InnerText);	// whole document
		}

		[Test]
		public void InnerXmlWithXmlns ()
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement ("KeyValue", "http://www.w3.org/2000/09/xmldsig#");
			xel.SetAttribute ("xmlns", "http://www.w3.org/2000/09/xmldsig#");
			xel.InnerXml = "<DSAKeyValue>blablabla</DSAKeyValue>";
			string expected = "<KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><DSAKeyValue>blablabla</DSAKeyValue></KeyValue>";
			AssertEquals (expected, xel.OuterXml);
		}

		[Test]
		public void SelectNodes ()
		{
			// This test is done in this class since it tests only XmlDocumentNavigator.
			string xpath = "//@*|//namespace::*";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			XmlNodeList nl = doc.SelectNodes (xpath);
			AssertEquals (6, nl.Count);
			// BTW, as for namespace nodes, Node does not exist
			// in the tree, so the return value should be
			// implementation dependent.
			AssertEquals (XmlNodeType.Attribute, nl [0].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [1].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [2].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [3].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [4].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [5].NodeType);
			AssertEquals ("xmlns", nl [0].LocalName);
			AssertEquals ("xml", nl [1].LocalName);
			AssertEquals ("xmlns", nl [2].LocalName);
			AssertEquals ("xml", nl [3].LocalName);
			AssertEquals ("xmlns", nl [4].LocalName);
			AssertEquals ("xml", nl [5].LocalName);
		}

		[Test]
		[Ignore ("MS.NET has a bug; it fails to return nodes in document order.")]
		public void SelectNodes2 ()
		{
			// This test is done in this class since it tests only XmlDocumentNavigator.
			string xpath = "//*|//@*|//namespace::*";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			XmlNodeList nl = doc.SelectNodes (xpath);
			AssertEquals (9, nl.Count);
			AssertEquals (XmlNodeType.Element, nl [0].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [1].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [2].NodeType);
			AssertEquals (XmlNodeType.Element, nl [3].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [4].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [5].NodeType);
			AssertEquals (XmlNodeType.Element, nl [6].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [7].NodeType);
			AssertEquals (XmlNodeType.Attribute, nl [8].NodeType);
			AssertEquals ("element", nl [0].LocalName);
			AssertEquals ("xmlns", nl [1].LocalName);
			AssertEquals ("xml", nl [2].LocalName);
			AssertEquals ("foo", nl [3].LocalName);
			AssertEquals ("xmlns", nl [4].LocalName);
			AssertEquals ("xml", nl [5].LocalName);
			AssertEquals ("bar", nl [6].LocalName);
			AssertEquals ("xmlns", nl [7].LocalName);
			AssertEquals ("xml", nl [8].LocalName);
		}

		[Test]
		public void BaseURI ()
		{
			// See bug #64120.
			XmlDocument doc = new XmlDocument ();
			doc.Load ("Test/XmlFiles/simple.xml");
			XmlElement el = doc.CreateElement ("foo");
			AssertEquals (String.Empty, el.BaseURI);
			doc.DocumentElement.AppendChild (el);
			Assert (String.Empty != el.BaseURI);
			XmlAttribute attr = doc.CreateAttribute ("attr");
			AssertEquals (String.Empty, attr.BaseURI);
		}
	}
}
