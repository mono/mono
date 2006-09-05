//
// CipherModeTest.cs - NUnit Test Cases for CipherMode
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class CipherModeTest {

		// Enum tests

		[Test]
		public void CipherModeEnum ()
		{
			Assert.AreEqual (1, (int)CipherMode.CBC, "CBC");
			Assert.AreEqual (4, (int)CipherMode.CFB, "CFB");
			Assert.AreEqual (5, (int)CipherMode.CTS, "CTS");
			Assert.AreEqual (2, (int)CipherMode.ECB, "ECB");
			Assert.AreEqual (3, (int)CipherMode.OFB, "OFB");
		}

		// SymmetricAlgorithm tests

		private byte[] GetKey (SymmetricAlgorithm sa)
		{
			byte[] key = new byte [sa.KeySize >> 3];
			// no weak key this way (DES, TripleDES)
			for (byte i = 0; i < key.Length; i++)
				key [i] = i;
			return key;
		}

		private byte[] GetIV (SymmetricAlgorithm sa)
		{
			return new byte [sa.BlockSize >> 3];
		}

		private string Roundtrip (SymmetricAlgorithm sa, CipherMode mode) 
		{
			sa.Key = GetKey (sa);
			sa.IV = GetIV (sa);
			sa.Mode = mode;

			// two full blocks
			int bs = (sa.BlockSize >> 3) * 2;
			byte[] data = new byte [bs]; // in bytes
			ICryptoTransform enc = sa.CreateEncryptor ();
			byte[] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			string result = BitConverter.ToString (encdata);

			ICryptoTransform dec = sa.CreateDecryptor ();
			byte[] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);

			for (int i = 0; i < bs; i++)
				Assert.AreEqual (data [i], decdata [i], i.ToString ());

			return result;
		}

		[Test]
		public void DES_ECB () 
		{
			Assert.AreEqual ("A5-17-3A-D5-95-7B-43-70-A5-17-3A-D5-95-7B-43-70-E4-81-A8-D3-97-14-D0-DE",
				Roundtrip (DES.Create (), CipherMode.ECB), "Encrypted data");
		}

		[Test]
		public void DES_CBC ()
		{
			Assert.AreEqual ("A5-17-3A-D5-95-7B-43-70-79-6F-FD-B4-90-21-70-9D-FF-C8-76-01-24-7C-C3-82",
				Roundtrip (DES.Create (), CipherMode.CBC), "Encrypted data");
		}

		[Test]
		public void DES_CFB ()
		{
			Assert.AreEqual ("A5-AA-9B-16-02-77-16-A1-86-BC-38-B6-E5-BA-53-4C-A5-F9-49-21-A9-8E-84-A0",
				Roundtrip (DES.Create (), CipherMode.CFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DES_OFB ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (DES.Create (), CipherMode.OFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DES_CTS ()
		{
			Assert.AreEqual ("not implemented in any released framework", 
				Roundtrip (DES.Create (), CipherMode.CTS), "Encrypted data");
		}

		[Test]
		public void RC2_ECB ()
		{
			Assert.AreEqual ("9C-4B-FE-6D-FE-73-9C-2B-9C-4B-FE-6D-FE-73-9C-2B-AB-C5-6E-FB-C4-0E-63-34",
				Roundtrip (RC2.Create (), CipherMode.ECB), "Encrypted data");
		}

		[Test]
		public void RC2_CBC ()
		{
			Assert.AreEqual ("9C-4B-FE-6D-FE-73-9C-2B-52-8F-C8-47-2B-66-F9-70-B2-67-CF-23-D7-D7-6D-A6",
				Roundtrip (RC2.Create (), CipherMode.CBC), "Encrypted data");
		}

		[Test]
		public void RC2_CFB ()
		{
			Assert.AreEqual ("9C-5A-41-95-9A-15-12-C2-54-1C-9C-6C-4B-65-A0-36-DD-7F-2B-0D-D5-D2-C0-CD",
				Roundtrip (RC2.Create (), CipherMode.CFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RC2_OFB ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (RC2.Create (), CipherMode.OFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RC2_CTS ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (RC2.Create (), CipherMode.CTS), "Encrypted data");
		}

		[Test]
		public void Rijndael_ECB ()
		{
			Assert.AreEqual ("F2-90-00-B6-2A-49-9F-D0-A9-F3-9A-6A-DD-2E-77-80-F2-90-00-B6-2A-49-9F-D0-A9-F3-9A-6A-DD-2E-77-80-9F-3B-75-04-92-6F-8B-D3-6E-31-18-E9-03-A4-CD-4A",
				Roundtrip (Rijndael.Create (), CipherMode.ECB), "Encrypted data");
		}

		[Test]
		public void Rijndael_CBC ()
		{
			Assert.AreEqual ("F2-90-00-B6-2A-49-9F-D0-A9-F3-9A-6A-DD-2E-77-80-D4-E9-69-25-C0-BF-CF-FB-52-F8-A1-87-EE-77-4A-AB-C3-28-91-ED-46-6E-6F-98-C1-4D-65-14-ED-9D-1E-5B",
				Roundtrip (Rijndael.Create (), CipherMode.CBC), "Encrypted data");
		}
#if NET_2_0
		[Test]
		public void Rijndael_CFB ()
		{
			Assert.AreEqual ("F2-90-00-B6-2A-49-9F-D0-A9-F3-9A-6A-DD-2E-77-80-D4-E9-69-25-C0-BF-CF-FB-52-F8-A1-87-EE-77-4A-AB-26-03-7F-B7-B5-88-AE-0A-F8-AF-1E-CF-9C-F5-3A-8A",
				Roundtrip (Rijndael.Create (), CipherMode.CFB), "Encrypted data");
		}
#else
		// CFB was confused with OFB in Fx 1.0 and 1.1 (and wasn't supported because of that)
		// However Mono does support it (because the same code is executed for all managed ciphers).
		[Test]
		public void Rijndael_CFB ()
		{
			try {
				Assert.AreEqual ("F2-90-00-B6-2A-49-9F-D0-A9-F3-9A-6A-DD-2E-77-80-D4-E9-69-25-C0-BF-CF-FB-52-F8-A1-87-EE-77-4A-AB-26-03-7F-B7-B5-88-AE-0A-F8-AF-1E-CF-9C-F5-3A-8A",
					Roundtrip (Rijndael.Create (), CipherMode.CFB), "Encrypted data");
			}
			catch (CryptographicException) {
				// we assume this is the bugged MS implementation
			}
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Rijndael_OFB ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (Rijndael.Create (), CipherMode.OFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Rijndael_CTS ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (Rijndael.Create (), CipherMode.CTS), "Encrypted data");
		}

		[Test]
		public void TripleDES_ECB ()
		{
			Assert.AreEqual ("89-4B-C3-08-54-26-A4-41-89-4B-C3-08-54-26-A4-41-A3-CF-6E-C8-8B-D9-7D-73",
				Roundtrip (TripleDES.Create (), CipherMode.ECB), "Encrypted data");
		}

		[Test]
		public void TripleDES_CBC ()
		{
			Assert.AreEqual ("89-4B-C3-08-54-26-A4-41-06-8E-DF-B5-F0-23-AB-B4-76-40-68-9A-26-7D-8D-6E",
				Roundtrip (TripleDES.Create (), CipherMode.CBC), "Encrypted data");
		}

		[Test]
		public void TripleDES_CFB ()
		{
			Assert.AreEqual ("89-9F-00-9D-26-BB-21-59-85-6D-A2-BF-15-FE-73-53-1F-CE-35-26-5B-DF-43-26",
				Roundtrip (TripleDES.Create (), CipherMode.CFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void TripleDES_OFB ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (TripleDES.Create (), CipherMode.OFB), "Encrypted data");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void TripleDES_CTS ()
		{
			Assert.AreEqual ("not implemented in any released framework",
				Roundtrip (TripleDES.Create (), CipherMode.CTS), "Encrypted data");
		}
	}
}
