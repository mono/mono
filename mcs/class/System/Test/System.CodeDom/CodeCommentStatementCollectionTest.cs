//
// CodeCommentStatementCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeCommentStatementCollection
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
	public class CodeCommentStatementCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeCommentStatement ccs1 = new CodeCommentStatement ();
			CodeCommentStatement ccs2 = new CodeCommentStatement ();

			CodeCommentStatement[] statements = new CodeCommentStatement[] { ccs1, ccs2 };
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection (
				statements);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ccs1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ccs2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeCommentStatement[] statements = new CodeCommentStatement[] { 
				new CodeCommentStatement (), null };

			CodeCommentStatementCollection coll = new CodeCommentStatementCollection (
				statements);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection (
				(CodeCommentStatement[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeCommentStatement ccs1 = new CodeCommentStatement ();
			CodeCommentStatement ccs2 = new CodeCommentStatement ();

			CodeCommentStatementCollection c = new CodeCommentStatementCollection ();
			c.Add (ccs1);
			c.Add (ccs2);

			CodeCommentStatementCollection coll = new CodeCommentStatementCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ccs1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ccs2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection (
				(CodeCommentStatementCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeCommentStatement ccs1 = new CodeCommentStatement ();
			CodeCommentStatement ccs2 = new CodeCommentStatement ();

			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			Assert.AreEqual (0, coll.Add (ccs1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (ccs1), "#3");

			Assert.AreEqual (1, coll.Add (ccs2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (ccs2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Add ((CodeCommentStatement) null);
		}

		[Test]
		public void Insert ()
		{
			CodeCommentStatement ccs1 = new CodeCommentStatement ();
			CodeCommentStatement ccs2 = new CodeCommentStatement ();

			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Add (ccs1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ccs1), "#2");
			coll.Insert (0, ccs2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (ccs1), "#4");
			Assert.AreEqual (0, coll.IndexOf (ccs2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Insert (0, (CodeCommentStatement) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeCommentStatement ccs1 = new CodeCommentStatement ();
			CodeCommentStatement ccs2 = new CodeCommentStatement ();
			CodeCommentStatement ccs3 = new CodeCommentStatement ();

			CodeCommentStatementCollection coll1 = new CodeCommentStatementCollection ();
			coll1.Add (ccs1);
			coll1.Add (ccs2);

			CodeCommentStatementCollection coll2 = new CodeCommentStatementCollection ();
			coll2.Add (ccs3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (ccs1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (ccs2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (ccs3), "#4");

			CodeCommentStatementCollection coll3 = new CodeCommentStatementCollection ();
			coll3.Add (ccs3);
			coll3.AddRange (new CodeCommentStatement[] { ccs1, ccs2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (ccs1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (ccs2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (ccs3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.AddRange ((CodeCommentStatement[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.AddRange (new CodeNamespace[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.AddRange ((CodeCommentStatementCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Add (new CodeCommentStatement ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeCommentStatement ccs1 = new CodeCommentStatement ();
			CodeCommentStatement ccs2 = new CodeCommentStatement ();

			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Add (ccs1);
			coll.Add (ccs2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ccs1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ccs2), "#3");
			coll.Remove (ccs1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (ccs1), "#5");
			Assert.AreEqual (0, coll.IndexOf (ccs2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Remove (new CodeCommentStatement ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeCommentStatementCollection coll = new CodeCommentStatementCollection ();
			coll.Remove ((CodeCommentStatement) null);
		}
	}
}
