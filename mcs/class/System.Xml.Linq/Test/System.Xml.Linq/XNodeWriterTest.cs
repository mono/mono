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

using XPI = System.Xml.Linq.XProcessingInstruction;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XNodeWriterTest
	{
		[Test]
		public void WriteEmptyElements ()
		{
			var doc = new XDocument ();
			XmlWriter w = doc.CreateWriter ();
			w.WriteStartElement ("root");
			w.WriteStartElement ("foo");
			w.WriteEndElement ();
			w.WriteStartElement ("bar");
			w.WriteFullEndElement ();
			w.WriteEndElement ();
			w.Close ();
			Assert.IsTrue (((XElement) doc.Root.FirstNode).IsEmpty, "#1");
			Assert.IsFalse (((XElement) doc.Root.LastNode).IsEmpty, "#2");
		}

		[Test]
		public void CreateWriter1 ()
		{
			string xml = "<root><foo/><bar></bar><baz a='v' xmlns='urn:foo' xmlns:x='urn:x'><x:ext xmlns=''>test</x:ext><!-- comment -->  <?some-pi some-data?></baz></root>";
			XDocument doc = new XDocument ();
			XmlWriter xw = doc.CreateWriter ();
			XmlReader xr = XmlReader.Create (new StringReader (xml));
			while (!xr.EOF)
				xw.WriteNode (xr, false);
			xw.Close ();

			Assert.AreEqual ("root", doc.Root.Name.LocalName, "#1");
			XElement el = doc.Root.FirstNode as XElement;
			Assert.AreEqual ("foo", el.Name.LocalName, "#2-1");
			Assert.IsTrue (el.IsEmpty, "#2-2");
			Assert.IsFalse (el.HasAttributes, "#2-3");
			el = el.NextNode as XElement;
			Assert.IsFalse (el.IsEmpty, "#3");
			el = el.NextNode as XElement;
			Assert.AreEqual ("a", el.FirstAttribute.Name.LocalName, "#4-1");
			Assert.AreEqual ("xmlns", el.FirstAttribute.NextAttribute.Name.LocalName, "#4-2");
			Assert.AreEqual ("x", el.LastAttribute.Name.LocalName, "#4-3");
			Assert.AreEqual (XNamespace.Xmlns, el.LastAttribute.Name.Namespace, "#4-4");
			el = el.FirstNode as XElement;
			// <x:ext
			Assert.AreEqual ("ext", el.Name.LocalName, "#5-1");
			Assert.AreEqual (XNamespace.Get ("urn:x"), el.Name.Namespace, "#5-2");
			// xmlns=''
			Assert.AreEqual ("xmlns", el.FirstAttribute.Name.LocalName, "#5-3");
			Assert.AreEqual (XNamespace.Get (String.Empty), el.FirstAttribute.Name.Namespace, "#5-4");
			XText t = el.FirstNode as XText;
			Assert.AreEqual ("test", t.Value, "#6");
			XComment c = el.NextNode as XComment;
			Assert.AreEqual (" comment ", c.Value, "#7");
			t = c.NextNode as XText;
			Assert.AreEqual ("  ", t.Value, "#8");
			XPI pi = t.NextNode as XPI;
			Assert.AreEqual ("some-pi", pi.Target, "#9-1");
			Assert.AreEqual ("some-data", pi.Data, "#9-2");
			Assert.IsNull (el.Parent.NextNode, "#10");
			Assert.IsNull (el.Parent.Parent.NextNode, "#11");
		}
	}
}
