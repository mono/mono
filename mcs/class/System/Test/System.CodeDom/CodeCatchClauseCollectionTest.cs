//
// CodeCatchClauseCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeCatchClauseCollection
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
	public class CodeCatchClauseCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeCatchClause cc1 = new CodeCatchClause ();
			CodeCatchClause cc2 = new CodeCatchClause ();

			CodeCatchClause[] catchClauses = new CodeCatchClause[] { cc1, cc2 };
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection (
				catchClauses);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cc1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cc2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeCatchClause[] catchClauses = new CodeCatchClause[] { 
				new CodeCatchClause (), null };

			CodeCatchClauseCollection coll = new CodeCatchClauseCollection (
				catchClauses);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection (
				(CodeCatchClause[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeCatchClause cc1 = new CodeCatchClause ();
			CodeCatchClause cc2 = new CodeCatchClause ();

			CodeCatchClauseCollection c = new CodeCatchClauseCollection ();
			c.Add (cc1);
			c.Add (cc2);

			CodeCatchClauseCollection coll = new CodeCatchClauseCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cc1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cc2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection (
				(CodeCatchClauseCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeCatchClause cc1 = new CodeCatchClause ();
			CodeCatchClause cc2 = new CodeCatchClause ();

			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			Assert.AreEqual (0, coll.Add (cc1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (cc1), "#3");

			Assert.AreEqual (1, coll.Add (cc2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (cc2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Add ((CodeCatchClause) null);
		}

		[Test]
		public void Insert ()
		{
			CodeCatchClause cc1 = new CodeCatchClause ();
			CodeCatchClause cc2 = new CodeCatchClause ();

			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Add (cc1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cc1), "#2");
			coll.Insert (0, cc2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (cc1), "#4");
			Assert.AreEqual (0, coll.IndexOf (cc2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Insert (0, (CodeCatchClause) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeCatchClause cc1 = new CodeCatchClause ();
			CodeCatchClause cc2 = new CodeCatchClause ();
			CodeCatchClause cc3 = new CodeCatchClause ();

			CodeCatchClauseCollection coll1 = new CodeCatchClauseCollection ();
			coll1.Add (cc1);
			coll1.Add (cc2);

			CodeCatchClauseCollection coll2 = new CodeCatchClauseCollection ();
			coll2.Add (cc3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (cc1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (cc2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (cc3), "#4");

			CodeCatchClauseCollection coll3 = new CodeCatchClauseCollection ();
			coll3.Add (cc3);
			coll3.AddRange (new CodeCatchClause[] { cc1, cc2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (cc1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (cc2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (cc3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.AddRange ((CodeCatchClause[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.AddRange (new CodeCatchClause[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.AddRange ((CodeCatchClauseCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Add (new CodeCatchClause ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeCatchClause ccc1 = new CodeCatchClause ();
			CodeCatchClause ccc2 = new CodeCatchClause ();

			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Add (ccc1);
			coll.Add (ccc2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ccc1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ccc2), "#3");
			coll.Remove (ccc1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (ccc1), "#5");
			Assert.AreEqual (0, coll.IndexOf (ccc2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Remove (new CodeCatchClause ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeCatchClauseCollection coll = new CodeCatchClauseCollection ();
			coll.Remove ((CodeCatchClause) null);
		}
	}
}
