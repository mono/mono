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
			AssertEquals ("initial.ReadState", ReadState.Error, nrdr.ReadState);
			AssertEquals ("initial.EOF", true, nrdr.EOF);
			AssertEquals ("initial.NodeType", XmlNodeType.None, nrdr.NodeType);
		}

	}

}
