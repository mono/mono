//
// CryptographicAttributeObjectCollectionTest.cs - NUnit tests for 
//	System.Security.Cryptography.CryptographicAttributeObjectCollection
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class CryptographicAttributeObjectCollectionTest {

		static string defaultOid = "1.2.840.113549.1.7.1";

		private void CommonStuff (CryptographicAttributeObjectCollection coll)
		{
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
			Assert.AreSame (coll, coll.SyncRoot, "SyncRoot");
			Assert.IsNotNull (coll.GetEnumerator (), "GetEnumerator");

			int i = coll.Count;
			Oid o1 = new Oid ("1.2.840.113549.1.7.3");
			AsnEncodedData aed = new AsnEncodedData (o1, new byte[] { 0x05, 0x00 });
			Assert.AreEqual (i, coll.Add (aed), "Add(AsnEncodedData)");
			Assert.IsTrue ((coll[i++] is CryptographicAttributeObject), "converted");

			Oid o2 = new Oid ("1.2.840.113549.1.7.2");
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o2);
			Assert.AreEqual (i, coll.Add (cao), "Add(CryptographicAttributeObject)");

			CryptographicAttributeObject[] array = new CryptographicAttributeObject [coll.Count];
			coll.CopyTo (array, 0);

			Array a = (Array) new object [coll.Count];
			ICollection c = (ICollection) coll;
			c.CopyTo (a, 0);

			IEnumerable e = (IEnumerable) coll;
			Assert.IsNotNull (e.GetEnumerator (), "GetEnumerator");

			coll.Remove (cao);
			Assert.AreEqual (i, coll.Count, "Remove(CryptographicAttributeObject)");
		}

		[Test]
		public void Constructor_Empty ()
		{
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection ();
			Assert.AreEqual (0, coll.Count, "Count");
			CommonStuff (coll);
		}

		[Test]
		public void Constructor_CryptographicAttributeObject () 
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o);
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection (cao);
			Assert.AreEqual (1, coll.Count, "Count");
			Assert.AreSame (cao, coll[0], "this[int]");
			CommonStuff (coll);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_AsnEncodedData_Null ()
		{
			AsnEncodedData aed = null;
			new CryptographicAttributeObjectCollection ().Add (aed);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_CryptographicAttributeObject_Null ()
		{
			CryptographicAttributeObject cao = null;
			new CryptographicAttributeObjectCollection ().Add (cao);
		}

		[Test]
		public void Add_MultipleSameOid ()
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o);
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection (cao);

			int i = 0;
			while (i < 10) {
				Assert.AreEqual (1, coll.Count, String.Format ("Count-{0}", i));
				Assert.AreEqual (i * 2, coll[0].Values.Count, String.Format ("Values.Count-{0}", i++));

				Oid o1 = new Oid (defaultOid);
				AsnEncodedData aed = new AsnEncodedData (o1, new byte[] { 0x04, (byte)i });
				coll.Add (aed);

				aed = new AsnEncodedData (o1, new byte[] { 0x04, (byte) i });
				coll.Add (aed);

				Oid o2 = new Oid (defaultOid);
				coll.Add (new CryptographicAttributeObject (o2));
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_Null ()
		{
			new CryptographicAttributeObjectCollection ().CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ICollection_CopyTo_Null ()
		{
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection ();
			ICollection c = (coll as ICollection);
			c.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			new CryptographicAttributeObjectCollection ().Remove (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null_WithNullItem ()
		{
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection (null);
			Assert.AreEqual (1, coll.Count, "Count");
			Assert.IsNull (coll[0], "this[int]");
			coll.Remove (null);
		}

		[Test]
		public void Remove_MultipleSameOid_First ()
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o);
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection (cao);

			Oid o1 = new Oid (defaultOid);
			AsnEncodedData aed = new AsnEncodedData (o1, new byte[] { 0x04, (byte) 0 });
			coll.Add (aed);

			aed = new AsnEncodedData (o1, new byte[] { 0x04, (byte) 0 });
			coll.Add (aed);

			Oid o2 = new Oid (defaultOid);
			coll.Add (new CryptographicAttributeObject (o2));

			Assert.AreEqual (1, coll.Count, "before Remove");
			coll.Remove (cao);
			Assert.AreEqual (0, coll.Count, "after Remove");
		}

		[Test]
		public void Remove_MultipleSameOid_Last ()
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o);
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection (cao);

			Oid o1 = new Oid (defaultOid);
			AsnEncodedData aed = new AsnEncodedData (o1, new byte[] { 0x04, (byte) 0 });
			coll.Add (aed);

			aed = new AsnEncodedData (o1, new byte[] { 0x04, (byte) 0 });
			coll.Add (aed);

			Oid o2 = new Oid (defaultOid);
			CryptographicAttributeObject last = new CryptographicAttributeObject (o2);
			coll.Add (last);

			Assert.AreEqual (1, coll.Count, "before Remove");
			coll.Remove (last);
			Assert.AreEqual (1, coll.Count, "after Remove");
		}

		[Test]
		public void Remove_WithDifferentInstance ()
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o);
			CryptographicAttributeObjectCollection coll = new CryptographicAttributeObjectCollection (cao);

			Assert.AreEqual (1, coll.Count, "before Remove");
			cao = new CryptographicAttributeObject (o);
			coll.Remove (cao);
			Assert.AreEqual (1, coll.Count, "after Remove");
		}
	}
}

#endif
