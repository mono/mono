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
using System.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XTextTest
	{
		[Test]
		public void NodeType ()
		{
			Assert.AreEqual (XmlNodeType.Text, new XText ("test").NodeType, "#1");
			Assert.AreEqual (XmlNodeType.Text, new XText ("    ").NodeType, "#2");
		}

		[Test]
		public void ToString ()
		{
			Assert.AreEqual ("Foo", new XText ("Foo").ToString ());
		}

		[Test]
		public void AddXTextElementCloning ()
		{
			XDocument document = new XDocument (new XElement ("root", "This is the root"));
			Assert.IsNotNull (document);
			Assert.IsNotNull (document.Elements ().First ());

			XDocument newDocument = new XDocument (document.Root);
			Assert.IsNotNull (newDocument);
			Assert.IsNotNull (newDocument.Elements ().First ());
		}
		
		[Test]
		public void WriteWhitespaceToXml ()
		{
			var doc = new XDocument (new XText ("\n"), new XElement ("root"));
			doc.Save (TextWriter.Null);
		}
	}
}
