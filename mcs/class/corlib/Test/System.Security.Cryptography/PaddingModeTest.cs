//
// PaddingModeTest.cs - NUnit Test Cases for PaddingMode
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
	public class PaddingModeTest {

		private static byte[] key1 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
		private static byte[] key2 = { 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01 };
		private static byte[] key3 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };

		public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			AllTests.AssertEquals (msg, array1, array2);
		}

		protected byte[] CombineKeys (byte[] key1, byte[] key2, byte[] key3) 
		{
			int k1l = key1.Length;
			int k2l = key2.Length;
			int k3l = key3.Length;
			byte[] key = new byte [k1l + k2l + k3l];
			Array.Copy (key1, 0, key, 0, k1l);
			Array.Copy (key2, 0, key, k1l, k2l);
			Array.Copy (key3, 0, key, k1l + k2l, k3l);
			return key;
		}

		private byte[] Decrypt (SymmetricAlgorithm algo, PaddingMode padding, byte[] data) 
		{
			algo.IV = new byte [algo.BlockSize >> 3];
			algo.Mode = CipherMode.CBC;
			algo.Padding = padding;
			ICryptoTransform ct = algo.CreateDecryptor ();
			return ct.TransformFinalBlock (data, 0, data.Length);
		}

		private byte[] Encrypt (SymmetricAlgorithm algo, PaddingMode padding, byte[] data) 
		{
			algo.IV = new byte [algo.BlockSize >> 3];
			algo.Mode = CipherMode.CBC;
			algo.Padding = padding;
			ICryptoTransform ct = algo.CreateEncryptor ();
			return ct.TransformFinalBlock (data, 0, data.Length);
		}

		private byte[] GetData (byte size) 
		{
			byte[] data = new byte [size];
			for (byte i=0; i < size; i++) {
				data [i] = i;
			}
			return data;
		}

		private TripleDES GetTripleDES () 
		{
			TripleDES tdes = TripleDES.Create ();
			tdes.Key = CombineKeys (key1, key2, key3);
			return tdes;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void TripleDESNone_SmallerThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (7);
			byte[] encdata = Encrypt (tdes, PaddingMode.None, data);
		}

		[Test]
		public void TripleDESNone_ExactlyOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (tdes, PaddingMode.None, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51", BitConverter.ToString (encdata), "TripleDESNone_ExactlyOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.None, encdata);
			AssertEquals ("TripleDESNone_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void TripleDESNone_MoreThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (12);
			byte[] encdata = Encrypt (tdes, PaddingMode.None, data);
		}

		[Test]
		public void TripleDESNone_ExactMultipleBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (tdes, PaddingMode.None, data);
			// note: encrypted data is truncated to a multiple of block size
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51-BF-AB-79-9C-CD-5E-79-40-16-81-0D-6B-40-E6-B2-E9-86-34-8A-9E-5D-56-DA-D1-0C-8C-76-0A-E1-69-A1-0A-B5-B5-7F-FC-5A-D0-6E-6A", BitConverter.ToString (encdata), "TripleDESNone_ExactMultipleBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.None, encdata);
			AssertEquals ("TripleDESNone_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}

		[Test]
		public void TripleDESPKCS7_SmallerThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (7);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("C6-59-0E-E3-7F-26-92-B0", BitConverter.ToString (encdata), "TripleDESPKCS7_SmallerThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_SmallerThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESPKCS7_ExactlyOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51-C0-60-5B-6A-5C-B7-69-62", BitConverter.ToString (encdata), "TripleDESPKCS7_ExactlyOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESPKCS7_MoreThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (12);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51-D9-CB-92-8C-76-89-35-84", BitConverter.ToString (encdata), "TripleDESPKCS7_MoreThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_MoreThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESPKCS7_ExactMultipleBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51-BF-AB-79-9C-CD-5E-79-40-16-81-0D-6B-40-E6-B2-E9-86-34-8A-9E-5D-56-DA-D1-0C-8C-76-0A-E1-69-A1-0A-B5-B5-7F-FC-5A-D0-6E-6A-73-61-63-1C-58-A2-9C-B3", BitConverter.ToString (encdata), "TripleDESPKCS7_ExactMultipleBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_ExactMultipleBlockSize_Decrypt", data, decdata);
		}
		
		[Test]
		public void TripleDESZeros_SmallerThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (7);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			Assert.AreEqual ("B8-5C-5B-A5-06-0B-7E-C6", BitConverter.ToString (encdata), "TripleDESZeros_SmallerThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			Assert.AreEqual ("00-01-02-03-04-05-06-00", BitConverter.ToString (decdata), "TripleDESZeros_SmallerThanOneBlockSize_Decrypt");
		}

		[Test]
		public void TripleDESZeros_ExactlyOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51", BitConverter.ToString (encdata), "TripleDESZeros_ExactlyOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			AssertEquals ("TripleDESZeros_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESZeros_MoreThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (12);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51-6E-75-2E-33-12-09-5D-66", BitConverter.ToString (encdata), "TripleDESZeros_MoreThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			Assert.AreEqual ("00-01-02-03-04-05-06-07-08-09-0A-0B-00-00-00-00", BitConverter.ToString (decdata), "TripleDESZeros_MoreThanOneBlockSize_Decrypt");
		}

		[Test]
		public void TripleDESZeros_ExactMultipleBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			Assert.AreEqual ("23-61-AC-E6-C5-17-10-51-BF-AB-79-9C-CD-5E-79-40-16-81-0D-6B-40-E6-B2-E9-86-34-8A-9E-5D-56-DA-D1-0C-8C-76-0A-E1-69-A1-0A-B5-B5-7F-FC-5A-D0-6E-6A", BitConverter.ToString (encdata), "TripleDESZeros_ExactMultipleBlockSize_Encrypt");
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			AssertEquals ("TripleDESZeros_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}

		private Rijndael GetAES () 
		{
			Rijndael aes = Rijndael.Create ();
			aes.Key = CombineKeys (key1, key2, key3);
			return aes;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RijndaelNone_SmallerThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (aes, PaddingMode.None, data);
		}

		[Test]
		public void RijndaelNone_ExactlyOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (16);
			byte[] encdata = Encrypt (aes, PaddingMode.None, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB", BitConverter.ToString (encdata), "RijndaelNone_ExactlyOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.None, encdata);
			AssertEquals ("RijndaelNone_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RijndaelNone_MoreThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (20);
			byte[] encdata = Encrypt (aes, PaddingMode.None, data);
		}

		[Test]
		public void RijndaelNone_ExactMultipleBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (aes, PaddingMode.None, data);
			// note: encrypted data is truncated to a multiple of block size
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-E1-27-3E-21-5A-BE-D5-12-F4-AF-06-8D-0A-BD-02-64-02-CB-FF-D7-32-19-5E-69-3C-54-C2-8C-A1-D7-72-FF", BitConverter.ToString (encdata), "RijndaelNone_ExactMultipleBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.None, encdata);
			AssertEquals ("RijndaelNone_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}

		[Test]
		public void RijndaelPKCS7_SmallerThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("AB-E0-20-5E-BC-28-A0-B7-A7-56-A3-BF-13-55-13-7E", BitConverter.ToString (encdata), "RijndaelPKCS7_SmallerThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_SmallerThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelPKCS7_ExactlyOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (16);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-60-CE-9F-E0-72-3B-D6-D1-A5-F8-33-D8-25-31-7F-D4", BitConverter.ToString (encdata), "RijndaelPKCS7_ExactlyOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelPKCS7_MoreThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (20);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-06-3F-D3-51-8D-55-E9-2F-02-4A-4E-F2-91-55-31-83", BitConverter.ToString (encdata), "RijndaelPKCS7_MoreThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_MoreThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelPKCS7_ExactMultipleBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-E1-27-3E-21-5A-BE-D5-12-F4-AF-06-8D-0A-BD-02-64-02-CB-FF-D7-32-19-5E-69-3C-54-C2-8C-A1-D7-72-FF-37-42-81-21-47-A7-E0-AA-64-A7-8B-65-25-95-AA-54", BitConverter.ToString (encdata), "RijndaelPKCS7_ExactMultipleBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_ExactMultipleBlockSize_Decrypt", data, decdata);
		}
		
		[Test]
		public void RijndaelZeros_SmallerThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			Assert.AreEqual ("DD-BE-D7-CE-E2-DD-5C-A3-3E-44-A1-76-00-E5-5B-5D", BitConverter.ToString (encdata), "RijndaelZeros_SmallerThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			Assert.AreEqual ("00-01-02-03-04-05-06-07-00-00-00-00-00-00-00-00", BitConverter.ToString (decdata), "RijndaelZeros_SmallerThanOneBlockSize_Decrypt");
		}

		[Test]
		public void RijndaelZeros_ExactlyOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (16);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB", BitConverter.ToString (encdata), "RijndaelZeros_ExactlyOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			AssertEquals ("RijndaelZeros_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelZeros_MoreThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (20);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-04-6C-F7-A5-DE-FF-B4-30-29-7A-0E-04-3B-D4-B8-F2", BitConverter.ToString (encdata), "RijndaelZeros_MoreThanOneBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			Assert.AreEqual ("00-01-02-03-04-05-06-07-08-09-0A-0B-0C-0D-0E-0F-10-11-12-13-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString (decdata), "RijndaelZeros_MoreThanOneBlockSize_Decrypt");
		}

		[Test]
		public void RijndaelZeros_ExactMultipleBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			Assert.AreEqual ("79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-E1-27-3E-21-5A-BE-D5-12-F4-AF-06-8D-0A-BD-02-64-02-CB-FF-D7-32-19-5E-69-3C-54-C2-8C-A1-D7-72-FF", BitConverter.ToString (encdata), "RijndaelZeros_ExactMultipleBlockSize_Encrypt");
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			AssertEquals ("RijndaelZeros_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}

		// Enum tests

		[Test]
		public void PaddingModeEnum ()
		{
#if NET_2_0
			Assert.AreEqual (4, (int)PaddingMode.ANSIX923, "ANSIX923");
			Assert.AreEqual (5, (int)PaddingMode.ISO10126, "ISO10126");
#endif
			Assert.AreEqual (1, (int)PaddingMode.None, "None");
			Assert.AreEqual (2, (int)PaddingMode.PKCS7, "PKCS7");
			Assert.AreEqual (3, (int)PaddingMode.Zeros, "Zeros");
		}

		// SymmetricAlgorithm tests

		private byte[] GetKey (SymmetricAlgorithm sa) 
		{
			byte[] key = new byte [sa.KeySize >> 3];
			// no weak key this way (DES, TripleDES)
			for (byte i=0; i < key.Length; i++)
				key [i] = i;
			return key;
		}

		private byte[] GetIV (SymmetricAlgorithm sa)
		{
			return new byte [sa.BlockSize >> 3];
		}

		private ICryptoTransform GetEncryptor (SymmetricAlgorithm sa, PaddingMode mode) 
		{
			sa.Mode = CipherMode.ECB; // basic (no) mode
			sa.Padding = mode;
			return sa.CreateEncryptor (GetKey (sa), GetIV (sa));
		}

		private ICryptoTransform GetDecryptor (SymmetricAlgorithm sa, PaddingMode mode)
		{
			sa.Mode = CipherMode.ECB; // basic (no) mode
			sa.Padding = mode;
			return sa.CreateDecryptor (GetKey (sa), GetIV (sa));
		}

		// the best way to verify padding is to:
		// a. encrypt data larger than one block with a padding mode "X"
		// b. decrypt the data with padding mode "None"
		// c. compare the last (padding) bytes with the expected padding
#if NET_2_0
		private void ANSIX923_Full (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one full block
			byte[] data = new byte [bs]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.ANSIX923);
			byte[] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (data.Length * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte[] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - ANSI X.923 is all 0 except last byte (length)
			for (int i=0; i < bs - 1; i++)
				Assert.AreEqual (0x00, decdata [decdata.Length - pd + i], i.ToString ());
			Assert.AreEqual (pd, decdata [decdata.Length - 1], "last byte");
		}

		private void ANSIX923_Partial (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one and an half block
			byte[] data = new byte [bs + (bs >> 1)]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.ANSIX923);
			byte[] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (bs * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte[] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - ANSI X.923 is all 0 except last byte (length)
			for (int i = 0; i < pd - 1; i++)
				Assert.AreEqual (0x00, decdata [decdata.Length - pd + i], i.ToString ());
			Assert.AreEqual (pd, decdata [decdata.Length - 1], "last byte");
		}

		private void ISO10126_Full (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one full block
			byte [] data = new byte [bs]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.ISO10126);
			byte [] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (data.Length * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte [] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - ISO10126 is all random except last byte (length)
			Assert.AreEqual (pd, decdata [decdata.Length - 1], "last byte");
		}

		private void ISO10126_Partial (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one and an half block
			byte [] data = new byte [bs + (bs >> 1)]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.ISO10126);
			byte [] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (bs * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte [] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - ISO10126 is all random except last byte (length)
			Assert.AreEqual (pd, decdata [decdata.Length - 1], "last byte");
		}
#endif
		private void PKCS7_Full (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one full block
			byte[] data = new byte [bs]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.PKCS7);
			byte[] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (data.Length * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte[] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - PKCS7 is all padding char
			for (int i = 0; i < bs; i++)
				Assert.AreEqual (pd, decdata [decdata.Length - pd + i], i.ToString ());
		}

		private void PKCS7_Partial (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one and an half block
			byte[] data = new byte[bs + (bs >> 1)]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.PKCS7);
			byte[] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (bs * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte[] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - PKCS7 is all padding char
			for (int i = 0; i < pd; i++)
				Assert.AreEqual (pd, decdata [decdata.Length - pd + i], i.ToString ());
		}

		private void Zeros_Full (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one full block
			byte [] data = new byte [bs]; // in bytes
			for (int i = 0; i < data.Length; i++)
				data [i] = 0xFF;

			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.Zeros);
			byte [] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// NO extra block is used for zero padding
			Assert.AreEqual (data.Length, encdata.Length, "no extra block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte [] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			// now validate absence of padding
			Assert.AreEqual (0xFF, decdata [decdata.Length - 1], "no padding");
		}

		private void Zeros_Partial (SymmetricAlgorithm sa)
		{
			int bs = (sa.BlockSize >> 3);
			// one and an half block
			byte [] data = new byte [bs + (bs >> 1)]; // in bytes
			for (int i=0; i < data.Length; i++)
				data [i] = 0xFF;

			ICryptoTransform enc = GetEncryptor (sa, PaddingMode.Zeros);
			byte [] encdata = enc.TransformFinalBlock (data, 0, data.Length);
			// one block of padding is added			
			Assert.AreEqual (bs * 2, encdata.Length, "one more block added");

			ICryptoTransform dec = GetDecryptor (sa, PaddingMode.None);
			byte [] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (encdata.Length, decdata.Length, "no unpadding");

			int pd = decdata.Length - data.Length;
			// now validate padding - Zeros is all 0x00 char
			for (int i = 0; i < pd; i++)
				Assert.AreEqual (0x00, decdata [decdata.Length - pd + i], i.ToString ());
		}
#if NET_2_0
		// ANSI X.923

		[Test]
		public void DES_ANSIX923_Full ()
		{
			ANSIX923_Full (DES.Create ());
		}

		[Test]
		public void DES_ANSIX923_Partial ()
		{
			ANSIX923_Partial (DES.Create ());
		}

		[Test]
		public void RC2_ANSIX923_Full ()
		{
			ANSIX923_Full (RC2.Create ());
		}

		[Test]
		public void RC2_ANSIX923_Partial ()
		{
			ANSIX923_Partial (RC2.Create ());
		}

		[Test]
		public void Rijndael_ANSIX923_Full () 
		{
			ANSIX923_Full (Rijndael.Create ());
		}

		[Test]
		public void Rijndael_ANSIX923_Partial ()
		{
			ANSIX923_Partial (Rijndael.Create ());
		}

		[Test]
		public void TripleDES_ANSIX923_Full ()
		{
			ANSIX923_Full (TripleDES.Create ());
		}

		[Test]
		public void TripleDES_ANSIX923_Partial ()
		{
			ANSIX923_Partial (TripleDES.Create ());
		}

		// ISO 10126

		[Test]
		public void DES_ISO10126_Full ()
		{
			ISO10126_Full (DES.Create ());
		}

		[Test]
		public void DES_ISO10126_Partial ()
		{
			ISO10126_Partial (DES.Create ());
		}

		[Test]
		public void RC2_ISO10126_Full ()
		{
			ISO10126_Full (RC2.Create ());
		}

		[Test]
		public void RC2_ISO10126_Partial ()
		{
			ISO10126_Partial (RC2.Create ());
		}

		[Test]
		public void Rijndael_ISO10126_Full ()
		{
			ISO10126_Full (Rijndael.Create ());
		}

		[Test]
		public void Rijndael_ISO10126_Partial ()
		{
			ISO10126_Partial (Rijndael.Create ());
		}

		[Test]
		public void TripleDES_ISO10126_Full ()
		{
			ISO10126_Full (TripleDES.Create ());
		}

		[Test]
		public void TripleDES_ISO10126_Partial ()
		{
			ISO10126_Partial (TripleDES.Create ());
		}
#endif
		// PKCS #7

		[Test]
		public void DES_PKCS7_Full ()
		{
			PKCS7_Full (DES.Create ());
		}

		[Test]
		public void DES_PKCS7_Partial ()
		{
			PKCS7_Partial (DES.Create ());
		}

		[Test]
		public void RC2_PKCS7_Full ()
		{
			PKCS7_Full (RC2.Create ());
		}

		[Test]
		public void RC2_PKCS7_Partial ()
		{
			PKCS7_Partial (RC2.Create ());
		}

		[Test]
		public void Rijndael_PKCS7_Full ()
		{
			PKCS7_Full (Rijndael.Create ());
		}

		[Test]
		public void Rijndael_PKCS7_Partial ()
		{
			PKCS7_Partial (Rijndael.Create ());
		}

		[Test]
		public void TripleDES_PKCS7_Full ()
		{
			PKCS7_Full (TripleDES.Create ());
		}

		[Test]
		public void TripleDES_PKCS7_Partial ()
		{
			PKCS7_Partial (TripleDES.Create ());
		}

		// Zeros

		[Test]
		public void DES_Zeros_Full ()
		{
			Zeros_Full (DES.Create ());
		}

		[Test]
		public void DES_Zeros_Partial ()
		{
			Zeros_Partial (DES.Create ());
		}

		[Test]
		public void RC2_Zeros_Full ()
		{
			Zeros_Full (RC2.Create ());
		}

		[Test]
		public void RC2_Zeros_Partial ()
		{
			Zeros_Partial (RC2.Create ());
		}

		[Test]
		public void Rijndael_Zeros_Full ()
		{
			Zeros_Full (Rijndael.Create ());
		}

		[Test]
		public void Rijndael_Zeros_Partial ()
		{
			Zeros_Partial (Rijndael.Create ());
		}

		[Test]
		public void TripleDES_Zeros_Full ()
		{
			Zeros_Full (TripleDES.Create ());
		}

		[Test]
		public void TripleDES_Zeros_Partial ()
		{
			Zeros_Partial (TripleDES.Create ());
		}

		// Padding mismatches

		// the best way to test bad padding is to:
		// a. encrypt data larger than one block with a padding mode "X"
		// b. decrypt the data with padding mode "Y" (different with Y)
		// c. check if the "bad" padding was removed correctly
		//
		// returns (bitmask)
		// 1 - length difference
		// 2 - original data lost
		// 4 - CryptographicException thrown while decryption
		private int Mismatch (PaddingMode encrypt, PaddingMode decrypt)
		{
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create ();
			int bs = (sa.BlockSize >> 3);
			// one full block
			byte [] data = new byte [bs]; // in bytes
			ICryptoTransform enc = GetEncryptor (sa, encrypt);
			byte [] encdata = enc.TransformFinalBlock (data, 0, data.Length);

			int result = 0;
			try {
				ICryptoTransform dec = GetDecryptor (sa, decrypt);
				byte [] decdata = dec.TransformFinalBlock (encdata, 0, encdata.Length);
				
				if (data.Length != decdata.Length)
					result += 1;

				for (int i=0; i < data.Length; i++) {
					if (data [i] != decdata [i]) {
						result += 2;
						break;
					}
				}
			}
			catch (CryptographicException) {
				result += 4;
			}
			return result;
		}
#if NET_2_0
		[Test]
		public void ANSIX923_ISO10126 () 
		{
			Assert.AreEqual (0, Mismatch (PaddingMode.ANSIX923, PaddingMode.ISO10126));
		}

		[Test]
		public void ANSIX923_None ()
		{
			Assert.AreEqual (1, Mismatch (PaddingMode.ANSIX923, PaddingMode.None));
		}

		[Test]
		public void ANSIX923_PKCS7 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.ANSIX923, PaddingMode.PKCS7));
		}

		[Test]
		public void ANSIX923_Zeros ()
		{
			Assert.AreEqual (1, Mismatch (PaddingMode.ANSIX923, PaddingMode.Zeros));
		}

		[Test]
		public void ISO10126_ANSIX923 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.ISO10126, PaddingMode.ANSIX923));
		}

		[Test]
		public void ISO10126_None ()
		{
			Assert.AreEqual (1, Mismatch (PaddingMode.ISO10126, PaddingMode.None));
		}

		[Test]
		public void ISO10126_PKCS7 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.ISO10126, PaddingMode.PKCS7));
		}

		[Test]
		public void ISO10126_Zeros ()
		{
			Assert.AreEqual (1, Mismatch (PaddingMode.ISO10126, PaddingMode.Zeros));
		}

		[Test]
		public void None_ANSIX923 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.None, PaddingMode.ANSIX923));
		}

		[Test]
		public void None_ISO10126 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.None, PaddingMode.ISO10126));
		}
