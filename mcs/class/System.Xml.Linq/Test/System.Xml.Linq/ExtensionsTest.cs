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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class ExtensionsTest
	{
/* It does not compile probably due to bug #359733.
		[Test]
		public void Remove ()
		{
			XDocument doc = XDocument.Parse ("<root><foo/><bar/><baz/></root>");
			doc.Root.Nodes ().Remove<XNode> ();
			Assert.IsNull (doc.Root.FirstNode, "#1");
		}
*/

		[Test]
		public void InDocumentOrder ()
		{
			XElement el = XDocument.Parse ("<root><foo><f1/><f2/></foo><bar/></root>").Root;
			XElement c = el.FirstNode as XElement;
			int n = 0;
			string [] names = {"foo", "f1", "f2", "bar"};
			foreach (XElement e2 in new XNode [] {el.LastNode, c.LastNode, c.FirstNode, c}.InDocumentOrder ())
				Assert.AreEqual (names [n], e2.Name.LocalName, "#" + n++);
		}
	}
}
