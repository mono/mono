//
// RSAPKCS1KeyExchangeFormatterTest.cs - NUnit Test Cases for RSAPKCS1KeyExchangeFormatter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography
{

public class RSAPKCS1KeyExchangeFormatterTest : TestCase 
{
	public RSAPKCS1KeyExchangeFormatterTest () : base ("System.Security.Cryptography.RSAPKCS1KeyExchangeFormatter testsuite") {}
	public RSAPKCS1KeyExchangeFormatterTest (string name) : base (name) {}

	protected static RSA key;

	protected override void SetUp () 
	{
		// generating a keypair is REALLY long and the framework
		// makes sure that we generate one (even if create an object
		// to import an exsting key)
		if (key == null) {
			key = RSA.Create ();
			key.ImportParameters (AllTests.GetRsaKey (true));
		}
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (RSAPKCS1KeyExchangeFormatterTest)); 
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2)
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	public void TestProperties () 
	{
		RSAPKCS1KeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter ();
		keyex.SetKey (key);
		AssertEquals("RSAPKCS1KeyExchangeFormatter.Parameters", "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />", keyex.Parameters);
		// null (default)
		AssertNull("RSAPKCS1KeyExchangeFormatter.Rng", keyex.Rng);
		AssertEquals("RSAPKCS1KeyExchangeFormatter.ToString()", "System.Security.Cryptography.RSAPKCS1KeyExchangeFormatter", keyex.ToString ());
	}

	// TestExchangeMin (1)
	public void TestExchangeMin()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = { 0x01 };
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// TestExchange64, 128, 192, 256

	// test with a message 128 bits (16 bytes) long
	public void TestExchange128()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49 };
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// test with a message 160 bits (20 bytes) long
	public void TestExchange192() 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49, 0x00, 0x00, 0x00, 0x00 };
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// Max = k - m - 11
	public void TestExchangeMax()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = new byte [(key.KeySize >> 3)- 11];
		byte[] EM = keyex.CreateKeyExchange (M);

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// TestExchangeTooBig
	public void TestExchangeTooBig()
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAPKCS1KeyExchangeFormatter (key);
		byte[] M = new byte [(key.KeySize >> 3)- 10];
		try {
			byte[] EM = keyex.CreateKeyExchange (M);
			Fail ("Expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected CryptographicException but got : " + e.ToString ());
		}
	}
}

}
