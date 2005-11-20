//
// CodeDirectiveCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeDirectiveCollection
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Collections;
using System.CodeDom;

namespace MonoTests.System.CodeDom {
	[TestFixture]
	public class CodeDirectiveCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeDirective cd1 = new CodeDirective ();
			CodeDirective cd2 = new CodeDirective ();

			CodeDirective[] directives = new CodeDirective[] { cd1, cd2 };
			CodeDirectiveCollection coll = new CodeDirectiveCollection (
				directives);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cd1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cd2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeDirective[] directives = new CodeDirective[] { 
				new CodeDirective (), null };

			CodeDirectiveCollection coll = new CodeDirectiveCollection (
				directives);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeDirectiveCollection coll = new CodeDirectiveCollection (
				(CodeDirective[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeDirective cd1 = new CodeDirective ();
			CodeDirective cd2 = new CodeDirective ();

			CodeDirectiveCollection c = new CodeDirectiveCollection ();
			c.Add (cd1);
			c.Add (cd2);

			CodeDirectiveCollection coll = new CodeDirectiveCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cd1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cd2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection (
				(CodeDirectiveCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeDirective cd1 = new CodeDirective ();
			CodeDirective cd2 = new CodeDirective ();

			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			Assert.AreEqual (0, coll.Add (cd1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (cd1), "#3");

			Assert.AreEqual (1, coll.Add (cd2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (cd2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Add ((CodeDirective) null);
		}

		[Test]
		public void Insert ()
		{
			CodeDirective cd1 = new CodeDirective ();
			CodeDirective cd2 = new CodeDirective ();

			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Add (cd1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cd1), "#2");
			coll.Insert (0, cd2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (cd1), "#4");
			Assert.AreEqual (0, coll.IndexOf (cd2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Insert (0, (CodeDirective) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeDirective cd1 = new CodeDirective ();
			CodeDirective cd2 = new CodeDirective ();
			CodeDirective cd3 = new CodeDirective ();

			CodeDirectiveCollection coll1 = new CodeDirectiveCollection ();
			coll1.Add (cd1);
			coll1.Add (cd2);

			CodeDirectiveCollection coll2 = new CodeDirectiveCollection ();
			coll2.Add (cd3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (cd1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (cd2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (cd3), "#4");

			CodeDirectiveCollection coll3 = new CodeDirectiveCollection ();
			coll3.Add (cd3);
			coll3.AddRange (new CodeDirective[] { cd1, cd2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (cd1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (cd2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (cd3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.AddRange ((CodeDirective[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.AddRange ((CodeDirectiveCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Add (new CodeDirective ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeDirective cd1 = new CodeDirective ();
			CodeDirective cd2 = new CodeDirective ();

			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Add (cd1);
			coll.Add (cd2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cd1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cd2), "#3");
			coll.Remove (cd1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (cd1), "#5");
			Assert.AreEqual (0, coll.IndexOf (cd2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Remove (new CodeDirective ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeDirectiveCollection coll = new CodeDirectiveCollection ();
			coll.Remove ((CodeDirective) null);
		}
	}
}

#endif

