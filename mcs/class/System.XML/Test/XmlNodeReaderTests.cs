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

		// MS.NET's not-overriden XmlNodeReader.WriteStartElement(name)
		// invokes WriteStartElement(null, name, null). 
		// WriteStartElement(name, ns) invokes (null, name, ns), too.
		[Test]
		public void InitialState ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document);
			Assertion.AssertEquals ("Depth", 0, nrdr.Depth);
			Assertion.AssertEquals ("EOF", false, nrdr.EOF);
			Assertion.AssertEquals ("HasValue", false, nrdr.HasValue);
			Assertion.AssertEquals ("IsEmptyElement", false, nrdr.IsEmptyElement);
			Assertion.AssertEquals ("LocalName", String.Empty, nrdr.LocalName);
			Assertion.AssertEquals ("NodeType", XmlNodeType.None, nrdr.NodeType);
			Assertion.AssertEquals ("ReadState", ReadState.Initial, nrdr.ReadState);
		}

		[Test]
		public void InvalidConstruction ()
		{
			XmlNodeReader nrdr;
			try {
				nrdr = new XmlNodeReader (null);
				Assertion.Fail ("null reference exception is preferable.");
			} catch (NullReferenceException ex) {
			}
			nrdr = new XmlNodeReader (new XmlDocument ());
			nrdr.Read ();
			Assertion.AssertEquals ("newDoc.ReadState", ReadState.Error, nrdr.ReadState);
			Assertion.AssertEquals ("newDoc.EOF", true, nrdr.EOF);
			Assertion.AssertEquals ("newDoc.NodeType", XmlNodeType.None, nrdr.NodeType);
			nrdr = new XmlNodeReader (document.CreateDocumentFragment ());
			nrdr.Read ();
			Assertion.AssertEquals ("Fragment.ReadState", ReadState.Error, nrdr.ReadState);
			Assertion.AssertEquals ("Fragment.EOF", true, nrdr.EOF);
			Assertion.AssertEquals ("Fragment.NodeType", XmlNodeType.None, nrdr.NodeType);
		}

		[Test]
		public void Read ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document);
			nrdr.Read ();
			Assertion.AssertEquals ("<root>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			Assertion.AssertEquals ("<root>.Name", "root", nrdr.Name);
			Assertion.AssertEquals ("<root>.ReadState", ReadState.Interactive, nrdr.ReadState);
			Assertion.AssertEquals ("<root>.Depth", 0, nrdr.Depth);

			// move to 'child'
			nrdr.Read ();
			Assertion.AssertEquals ("<child/>.Depth", 1, nrdr.Depth);
			Assertion.AssertEquals ("<child/>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			Assertion.AssertEquals ("<child/>.Name", "child", nrdr.Name);

			nrdr.Read ();
			Assertion.AssertEquals ("</root>.Depth", 0, nrdr.Depth);
			Assertion.AssertEquals ("</root>.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
			Assertion.AssertEquals ("</root>.Name", "root", nrdr.Name);

			nrdr.Read ();
			Assertion.AssertEquals ("end.EOF", true, nrdr.EOF);
			Assertion.AssertEquals ("end.NodeType", XmlNodeType.None, nrdr.NodeType);
		}

		[Test]
		public void ReadFromElement ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document.DocumentElement);
			nrdr.Read ();
			Assertion.AssertEquals ("<root>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			Assertion.AssertEquals ("<root>.Name", "root", nrdr.Name);
			Assertion.AssertEquals ("<root>.ReadState", ReadState.Interactive, nrdr.ReadState);
			Assertion.AssertEquals ("<root>.Depth", 0, nrdr.Depth);
		}

		[Test]
		public void ReadString ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root>test of <b>mixed</b> string.<![CDATA[ cdata string.]]></root>");
			XmlNodeReader nrdr = new XmlNodeReader (doc);
			nrdr.Read ();
			string s = nrdr.ReadString ();
			Assertion.AssertEquals ("readString.1.ret_val", "test of ", s);
			Assertion.AssertEquals ("readString.1.Name", "b", nrdr.Name);
			s = nrdr.ReadString ();
			Assertion.AssertEquals ("readString.2.ret_val", "mixed", s);
			Assertion.AssertEquals ("readString.2.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
			s = nrdr.ReadString ();	// never proceeds.
			Assertion.AssertEquals ("readString.3.ret_val", String.Empty, s);
			Assertion.AssertEquals ("readString.3.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
			nrdr.Read ();
			Assertion.AssertEquals ("readString.4.NodeType", XmlNodeType.Text, nrdr.NodeType);
			Assertion.AssertEquals ("readString.4.Value", " string.", nrdr.Value);
			s = nrdr.ReadString ();	// reads the same Text node.
			Assertion.AssertEquals ("readString.5.ret_val", " string. cdata string.", s);
			Assertion.AssertEquals ("readString.5.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
		}

		[Test]
		public void RedInnerXml ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root>test of <b>mixed</b> string.</root>");
			XmlNodeReader nrdr = new XmlNodeReader (doc);
			nrdr.ReadInnerXml ();
			Assertion.AssertEquals ("initial.ReadState", ReadState.Error, nrdr.ReadState);
			Assertion.AssertEquals ("initial.EOF", true, nrdr.EOF);
			Assertion.AssertEquals ("initial.NodeType", XmlNodeType.None, nrdr.NodeType);
		}
	}

}
