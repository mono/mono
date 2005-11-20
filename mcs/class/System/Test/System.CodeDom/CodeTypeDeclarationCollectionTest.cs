//
// CodeTypeDeclarationCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeTypeDeclarationCollection
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
	public class CodeTypeDeclarationCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeDeclaration td1 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td2 = new CodeTypeDeclaration ();

			CodeTypeDeclaration[] declarations = new CodeTypeDeclaration[] { td1, td2 };
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection (
				declarations);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (td1), "#2");
			Assert.AreEqual (1, coll.IndexOf (td2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeTypeDeclaration[] declarations = new CodeTypeDeclaration[] { 
				new CodeTypeDeclaration (), null };

			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection (
				declarations);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection (
				(CodeTypeDeclaration[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeTypeDeclaration td1 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td2 = new CodeTypeDeclaration ();

			CodeTypeDeclarationCollection c = new CodeTypeDeclarationCollection ();
			c.Add (td1);
			c.Add (td2);

			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (td1), "#2");
			Assert.AreEqual (1, coll.IndexOf (td2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection (
				(CodeTypeDeclarationCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeTypeDeclaration td1 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td2 = new CodeTypeDeclaration ();

			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			Assert.AreEqual (0, coll.Add (td1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (td1), "#3");

			Assert.AreEqual (1, coll.Add (td2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (td2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Add ((CodeTypeDeclaration) null);
		}

		[Test]
		public void Insert ()
		{
			CodeTypeDeclaration td1 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td2 = new CodeTypeDeclaration ();

			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Add (td1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (td1), "#2");
			coll.Insert (0, td2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (td1), "#4");
			Assert.AreEqual (0, coll.IndexOf (td2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Insert (0, (CodeTypeDeclaration) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeTypeDeclaration td1 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td2 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td3 = new CodeTypeDeclaration ();

			CodeTypeDeclarationCollection coll1 = new CodeTypeDeclarationCollection ();
			coll1.Add (td1);
			coll1.Add (td2);

			CodeTypeDeclarationCollection coll2 = new CodeTypeDeclarationCollection ();
			coll2.Add (td3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (td1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (td2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (td3), "#4");

			CodeTypeDeclarationCollection coll3 = new CodeTypeDeclarationCollection ();
			coll3.Add (td3);
			coll3.AddRange (new CodeTypeDeclaration[] { td1, td2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (td1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (td2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (td3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.AddRange ((CodeTypeDeclaration[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.AddRange (new CodeTypeDeclaration[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.AddRange ((CodeTypeDeclarationCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Add (new CodeTypeDeclaration ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeTypeDeclaration td1 = new CodeTypeDeclaration ();
			CodeTypeDeclaration td2 = new CodeTypeDeclaration ();

			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Add (td1);
			coll.Add (td2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (td1), "#2");
			Assert.AreEqual (1, coll.IndexOf (td2), "#3");
			coll.Remove (td1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (td1), "#5");
			Assert.AreEqual (0, coll.IndexOf (td2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Remove (new CodeTypeDeclaration ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeTypeDeclarationCollection coll = new CodeTypeDeclarationCollection ();
			coll.Remove ((CodeTypeDeclaration) null);
		}
	}
}
