//
// StrongNamePublicKeyBlobTest.cs - NUnit Test Cases for StrongNamePublicKeyBlob
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

[TestFixture]
public class StrongNamePublicKeyBlobTest {

	static byte[] bad = { 0xB, 0xAD };
	// should be a valid one (see StrongNameKeyPairTest.cs)
	static byte[] pk = { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3D, 0xBD, 0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0, 0x58, 0x07, 0x2B, 0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22, 0xDB, 0x20, 0xB2, 0xA9, 0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64, 0x2F, 0x5D, 0x8D, 0xEB, 0x78, 0x02, 0xF7, 0xA5, 0x36, 0x77, 0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68, 0xDB, 0xEB, 0x24, 0x09, 0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E, 0x59, 0x54, 0x4A, 0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F, 0xA8, 0x3E, 0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D, 0x10, 0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB, 0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F, 0xF5, 0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D, 0xD3, 0x56, 0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07, 0x93, 0x44, 0x00, 0x4A, 0xF8, 0xFF, 0xA4, 0xCB };

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestNull () 
	{
		StrongNamePublicKeyBlob snpkb = new StrongNamePublicKeyBlob (null);
	}

	[Test]
	public void InvalidPublicKey () 
	{
		StrongNamePublicKeyBlob snpkb = new StrongNamePublicKeyBlob (bad);
		// Proof that there's no validation on public key
		Assert.AreEqual ("0BAD", snpkb.ToString (), "ToString(BAD)");
	}

	[Test]
	public void PublicKey () 
	{
		StrongNamePublicKeyBlob snpkb = new StrongNamePublicKeyBlob (pk);
		Assert.IsTrue (snpkb.Equals (snpkb), "Equals(Self)");
		Assert.IsFalse (snpkb.Equals (null), "Equals(null)");
		Assert.AreEqual ("00240000048000009400000006020000002400005253413100040000010001003DBD7208C62B0EA8C1C058072B635F7C9ABDCB22DB20B2A9DADAEFE800642F5D8DEB7802F7A5367728D7558D1468DBEB2409D02B131B926E2E59544AAC18CFC909023F4FA83E94001FC2F11A27477D1084F514B861621A0C66ABD24C4B9FC90F3CD8920FF5FFCED76E5C6FB1F57DD356F96727A4A5485B079344004AF8FFA4CB", snpkb.ToString (), "ToString(pk)");

		StrongNamePublicKeyBlob snpkb2 = new StrongNamePublicKeyBlob (pk);
		Assert.IsTrue (snpkb.Equals (snpkb2), "Equals()-true");
		StrongNamePublicKeyBlob snpkb3 = new StrongNamePublicKeyBlob (bad);
		Assert.IsFalse (snpkb.Equals (snpkb3), "Equals()-false");

		// non standard get hash code - why ???
		Assert.AreEqual (snpkb2.GetHashCode (), snpkb.GetHashCode (), "GetHashCode-0");

		// the first 4 bytes has code has been fixed in 2.0 beta 1

// Historical data:
// #elif NET_1_1
// 		// It seems to be the first four bytes of the public key data
// 		// which seems like non sense as all valid public key will have the same header ?
// 		Assert.AreEqual (2359296, snpkb.GetHashCode (), "GetHashCode-1");
// 		Assert.AreEqual (2359296, snpkb2.GetHashCode (), "GetHashCode-2");
// 		Assert.AreEqual (2989, snpkb3.GetHashCode (), "GetHashCode-3");
// 		byte[] header = { 0x00, 0x24, 0x00, 0x00 };
// 		StrongNamePublicKeyBlob snpkb4 = new StrongNamePublicKeyBlob (header);
// 		Assert.AreEqual (2359296, snpkb4.GetHashCode (), "GetHashCode-4");
// #endif
	}
}

}