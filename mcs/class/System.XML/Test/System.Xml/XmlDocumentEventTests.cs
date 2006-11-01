//
// XmlDocumentEventTests.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// This class is a set of event test.
//
using NUnit.Framework;
using System;
using System.Text;
using System.Xml;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDocumentEventTests : Assertion
	{
		public static void Main ()
		{
			new XmlDocumentEventTests ().InsertingOrder ();
		}

		private StringBuilder eventLogBuilder = new StringBuilder ();

		private XmlDocument GetEventDocument ()
		{
			XmlDocument document = new XmlDocument ();
			SetEvents (document);
			return document;
		}

		private void SetEvents (XmlDocument document)
		{
			document.NodeInserting += new XmlNodeChangedEventHandler (OnInsertingLog);
			document.NodeInserted += new XmlNodeChangedEventHandler (OnInsertedLog);
			document.NodeChanging += new XmlNodeChangedEventHandler (OnChangingLog);
			document.NodeChanged += new XmlNodeChangedEventHandler (OnChangedLog);
			document.NodeRemoving += new XmlNodeChangedEventHandler (OnRemovingLog);
			document.NodeRemoved += new XmlNodeChangedEventHandler (OnRemovedLog);
		}

		private void OnInsertingLog (object o, XmlNodeChangedEventArgs e)
		{
			eventLogBuilder.Append ("Inserting: " + e.Node.NodeType + " into " + e.NewParent.NodeType + ".\n");
		}

		private void OnInsertedLog (object o, XmlNodeChangedEventArgs e)
		{
			eventLogBuilder.Append ("Inserted: " + e.Node.NodeType + " into " + e.NewParent.NodeType + ".\n");
		}

		private void OnChangingLog (object o, XmlNodeChangedEventArgs e)
		{
			eventLogBuilder.Append ("Changing: " + e.Node.NodeType + " into " + e.NewParent.NodeType + ".\n");
		}

		private void OnChangedLog (object o, XmlNodeChangedEventArgs e)
		{
			eventLogBuilder.Append ("Changed: " + e.Node.NodeType + " into " + e.NewParent.NodeType + ".\n");
		}

		private void OnRemovingLog (object o, XmlNodeChangedEventArgs e)
		{
			eventLogBuilder.Append ("Removing: " + e.Node.NodeType + " from " + e.OldParent.NodeType + ".\n");
		}

		private void OnRemovedLog (object o, XmlNodeChangedEventArgs e)
		{
			eventLogBuilder.Append ("Removed: " + e.Node.NodeType + " from " + e.OldParent.NodeType + ".\n");
		}

		[SetUp]
		public void SetUp ()
		{
			eventLogBuilder.Length = 0;
		}

		private string EventLog {
			get { return eventLogBuilder.ToString (); }
		}

		[Test]
		public void InsertingOrder ()
		{
			XmlDocument document = GetEventDocument ();
			XmlElement el = document.CreateElement ("root");
			el.AppendChild (document.CreateTextNode ("simple text node."));
			document.AppendChild (el);
			AssertEquals (
@"Inserting: Text into Element.
Inserted: Text into Element.
Inserting: Element into Document.
Inserted: Element into Document.
", EventLog);
		}

		[Test]
		public void DefaultAttributeRemoval ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<!DOCTYPE root [<!ELEMENT root (#PCDATA)><!ATTLIST root foo CDATA 'foo-def'>]><root></root>");
			SetEvents (doc);
			doc.DocumentElement.RemoveAll ();
			AssertEquals (
@"Removing: Attribute from Element.
Removed: Attribute from Element.
Inserting: Text into Attribute.
Inserted: Text into Attribute.
Inserting: Attribute into Element.
Inserted: Attribute into Element.
", EventLog);
		}
	}
}
