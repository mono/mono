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
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XNodeDocumentOrderComparerTest
	{
		[Test]
		public void CompareNulls ()
		{
			Assert.AreEqual (0, XNode.DocumentOrderComparer.Compare (null, null));
		}

		[Test]
		public void Compare1 ()
		{
			// ancestors/descendants
			XNodeDocumentOrderComparer c = XNode.DocumentOrderComparer;
			XElement el = XElement.Parse ("<foo><bar/></foo>");
			Assert.IsTrue (c.Compare (el, el.FirstNode) < 0, "#1-1");
			Assert.IsTrue (c.Compare (el.FirstNode, el) > 0, "#1-2");

			XDocument doc = XDocument.Parse ("<foo><bar/></foo>");
			Assert.IsTrue (c.Compare (doc, doc.FirstNode) < 0, "#2-1");
			Assert.IsTrue (c.Compare (doc.FirstNode, doc) > 0, "#2-2");

			el = XDocument.Parse ("<root><foo><f1/><f2/></foo><bar/></root>").Root;
			XElement e2 = el.FirstNode as XElement;
			Assert.IsTrue (c.Compare (e2, e2.FirstNode) < 0, "#3-1");
			Assert.IsTrue (c.Compare (e2, e2.LastNode) < 0, "#3-2");
		}

		[Test]
		public void Compare2 ()
		{
			// sibling/following/preceding
			XNodeDocumentOrderComparer c = XNode.DocumentOrderComparer;
			XElement el = XElement.Parse ("<n1><n11><n111/><n112/></n11><n12><n121><n1211/><n1212/></n121></n12></n1>");
			Assert.IsTrue (c.Compare (el.FirstNode, el.LastNode) < 0, "#3-1"); // following-sibling
			Assert.IsTrue (c.Compare (el.LastNode, el.FirstNode) > 0, "#3-2"); // preceding-sibling
			Assert.IsTrue (c.Compare (el.FirstNode, ((XContainer) el.LastNode).FirstNode) < 0, "#3-3"); // following
			Assert.IsTrue (c.Compare (((XContainer) el.LastNode).FirstNode, el.FirstNode) > 0, "#3-4"); // preceding
		}
	}
}
