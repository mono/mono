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
	public class XObjectTest
	{
		[Test]
		public void Annotations ()
		{
			Assert.IsNull (new XDocument ().Annotation (typeof (object)));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadFromInitial ()
		{
			// must be Interactive
			XNode.ReadFrom (XmlReader.Create (new StringReader ("<root />")));
		}

		[Test]
		public void ReadFromElement ()
		{
			var r = XmlReader.Create (new StringReader ("<root><foo/></root>"));
			r.MoveToContent ();
			XElement el = XNode.ReadFrom (r) as XElement;
			Assert.IsNotNull (el, "#1");
			Assert.IsFalse (((IXmlLineInfo) el).HasLineInfo (), "#2");
		}

		[Test]
		public void LineInfo ()
		{
			// must be Interactive
			var r = XmlReader.Create (new StringReader ("<root><foo x='y'/></root>"));
			XElement el = XElement.Load (r, LoadOptions.SetLineInfo) as XElement;
			IXmlLineInfo li = el as IXmlLineInfo;
			Assert.AreEqual (1, li.LineNumber, "#1");
			Assert.AreEqual (2, li.LinePosition, "#2");
			li = el.FirstNode as IXmlLineInfo;
			Assert.AreEqual (1, li.LineNumber, "#3");
			Assert.AreEqual (8, li.LinePosition, "#4");
			li = ((XElement) el.FirstNode).FirstAttribute as IXmlLineInfo;
			Assert.AreEqual (1, li.LineNumber, "#5");
			Assert.AreEqual (12, li.LinePosition, "#6");
		}
	}
}
