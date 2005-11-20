//
// CodeAttributeArgumentCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeAttributeArgumentCollection
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.Collections;
using System.CodeDom;

namespace MonoTests.System.CodeDom {
	[TestFixture]
	public class CodeAttributeArgumentCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeAttributeArgument caa1 = new CodeAttributeArgument ();
			CodeAttributeArgument caa2 = new CodeAttributeArgument ();

			CodeAttributeArgument[] arguments = new CodeAttributeArgument[] { caa1, caa2 };
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection (
				arguments);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (caa1), "#2");
			Assert.AreEqual (1, coll.IndexOf (caa2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeAttributeArgument[] arguments = new CodeAttributeArgument[] { 
				new CodeAttributeArgument (), null };

			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection (
				arguments);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection (
				(CodeAttributeArgument[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeAttributeArgument caa1 = new CodeAttributeArgument ();
			CodeAttributeArgument caa2 = new CodeAttributeArgument ();

			CodeAttributeArgumentCollection c = new CodeAttributeArgumentCollection ();
			c.Add (caa1);
			c.Add (caa2);

			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (caa1), "#2");
			Assert.AreEqual (1, coll.IndexOf (caa2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection (
				(CodeAttributeArgumentCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeAttributeArgument caa1 = new CodeAttributeArgument ();
			CodeAttributeArgument caa2 = new CodeAttributeArgument ();

			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			Assert.AreEqual (0, coll.Add (caa1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (caa1), "#3");

			Assert.AreEqual (1, coll.Add (caa2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (caa2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Add ((CodeAttributeArgument) null);
		}

		[Test]
		public void Insert ()
		{
			CodeAttributeArgument caa1 = new CodeAttributeArgument ();
			CodeAttributeArgument caa2 = new CodeAttributeArgument ();

			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Add (caa1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (caa1), "#2");
			coll.Insert (0, caa2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (caa1), "#4");
			Assert.AreEqual (0, coll.IndexOf (caa2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Insert (0, (CodeAttributeArgument) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeAttributeArgument caa1 = new CodeAttributeArgument ();
			CodeAttributeArgument caa2 = new CodeAttributeArgument ();
			CodeAttributeArgument caa3 = new CodeAttributeArgument ();

			CodeAttributeArgumentCollection coll1 = new CodeAttributeArgumentCollection ();
			coll1.Add (caa1);
			coll1.Add (caa2);

			CodeAttributeArgumentCollection coll2 = new CodeAttributeArgumentCollection();
			coll2.Add (caa3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (caa1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (caa2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (caa3), "#4");

			CodeAttributeArgumentCollection coll3 = new CodeAttributeArgumentCollection();
			coll3.Add (caa3);
			coll3.AddRange (new CodeAttributeArgument[] {caa1, caa2});
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (caa1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (caa2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (caa3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.AddRange ((CodeAttributeArgument[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.AddRange (new CodeAttributeArgument[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.AddRange ((CodeAttributeArgumentCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Add (new CodeAttributeArgument ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeAttributeArgument caa1 = new CodeAttributeArgument ();
			CodeAttributeArgument caa2 = new CodeAttributeArgument ();

			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Add (caa1);
			coll.Add (caa2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (caa1), "#2");
			Assert.AreEqual (1, coll.IndexOf (caa2), "#3");
			coll.Remove (caa1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (caa1), "#5");
			Assert.AreEqual (0, coll.IndexOf (caa2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Remove (new CodeAttributeArgument ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeAttributeArgumentCollection coll = new CodeAttributeArgumentCollection ();
			coll.Remove ((CodeAttributeArgument) null);
		}
	}
}
