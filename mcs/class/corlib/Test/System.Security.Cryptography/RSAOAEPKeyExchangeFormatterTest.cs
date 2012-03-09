//
// RSAOAEPKeyExchangeFormatterTest.cs - NUnit Test Cases for RSAOAEPKeyExchangeFormatter
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
	public class RSAOAEPKeyExchangeFormatterTest {

		protected static RSA key;

		[SetUp]
		public void SetUp () 
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
			Assert.IsNull (keyex.Parameter, "RSAOAEPKeyExchangeFormatter.Parameter");
			Assert.IsNull (keyex.Parameters, "RSAOAEPKeyExchangeFormatter.Parameters");
			Assert.IsNull (keyex.Rng, "RSAOAEPKeyExchangeFormatter.Rng");
			Assert.AreEqual ("System.Security.Cryptography.RSAOAEPKeyExchangeFormatter", keyex.ToString ());
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
				byte[] EM = keyex.CreateKeyExchange (M, typeof (Rijndael));
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

		[Test]
		public void Parameter () 
		{
			RSAOAEPKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
			keyex.Parameter = new byte [1];
			Assert.AreEqual (1, keyex.Parameter.Length);
		}

		[Test]
		public void Rng () 
		{
			RSAOAEPKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (key);
			Assert.IsNull (keyex.Rng, "Rng");
			keyex.Rng = RandomNumberGenerator.Create ();
			Assert.IsNotNull (keyex.Rng, "Rng 2");
		}

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void ExchangeNoKey () 
		{
			AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter ();
			byte[] M = keyex.CreateKeyExchange (new byte [16]);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ExchangeDSAKey () 
		{
			DSA dsa = DSA.Create ();
			AsymmetricKeyExchangeFormatter keyex = new RSAOAEPKeyExchangeFormatter (dsa);
		}
	}
}
