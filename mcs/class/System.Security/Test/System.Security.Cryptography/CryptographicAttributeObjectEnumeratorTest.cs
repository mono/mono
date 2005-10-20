//
// CryptographicAttributeObjectEnumeratorTest.cs - NUnit tests for 
//	System.Security.Cryptography.CryptographicAttributeObjectEnumerator
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
	public class CryptographicAttributeObjectEnumeratorTest {

		static string defaultOid = "1.2.840.113549.1.7.1";

		private CryptographicAttributeObjectCollection coll;

		private void Count (int count)
		{
			Assert.AreEqual (count, coll.Count, "Count");
			int i = 0;
			foreach (CryptographicAttributeObject cao in coll) {
				i++;
			}
			Assert.AreEqual (count, i, "foreach");

			i = 0;
			CryptographicAttributeObjectEnumerator e = coll.GetEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is CryptographicAttributeObject)
					i++;
			}
			Assert.AreEqual (count, i, "GetEnumerator");

			i = 0;
			e.Reset ();
			while (e.MoveNext ()) {
				if (e.Current is CryptographicAttributeObject)
					i++;
			}
			Assert.AreEqual (count, i, "Reset");
		}

		[Test]
		public void Empty ()
		{
			coll = new CryptographicAttributeObjectCollection ();
			Count (0);
		}

		[Test]
		public void One_CryptographicAttributeObject () 
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttributeObject cao = new CryptographicAttributeObject (o);
			coll = new CryptographicAttributeObjectCollection (cao);
			Count (1);
		}

		[Test]
		public void One_AsnEncodedData ()
		{
			Oid o = new Oid (defaultOid);
			AsnEncodedData aed = new AsnEncodedData (o, new byte[] { 0x05, 0x00 });
			coll = new CryptographicAttributeObjectCollection ();
			coll.Add (aed);
			Count (1);
		}

		[Test]
		public void Two_Both ()
		{
			coll = new CryptographicAttributeObjectCollection ();

			Oid o1 = new Oid (defaultOid + ".1");
			AsnEncodedData aed = new AsnEncodedData (o1, new byte[] { 0x05, 0x00 });
			coll.Add (aed);

			Oid o2 = new Oid (defaultOid + ".2");
			coll.Add (new CryptographicAttributeObject (o2));

			Count (2);
		}
	}
}

#endif
