//
// RSAOAEPKeyExchangeFormatterTest.cs - NUnit Test Cases for RSAOAEPKeyExchangeFormatter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

public class RSAOAEPKeyExchangeFormatterTest : TestCase {
	public RSAOAEPKeyExchangeFormatterTest () : base ("System.Security.Cryptography.RSAOAEPKeyExchangeFormatter testsuite") {}
	public RSAOAEPKeyExchangeFormatterTest (string name) : base (name) {}

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
			return new TestSuite (typeof (RSAOAEPKeyExchangeFormatterTest)); 
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	public void TestProperties () 
	{
		RSAOAEPKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter ();
		keyex.SetKey (key);
		AssertEquals("RSAOAEPKeyExchangeFormatter.Parameters", null, keyex.Parameters);
		// null (default)
		AssertNull("RSAOAEPKeyExchangeFormatter.Rng", keyex.Rng);
		AssertEquals("RSAOAEPKeyExchangeFormatter.ToString()", "System.Security.Cryptography.RSAOAEPKeyExchangeFormatter", keyex.ToString ());
	}

	// TestExchangeMin (1)
	public void TestExchangeMin() 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
		byte[] M = { 0x01 };
		try {
			byte[] EM = keyex.CreateKeyExchange (M);
			AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
			byte[] Mback = keyback.DecryptKeyExchange (EM);
			AssertEquals ("RSAOAEPKeyExchangeFormatter Min", M, Mback);
		}
		catch (CryptographicException) {
			// not supported by every version of Windows
			// Minimum: Windows 2000 + High Encryption Pack
		}
	}

	// test with a message 128 bits (16 bytes) long
	public void TestExchange128() 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49 };
		try {
			byte[] EM = keyex.CreateKeyExchange (M);
			AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
			byte[] Mback = keyback.DecryptKeyExchange (EM);
			AssertEquals ("RSAOAEPKeyExchangeFormatter 128", M, Mback);
		}
		catch (CryptographicException) {
			// not supported by every version of Windows
			// Minimum: Windows 2000 + High Encryption Pack
		}
	}

	// test with a message 160 bits (20 bytes) long
	public void TestExchange192() 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49, 0x00, 0x00, 0x00, 0x00 };
		try {
			byte[] EM = keyex.CreateKeyExchange (M);
			AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
			byte[] Mback = keyback.DecryptKeyExchange (EM);
			AssertEquals ("RSAOAEPKeyExchangeFormatter 192", M, Mback);
		}
		catch (CryptographicException) {
			// not supported by every version of Windows
			// Minimum: Windows 2000 + High Encryption Pack
		}
	}

	// Max = (key size in bytes) - 2 * (hash length) - 2
	public void TestExchangeMax() 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
		// use SHA1 internaly
		byte[] M = new byte [(key.KeySize >> 3) - 2 * 20 - 2];
		try {
			byte[] EM = keyex.CreateKeyExchange (M);
			AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
			byte[] Mback = keyback.DecryptKeyExchange (EM);
			AssertEquals ("RSAOAEPKeyExchangeFormatter Max", M, Mback);
		}
		catch (CryptographicException) {
			// not supported by every version of Windows
			// Minimum: Windows 2000 + High Encryption Pack
		}
	}

	// TestExchangeTooBig
	public void TestExchangeTooBig() 
	{
		AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
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
