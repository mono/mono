//
// CodeTypeReferenceCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeTypeReferenceCollection
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

namespace MonoTests.System.CodeDom 
{
	[TestFixture]
	public class CodeTypeReferenceCollectionTest 
	{
		[Test]
		public void Constructor0 () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
		}

		[Test]
		public void Constructor1 () 
		{
			CodeTypeReference ref1 = new CodeTypeReference (string.Empty);
			CodeTypeReference ref2 = new CodeTypeReference (string.Empty);

			CodeTypeReference[] refs = new CodeTypeReference[] { ref1, ref2 };
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection (
				refs);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ref1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ref2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem () 
		{
			CodeTypeReference[] refs = new CodeTypeReference[] {
				new CodeTypeReference (string.Empty), null };

			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection (
				refs);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection (
				(CodeTypeReference[]) null);
		}

		[Test]
		public void Constructor2 () 
		{
			CodeTypeReference ref1 = new CodeTypeReference (string.Empty);
			CodeTypeReference ref2 = new CodeTypeReference (string.Empty);

			CodeTypeReferenceCollection c = new CodeTypeReferenceCollection ();
			c.Add (ref1);
			c.Add (ref2);

			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ref1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ref2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection (
				(CodeTypeReferenceCollection) null);
		}

		[Test]
		public void Add () 
		{
			CodeTypeReference ref1 = new CodeTypeReference (string.Empty);
			CodeTypeReference ref2 = new CodeTypeReference (string.Empty);

			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			Assert.AreEqual (0, coll.Add (ref1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (ref1), "#3");

			Assert.AreEqual (1, coll.Add (ref2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (ref2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Add ((CodeTypeReference) null);
		}

		[Test]
		public void Insert () 
		{
			CodeTypeReference ref1 = new CodeTypeReference (string.Empty);
			CodeTypeReference ref2 = new CodeTypeReference (string.Empty);

			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Add (ref1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ref1), "#2");
			coll.Insert (0, ref2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (ref1), "#4");
			Assert.AreEqual (0, coll.IndexOf (ref2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Insert (0, (CodeTypeReference) null);
		}

		[Test]
		public void AddRange () 
		{
			CodeTypeReference ref1 = new CodeTypeReference (string.Empty);
			CodeTypeReference ref2 = new CodeTypeReference (string.Empty);
			CodeTypeReference ref3 = new CodeTypeReference (string.Empty);

			CodeTypeReferenceCollection coll1 = new CodeTypeReferenceCollection ();
			coll1.Add (ref1);
			coll1.Add (ref2);

			CodeTypeReferenceCollection coll2 = new CodeTypeReferenceCollection ();
			coll2.Add (ref3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (ref1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (ref2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (ref3), "#4");

			CodeTypeReferenceCollection coll3 = new CodeTypeReferenceCollection ();
			coll3.Add (ref3);
			coll3.AddRange (new CodeTypeReference[] { ref1, ref2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (ref1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (ref2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (ref3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.AddRange ((CodeTypeReference[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Collection () 
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.AddRange ((CodeTypeReferenceCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Add (new CodeTypeReference (string.Empty));
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeTypeReference ctr1 = new CodeTypeReference (string.Empty);
			CodeTypeReference ctr2 = new CodeTypeReference (string.Empty);

			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Add (ctr1);
			coll.Add (ctr2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ctr1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ctr2), "#3");
			coll.Remove (ctr1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (ctr1), "#5");
			Assert.AreEqual (0, coll.IndexOf (ctr2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Remove (new CodeTypeReference (string.Empty));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeTypeReferenceCollection coll = new CodeTypeReferenceCollection ();
			coll.Remove ((CodeTypeReference) null);
		}
	}
}
