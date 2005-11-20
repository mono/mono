//
// CodeTypeParameterCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeTypeParameterCollection
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
	public class CodeTypeParameterCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeParameter tp1 = new CodeTypeParameter ();
			CodeTypeParameter tp2 = new CodeTypeParameter ();

			CodeTypeParameter[] typeParams = new CodeTypeParameter[] { tp1, tp2 };
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection (
				typeParams);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (tp1), "#2");
			Assert.AreEqual (1, coll.IndexOf (tp2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeTypeParameter[] typeParams = new CodeTypeParameter[] { 
				new CodeTypeParameter (), null };

			CodeTypeParameterCollection coll = new CodeTypeParameterCollection (
				typeParams);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection (
				(CodeTypeParameter[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeTypeParameter tp1 = new CodeTypeParameter ();
			CodeTypeParameter tp2 = new CodeTypeParameter ();

			CodeTypeParameterCollection c = new CodeTypeParameterCollection ();
			c.Add (tp1);
			c.Add (tp2);

			CodeTypeParameterCollection coll = new CodeTypeParameterCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (tp1), "#2");
			Assert.AreEqual (1, coll.IndexOf (tp2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection (
				(CodeTypeParameterCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeTypeParameter tp1 = new CodeTypeParameter ();
			CodeTypeParameter tp2 = new CodeTypeParameter ();

			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			Assert.AreEqual (0, coll.Add (tp1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (tp1), "#3");

			Assert.AreEqual (1, coll.Add (tp2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (tp2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Add ((CodeTypeParameter) null);
		}

		[Test]
		public void Insert ()
		{
			CodeTypeParameter tp1 = new CodeTypeParameter ();
			CodeTypeParameter tp2 = new CodeTypeParameter ();

			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Add (tp1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (tp1), "#2");
			coll.Insert (0, tp2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (tp1), "#4");
			Assert.AreEqual (0, coll.IndexOf (tp2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Insert (0, (CodeTypeParameter) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeTypeParameter tp1 = new CodeTypeParameter ();
			CodeTypeParameter tp2 = new CodeTypeParameter ();
			CodeTypeParameter tp3 = new CodeTypeParameter ();

			CodeTypeParameterCollection coll1 = new CodeTypeParameterCollection ();
			coll1.Add (tp1);
			coll1.Add (tp2);

			CodeTypeParameterCollection coll2 = new CodeTypeParameterCollection ();
			coll2.Add (tp3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (tp1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (tp2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (tp3), "#4");

			CodeTypeParameterCollection coll3 = new CodeTypeParameterCollection ();
			coll3.Add (tp3);
			coll3.AddRange (new CodeTypeParameter[] { tp1, tp2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (tp1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (tp2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (tp3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.AddRange ((CodeTypeParameter[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.AddRange ((CodeTypeParameterCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Add (new CodeTypeParameter ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeTypeParameter ctp1 = new CodeTypeParameter ();
			CodeTypeParameter ctp2 = new CodeTypeParameter ();

			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Add (ctp1);
			coll.Add (ctp2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ctp1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ctp2), "#3");
			coll.Remove (ctp1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (ctp1), "#5");
			Assert.AreEqual (0, coll.IndexOf (ctp2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Remove (new CodeTypeParameter ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeTypeParameterCollection coll = new CodeTypeParameterCollection ();
			coll.Remove ((CodeTypeParameter) null);
		}
	}
}

#endif
