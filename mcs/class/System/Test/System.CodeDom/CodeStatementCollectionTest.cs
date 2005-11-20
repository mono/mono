//
// CodeStatementCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeStatementCollection
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
	public class CodeStatementCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();

			CodeStatement[] statements = new CodeStatement[] { cs1, cs2 };
			CodeStatementCollection coll = new CodeStatementCollection (
				statements);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cs1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cs2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeStatement[] statements = new CodeStatement[] { 
				new CodeStatement (), null };

			CodeStatementCollection coll = new CodeStatementCollection (
				statements);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeStatementCollection coll = new CodeStatementCollection (
				(CodeStatement[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();

			CodeStatementCollection c = new CodeStatementCollection ();
			c.Add (cs1);
			c.Add (cs2);

			CodeStatementCollection coll = new CodeStatementCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cs1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cs2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeStatementCollection coll = new CodeStatementCollection (
				(CodeStatementCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();

			CodeStatementCollection coll = new CodeStatementCollection ();
			Assert.AreEqual (0, coll.Add (cs1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (cs1), "#3");

			Assert.AreEqual (1, coll.Add (cs2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (cs2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Add ((CodeStatement) null);
		}

		[Test]
		public void Insert ()
		{
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();

			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Add (cs1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cs1), "#2");
			coll.Insert (0, cs2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (cs1), "#4");
			Assert.AreEqual (0, coll.IndexOf (cs2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Insert (0, (CodeStatement) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();
			CodeStatement cs3 = new CodeStatement ();

			CodeStatementCollection coll1 = new CodeStatementCollection ();
			coll1.Add (cs1);
			coll1.Add (cs2);

			CodeStatementCollection coll2 = new CodeStatementCollection ();
			coll2.Add (cs3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (cs1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (cs2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (cs3), "#4");

			CodeStatementCollection coll3 = new CodeStatementCollection ();
			coll3.Add (cs3);
			coll3.AddRange (new CodeStatement[] { cs1, cs2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (cs1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (cs2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (cs3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.AddRange ((CodeStatement[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.AddRange (new CodeStatement[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.AddRange ((CodeStatementCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Add (new CodeStatement ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeStatement cs1 = new CodeStatement ();
			CodeStatement cs2 = new CodeStatement ();

			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Add (cs1);
			coll.Add (cs2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cs1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cs2), "#3");
			coll.Remove (cs1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (cs1), "#5");
			Assert.AreEqual (0, coll.IndexOf (cs2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Remove (new CodeStatement ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeStatementCollection coll = new CodeStatementCollection ();
			coll.Remove ((CodeStatement) null);
		}
	}
}
