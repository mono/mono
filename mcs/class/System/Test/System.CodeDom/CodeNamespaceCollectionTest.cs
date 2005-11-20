//
// CodeNamespaceCollectionTest.cs 
//	- Unit tests for System.CodeDom.CodeNamespaceCollection
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
	public class CodeNamespaceCollectionTest {
		[Test]
		public void Constructor0 ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			Assert.IsFalse (((IList) coll).IsFixedSize, "#1");
			Assert.IsFalse (((IList) coll).IsReadOnly, "#2");
			Assert.AreEqual (0, coll.Count, "#3");
			Assert.IsFalse (((ICollection) coll).IsSynchronized, "#4");
			Assert.IsNotNull (((ICollection) coll).SyncRoot, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeNamespace ns1 = new CodeNamespace ();
			CodeNamespace ns2 = new CodeNamespace ();

			CodeNamespace[] namespaces = new CodeNamespace[] { ns1, ns2 };
			CodeNamespaceCollection coll = new CodeNamespaceCollection (
				namespaces);

			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ns1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ns2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_NullItem ()
		{
			CodeNamespace[] namespaces = new CodeNamespace[] { 
				new CodeNamespace (), null };

			CodeNamespaceCollection coll = new CodeNamespaceCollection (
				namespaces);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor1_Null () {
			CodeNamespaceCollection coll = new CodeNamespaceCollection (
				(CodeNamespace[]) null);
		}

		[Test]
		public void Constructor2 ()
		{
			CodeNamespace ns1 = new CodeNamespace ();
			CodeNamespace ns2 = new CodeNamespace ();

			CodeNamespaceCollection c = new CodeNamespaceCollection ();
			c.Add (ns1);
			c.Add (ns2);

			CodeNamespaceCollection coll = new CodeNamespaceCollection (c);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ns1), "#2");
			Assert.AreEqual (1, coll.IndexOf (ns2), "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor2_Null ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection (
				(CodeNamespaceCollection) null);
		}

		[Test]
		public void Add ()
		{
			CodeNamespace ns1 = new CodeNamespace ();
			CodeNamespace ns2 = new CodeNamespace ();

			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			Assert.AreEqual (0, coll.Add (ns1), "#1");
			Assert.AreEqual (1, coll.Count, "#2");
			Assert.AreEqual (0, coll.IndexOf (ns1), "#3");

			Assert.AreEqual (1, coll.Add (ns2), "#4");
			Assert.AreEqual (2, coll.Count, "#5");
			Assert.AreEqual (1, coll.IndexOf (ns2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null () {
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Add ((CodeNamespace) null);
		}

		[Test]
		public void Insert ()
		{
			CodeNamespace ns1 = new CodeNamespace ();
			CodeNamespace ns2 = new CodeNamespace ();

			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Add (ns1);
			Assert.AreEqual (1, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (ns1), "#2");
			coll.Insert (0, ns2);
			Assert.AreEqual (2, coll.Count, "#3");
			Assert.AreEqual (1, coll.IndexOf (ns1), "#4");
			Assert.AreEqual (0, coll.IndexOf (ns2), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_Null ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Insert (0, (CodeNamespace) null);
		}

		[Test]
		public void AddRange ()
		{
			CodeNamespace ns1 = new CodeNamespace ();
			CodeNamespace ns2 = new CodeNamespace ();
			CodeNamespace ns3 = new CodeNamespace ();

			CodeNamespaceCollection coll1 = new CodeNamespaceCollection ();
			coll1.Add (ns1);
			coll1.Add (ns2);

			CodeNamespaceCollection coll2 = new CodeNamespaceCollection ();
			coll2.Add (ns3);
			coll2.AddRange (coll1);
			Assert.AreEqual (3, coll2.Count, "#1");
			Assert.AreEqual (1, coll2.IndexOf (ns1), "#2");
			Assert.AreEqual (2, coll2.IndexOf (ns2), "#3");
			Assert.AreEqual (0, coll2.IndexOf (ns3), "#4");

			CodeNamespaceCollection coll3 = new CodeNamespaceCollection ();
			coll3.Add (ns3);
			coll3.AddRange (new CodeNamespace[] { ns1, ns2 });
			Assert.AreEqual (3, coll2.Count, "#5");
			Assert.AreEqual (1, coll2.IndexOf (ns1), "#6");
			Assert.AreEqual (2, coll2.IndexOf (ns2), "#7");
			Assert.AreEqual (0, coll2.IndexOf (ns3), "#8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null_Array ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.AddRange ((CodeNamespace[]) null);
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
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.AddRange ((CodeNamespaceCollection) null);
		}

		[Test]
		public void AddRange_Self ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Add (new CodeNamespace ());
			Assert.AreEqual (1, coll.Count, "#1");
			coll.AddRange (coll);
			Assert.AreEqual (2, coll.Count, "#2");
		}

		[Test]
		public void Remove ()
		{
			CodeNamespace cns1 = new CodeNamespace ();
			CodeNamespace cns2 = new CodeNamespace ();

			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Add (cns1);
			coll.Add (cns2);
			Assert.AreEqual (2, coll.Count, "#1");
			Assert.AreEqual (0, coll.IndexOf (cns1), "#2");
			Assert.AreEqual (1, coll.IndexOf (cns2), "#3");
			coll.Remove (cns1);
			Assert.AreEqual (1, coll.Count, "#4");
			Assert.AreEqual (-1, coll.IndexOf (cns1), "#5");
			Assert.AreEqual (0, coll.IndexOf (cns2), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Remove_NotInCollection ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Remove (new CodeNamespace ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			CodeNamespaceCollection coll = new CodeNamespaceCollection ();
			coll.Remove ((CodeNamespace) null);
		}
	}
}
