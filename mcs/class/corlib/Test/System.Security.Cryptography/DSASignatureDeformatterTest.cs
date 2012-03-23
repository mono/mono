//
// DSASignatureDeformatterTest.cs - NUnit Test Cases for DSASignatureDeformatter
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class DSASignatureDeformatterTest {
	protected DSASignatureDeformatter def;
	protected static DSA dsa;
	protected static RSA rsa;

	static byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
	static byte[] sign = { 0x50, 0xd2, 0xb0, 0x8b, 0xcd, 0x5e, 0xb2, 0xc2, 0x35, 0x82, 0xd3, 0x76, 0x07, 0x79, 0xbb, 0x55, 0x98, 0x72, 0x43, 0xe8,
			       0x74, 0xc9, 0x35, 0xf8, 0xc9, 0xbd, 0x69, 0x2f, 0x08, 0x34, 0xfa, 0x5a, 0x59, 0x23, 0x2a, 0x85, 0x7b, 0xa3, 0xb3, 0x82 };

	public DSASignatureDeformatterTest () 
	{
		// key generation is VERY long so one time is enough
		dsa = DSA.Create ();
		rsa = RSA.Create ();
	}

	[SetUp]
	public void SetUp () 
	{
		def = new DSASignatureDeformatter ();
	}

	[Test]
	public void Constructor_Empty () 
	{
		DSASignatureDeformatter def = new DSASignatureDeformatter ();
		Assert.IsNotNull (def);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#endif
	public void Constructor_Null ()
	{
		DSASignatureDeformatter def = new DSASignatureDeformatter (null);
		Assert.IsNotNull (def);
	}

	[Test]
	public void Constructor_DSA ()
	{
		DSASignatureDeformatter def = new DSASignatureDeformatter (dsa);
		Assert.IsNotNull (def);
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void Constructor_RSA ()
	{
		DSASignatureDeformatter def = new DSASignatureDeformatter (rsa);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void SetHash_Null ()
	{
		def.SetHashAlgorithm (null);
	}

	[Test]
	public void SetHash_SHA1 ()
	{
		def.SetHashAlgorithm ("SHA1");
	}

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	public void SetHash_MD5 ()
	{
		def.SetHashAlgorithm ("MD5");
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#endif
	public void SetKey_Null () 
	{
		def.SetKey (null);
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void SetKey_RSA ()
	{
		def.SetKey (rsa);
	}

	[Test]
	public void SetKey_DSA ()
	{
		def.SetKey (dsa);
	}

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	public void Verify_NoKeyPair () 
	{
		def.VerifySignature (hash, sign);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Verify_NullSignature ()
	{
		dsa.ImportParameters (AllTests.GetKey (false));
		def.SetKey (dsa);
		def.VerifySignature (hash, null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Verify_NullHash ()
	{
		dsa.ImportParameters (AllTests.GetKey (false));
		def.SetKey (dsa);
		byte[] s = null; // overloaded method
		def.VerifySignature (s, sign);
	}

	[Test]
	public void Verify ()
	{
		dsa.ImportParameters (AllTests.GetKey (false));
		def.SetKey (dsa);
		Assert.IsTrue (def.VerifySignature (hash, sign));
	}

	[Test]
	public void Verify_Bad ()
	{
		dsa.ImportParameters (AllTests.GetKey (false));
		def.SetKey (dsa);
		byte[] badSign = { 0x49, 0xd2, 0xb0, 0x8b, 0xcd, 0x5e, 0xb2, 0xc2, 0x35, 0x82, 0xd3, 0x76, 0x07, 0x79, 0xbb, 0x55, 0x98, 0x72, 0x43, 0xe8,
				   0x74, 0xc9, 0x35, 0xf8, 0xc9, 0xbd, 0x69, 0x2f, 0x08, 0x34, 0xfa, 0x5a, 0x59, 0x23, 0x2a, 0x85, 0x7b, 0xa3, 0xb3, 0x82 };
		Assert.IsFalse (def.VerifySignature (hash, badSign));
	}
}

}