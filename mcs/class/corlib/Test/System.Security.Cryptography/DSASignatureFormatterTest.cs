//
// DSASignatureFormatterTest.cs - NUnit Test Cases for DSASignatureFormatter
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
public class DSASignatureFormatterTest {
	protected DSASignatureFormatter fmt;
	protected static DSA dsa;
	protected static RSA rsa;

	public DSASignatureFormatterTest () 
	{
		// key generation is VERY long so one time is enough
		dsa = DSA.Create ();
		rsa = RSA.Create ();
	}

	[SetUp]
	public void SetUp () 
	{
		fmt = new DSASignatureFormatter ();
	}

	[Test]
	public void Constructor_Empty () 
	{
		DSASignatureFormatter fmt = new DSASignatureFormatter ();
		Assert.IsNotNull (fmt);
	}

	[Test]
	public void Constructor_DSA ()
	{
		DSASignatureFormatter fmt = new DSASignatureFormatter (dsa);
		Assert.IsNotNull (fmt);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#endif
	public void Constructor_Null () 
	{
		DSASignatureFormatter fmt = new DSASignatureFormatter (null);
		Assert.IsNotNull (fmt);
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void Constructor_RSA ()
	{
		DSASignatureFormatter fmt = new DSASignatureFormatter (rsa);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void SetHash_Null ()
	{
		fmt.SetHashAlgorithm (null);
	}

	[Test]
	public void SetHash_SHA1 ()
	{
		fmt.SetHashAlgorithm ("SHA1");
	}

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	public void SetHash_MD5 ()
	{
		fmt.SetHashAlgorithm ("MD5");
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#endif
	public void SetKey_Null ()
	{
		fmt.SetKey (null);
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void SetKey_RSA ()
	{
		fmt.SetKey (rsa);
	}

	[Test]
	public void SetKey_DSA ()
	{
		fmt.SetKey (dsa);
	}

	// note: There's a bug in MS Framework where you can't re-import a key into
	// the same object

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	public void Signature_NoKeyPair ()
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		byte[] sign = fmt.CreateSignature (hash);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void Signature_OnlyPublicKey ()
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		dsa.ImportParameters (AllTests.GetKey (false));
		fmt.SetKey (dsa);
		byte[] sign = fmt.CreateSignature (hash);
	}

	[Test]
	public void Signature ()
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		dsa.ImportParameters (AllTests.GetKey (true));
		fmt.SetKey (dsa);
		byte[] sign = fmt.CreateSignature (hash);
		Assert.IsTrue (dsa.VerifySignature (hash, sign));
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Signature_NullHash ()
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		dsa.ImportParameters (AllTests.GetKey (true));
		fmt.SetKey (dsa);

		byte[] h = null; // overloaded method
		byte[] sign = fmt.CreateSignature (h); 
	}
}

}