#endif
		[Test]
		public void None_PKCS7 ()
		{
#if NET_2_0
			Assert.AreEqual (4, Mismatch (PaddingMode.None, PaddingMode.PKCS7));
#else
			Assert.AreEqual (0, Mismatch (PaddingMode.None, PaddingMode.PKCS7));
#endif
		}

		[Test]
		public void None_Zeros ()
		{
			Assert.AreEqual (0, Mismatch (PaddingMode.None, PaddingMode.Zeros));
		}
#if NET_2_0
		[Test]
		public void PKCS7_ANSIX923 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.PKCS7, PaddingMode.ANSIX923));
		}

		[Test]
		public void PKCS7_ISO10126 ()
		{
			Assert.AreEqual (0, Mismatch (PaddingMode.PKCS7, PaddingMode.ISO10126));
		}
#endif
		[Test]
		public void PKCS7_None ()
		{
			Assert.AreEqual (1, Mismatch (PaddingMode.PKCS7, PaddingMode.None));
		}

		[Test]
		public void PKCS7_Zeros ()
		{
			Assert.AreEqual (1, Mismatch (PaddingMode.PKCS7, PaddingMode.Zeros));
		}
#if NET_2_0
		[Test]
		public void Zeros_ANSIX923 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.Zeros, PaddingMode.ANSIX923));
		}

		[Test]
		public void Zeros_ISO10126 ()
		{
			Assert.AreEqual (4, Mismatch (PaddingMode.Zeros, PaddingMode.ISO10126));
		}
