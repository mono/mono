//
// RSAPKCS1SignatureFormatterTest.cs - NUnit tests for PKCS#1 v.1.5 signature.
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
	public class RSAPKCS1SignatureFormatterTest : Assertion {

		private static byte[] signatureRSASHA256 = { 0x0F, 0xE3, 0x15, 0x5B, 0x4D, 0xA1, 0xB4, 0x13, 0x93, 0x91, 0x1E, 0x17, 0xF9, 0x36, 0xB3, 0x2C, 0xAC, 0x51, 0x77, 0xBC, 0x86, 0x21, 0xB0, 0x69, 0x75, 0x57, 0xAF, 0xB0, 0xAD, 0xF9, 0x42, 0xF5, 0x58, 0xBC, 0xD5, 0x61, 0xD5, 0x14, 0x8E, 0xC6, 0xE0, 0xB3, 0xB5, 0x51, 0xCD, 0x17, 0x68, 0x58, 0x27, 0x74, 0x8A, 0xA7, 0x88, 0xB9, 0x24, 0xD6, 0xE4, 0xC4, 0x93, 0x82, 0x95, 0xB4, 0x36, 0x14, 0x48, 0xA7, 0xF6, 0x27, 0x87, 0xEB, 0xD8, 0xB9, 0x75, 0x14, 0x75, 0xFB, 0x6E, 0xA1, 0xF7, 0xAB, 0xA6, 0x78, 0x32, 0xEF, 0x1A, 0x23, 0x60, 0xD3, 0x0C, 0x8D, 0xFE, 0x89, 0x72, 0xB7, 0x93, 0x6D, 0x00, 0x25, 0xED, 0xF5, 0x55, 0x66, 0xA8, 0x52, 0x7F, 0x20, 0xFD, 0x77, 0xDA, 0x10, 0x77, 0xE9, 0xF0, 0x58, 0x8D, 0xE6, 0x3A, 0x5A, 0x00, 0x83, 0x64, 0x42, 0xA5, 0x15, 0x79, 0x3C, 0xB0, 0x8F };
		private static byte[] signatureRSASHA384 = { 0x86, 0x20, 0x2A, 0xB6, 0xA8, 0x0F, 0x59, 0x42, 0xCA, 0x83, 0xC3, 0x46, 0x2C, 0xA9, 0x2E, 0x62, 0x73, 0x2C, 0xEE, 0x52, 0xA5, 0xAE, 0x4F, 0xFD, 0xB1, 0x1F, 0xFA, 0x0C, 0x71, 0x4A, 0xFD, 0xE2, 0xAC, 0x64, 0x1C, 0x63, 0x41, 0xB8, 0x43, 0x3F, 0x8A, 0xF3, 0x7E, 0x1C, 0x25, 0xBE, 0xEE, 0xFC, 0x7C, 0xCB, 0x33, 0x72, 0x3B, 0x91, 0x1F, 0xF3, 0x78, 0xC2, 0xD0, 0xEA, 0xDF, 0x69, 0xE9, 0x31, 0x2F, 0x39, 0x32, 0x5F, 0x4A, 0x51, 0xAE, 0x24, 0x9E, 0x96, 0x77, 0xFB, 0x16, 0xC4, 0xDD, 0x98, 0xDA, 0xA9, 0x9D, 0xA0, 0x7C, 0x2C, 0x95, 0x12, 0x53, 0x1F, 0x7B, 0x23, 0xEE, 0x78, 0x95, 0x57, 0xFF, 0x02, 0x57, 0x2B, 0x4A, 0x3E, 0x62, 0x6A, 0xC0, 0x99, 0xDF, 0x4B, 0x7E, 0xBF, 0x86, 0xC4, 0xFB, 0x8E, 0xF3, 0x70, 0xA2, 0xEE, 0x7B, 0xCA, 0x8B, 0x22, 0xA4, 0x07, 0xBA, 0xBD, 0x16, 0xA9 };
		private static byte[] signatureRSASHA512 = { 0xB7, 0x7E, 0x7E, 0xEF, 0x95, 0xCE, 0xE8, 0x9D, 0x0F, 0x40, 0x35, 0x50, 0x88, 0xFE, 0x8B, 0xA3, 0x26, 0xD3, 0x9E, 0xA7, 0x82, 0x23, 0x1A, 0x46, 0x13, 0x46, 0x81, 0x59, 0xD1, 0x24, 0x45, 0xAC, 0x53, 0xEF, 0x5A, 0x06, 0x31, 0xA7, 0xC2, 0x76, 0xDC, 0x2B, 0x60, 0x69, 0xB1, 0x36, 0x1D, 0xE1, 0xFC, 0xD5, 0x9A, 0x01, 0x71, 0x08, 0xE9, 0x0C, 0xAE, 0xF4, 0x29, 0xCF, 0xC4, 0xB0, 0x60, 0xA4, 0xBE, 0x1C, 0x9B, 0x05, 0x2A, 0xA9, 0x6A, 0x12, 0xFF, 0x73, 0x84, 0x5C, 0xA8, 0x74, 0x5B, 0x9C, 0xA2, 0x07, 0x9D, 0x73, 0xB8, 0xE3, 0x20, 0x16, 0x3C, 0x47, 0x8F, 0x27, 0x7A, 0x48, 0xAF, 0x01, 0x07, 0xA0, 0x6A, 0x2D, 0x71, 0xAD, 0xDD, 0x8B, 0x68, 0xC8, 0x32, 0x61, 0x95, 0x68, 0x22, 0x1B, 0x8B, 0xD9, 0x86, 0xA7, 0xBE, 0x60, 0x06, 0x70, 0x7C, 0xED, 0x51, 0x28, 0x66, 0x28, 0xF0, 0x65 };

		private RSAPKCS1SignatureFormatter fmt;

		public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			AllTests.AssertEquals (msg, array1, array2);
		}

		[Test]
		public void ConstructorEmpty () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			AssertNotNull ("RSAPKCS1SignatureFormatter()", fmt);
		}

		[Test]
		public void ConstructorNull () 
		{
			fmt = new RSAPKCS1SignatureFormatter (null);
			AssertNotNull ("RSAPKCS1SignatureFormatter(null)", fmt);
		}

		[Test]
		public void ConstructorRSA () 
		{
			RSA rsa = RSA.Create ();
			fmt = new RSAPKCS1SignatureFormatter (rsa);
			AssertNotNull ("RSAPKCS1SignatureFormatter(rsa)", fmt);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConstructorDSA () 
		{
			DSA dsa = DSA.Create ();
			fmt = new RSAPKCS1SignatureFormatter (dsa);
		}

		[Test]
		public void SetKeyRSA () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetKey (RSA.Create ());
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void SetKeyDSA () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			fmt.SetKey (DSA.Create ());
		}

		[Test]
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
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);
			// good SHA1
			fmt.SetHashAlgorithm ("SHA1");
			byte[] hash = new byte [20];
			byte[] signature = fmt.CreateSignature (hash);
			AssertNotNull ("CreateSignature(SHA1)", signature);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateSignatureRSASHA1BadLength () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// we need the private key 
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRsaKey (true));
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
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);
			// good MD5
			fmt.SetHashAlgorithm ("MD5");
			byte[] hash = new byte [16];
			byte[] signature = fmt.CreateSignature (hash);
			AssertNotNull ("CreateSignature(MD5)", signature);
		}

		private byte[] CreateSignature (string hashName, int hashSize) 
		{
			byte[] signature = null;
			fmt = new RSAPKCS1SignatureFormatter ();
			bool ms = false;
			// we need the private key 
			RSA rsa = RSA.Create ("Mono.Security.Cryptography.RSAManaged");	// only available with Mono::
			if (rsa == null) {
				ms = true;
				rsa = RSA.Create ();
			}
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);
			try {
				HashAlgorithm ha = HashAlgorithm.Create (hashName);
				byte[] data = new byte [ha.HashSize >> 3];
				// this way we get the same results as CreateSignatureHash
				data = ha.ComputeHash (data);

				fmt.SetHashAlgorithm (hashName);
				signature = fmt.CreateSignature (data);
				if (ms)
					Fail ("CreateSignature(" + hashName + ") Expected CryptographicException but got none");
			}
			catch (CryptographicException) {
				if (!ms)
					throw;
			}
			catch (Exception e) {
				if (ms)
					Fail ("CreateSignatureHash(" + hashName + ") Expected CryptographicException but got " + e.ToString ());
			}
			return signature;
		}

		// not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// supported by Mono::
		[Test]
		public void CreateSignatureRSASHA256 () 
		{
			byte[] signature = CreateSignature ("SHA256", 32);
			if (signature != null)
				AssertEquals ("CreateSignature(SHA256)", signatureRSASHA256, signature);
		}

		// not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// supported by Mono::
		[Test]
		public void CreateSignatureRSASHA384 () 
		{
			byte[] signature = CreateSignature ("SHA384", 48);
			if (signature != null)
				AssertEquals ("CreateSignature(SHA384)", signatureRSASHA384, signature);
		}

		// not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// supported by Mono::
		[Test]
		public void CreateSignatureRSASHA512 () 
		{
			byte[] signature = CreateSignature ("SHA512", 64);
			if (signature != null)
				AssertEquals ("CreateSignature(SHA512)", signatureRSASHA512, signature);
		}

		[Test]
		[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
		public void CreateSignatureRSABadHash () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			// we need the private key 
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRsaKey (true));
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
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);
			// good SHA1
			HashAlgorithm hash = SHA1.Create ();
			hash.ComputeHash (data);
			byte[] signature = fmt.CreateSignature (hash);
			AssertNotNull ("CreateSignature(SHA1)", signature);
		}

		[Test]
		public void CreateSignatureHashMD5 () 
		{
			fmt = new RSAPKCS1SignatureFormatter ();
			byte[] data = new byte [16];
			// we need the private key 
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);
			// good MD5
			HashAlgorithm hash = MD5.Create ();
			hash.ComputeHash (data);
			byte[] signature = fmt.CreateSignature (hash);
			AssertNotNull ("CreateSignature(MD5)", signature);
		}

		private byte[] CreateSignatureHash (string hashName) 
		{
			byte[] signature = null;
			fmt = new RSAPKCS1SignatureFormatter ();
			bool ms = false;
			// we need the private key 
			RSA rsa = RSA.Create ("Mono.Security.Cryptography.RSAManaged");	// only available with Mono::
			if (rsa == null) {
				ms = true;
				rsa = RSA.Create ();
			}
			rsa.ImportParameters (AllTests.GetRsaKey (true));
			fmt.SetKey (rsa);
			try {
				HashAlgorithm hash = HashAlgorithm.Create (hashName);
				byte[] data = new byte [(hash.HashSize >> 3)];
				hash.ComputeHash (data);
				signature = fmt.CreateSignature (hash);
				if (ms)
					Fail ("CreateSignatureHash(" + hashName + ") Expected CryptographicException but got none");
			}
			catch (CryptographicException) {
				if (!ms)
					throw;
			}
			catch (Exception e) {
				if (ms)
					Fail ("CreateSignatureHash(" + hashName + ") Expected CryptographicException but got " + e.ToString ());
			}
			return signature;																																																																																																      
		}

		// not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// supported by Mono::
		[Test]
		public void CreateSignatureHashSHA256 () 
		{
			byte[] signature = CreateSignatureHash ("SHA256");
			if (signature != null)
				AssertEquals ("CreateSignatureHash(SHA256)", signatureRSASHA256, signature);
		}

		// not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// supported by Mono::
		[Test]
		public void CreateSignatureHashSHA384 () 
		{
			byte[] signature = CreateSignatureHash ("SHA384");
			if (signature != null)
				AssertEquals ("CreateSignatureHash(SHA384)", signatureRSASHA384, signature);
		}

		// not supported using MS framework 1.0 and 1.1 (CryptographicException)
		// supported by Mono::
		[Test]
		public void CreateSignatureHashSHA512 () 
		{
			byte[] signature = CreateSignatureHash ("SHA512");
			if (signature != null)
				AssertEquals ("CreateSignatureHash(SHA512)", signatureRSASHA512, signature);
		}
	}
}
