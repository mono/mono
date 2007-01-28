//
// PermissionSetCollectionTest.cs 
//	- NUnit Test Cases for PermissionSetCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0 && !TARGET_JVM

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security {

	// "alternate" IList implementation
	class TestList : IList {

		private IList l;

		public TestList ()
		{
			l = (IList) new ArrayList ();
		}

		public int Add (object value)
		{
			return l.Add (value);
		}

		public void Clear ()
		{
			l.Clear ();
		}

		public bool Contains (object value)
		{
			return l.Contains (value);
		}

		public int IndexOf (object value)
		{
			return l.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			l.Insert (index, value);
		}

		public bool IsFixedSize {
			get { return l.IsFixedSize; }
		}

		public bool IsReadOnly {
			get { return l.IsReadOnly; }
		}

		public void Remove (object value)
		{
			l.Remove (value);
		}

		public void RemoveAt (int index)
		{
			l.RemoveAt (index);
		}

		public object this [int index] {
			get { return l [index]; }
			set { l [index] = value; }
		}

		public void CopyTo (Array array, int index)
		{
			l.CopyTo (array, index);
		}

		public int Count {
			get { return l.Count; }
		}

		public bool IsSynchronized {
			get { return l.IsSynchronized; }
		}

		public object SyncRoot {
			get { return l.SyncRoot; }
		}

		public IEnumerator GetEnumerator ()
		{
			return l.GetEnumerator ();
		}
	}

	[TestFixture]
	public class PermissionSetCollectionTest {

		[Test]
		public void Constructor ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			Assert.AreEqual (0, psc.Count, "Count");
			Assert.IsFalse (psc.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, psc.PermissionSets.Count, "PermissionSets.Count");
			Assert.AreEqual (psc.ToXml ().ToString (), psc.ToString (), "ToXml().ToString()==ToString()");
			Assert.IsNotNull (psc.GetEnumerator (), "GetEnumerator");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SyncRoot ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			Assert.IsNull (psc.SyncRoot, "SyncRoot");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.Add (null);
		}

		[Test]
		public void Add ()
		{
			PermissionSet none = new PermissionSet (PermissionState.None);
			PermissionSet unr = new PermissionSet (PermissionState.Unrestricted);
			PermissionSetCollection psc = new PermissionSetCollection ();
			Assert.AreEqual (0, psc.PermissionSets.Count, "Count-0");
			Assert.AreEqual (0, psc.Count, "Count-0");
			psc.Add (none);
			Assert.AreEqual (1, psc.PermissionSets.Count, "Count-1");
			// re-add same permissionset
			psc.Add (none);
			Assert.AreEqual (2, psc.PermissionSets.Count, "Count-2");
			psc.Add (unr);
			Assert.AreEqual (3, psc.PermissionSets.Count, "Count-3");
			Assert.AreEqual (3, psc.Count, "Count-3");
		}

		[Test]
		public void Copy ()
		{
			PermissionSet none = new PermissionSet (PermissionState.None);
			PermissionSet unr = new PermissionSet (PermissionState.Unrestricted);
			PermissionSetCollection psc = new PermissionSetCollection ();
			PermissionSetCollection copy = psc.Copy ();
			Assert.AreEqual (0, copy.PermissionSets.Count, "Count-0");
			psc.Add (none);
			Assert.AreEqual (0, copy.PermissionSets.Count, "Count-0b");
			copy = psc.Copy ();
			Assert.AreEqual (1, copy.PermissionSets.Count, "Count-1");
			psc.Add (none); // re-add same permissionset
			Assert.AreEqual (1, copy.PermissionSets.Count, "Count-1b");
			copy = psc.Copy ();
			Assert.AreEqual (2, copy.PermissionSets.Count, "Count-2");
			psc.Add (unr);
			Assert.AreEqual (2, copy.PermissionSets.Count, "Count-2b");
			copy = psc.Copy ();
			Assert.AreEqual (3, copy.PermissionSets.Count, "Count-3");
			Assert.AreEqual (3, copy.Count, "Count-3");
		}

		[Test]
		public void Copy_References ()
		{
			PermissionSet none = new PermissionSet (PermissionState.None);
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.Add (none);
			PermissionSetCollection copy = psc.Copy ();
			Assert.AreEqual (1, copy.PermissionSets.Count, "Count-1");

			string before = psc.ToString ();
			none.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));

			Assert.AreEqual (none.ToString (), psc.PermissionSets[0].ToString (), "psc");
			Assert.AreEqual (before, copy.ToString (), "copy");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CopyTo ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CopyTo_ICollection ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			ICollection c = (psc as ICollection);
			c.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_BadName ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			SecurityElement se = new SecurityElement ("PermissionZetCollection");
			psc.FromXml (se);
		}

		[Test]
		public void FromXml_Roundtrip ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			string expected = psc.ToString ();
			SecurityElement se = psc.ToXml ();
			psc.FromXml (se);
			string actual = psc.ToString ();
			Assert.AreEqual (expected, actual, "Empty");

			PermissionSet none = new PermissionSet (PermissionState.None);
			psc.Add (none);
			expected = psc.ToString ();
			se = psc.ToXml ();
			psc.FromXml (se);
			actual = psc.ToString ();
			Assert.AreEqual (expected, actual, "1-None");

			none.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			expected = psc.ToString ();
			se = psc.ToXml ();
			psc.FromXml (se);
			actual = psc.ToString ();
			Assert.AreEqual (expected, actual, "1-Assertion");
			Assert.AreEqual (1, psc.Count, "1");

			PermissionSet unr = new PermissionSet (PermissionState.Unrestricted);
			psc.Add (unr);
			expected = psc.ToString ();
			se = psc.ToXml ();
			psc.FromXml (se);
			actual = psc.ToString ();
			Assert.AreEqual (expected, actual, "2-Assertion+Unrestricted");
			Assert.AreEqual (2, psc.Count, "2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetSet_Negative ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.GetSet (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetSet_Zero_Empty ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.GetSet (0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetSet_MaxInt ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.GetSet (Int32.MaxValue);
		}

		[Test]
		public void GetSet ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			PermissionSet unr = new PermissionSet (PermissionState.Unrestricted);
			psc.Add (unr);
			PermissionSet ps = psc.GetSet (0);
			Assert.AreEqual (unr.ToString (), ps.ToString (), "Same XML");
			Assert.IsTrue (Object.ReferenceEquals (unr, ps), "Same Object Reference");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveSet_Negative ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.RemoveSet (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveSet_Zero_Empty ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.RemoveSet (0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveSet_MaxInt ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			psc.RemoveSet (Int32.MaxValue);
		}

		[Test]
		public void RemoveSet ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			PermissionSet unr = new PermissionSet (PermissionState.Unrestricted);
			psc.Add (unr);
			psc.RemoveSet (0);
			Assert.AreEqual (0, psc.Count, "Count");
		}

		[Test]
		public void ToXml ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			SecurityElement se = psc.ToXml ();
			Assert.IsNull (se.Children, "Children==null for 0");
			PermissionSet unr = new PermissionSet (PermissionState.Unrestricted);
			psc.Add (unr);
			se = psc.ToXml ();
			Assert.AreEqual (1, se.Children.Count, "Children==1");
			Assert.AreEqual (unr.ToString (), se.Children[0].ToString (), "XML");
		}
	}
}

#endif