#endif
		[Test]
		public void Zeros_None ()
		{
			Assert.AreEqual (0, Mismatch (PaddingMode.Zeros, PaddingMode.None));
		}

		[Test]
		public void Zeros_PKCS7 ()
		{
#if NET_2_0
			Assert.AreEqual (4, Mismatch (PaddingMode.Zeros, PaddingMode.PKCS7));
#else
			Assert.AreEqual (0, Mismatch (PaddingMode.Zeros, PaddingMode.PKCS7));
#endif
		}

		// MACTripleDES tests
#if NET_2_0
		private string MAC (PaddingMode padding, int length)
		{
			byte[] key = new byte [24] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
			MACTripleDES mac = new MACTripleDES (key);
			mac.Padding = padding;
			byte[] data = new byte [length];
			byte[] hash = mac.TransformFinalBlock (data, 0, data.Length);
			string result = BitConverter.ToString (mac.Hash);
			return result;
		}

		// Note: TripleDES block size is 8 bytes

		[Test]
		public void MACTripleDES_ANSIX923 () 
		{
			Assert.AreEqual ("F6-61-3E-C8-E4-A4-D1-A8", MAC (PaddingMode.ANSIX923, 8), "Full");
			Assert.AreEqual ("62-C3-78-B0-27-FC-EB-E0", MAC (PaddingMode.ANSIX923, 4), "Partial");
		}

		[Test]
		public void MACTripleDES_ISO10126 ()
		{
			// ISO 10126 use random in it's padding so we can't use it to get "repeatable" results
			// (i.e. each call will get different result). This isn't a padding to use for MACing!!!
			Assert.AreEqual (23, MAC (PaddingMode.ISO10126, 8).Length, "Full");
			Assert.AreEqual (23, MAC (PaddingMode.ISO10126, 4).Length, "Partial");
		}

		[Test]
		public void MACTripleDES_None ()
		{
			Assert.AreEqual ("46-34-5C-8E-EB-DC-74-5C", MAC (PaddingMode.None, 8), "Full");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void MACTripleDES_None_Partial ()
		{
			// Illegal - must be a multiple of block size
			MAC (PaddingMode.None, 4);
		}

		[Test]
		public void MACTripleDES_PKCS7 ()
		{
			Assert.AreEqual ("17-71-9F-D5-0B-EF-1D-07", MAC (PaddingMode.PKCS7, 8), "Full");
			Assert.AreEqual ("5B-3A-13-6F-3F-6F-13-22", MAC (PaddingMode.PKCS7, 4), "Partial");
		}

		[Test]
		public void MACTripleDES_Zeros ()
		{
			Assert.AreEqual ("46-34-5C-8E-EB-DC-74-5C", MAC (PaddingMode.Zeros, 8));
			Assert.AreEqual ("46-34-5C-8E-EB-DC-74-5C", MAC (PaddingMode.Zeros, 4));
		}
#endif
	}
}
