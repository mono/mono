//
// CodeParameterDeclarationExpressionCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeParameterDeclarationExpressionCollection
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
	public class CodeParameterDeclarationExpressionCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression ();

			CodeParameterDeclarationExpression[] parameters = new CodeParameterDeclarationExpression[] { param1, param2 };
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection (
				parameters);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (param1), "#2");
			Assert.AreEqual (1, coll.IndexOf (param2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeParameterDeclarationExpression[] parameters = new CodeParameterDeclarationExpression[] { 
				new CodeParameterDeclarationExpression (), null };

			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection (
				parameters);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection (
				(CodeParameterDeclarationExpression[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression ();

			CodeParameterDeclarationExpressionCollection c = new CodeParameterDeclarationExpressionCollection ();
			c.Add (param1);
			c.Add (param2);

			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (param1), "#2");
			Assert.AreEqual (1, coll.IndexOf (param2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection (
				(CodeParameterDeclarationExpressionCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression ();

			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			Assert.AreEqual (0, coll.Add (param1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (param1), "#3");

			Assert.AreEqual (1, coll.Add (param2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (param2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Add ((CodeParameterDeclarationExpression) null);
		}

		[Test]
		public void Insert ()
		{
			CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression ();

			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Add (param1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (param1), "#2");
			coll.Insert (0, param2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (param1), "#4");
			Assert.AreEqual (0, coll.IndexOf (param2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Insert (0, (CodeParameterDeclarationExpression) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression param3 = new CodeParameterDeclarationExpression ();

			CodeParameterDeclarationExpressionCollection coll1 = new CodeParameterDeclarationExpressionCollection ();
			coll1.Add (param1);
			coll1.Add (param2);

			CodeParameterDeclarationExpressionCollection coll2 = new CodeParameterDeclarationExpressionCollection ();
			coll2.Add (param3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (param1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (param2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (param3), "#4");

			CodeParameterDeclarationExpressionCollection coll3 = new CodeParameterDeclarationExpressionCollection ();
			coll3.Add (param3);
			coll3.AddRange (new CodeParameterDeclarationExpression[] { param1, param2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (param1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (param2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (param3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.AddRange ((CodeParameterDeclarationExpression[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.AddRange (new CodeParameterDeclarationExpression[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.AddRange ((CodeParameterDeclarationExpressionCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Add (new CodeParameterDeclarationExpression ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeParameterDeclarationExpression cpde1 = new CodeParameterDeclarationExpression ();
			CodeParameterDeclarationExpression cpde2 = new CodeParameterDeclarationExpression ();

			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Add (cpde1);
			coll.Add (cpde2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cpde1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cpde2), "#3");
			coll.Remove (cpde1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (cpde1), "#5");
			Assert.AreEqual (0, coll.IndexOf (cpde2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Remove (new CodeParameterDeclarationExpression ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeParameterDeclarationExpressionCollection coll = new CodeParameterDeclarationExpressionCollection ();
			coll.Remove ((CodeParameterDeclarationExpression) null);
		}
	}
}
