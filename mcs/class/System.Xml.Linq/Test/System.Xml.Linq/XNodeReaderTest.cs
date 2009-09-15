//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XNodeReaderTest
	{
		[Test]
		public void CreateReader1 ()
		{
			string xml = "<root><foo a='v' /><bar></bar><baz>simple text<!-- comment --><mixed1 /><mixed2><![CDATA[cdata]]><?some-pi with-data ?></mixed2></baz></root>";
			XDocument doc = XDocument.Parse (xml);
			XmlReader xr = doc.CreateReader ();
			StringWriter sw = new StringWriter ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			XmlWriter xw = XmlWriter.Create (sw, s);
			while (!xr.EOF)
				xw.WriteNode (xr, false);
			xw.Close ();
			Assert.AreEqual (xml.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		public void CreateReader2 ()
		{
			string xml = "<?xml version='1.0' encoding='utf-16'?><root><foo a='v' /><bar></bar><baz>simple text<!-- comment --><mixed1 /><mixed2><![CDATA[cdata]]><?some-pi with-data ?></mixed2></baz></root>";
			XDocument doc = XDocument.Parse (xml);
			XmlReader xr = doc.CreateReader ();
			StringWriter sw = new StringWriter ();
			XmlWriter xw = XmlWriter.Create (sw);
			while (!xr.EOF)
				xw.WriteNode (xr, false);
			xw.Close ();
			Assert.AreEqual (xml.Replace ('\'', '"'), sw.ToString ());
		}

		[Test] // almost identical to CreateReader1().
		public void CreateReader3 ()
		{
			string xml = "<root><foo a='v' /><bar></bar><baz>simple text<!-- comment --><mixed1 /><mixed2><![CDATA[cdata]]><?some-pi with-data ?></mixed2></baz></root>";
			XDocument doc = XDocument.Parse (xml, LoadOptions.PreserveWhitespace);
			XmlReader xr = doc.CreateReader ();
			StringWriter sw = new StringWriter ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			XmlWriter xw = XmlWriter.Create (sw, s);
			while (!xr.EOF)
				xw.WriteNode (xr, false);
			xw.Close ();
			Assert.AreEqual (xml.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		public void GetAttribute ()
		{
			// bug #335975
			var rdr = new StringReader ("<root><val>Value</val></root>");
			var xDoc = XDocument.Load (rdr);
			XmlReader r = xDoc.CreateReader ();
			Assert.AreEqual (String.Empty, r.Value, "#1");
			Assert.IsNull (r.GetAttribute ("nil", "http://www.w3.org/2001/XMLSchema-instance"), "#2");
			r.Read ();
			Assert.AreEqual (String.Empty, r.Value, "#3");
			Assert.IsNull (r.GetAttribute ("nil", "http://www.w3.org/2001/XMLSchema-instance"), "#4");

			// bug #335975
			rdr = new StringReader ("<root xmlns='urn:foo'><val>Value</val></root>");
			xDoc = XDocument.Load (rdr);
			r = xDoc.CreateReader ();
			Assert.AreEqual (String.Empty, r.Value, "#5");
			Assert.IsNull (r.GetAttribute ("nil", "http://www.w3.org/2001/XMLSchema-instance"), "#6");
			r.Read ();
			Assert.AreEqual (String.Empty, r.Value, "#7");
			Assert.IsNull (r.GetAttribute ("nil", "http://www.w3.org/2001/XMLSchema-instance"), "#8");
		}

		[Test]
		public void NamespaceURIForXmlns ()
		{
			// bug #356522 (revised test case)
			var rdr = new StringReader ("<root xmlns='XNodeReaderTest.xsd'>Value</root>");
			var xDoc = XDocument.Load (rdr);
			var reader = xDoc.CreateReader ();
			reader.MoveToContent ();
			reader.MoveToFirstAttribute ();
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", reader.NamespaceURI);
		}

		[Test]
		public void CreateReaderFromElement ()
		{
			// bug #356522 (another revised test case)
			var rdr = new StringReader ("<root xmlns='XNodeReaderTest.xsd'>Value</root>");
			var xDoc = XDocument.Load (rdr);
			var reader = xDoc.Root.CreateReader ();
			reader.Read (); // do not move to text (Value)
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType);
		}

		[Test]
		public void CreateReaderFromElement2 ()
		{
			// bug #356522 (another revised test case)
			var rdr = new StringReader ("<root><foo xmlns='urn:foo'>Value</foo><bar /></root>");
			var xDoc = XDocument.Load (rdr);
			var reader = xDoc.Root.FirstNode.CreateReader ();
			reader.Read (); // <foo>
			reader.Read (); // Value
			reader.Read (); // </foo>
			Assert.IsFalse (reader.Read ()); // do not move to <bar>
		}

		[Test]
		public void NodeTypeAtInitialStateIsNone ()
		{
			// bug #356522 (another revised test case)
			var rdr = new StringReader ("<root xmlns='XNodeReaderTest.xsd'>Value</root>");
			var xDoc = XDocument.Load (rdr);
			var reader = xDoc.Root.CreateReader ();
			Assert.AreEqual (XmlNodeType.None, reader.NodeType, "#1");
			reader.MoveToContent (); // do Read() and shift state to Interactive.
			Assert.AreEqual (true, reader.MoveToFirstAttribute (), "#2");
		}

		[Test]
		public void IsEmptyElement ()
		{
			string xml = @"<Dummy xmlns='http://example.com/schemas/asx' />";
			XElement element = XElement.Parse (xml);
			var r = element.CreateReader ();
			r.Read ();
			r.MoveToAttribute (0);
			Assert.IsFalse (r.IsEmptyElement);
		}

		[Test]
		public void PrefixOnEmptyNS ()
		{
			string xml = @"<Dummy xmlns='http://example.com/schemas/asx' />";
			XElement element = XElement.Parse (xml);
			var r = element.CreateReader ();
			r.Read ();
			Assert.AreEqual (String.Empty, r.Prefix);
		}

		[Test]
		public void NamePropsOnEOF ()
		{
			string xml = @"<Dummy xmlns='http://example.com/schemas/asx' />";
			XElement element = XElement.Parse (xml);
			var r = element.CreateReader ();
			r.Read ();
			r.Read ();
			Assert.AreEqual (String.Empty, r.LocalName, "#1");
			Assert.AreEqual (String.Empty, r.NamespaceURI, "#2");
			Assert.AreEqual (0, r.Depth, "#3");
			Assert.IsFalse (r.HasAttributes, "#4");
		}

		[Test]
		public void LookupNamespace ()
		{
			string xml = @"<Dummy xmlns='http://example.com/schemas/asx' />";
			XElement element = XElement.Parse (xml);
			var r = element.CreateReader ();
			r.Read ();
			Assert.AreEqual ("http://example.com/schemas/asx", r.LookupNamespace (String.Empty), "#1");
			Assert.IsNull (r.LookupNamespace ("nonexistent"), "#2");
			r.Read ();
			Assert.IsNull (r.LookupNamespace (String.Empty), "#3");
		}
	}
}
