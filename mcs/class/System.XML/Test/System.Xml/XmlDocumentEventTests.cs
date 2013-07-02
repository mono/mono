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
	public class XmlDocumentEventTests
	{
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
			Assert.AreEqual ("Inserting: Text into Element.\n" +
				"Inserted: Text into Element.\n" +
				"Inserting: Element into Document.\n" +
				"Inserted: Element into Document.\n",
				EventLog);
		}

		[Test]
		public void DefaultAttributeRemoval ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<!DOCTYPE root [<!ELEMENT root (#PCDATA)><!ATTLIST root foo CDATA 'foo-def'>]><root></root>");
			SetEvents (doc);
			doc.DocumentElement.RemoveAll ();
			Assert.AreEqual ("Removing: Attribute from Element.\n" +
				"Removed: Attribute from Element.\n" +
				"Inserting: Text into Attribute.\n" +
				"Inserted: Text into Attribute.\n" +
				"Inserting: Attribute into Element.\n" +
				"Inserted: Attribute into Element.\n",
				EventLog);
		}
	}
}
