//
// System.Xml.XmlNodeReaderTests
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2003 Atsushi Enomoto
// (C) 2003 Martin Willemoes Hansen
//
//

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlNodeReaderTests
	{
		[SetUp]
		public void GetReady ()
		{
			document.LoadXml ("<root attr1='value1'><child /></root>");
		}

		XmlDocument document = new XmlDocument ();

		[Test]
		public void InvalidConstruction ()
		{
			XmlNodeReader nrdr;
			try {
				nrdr = new XmlNodeReader (null);
				Assert.Fail ("null reference exception is preferable.");
			} catch (NullReferenceException) {
			}
			nrdr = new XmlNodeReader (new XmlDocument ());
			nrdr.Read ();
			Assert.AreEqual (ReadState.Error, nrdr.ReadState, "newDoc.ReadState");
			Assert.AreEqual (true, nrdr.EOF, "newDoc.EOF");
			Assert.AreEqual (XmlNodeType.None, nrdr.NodeType, "newDoc.NodeType");
			nrdr = new XmlNodeReader (document.CreateDocumentFragment ());
			nrdr.Read ();
			Assert.AreEqual (ReadState.Error, nrdr.ReadState, "Fragment.ReadState");
			Assert.AreEqual (true, nrdr.EOF, "Fragment.EOF");
			Assert.AreEqual (XmlNodeType.None, nrdr.NodeType, "Fragment.NodeType");
		}

		[Test]
		public void ReadFromElement ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document.DocumentElement);
			nrdr.Read ();
			Assert.AreEqual (XmlNodeType.Element, nrdr.NodeType, "<root>.NodeType");
			Assert.AreEqual ("root", nrdr.Name, "<root>.Name");
			Assert.AreEqual (ReadState.Interactive, nrdr.ReadState, "<root>.ReadState");
			Assert.AreEqual (0, nrdr.Depth, "<root>.Depth");
		}


		[Test]
		public void ReadInnerXmlWrongInit ()
		{
			document.LoadXml ("<root>test of <b>mixed</b> string.</root>");
			XmlNodeReader nrdr = new XmlNodeReader (document);
			nrdr.ReadInnerXml ();
			Assert.AreEqual (ReadState.Initial, nrdr.ReadState, "initial.ReadState");
			Assert.AreEqual (false, nrdr.EOF, "initial.EOF");
			Assert.AreEqual (XmlNodeType.None, nrdr.NodeType, "initial.NodeType");
		}

		[Test]
		public void ResolveEntity ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent;&ent2;</root>";
			document.LoadXml (xml);
			Assert.AreEqual (xml, document.OuterXml);
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();	// DTD
			nr.Read ();	// root
			nr.Read ();	// &ent;
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			Assert.AreEqual (1, nr.Depth, "depth#1");
			nr.ResolveEntity ();
			// It is still entity reference.
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			nr.Read ();
			Assert.AreEqual (XmlNodeType.Text, nr.NodeType);
			Assert.AreEqual (2, nr.Depth, "depth#2");
			Assert.AreEqual ("entity string", nr.Value);
			nr.Read ();
			Assert.AreEqual (XmlNodeType.EndEntity, nr.NodeType);
			Assert.AreEqual (1, nr.Depth, "depth#3");
			Assert.AreEqual ("", nr.Value);

			nr.Read ();	// &ent2;
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			Assert.AreEqual (1, nr.Depth, "depth#4");
			nr.ResolveEntity ();
			Assert.AreEqual (xml, document.OuterXml);
			// It is still entity reference.
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			// It now became element node.
			nr.Read ();
			Assert.AreEqual (XmlNodeType.Element, nr.NodeType);
			Assert.AreEqual (2, nr.Depth, "depth#5");

			Assert.AreEqual (xml, document.OuterXml);
		}

		[Test]
#if NET_2_0
		[Ignore (".NET 2.0 XmlNodeReader does not allow undeclared entities at all.")]
#endif
		public void ResolveEntity2 ()
		{
			document.RemoveAll ();
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent3;&ent2;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Read ();
			document.AppendChild (document.ReadNode (xtr));
			document.AppendChild (document.ReadNode (xtr));
			xtr.Close ();
			Assert.AreEqual (xml, document.OuterXml);
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();	// DTD
			nr.Read ();	// root
			nr.Read ();	// &ent3;
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			// ent3 does not exists in this dtd.
			nr.ResolveEntity ();
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			nr.Read ();
#if false
			// Hmm... MS.NET returned as it is a Text node.
			Assert.AreEqual (XmlNodeType.Text, nr.NodeType);
			Assert.AreEqual (String.Empty, nr.Value);
			nr.Read ();
			// Really!?
			Assert.AreEqual (XmlNodeType.EndEntity, nr.NodeType);
			Assert.AreEqual (String.Empty, nr.Value);
#endif
		}

		[Test]
#if NET_2_0
		[Ignore (".NET 2.0 XmlNodeReader does not allow undeclared entities at all.")]
#endif
		public void ResolveEntityWithoutDTD ()
		{
			document.RemoveAll ();
			string xml = "<root>&ent;&ent2;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Read ();
			document.AppendChild (document.ReadNode (xtr));
			xtr.Close ();
			Assert.AreEqual (xml, document.OuterXml);
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();	// root
			nr.Read ();	// &ent;
			Assert.AreEqual (XmlNodeType.EntityReference, nr.NodeType);
			// ent does not exists in this dtd.
			nr.ResolveEntity ();
		}

		[Test] // bug #76260
		public void FromEmptyNonDocumentElement ()
		{
			document.LoadXml ("<root><child/></root>");
			XmlNodeReader nr = new XmlNodeReader (
				document.DocumentElement.FirstChild);
			nr.Read ();
			Assert.AreEqual (true, nr.IsEmptyElement, "#0");
			Assert.IsTrue (!nr.Read (), "#1");

			document.LoadXml ("<root><child></child></root>");
			nr = new XmlNodeReader (
				document.DocumentElement.FirstChild);
			nr.Read ();
			Assert.IsTrue (nr.Read (), "#2");
			Assert.AreEqual (false, nr.IsEmptyElement, "#2.2");
			Assert.IsTrue (!nr.Read (), "#3");
		}

		[Test] // bug #550379
		public void MoveToNextAttributeFromValue ()
		{
			document.LoadXml ("<ul test='xxx'></ul>");
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();
			nr.Read ();
			Assert.IsTrue (nr.MoveToFirstAttribute (), "#1");
			Assert.IsTrue (nr.ReadAttributeValue (), "#2");
			Assert.IsFalse (nr.MoveToNextAttribute (), "#3");
		}
	}

}
