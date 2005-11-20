//
// CodeAttributeDeclarationCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeAttributeDeclarationCollection
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
	public class CodeAttributeDeclarationCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeAttributeDeclaration cad1 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad2 = new CodeAttributeDeclaration ();

			CodeAttributeDeclaration[] declarations = new CodeAttributeDeclaration[] { cad1, cad2 };
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection (
				declarations);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cad1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cad2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeAttributeDeclaration[] declarations = new CodeAttributeDeclaration[] { 
				new CodeAttributeDeclaration (), null };

			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection (
				declarations);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection (
				(CodeAttributeDeclaration[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeAttributeDeclaration cad1 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad2 = new CodeAttributeDeclaration ();

			CodeAttributeDeclarationCollection c = new CodeAttributeDeclarationCollection ();
			c.Add (cad1);
			c.Add (cad2);

			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cad1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cad2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection (
				(CodeAttributeDeclarationCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeAttributeDeclaration cad1 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad2 = new CodeAttributeDeclaration ();

			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			Assert.AreEqual (0, coll.Add (cad1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (cad1), "#3");

			Assert.AreEqual (1, coll.Add (cad2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (cad2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Add ((CodeAttributeDeclaration) null);
		}

		[Test]
		public void Insert ()
		{
			CodeAttributeDeclaration cad1 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad2 = new CodeAttributeDeclaration ();

			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Add (cad1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cad1), "#2");
			coll.Insert (0, cad2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (cad1), "#4");
			Assert.AreEqual (0, coll.IndexOf (cad2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Insert (0, (CodeAttributeDeclaration) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeAttributeDeclaration cad1 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad2 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad3 = new CodeAttributeDeclaration ();

			CodeAttributeDeclarationCollection coll1 = new CodeAttributeDeclarationCollection ();
			coll1.Add (cad1);
			coll1.Add (cad2);

			CodeAttributeDeclarationCollection coll2 = new CodeAttributeDeclarationCollection ();
			coll2.Add (cad3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (cad1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (cad2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (cad3), "#4");

			CodeAttributeDeclarationCollection coll3 = new CodeAttributeDeclarationCollection ();
			coll3.Add (cad3);
			coll3.AddRange (new CodeAttributeDeclaration[] { cad1, cad2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (cad1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (cad2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (cad3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.AddRange ((CodeAttributeDeclaration[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Item ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.AddRange (new CodeAttributeDeclaration[] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.AddRange ((CodeAttributeDeclarationCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Add (new CodeAttributeDeclaration ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeAttributeDeclaration cad1 = new CodeAttributeDeclaration ();
			CodeAttributeDeclaration cad2 = new CodeAttributeDeclaration ();

			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Add (cad1);
			coll.Add (cad2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cad1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cad2), "#3");
			coll.Remove (cad1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (cad1), "#5");
			Assert.AreEqual (0, coll.IndexOf (cad2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Remove (new CodeAttributeDeclaration ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeAttributeDeclarationCollection coll = new CodeAttributeDeclarationCollection ();
			coll.Remove ((CodeAttributeDeclaration) null);
		}
	}
}
