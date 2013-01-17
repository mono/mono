//
// RSAPKCS1KeyExchangeFormatterTest.cs - NUnit Test Cases for RSAPKCS1KeyExchangeFormatter
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class RSAPKCS1KeyExchangeFormatterTest {

	protected static RSA key;

	[SetUp]
	public void SetUp () 
	{
		// generating a keypair is REALLY long and the framework
		// makes sure that we generate one (even if create an object
		// to import an exsting key)
		if (key == null) {
			key = RSA.Create ();
			key.ImportParameters (AllTests.GetRsaKey (true));
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2)
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	[Test]
	public void Properties () 
	{
		RSAPKCS1KeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter ();
		keyex.SetKey (key);
		Assert.AreEqual("<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />", keyex.Parameters);
		// null (default)
		Assert.IsNull (keyex.Rng, "RSAPKCS1KeyExchangeFormatter.Rng");
		Assert.AreEqual("System.Security.Cryptography.RSAPKCS1KeyExchangeFormatter", keyex.ToString ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void KeyExchangeNull ()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] EM = keyex.CreateKeyExchange (null);
	}

	// TestExchangeMin (1)
	[Test]
	public void KeyExchangeMin ()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = { 0x01 };
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// test with a message 128 bits (16 bytes) long
	[Test]
	public void KeyExchange128bits ()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49 };
		byte[] EM = keyex.CreateKeyExchange (M, typeof (Rijndael));

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// test with a message 160 bits (20 bytes) long
	[Test]
	public void KeyExchange160bits () 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49, 0x00, 0x00, 0x00, 0x00 };
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// Max = k - m - 11
	[Test]
	public void KeyExchangeMax()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = new byte [(key.KeySize >> 3)- 11];
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// TestExchangeTooBig
	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void KeyExchangeTooBig()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = new byte [(key.KeySize >> 3)- 10];
		byte[] EM = keyex.CreateKeyExchange (M);
	}

	[Test]
	public void Rng () 
	{
		RSAPKCS1KeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		Assert.IsNull (keyex.Rng, "Rng 1");
		keyex.Rng = RandomNumberGenerator.Create ();
		Assert.IsNotNull (keyex.Rng, "Rng 2");
	}

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	public void ExchangeNoKey () 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter ();
		byte[] M = keyex.CreateKeyExchange (new byte [16]);
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void ExchangeDSAKey () 
	{
		DSA dsa = DSA.Create ();
		AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (dsa);
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void CreateWithNullKey ()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void CreateAndSetNullKey ()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter ();
		keyex.SetKey (null);
	}
}

}
