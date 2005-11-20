//
// CodeTypeMemberCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeTypeMemberCollection
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
	public class CodeTypeMemberCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeTypeMember tm1 = new CodeTypeMember ();
			CodeTypeMember tm2 = new CodeTypeMember ();

			CodeTypeMember[] typeMembers = new CodeTypeMember[] { tm1, tm2 };
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection (
				typeMembers);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (tm1), "#2");
			Assert.AreEqual (1, coll.IndexOf (tm2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeTypeMember[] typeMembers = new CodeTypeMember[] { 
				new CodeTypeMember (), null };

			CodeTypeMemberCollection coll = new CodeTypeMemberCollection (
				typeMembers);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection (
				(CodeTypeMember[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeTypeMember tm1 = new CodeTypeMember ();
			CodeTypeMember tm2 = new CodeTypeMember ();

			CodeTypeMemberCollection c = new CodeTypeMemberCollection ();
			c.Add (tm1);
			c.Add (tm2);

			CodeTypeMemberCollection coll = new CodeTypeMemberCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (tm1), "#2");
			Assert.AreEqual (1, coll.IndexOf (tm2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection (
				(CodeTypeMemberCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeTypeMember tm1 = new CodeTypeMember ();
			CodeTypeMember tm2 = new CodeTypeMember ();

			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			Assert.AreEqual (0, coll.Add (tm1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (tm1), "#3");

			Assert.AreEqual (1, coll.Add (tm2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (tm2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Add ((CodeTypeMember) null);
		}

		[Test]
		public void Insert ()
		{
			CodeTypeMember tm1 = new CodeTypeMember ();
			CodeTypeMember tm2 = new CodeTypeMember ();

			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Add (tm1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (tm1), "#2");
			coll.Insert (0, tm2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (tm1), "#4");
			Assert.AreEqual (0, coll.IndexOf (tm2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Insert (0, (CodeTypeMember) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeTypeMember tm1 = new CodeTypeMember ();
			CodeTypeMember tm2 = new CodeTypeMember ();
			CodeTypeMember tm3 = new CodeTypeMember ();

			CodeTypeMemberCollection coll1 = new CodeTypeMemberCollection ();
			coll1.Add (tm1);
			coll1.Add (tm2);

			CodeTypeMemberCollection coll2 = new CodeTypeMemberCollection ();
			coll2.Add (tm3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (tm1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (tm2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (tm3), "#4");

			CodeTypeMemberCollection coll3 = new CodeTypeMemberCollection ();
			coll3.Add (tm3);
			coll3.AddRange (new CodeTypeMember[] { tm1, tm2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (tm1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (tm2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (tm3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.AddRange ((CodeTypeMember[]) null);
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
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.AddRange ((CodeTypeMemberCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Add (new CodeTypeMember ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeTypeMember ctm1 = new CodeTypeMember ();
			CodeTypeMember ctm2 = new CodeTypeMember ();

			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Add (ctm1);
			coll.Add (ctm2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ctm1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ctm2), "#3");
			coll.Remove (ctm1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (ctm1), "#5");
			Assert.AreEqual (0, coll.IndexOf (ctm2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Remove (new CodeTypeMember ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeTypeMemberCollection coll = new CodeTypeMemberCollection ();
			coll.Remove ((CodeTypeMember) null);
		}
	}
}
