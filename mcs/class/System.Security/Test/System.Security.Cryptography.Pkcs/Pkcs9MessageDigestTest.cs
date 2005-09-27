//
// Pkcs9MessageDigestTest.cs - NUnit tests for Pkcs9MessageDigest
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
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs9MessageDigestTest {

		[Test]
		public void Constructor_Empty ()
		{
			Pkcs9MessageDigest md = new Pkcs9MessageDigest ();
			Assert.AreEqual ("1.2.840.113549.1.9.4", md.Oid.Value, "Oid.Value");
			Assert.AreEqual ("Message Digest", md.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.IsNull (md.RawData, "RawData");
			Assert.IsNull (md.MessageDigest, "MessageDigest");
			Assert.AreEqual (String.Empty, md.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, md.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			new Pkcs9MessageDigest ().CopyFrom (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyFrom ()
		{
			byte[] data = { 0x04, 0x10, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
			AsnEncodedData aed = new AsnEncodedData (data);
			Pkcs9MessageDigest md = new Pkcs9MessageDigest ();
			md.CopyFrom (aed);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyFrom_BadData ()
		{
			byte[] data = { 0x30, 0x18, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x03, 0x30, 0x0B, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x01 };
			AsnEncodedData aed = new AsnEncodedData (data);
			Pkcs9MessageDigest md = new Pkcs9MessageDigest ();
			md.CopyFrom (aed);
		}
	}
}

#endif
