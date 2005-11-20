//
// CodeNamespaceImportCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeNamespaceImportCollection
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
	public class CodeNamespaceImportCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Add ()
		{
			CodeNamespaceImport ni1 = new CodeNamespaceImport ("A");
			CodeNamespaceImport ni2 = new CodeNamespaceImport ("B");
			CodeNamespaceImport ni3 = new CodeNamespaceImport ("b");
			CodeNamespaceImport ni4 = new CodeNamespaceImport ("B");

			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection ();
			coll.Add (ni1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, ((IList) coll).IndexOf (ni1), "#2");

			coll.Add (ni2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, ((IList) coll).IndexOf (ni2), "#4");

			coll.Add (ni3);
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (-1, ((IList) coll).IndexOf (ni3), "#6");

			coll.Add (ni4);
			Assert.AreEqual (2, coll.Count, "#7");
			Assert.AreEqual (-1, ((IList) coll).IndexOf (ni4), "#8");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Add_Null () {
			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection ();
			coll.Add ((CodeNamespaceImport) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeNamespaceImport ni1 = new CodeNamespaceImport ("A");
			CodeNamespaceImport ni2 = new CodeNamespaceImport ("B");
			CodeNamespaceImport ni3 = new CodeNamespaceImport ("b");
			CodeNamespaceImport ni4 = new CodeNamespaceImport ("B");
			CodeNamespaceImport ni5 = new CodeNamespaceImport ("C");

			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection ();
			coll.AddRange (new CodeNamespaceImport[] {ni1, ni2});
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, ((IList) coll).IndexOf (ni1), "#2");
			Assert.AreEqual (1, ((IList) coll).IndexOf (ni2), "#3");

			coll.AddRange (new CodeNamespaceImport[] { ni3, ni4, ni5 });
			Assert.AreEqual (3, coll.Count, "#4");
			Assert.AreEqual (0, ((IList) coll).IndexOf (ni1), "#5");
			Assert.AreEqual (1, ((IList) coll).IndexOf (ni2), "#6");
			Assert.AreEqual (-1, ((IList) coll).IndexOf (ni3), "#7");
			Assert.AreEqual (-1, ((IList) coll).IndexOf (ni4), "#8");
			Assert.AreEqual (2, ((IList) coll).IndexOf (ni5), "#9");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection ();
			coll.AddRange ((CodeNamespaceImport[]) null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void AddRange_Null_Item()
		{
			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection();
			coll.AddRange(new CodeNamespaceImport[] { null });
		}
	}
}
