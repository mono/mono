//
// RSAOAEPKeyExchangeFormatterTest.cs - NUnit Test Cases for RSAOAEPKeyExchangeFormatter
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
	public class RSAOAEPKeyExchangeFormatterTest {

		protected static RSA key;

		[SetUp]
		void SetUp () 
		{
			// generating a keypair is REALLY long and the MS framework
			// makes sure that we generate one (even if create an object
			// to import an existing key). Mono is smarter in this case
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
			RSAOAEPKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter ();
			keyex.SetKey (key);
			Assertion.AssertNull ("RSAOAEPKeyExchangeFormatter.Parameter", keyex.Parameter);
			Assertion.AssertNull ("RSAOAEPKeyExchangeFormatter.Parameters", keyex.Parameters);
			Assertion.AssertNull ("RSAOAEPKeyExchangeFormatter.Rng", keyex.Rng);
			Assertion.AssertEquals ("RSAOAEPKeyExchangeFormatter.ToString()", "System.Security.Cryptography.RSAOAEPKeyExchangeFormatter", keyex.ToString ());
		}

		// ExchangeMin (1)
		[Test]
		public void ExchangeMin() 
		{
			AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
			byte[] M = { 0x01 };
			try {
				byte[] EM = keyex.CreateKeyExchange (M);
				AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
				byte[] Mback = keyback.DecryptKeyExchange (EM);
				AssertEquals ("RSAOAEPKeyExchangeFormatter Min", M, Mback);
			}
			catch (CryptographicException ce) {
				// not supported by every version of Windows - Minimum: Windows XP
				Console.WriteLine (ce.Message + " (" + Environment.OSVersion.ToString () + ")");
			}
		}

		// test with a message 128 bits (16 bytes) long
		[Test]
		public void Exchange128() 
		{
			AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
			byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49 };
			try {
				byte[] EM = keyex.CreateKeyExchange (M);
				AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
				byte[] Mback = keyback.DecryptKeyExchange (EM);
				AssertEquals ("RSAOAEPKeyExchangeFormatter 128", M, Mback);
			}
			catch (CryptographicException ce) {
				// not supported by every version of Windows - Minimum: Windows XP
				Console.WriteLine (ce.Message + " (" + Environment.OSVersion.ToString () + ")");
			}
		}

		// test with a message 160 bits (20 bytes) long
		[Test]
		public void Exchange192() 
		{
			AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
			byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49, 0x00, 0x00, 0x00, 0x00 };
			try {
				byte[] EM = keyex.CreateKeyExchange (M);
				AsymmetricKeyExchangeDeformatter keyback = new RSAOAEPKeyExchangeDeformatter (key);
				byte[] Mback = keyback.DecryptKeyExchange (EM);
				AssertEquals ("RSAOAEPKeyExchangeFormatter 192", M, Mback);
			}
			catch (CryptographicException ce) {
				// not supported by every version of Windows - Minimum: Windows XP
				Console.WriteLine (ce.Message + " (" + Environment.OSVersion.ToString () + ")");
			}
		}

		// Max = (key size in bytes) - 2 * (hash length) - 2
		[Test]
		public void ExchangeMax() 
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
			catch (CryptographicException ce) {
				// not supported by every version of Windows - Minimum: Windows XP
				Console.WriteLine (ce.Message + " (" + Environment.OSVersion.ToString () + ")");
			}
		}

		// TestExchangeTooBig
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ExchangeTooBig() 
		{
			AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
			byte[] M = new byte [(key.KeySize >> 3)- 10];
			byte[] EM = keyex.CreateKeyExchange (M);
		}
	}
}
