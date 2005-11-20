//
// CodeExpressionCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeExpressionCollection
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
	public class CodeExpressionCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeExpression exp1 = new CodeExpression ();
			CodeExpression exp2 = new CodeExpression ();

			CodeExpression[] expressions = new CodeExpression[] { exp1, exp2 };
			CodeExpressionCollection coll = new CodeExpressionCollection (
				expressions);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (exp1), "#2");
			Assert.AreEqual (1, coll.IndexOf (exp2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeExpression[] expressions = new CodeExpression[] { 
				new CodeExpression (), null };

			CodeExpressionCollection coll = new CodeExpressionCollection (
				expressions);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeExpressionCollection coll = new CodeExpressionCollection (
				(CodeExpression[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeExpression exp1 = new CodeExpression ();
			CodeExpression exp2 = new CodeExpression ();

			CodeExpressionCollection c = new CodeExpressionCollection ();
			c.Add (exp1);
			c.Add (exp2);

			CodeExpressionCollection coll = new CodeExpressionCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (exp1), "#2");
			Assert.AreEqual (1, coll.IndexOf (exp2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection (
				(CodeExpressionCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeExpression exp1 = new CodeExpression ();
			CodeExpression exp2 = new CodeExpression ();

			CodeExpressionCollection coll = new CodeExpressionCollection ();
			Assert.AreEqual (0, coll.Add (exp1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (exp1), "#3");

			Assert.AreEqual (1, coll.Add (exp2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (exp2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Add ((CodeExpression) null);
		}

		[Test]
		public void Insert ()
		{
			CodeExpression exp1 = new CodeExpression ();
			CodeExpression exp2 = new CodeExpression ();

			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Add (exp1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (exp1), "#2");
			coll.Insert (0, exp2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (exp1), "#4");
			Assert.AreEqual (0, coll.IndexOf (exp2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Insert (0, (CodeExpression) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeExpression exp1 = new CodeExpression ();
			CodeExpression exp2 = new CodeExpression ();
			CodeExpression exp3 = new CodeExpression ();

			CodeExpressionCollection coll1 = new CodeExpressionCollection ();
			coll1.Add (exp1);
			coll1.Add (exp2);

			CodeExpressionCollection coll2 = new CodeExpressionCollection ();
			coll2.Add (exp3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (exp1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (exp2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (exp3), "#4");

			CodeExpressionCollection coll3 = new CodeExpressionCollection ();
			coll3.Add (exp3);
			coll3.AddRange (new CodeExpression[] { exp1, exp2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (exp1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (exp2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (exp3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.AddRange ((CodeExpression[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.AddRange ((CodeExpressionCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Add (new CodeExpression ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeExpression ce1 = new CodeExpression ();
			CodeExpression ce2 = new CodeExpression ();

			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Add (ce1);
			coll.Add (ce2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ce1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ce2), "#3");
			coll.Remove (ce1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (ce1), "#5");
			Assert.AreEqual (0, coll.IndexOf (ce2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Remove (new CodeExpression ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeExpressionCollection coll = new CodeExpressionCollection ();
			coll.Remove ((CodeExpression) null);
		}
	}
}
