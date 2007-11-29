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
	public class XNodeEqualityComparerTest
	{
		[Test]
		public void CompareNulls ()
		{
			Assert.IsTrue (XNode.EqualityComparer.Equals (null, null));
		}

		[Test]
//		[ExpectedException (typeof (ArgumentNullException))]
		public void GetHashCodeNull ()
		{
			XNode.EqualityComparer.GetHashCode (null);
		}

		[Test]
		public void Compare1 ()
		{
			XNodeEqualityComparer c = XNode.EqualityComparer;
			XDocument doc = XDocument.Parse ("<root><foo/><bar/><foo/></root>");
			Assert.IsTrue (c.Equals (doc.Root.FirstNode, doc.Root.LastNode), "#1");
			Assert.IsFalse (c.Equals (doc.Root.FirstNode, doc.Root.FirstNode.NextNode), "#2");

			doc = XDocument.Parse ("<root><foo/><foo a='v'/><foo a='v2' /><foo a='v' b='v' /><foo a='v' b='v' /><foo b='v' a='v' /></root>");
			Assert.IsFalse (c.Equals (doc.Root.FirstNode, doc.Root.LastNode.NextNode), "#3");
			Assert.IsFalse (c.Equals (doc.Root.FirstNode, doc.Root.FirstNode.NextNode.NextNode), "#4");
			Assert.IsFalse (c.Equals (doc.Root.FirstNode, doc.Root.LastNode.PreviousNode), "#5");
			// huh?
			Assert.IsFalse (c.Equals (doc.Root.LastNode.PreviousNode, doc.Root.LastNode), "#6");
			Assert.IsTrue (c.Equals (doc.Root.LastNode.PreviousNode.PreviousNode, doc.Root.LastNode.PreviousNode), "#7");
		}

		[Test]
		public void Compare2 ()
		{
			XNodeEqualityComparer c = XNode.EqualityComparer;
			XElement e1 = XElement.Parse ("<foo><bar/></foo>");
			XElement e2 = XElement.Parse ("<foo><bar/></foo>");
			Assert.IsTrue (c.Equals (e1, e2), "#1");
			Assert.IsTrue (c.Equals (e1.FirstNode, e2.FirstNode), "#2");
		}
	}
}
