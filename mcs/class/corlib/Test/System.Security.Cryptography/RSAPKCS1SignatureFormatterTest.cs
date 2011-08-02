//
// RSAPKCS1SignatureFormatterTest.cs - NUnit tests for PKCS#1 v.1.5 signature.
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005, 2011 Novell, Inc (http://www.novell.com)
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
	public class RSAPKCS1SignatureFormatterTest {

		private static byte[] signatureRSASHA256 = { 0xAD, 0x6E, 0x29, 0xC8, 0x7D, 0xFE, 0x5F, 0xB3, 0x92, 0x07, 0x4C, 0x51, 0x08, 0xC5, 0x91, 0xA2, 0xCF, 0x7E, 0xA6, 0x05, 0x66, 0x85, 0xA3, 0x8E, 0x7C, 0xB0, 0xCA, 0x93, 0x4F, 0x4E, 0xF5, 0x45, 0x0F, 0xED, 0x46, 0xFB, 0x34, 0xBC, 0x8A, 0x6A, 0x48, 0xD9, 0x76, 0x28, 0xE1, 0x68, 0xA0, 0x1F, 0x7F, 0x3E, 0xCC, 0x0A, 0x5F, 0x06, 0x8E, 0xEB, 0xB7, 0xA7, 0x48, 0x6B, 0x92, 0x1A, 0x7A, 0x66, 0x42, 0x4F, 0x0B, 0xC1, 0x19, 0x96, 0xAC, 0x67, 0xA0, 0x6C, 0x3E, 0x39, 0xD2, 0xEB, 0xCA, 0xD7, 0x12, 0x29, 0x46, 0x0A, 0x60, 0x70, 0xA9, 0x2B, 0x80, 0x9F, 0xCD, 0x08, 0x02, 0xEB, 0xA5, 0x62, 0xEC, 0xAB, 0xBB, 0x64, 0x8B, 0x2D, 0xB9, 0x55, 0x0A, 0xE3, 0x5A, 0x2C, 0xDA, 0x54, 0xD4, 0x79, 0x0A, 0x8D, 0xB6, 0x57, 0x05, 0xF7, 0x6C, 0x6D, 0xB7, 0xD8, 0xB4, 0x07, 0xC4, 0xCD, 0x79, 0xD4 };
		private static byte[] signatureRSASHA384 = { 0x53, 0x80, 0xFD, 0x26, 0x8F, 0xCF, 0xE5, 0x44, 0x55, 0x4A, 0xC5, 0xB2, 0x46, 0x78, 0x89, 0x42, 0xF8, 0x51, 0xB8, 0x4D, 0x3B, 0xCA, 0x48, 0x5A, 0x36, 0x9F, 0x62, 0x01, 0x72, 0x1E, 0xD8, 0x2D, 0xC2, 0x2D, 0x3E, 0x67, 0x1C, 0x5D, 0x89, 0xAB, 0x39, 0x8D, 0x07, 0xC8, 0xD4, 0x47, 0x97, 0xA4, 0x68, 0x7A, 0x87, 0xA4, 0xCF, 0x7B, 0x32, 0x4F, 0xD3, 0xD1, 0x90, 0xDC, 0x76, 0x23, 0x51, 0xA7, 0xEE, 0xFC, 0x7F, 0xDF, 0x3C, 0xB0, 0x05, 0xF3, 0xE3, 0xAA, 0x96, 0x30, 0xE0, 0xE4, 0x8B, 0x09, 0xB1, 0x78, 0xAC, 0x99, 0xDB, 0xC5, 0x0E, 0xFA, 0xAB, 0x4F, 0xA1, 0x02, 0xCA, 0x77, 0x93, 0x74, 0x5A, 0xB8, 0x71, 0x9C, 0x3E, 0x2E, 0xAE, 0x62, 0xC7, 0xE5, 0xBF, 0xDA, 0xFE, 0x31, 0xA7, 0x91, 0xC0, 0x04, 0xE3, 0x95, 0xCB, 0x3F, 0x54, 0xA8, 0x09, 0x25, 0xF7, 0x09, 0x78, 0xE6, 0x09, 0x84 };
		private static byte[] signatureRSASHA512 = { 0xA8, 0xD0, 0x24, 0xCB, 0xA2, 0x4B, 0x5E, 0x0D, 0xBC, 0x3F, 0x6F, 0x0F, 0x8D, 0xE4, 0x31, 0x9E, 0x37, 0x84, 0xE0, 0x31, 0x5B, 0x63, 0x24, 0xC5, 0xA9, 0x05, 0x41, 0xAA, 0x69, 0x02, 0x8F, 0xC1, 0x57, 0x06, 0x1F, 0xBF, 0x3B, 0x8B, 0xC8, 0x86, 0xB3, 0x02, 0xEA, 0xF1, 0x75, 0xE4, 0x70, 0x21, 0x1E, 0x16, 0x4C, 0x37, 0xB2, 0x31, 0x78, 0xD0, 0xA0, 0x88, 0xA5, 0x1D, 0x5D, 0x8F, 0xBC, 0xC3, 0x87, 0x94, 0x4B, 0x8F, 0x4E, 0x92, 0xBC, 0x80, 0xF8, 0xA5, 0x90, 0xF7, 0xA0, 0x6D, 0x96, 0x61, 0x65, 0x0D, 0xD5, 0x3F, 0xD7, 0x4F, 0x07, 0x58, 0x40, 0xB8, 0xA4, 0x14, 0x14, 0x55, 0x39, 0x4F, 0xF0, 0xB5, 0x56, 0x99, 0xC8, 0x52, 0x0C, 0xDD, 0xBA, 0x8D, 0xFB, 0x06, 0x83, 0x6E, 0x79, 0x25, 0x75, 0xEF, 0x0D, 0x26, 0x14, 0x3A, 0xBB, 0x62, 0x29, 0x21, 0xF6, 0x4B, 0x9E, 0x87, 0x28, 0x57 };

		private static RSA rsa;
		private static DSA dsa;

		private RSAPKCS1SignatureFormatter fmt;

		[SetUp]
		public void SetUp () 
		{
			if (rsa == null) {
				rsa = RSA.Create ();
				rsa.ImportParameters (AllTests.GetRsaKey (true));
			}
			if (dsa == null)
				dsa = DSA.Create ();
		}

		public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			Assert.AreEqual (array1, array2, msg);
		}

		[Test]
		public void ConstructorEmpty () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			Assert.IsNotNull (fmt);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull () 
		{
			new RSAPKCS1SignatureFormatter (null);
		}

		[Test]
		public void ConstructorRSA () 
		{
			fmt = new RSAPKCS1SignatureFormatter (rsa);
			Assert.IsNotNull (fmt);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConstructorDSA () 
		{
			fmt = new RSAPKCS1SignatureFormatter (dsa);
		}

		[Test]
		public void SetKeyRSA () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetKey (rsa);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void SetKeyDSA () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetKey (dsa);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetKeyNull ()
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetKey (null);
		}

		[Test]
		public void SetHashAlgorithmSHA1 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetHashAlgorithm ("SHA1");
		}

		[Test]
		public void SetHashAlgorithmMD5 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetHashAlgorithm ("MD5");
		}

		[Test]
		public void SetHashAlgorithmSHA256 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetHashAlgorithm ("SHA256");
		}

		[Test]
		public void SetHashAlgorithmSHA384 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetHashAlgorithm ("SHA384");
		}

		[Test]
		public void SetHashAlgorithmSHA512 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetHashAlgorithm ("SHA512");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetHashAlgorithmNull () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetHashAlgorithm (null);
		}

		// see: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpcongeneratingsignatures.asp

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void CreateSignatureNullHash () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetKey (rsa);
			byte[] hash = null;
			byte[] signature = fmt.CreateSignature (hash);
		}

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void CreateSignatureNoHashAlgorithm () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// no hash algorithm
			byte[] hash = new byte [1];
			byte[] signature = fmt.CreateSignature (hash);
		}

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void CreateSignatureNoKey () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// no key
			fmt.SetHashAlgorithm ("SHA1");
			byte[] hash = new byte [20];
			byte[] signature = fmt.CreateSignature (hash);
		}

		[Test]
		public void CreateSignatureRSASHA1 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// we need the private key 
			fmt.SetKey (rsa);
			// good SHA1
			fmt.SetHashAlgorithm ("SHA1");
			byte[] hash = new byte [20];
			byte[] signature = fmt.CreateSignature (hash);
			Assert.IsNotNull (fmt);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateSignatureRSASHA1BadLength () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// we need the private key 
			fmt.SetKey (rsa);
			// wrong length SHA1
			fmt.SetHashAlgorithm ("SHA1");
			byte[] hash = new byte [19];
			byte[] signature = fmt.CreateSignature (hash);
		}

		[Test]
		public void CreateSignatureRSAMD5 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// we need the private key 
			fmt.SetKey (rsa);
			// good MD5
			fmt.SetHashAlgorithm ("MD5");
			byte[] hash = new byte [16];
			byte[] signature = fmt.CreateSignature (hash);
			Assert.IsNotNull (fmt);
		}

		private byte[] CreateSignature (string hashName, int hashSize) 
		{
			fmt = new RSAPKCS1SignatureFormatter ();

			// we need the private key 
			RSA rsa = RSA.Create ("Mono.Security.Cryptography.RSAManaged");	// only available with Mono::
			if (rsa == null) {
				rsa = RSA.Create ();
			}
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);

			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			byte[] data = new byte [ha.HashSize >> 3];
			// this way we get the same results as CreateSignatureHash
			data = ha.ComputeHash (data);

			fmt.SetHashAlgorithm (hashName);
			return fmt.CreateSignature (data);
		}

		// historical note: this was not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// but was supported by Mono:: - it's also supported on 2.0+ (at least on Win XP+)
		[Test]
		public void CreateSignatureRSASHA256 () 
		{
			byte[] signature = CreateSignature ("SHA256", 32);
			AssertEquals ("CreateSignature(SHA256)", signatureRSASHA256, signature);
		}

		// historical note: this was not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// but was supported by Mono:: - it's also supported on 2.0+ (at least on Win XP+)
		[Test]
		public void CreateSignatureRSASHA384 () 
		{
			byte[] signature = CreateSignature ("SHA384", 48);
			AssertEquals ("CreateSignature(SHA384)", signatureRSASHA384, signature);
		}

		// historical note: this was not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// but was supported by Mono:: - it's also supported on 2.0+ (at least on Win XP+)
		[Test]
		public void CreateSignatureRSASHA512 () 
		{
			byte[] signature = CreateSignature ("SHA512", 64);
			AssertEquals ("CreateSignature(SHA512)", signatureRSASHA512, signature);
		}

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void CreateSignatureRSABadHash () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// we need the private key 
			fmt.SetKey (rsa);
			// null (bad ;-)
			byte[] hash = null;
			byte[] signature  = fmt.CreateSignature (hash);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateSignatureHashBadHash () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			HashAlgorithm hash = null;
			byte[] data = new byte [20];
			// no hash algorithm
			byte[] signature = fmt.CreateSignature (hash);
		}

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void CreateSignatureHashNoKey () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			byte[] data = new byte [20];
			// no key
			HashAlgorithm hash = SHA1.Create ();
			hash.ComputeHash (data);
			byte[] signature = fmt.CreateSignature (hash);
		}

		[Test]
		public void CreateSignatureHashSHA1 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			byte[] data = new byte [20];
			// we need the private key 
			fmt.SetKey (rsa);
			// good SHA1
			HashAlgorithm hash = SHA1.Create ();
			hash.ComputeHash (data);
			byte[] signature = fmt.CreateSignature (hash);
			Assert.IsNotNull (signature);
		}

		[Test]
		public void CreateSignatureHashMD5 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			byte[] data = new byte [16];
			// we need the private key 
			fmt.SetKey (rsa);
			// good MD5
			HashAlgorithm hash = MD5.Create ();
			hash.ComputeHash (data);
			byte[] signature = fmt.CreateSignature (hash);
			Assert.IsNotNull (signature);
		}

		private byte[] CreateSignatureHash (string hashName) 
		{
			fmt = new RSAPKCS1SignatureFormatter ();

			// we need the private key 
			RSA rsa = RSA.Create ("Mono.Security.Cryptography.RSAManaged");	// only available with Mono::
			if (rsa == null) {
				rsa = RSA.Create ();
			}
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);

			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			byte[] data = new byte [(hash.HashSize >> 3)];
			hash.ComputeHash (data);
			return fmt.CreateSignature (hash);
		}

		// historical note: this was not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// but was supported by Mono:: - it's also supported on 2.0+ (at least on Win XP+)
		[Test]
		public void CreateSignatureHashSHA256 () 
		{
			byte[] signature = CreateSignatureHash ("SHA256");
			AssertEquals ("CreateSignatureHash(SHA256)", signatureRSASHA256, signature);
		}

		// historical note: this was not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// but was supported by Mono:: - it's also supported on 2.0+ (at least on Win XP+)
		[Test]
		public void CreateSignatureHashSHA384 () 
		{
			byte[] signature = CreateSignatureHash ("SHA384");
			AssertEquals ("CreateSignatureHash(SHA384)", signatureRSASHA384, signature);
		}

		// historical note: this was not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// but was supported by Mono:: - it's also supported on 2.0+ (at least on Win XP+)
		[Test]
		public void CreateSignatureHashSHA512 () 
		{
			byte[] signature = CreateSignatureHash ("SHA512");
			AssertEquals ("CreateSignatureHash(SHA512)", signatureRSASHA512, signature);
		}
	}
}
