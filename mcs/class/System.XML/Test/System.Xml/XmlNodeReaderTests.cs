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
	public class XmlNodeReaderTests : Assertion
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
				Fail ("null reference exception is preferable.");
			} catch (NullReferenceException) {
			}
			nrdr = new XmlNodeReader (new XmlDocument ());
			nrdr.Read ();
			AssertEquals ("newDoc.ReadState", ReadState.Error, nrdr.ReadState);
			AssertEquals ("newDoc.EOF", true, nrdr.EOF);
			AssertEquals ("newDoc.NodeType", XmlNodeType.None, nrdr.NodeType);
			nrdr = new XmlNodeReader (document.CreateDocumentFragment ());
			nrdr.Read ();
			AssertEquals ("Fragment.ReadState", ReadState.Error, nrdr.ReadState);
			AssertEquals ("Fragment.EOF", true, nrdr.EOF);
			AssertEquals ("Fragment.NodeType", XmlNodeType.None, nrdr.NodeType);
		}

		[Test]
		public void ReadFromElement ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document.DocumentElement);
			nrdr.Read ();
			AssertEquals ("<root>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			AssertEquals ("<root>.Name", "root", nrdr.Name);
			AssertEquals ("<root>.ReadState", ReadState.Interactive, nrdr.ReadState);
			AssertEquals ("<root>.Depth", 0, nrdr.Depth);
		}


		[Test]
		public void ReadInnerXmlWrongInit ()
		{
			document.LoadXml ("<root>test of <b>mixed</b> string.</root>");
			XmlNodeReader nrdr = new XmlNodeReader (document);
			nrdr.ReadInnerXml ();
			AssertEquals ("initial.ReadState", ReadState.Initial, nrdr.ReadState);
			AssertEquals ("initial.EOF", false, nrdr.EOF);
			AssertEquals ("initial.NodeType", XmlNodeType.None, nrdr.NodeType);
		}

		[Test]
		public void ResolveEntity ()
		{
			string ent1 = "<!ENTITY ent 'entity string'>";
			string ent2 = "<!ENTITY ent2 '<foo/><foo/>'>]>";
			string dtd = "<!DOCTYPE root[<!ELEMENT root (#PCDATA|foo)*>" + ent1 + ent2;
			string xml = dtd + "<root>&ent;&ent2;</root>";
			document.LoadXml (xml);
			AssertEquals (xml, document.OuterXml);
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();	// DTD
			nr.Read ();	// root
			nr.Read ();	// &ent;
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			AssertEquals ("depth#1", 1, nr.Depth);
			nr.ResolveEntity ();
			// It is still entity reference.
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			nr.Read ();
			AssertEquals (XmlNodeType.Text, nr.NodeType);
			AssertEquals ("depth#2", 2, nr.Depth);
			AssertEquals ("entity string", nr.Value);
			nr.Read ();
			AssertEquals (XmlNodeType.EndEntity, nr.NodeType);
			AssertEquals ("depth#3", 1, nr.Depth);
			AssertEquals ("", nr.Value);

			nr.Read ();	// &ent2;
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			AssertEquals ("depth#4", 1, nr.Depth);
			nr.ResolveEntity ();
			AssertEquals (xml, document.OuterXml);
			// It is still entity reference.
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			// It now became element node.
			nr.Read ();
			AssertEquals (XmlNodeType.Element, nr.NodeType);
			AssertEquals ("depth#5", 2, nr.Depth);

			AssertEquals (xml, document.OuterXml);
		}

		[Test]
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
			AssertEquals (xml, document.OuterXml);
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();	// DTD
			nr.Read ();	// root
			nr.Read ();	// &ent3;
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			// ent3 does not exists in this dtd.
			nr.ResolveEntity ();
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			nr.Read ();
#if false
			// Hmm... MS.NET returned as it is a Text node.
			AssertEquals (XmlNodeType.Text, nr.NodeType);
			AssertEquals (String.Empty, nr.Value);
			nr.Read ();
			// Really!?
			AssertEquals (XmlNodeType.EndEntity, nr.NodeType);
			AssertEquals (String.Empty, nr.Value);
#endif
		}

		[Test]
		public void ResolveEntityWithoutDTD ()
		{
			document.RemoveAll ();
			string xml = "<root>&ent;&ent2;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.Read ();
			document.AppendChild (document.ReadNode (xtr));
			xtr.Close ();
			AssertEquals (xml, document.OuterXml);
			XmlNodeReader nr = new XmlNodeReader (document);
			nr.Read ();	// root
			nr.Read ();	// &ent;
			AssertEquals (XmlNodeType.EntityReference, nr.NodeType);
			// ent does not exists in this dtd.
			nr.ResolveEntity ();
		}
	}

}
