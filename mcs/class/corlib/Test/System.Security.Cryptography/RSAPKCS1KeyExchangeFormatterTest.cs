//
// RSAPKCS1KeyExchangeFormatterTest.cs - NUnit Test Cases for RSAPKCS1KeyExchangeFormatter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class RSAPKCS1KeyExchangeFormatterTest : Assertion {

	protected static RSA key;

	[SetUp]
	void SetUp () 
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
		AssertEquals("RSAPKCS1KeyExchangeFormatter.Parameters", "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />", keyex.Parameters);
		// null (default)
		AssertNull("RSAPKCS1KeyExchangeFormatter.Rng", keyex.Rng);
		AssertEquals("RSAPKCS1KeyExchangeFormatter.ToString()", "System.Security.Cryptography.RSAPKCS1KeyExchangeFormatter", keyex.ToString ());
	}

	[Test]
	[ExpectedException (typeof (NullReferenceException))]
	[Ignore ("Sometime causes System.ExecutionEngineException on MS implementation")]
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
		byte[] EM = keyex.CreateKeyExchange (M);

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
}

}
