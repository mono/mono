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
			Assertion.Assert (inserted);
			Assertion.Assert (inserting);

			// Can only append to elements, documents, and attributes
			try 
			{
				comment = document.CreateComment ("baz");
				comment.AppendChild (element2);
				Assertion.Fail ("Expected an InvalidOperationException to be thrown.");
			} 
			catch (InvalidOperationException) {}

			// Can't append a node from one document into another document.
			XmlDocument document2 = new XmlDocument();
			Assertion.AssertEquals (1, element.ChildNodes.Count);
			try 
			{
				element2 = document2.CreateElement ("qux");
				element.AppendChild (element2);
				Assertion.Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
			Assertion.AssertEquals (1, element.ChildNodes.Count);

			// Can't append to a readonly node.
/* TODO put this in when I figure out how to create a read-only node.
			try 
			{
				XmlElement element3 = (XmlElement)element.CloneNode (false);
				Assertion.Assert (!element.IsReadOnly);
				Assertion.Assert (element3.IsReadOnly);
				element2 = document.CreateElement ("quux");
				element3.AppendChild (element2);
				Assertion.Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
*/
		}

		[Test]
		public void InsertBefore()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub /></root>");
			XmlElement docelem = document.DocumentElement;
			docelem.InsertBefore(document.CreateElement("good_child"), docelem.FirstChild);
			Assertion.AssertEquals("InsertBefore.Normal", "good_child", docelem.FirstChild.Name);
			// These are required for .NET 1.0 but not for .NET 1.1.
//			try {
//				document.InsertBefore (document.CreateElement ("BAD_MAN"), docelem);
//				Assertion.Fail ("#InsertBefore.BadPositionButNoError.1");
//			}
//			catch (XmlException) {}
		}

		[Test]
		public void InsertAfter()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub1 /><sub2 /></root>");
			XmlElement docelem = document.DocumentElement;
			XmlElement newelem = document.CreateElement("good_child");
			docelem.InsertAfter(newelem, docelem.FirstChild);
			Assertion.AssertEquals("InsertAfter.Normal", 3, docelem.ChildNodes.Count);
			Assertion.AssertEquals("InsertAfter.First", "sub1", docelem.FirstChild.Name);
			Assertion.AssertEquals("InsertAfter.Last", "sub2", docelem.LastChild.Name);
			Assertion.AssertEquals("InsertAfter.Prev", "good_child", docelem.FirstChild.NextSibling.Name);
			Assertion.AssertEquals("InsertAfter.Next", "good_child", docelem.LastChild.PreviousSibling.Name);
			// this doesn't throw an exception
			document.InsertAfter(document.CreateElement("BAD_MAN"), docelem);
			Assertion.AssertEquals("InsertAfter with bad location", 
				"<root><sub1 /><good_child /><sub2 /></root><BAD_MAN />",
				document.InnerXml);
}

		[Test]
		public void PrependChild()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub1 /><sub2 /></root>");
			XmlElement docelem = document.DocumentElement;
			docelem.PrependChild(document.CreateElement("prepender"));
			Assertion.AssertEquals("PrependChild", "prepender", docelem.FirstChild.Name);
		}

		public void saveTestRemoveAll ()
		{
			// TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveAll ();
			Assertion.Assert (removed);
			Assertion.Assert (removing);
		}

		[Test]
		public void RemoveChild ()
		{
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveChild (element2);
			Assertion.Assert (removed);
			Assertion.Assert (removing);
		}
		
		[Test]
		public void GetPrefixOfNamespace ()
		{
			document.LoadXml ("<root><c1 xmlns='urn:foo'><c2 xmlns:foo='urn:foo' xmlns='urn:bar'><c3 xmlns=''/></c2></c1></root>");
			Assertion.AssertEquals ("root", String.Empty, document.DocumentElement.GetPrefixOfNamespace ("urn:foo"));
			Assertion.AssertEquals ("c1", String.Empty, document.DocumentElement.GetPrefixOfNamespace ("urn:foo"));
			Assertion.AssertEquals ("c2", String.Empty, document.DocumentElement.FirstChild.GetPrefixOfNamespace ("urn:foo"));
			Assertion.AssertEquals ("c3", "foo", document.DocumentElement.FirstChild.FirstChild.GetPrefixOfNamespace ("urn:foo"));

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
			Assertion.AssertEquals ("root2", document.DocumentElement.Name);
			Assertion.AssertEquals (1, document.ChildNodes.Count);
			Assertion.Assert (inserted && removed && !changed);
		}
	}
}
