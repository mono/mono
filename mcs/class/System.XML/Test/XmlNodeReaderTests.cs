//
// System.Xml.XmlNodeReaderTests
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//
//



using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlNodeReaderTests : TestCase
	{
		public XmlNodeReaderTests () : base ("MonoTests.System.Xml.NodeReaderTests testsuite") {}
		public XmlNodeReaderTests (string name) : base (name) {}

		protected override void SetUp ()
		{
			document.LoadXml ("<root attr1='value1'><child /></root>");
		}

		XmlDocument document = new XmlDocument ();

		// MS.NET's not-overriden XmlNodeReader.WriteStartElement(name)
		// invokes WriteStartElement(null, name, null). 
		// WriteStartElement(name, ns) invokes (null, name, ns), too.
		public void TestInitialState ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document);
			AssertEquals ("Depth", 0, nrdr.Depth);
			AssertEquals ("EOF", false, nrdr.EOF);
			AssertEquals ("HasValue", false, nrdr.HasValue);
			AssertEquals ("IsEmptyElement", false, nrdr.IsEmptyElement);
			AssertEquals ("LocalName", String.Empty, nrdr.LocalName);
			AssertEquals ("NodeType", XmlNodeType.None, nrdr.NodeType);
			AssertEquals ("ReadState", ReadState.Initial, nrdr.ReadState);
		}

		public void TestInvalidConstruction ()
		{
			XmlNodeReader nrdr;
			try {
				nrdr = new XmlNodeReader (null);
				Fail ("null reference exception is preferable.");
			} catch (NullReferenceException ex) {
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

		public void TestRead ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document);
			nrdr.Read ();
			AssertEquals ("<root>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			AssertEquals ("<root>.Name", "root", nrdr.Name);
			AssertEquals ("<root>.ReadState", ReadState.Interactive, nrdr.ReadState);
			AssertEquals ("<root>.Depth", 0, nrdr.Depth);

			// move to 'child'
			nrdr.Read ();
			AssertEquals ("<child/>.Depth", 1, nrdr.Depth);
			AssertEquals ("<child/>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			AssertEquals ("<child/>.Name", "child", nrdr.Name);

			nrdr.Read ();
			AssertEquals ("</root>.Depth", 0, nrdr.Depth);
			AssertEquals ("</root>.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
			AssertEquals ("</root>.Name", "root", nrdr.Name);

			nrdr.Read ();
			AssertEquals ("end.EOF", true, nrdr.EOF);
			AssertEquals ("end.NodeType", XmlNodeType.None, nrdr.NodeType);
		}

		public void TestReadFromElement ()
		{
			XmlNodeReader nrdr = new XmlNodeReader (document.DocumentElement);
			nrdr.Read ();
			AssertEquals ("<root>.NodeType", XmlNodeType.Element, nrdr.NodeType);
			AssertEquals ("<root>.Name", "root", nrdr.Name);
			AssertEquals ("<root>.ReadState", ReadState.Interactive, nrdr.ReadState);
			AssertEquals ("<root>.Depth", 0, nrdr.Depth);
		}

		public void TestReadString ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root>test of <b>mixed</b> string.<![CDATA[ cdata string.]]></root>");
			XmlNodeReader nrdr = new XmlNodeReader (doc);
			nrdr.Read ();
			string s = nrdr.ReadString ();
			AssertEquals ("readString.1.ret_val", "test of ", s);
			AssertEquals ("readString.1.Name", "b", nrdr.Name);
			s = nrdr.ReadString ();
			AssertEquals ("readString.2.ret_val", "mixed", s);
			AssertEquals ("readString.2.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
			s = nrdr.ReadString ();	// never proceeds.
			AssertEquals ("readString.3.ret_val", String.Empty, s);
			AssertEquals ("readString.3.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
			nrdr.Read ();
			AssertEquals ("readString.4.NodeType", XmlNodeType.Text, nrdr.NodeType);
			AssertEquals ("readString.4.Value", " string.", nrdr.Value);
			s = nrdr.ReadString ();	// reads the same Text node.
			AssertEquals ("readString.5.ret_val", " string. cdata string.", s);
			AssertEquals ("readString.5.NodeType", XmlNodeType.EndElement, nrdr.NodeType);
		}

		public void TestRedInnerXml ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root>test of <b>mixed</b> string.</root>");
			XmlNodeReader nrdr = new XmlNodeReader (doc);
			nrdr.ReadInnerXml ();
			AssertEquals ("initial.ReadState", ReadState.Error, nrdr.ReadState);
			AssertEquals ("initial.EOF", true, nrdr.EOF);
			AssertEquals ("initial.NodeType", XmlNodeType.None, nrdr.NodeType);
		}
	}

}
