//
// PaddingModeTest.cs - NUnit Test Cases for PaddingMode
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class PaddingModeTest : Assertion {

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
			AssertEquals ("TripleDESNone_ExactlyOneBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51", BitConverter.ToString (encdata));
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
			AssertEquals ("TripleDESNone_ExactMultipleBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51-BF-AB-79-9C-CD-5E-79-40-16-81-0D-6B-40-E6-B2-E9-86-34-8A-9E-5D-56-DA-D1-0C-8C-76-0A-E1-69-A1-0A-B5-B5-7F-FC-5A-D0-6E-6A", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.None, encdata);
			AssertEquals ("TripleDESNone_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}

		[Test]
		public void TripleDESPKCS7_SmallerThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (7);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			AssertEquals ("TripleDESPKCS7_SmallerThanOneBlockSize_Encrypt", "C6-59-0E-E3-7F-26-92-B0", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_SmallerThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESPKCS7_ExactlyOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			AssertEquals ("TripleDESPKCS7_ExactlyOneBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51-C0-60-5B-6A-5C-B7-69-62", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESPKCS7_MoreThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (12);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			AssertEquals ("TripleDESPKCS7_MoreThanOneBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51-D9-CB-92-8C-76-89-35-84", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_MoreThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESPKCS7_ExactMultipleBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (tdes, PaddingMode.PKCS7, data);
			AssertEquals ("TripleDESPKCS7_ExactMultipleBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51-BF-AB-79-9C-CD-5E-79-40-16-81-0D-6B-40-E6-B2-E9-86-34-8A-9E-5D-56-DA-D1-0C-8C-76-0A-E1-69-A1-0A-B5-B5-7F-FC-5A-D0-6E-6A-73-61-63-1C-58-A2-9C-B3", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.PKCS7, encdata);
			AssertEquals ("TripleDESPKCS7_ExactMultipleBlockSize_Decrypt", data, decdata);
		}
		
		[Test]
		public void TripleDESZeros_SmallerThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (7);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			AssertEquals ("TripleDESZeros_SmallerThanOneBlockSize_Encrypt", "B8-5C-5B-A5-06-0B-7E-C6", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			AssertEquals ("TripleDESZeros_SmallerThanOneBlockSize_Decrypt", "00-01-02-03-04-05-06-00", BitConverter.ToString (decdata));
		}

		[Test]
		public void TripleDESZeros_ExactlyOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			AssertEquals ("TripleDESZeros_ExactlyOneBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			AssertEquals ("TripleDESZeros_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void TripleDESZeros_MoreThanOneBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (12);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			AssertEquals ("TripleDESZeros_MoreThanOneBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51-6E-75-2E-33-12-09-5D-66", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (tdes, PaddingMode.Zeros, encdata);
			AssertEquals ("TripleDESZeros_MoreThanOneBlockSize_Decrypt", "00-01-02-03-04-05-06-07-08-09-0A-0B-00-00-00-00", BitConverter.ToString (decdata));
		}

		[Test]
		public void TripleDESZeros_ExactMultipleBlockSize () 
		{
			TripleDES tdes = GetTripleDES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (tdes, PaddingMode.Zeros, data);
			AssertEquals ("TripleDESZeros_ExactMultipleBlockSize_Encrypt", "23-61-AC-E6-C5-17-10-51-BF-AB-79-9C-CD-5E-79-40-16-81-0D-6B-40-E6-B2-E9-86-34-8A-9E-5D-56-DA-D1-0C-8C-76-0A-E1-69-A1-0A-B5-B5-7F-FC-5A-D0-6E-6A", BitConverter.ToString (encdata));
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
			AssertEquals ("RijndaelNone_ExactlyOneBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB", BitConverter.ToString (encdata));
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
			AssertEquals ("RijndaelNone_ExactMultipleBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-E1-27-3E-21-5A-BE-D5-12-F4-AF-06-8D-0A-BD-02-64-02-CB-FF-D7-32-19-5E-69-3C-54-C2-8C-A1-D7-72-FF", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.None, encdata);
			AssertEquals ("RijndaelNone_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}

		[Test]
		public void RijndaelPKCS7_SmallerThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			AssertEquals ("RijndaelPKCS7_SmallerThanOneBlockSize_Encrypt", "AB-E0-20-5E-BC-28-A0-B7-A7-56-A3-BF-13-55-13-7E", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_SmallerThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelPKCS7_ExactlyOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (16);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			AssertEquals ("RijndaelPKCS7_ExactlyOneBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-60-CE-9F-E0-72-3B-D6-D1-A5-F8-33-D8-25-31-7F-D4", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelPKCS7_MoreThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (20);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			AssertEquals ("RijndaelPKCS7_MoreThanOneBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-06-3F-D3-51-8D-55-E9-2F-02-4A-4E-F2-91-55-31-83", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_MoreThanOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelPKCS7_ExactMultipleBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (aes, PaddingMode.PKCS7, data);
			AssertEquals ("RijndaelPKCS7_ExactMultipleBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-E1-27-3E-21-5A-BE-D5-12-F4-AF-06-8D-0A-BD-02-64-02-CB-FF-D7-32-19-5E-69-3C-54-C2-8C-A1-D7-72-FF-37-42-81-21-47-A7-E0-AA-64-A7-8B-65-25-95-AA-54", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.PKCS7, encdata);
			AssertEquals ("RijndaelPKCS7_ExactMultipleBlockSize_Decrypt", data, decdata);
		}
		
		[Test]
		public void RijndaelZeros_SmallerThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (8);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			AssertEquals ("RijndaelZeros_SmallerThanOneBlockSize_Encrypt", "DD-BE-D7-CE-E2-DD-5C-A3-3E-44-A1-76-00-E5-5B-5D", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			AssertEquals ("RijndaelZeros_SmallerThanOneBlockSize_Decrypt", "00-01-02-03-04-05-06-07-00-00-00-00-00-00-00-00", BitConverter.ToString (decdata));
		}

		[Test]
		public void RijndaelZeros_ExactlyOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (16);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			AssertEquals ("RijndaelZeros_ExactlyOneBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			AssertEquals ("RijndaelZeros_ExactlyOneBlockSize_Decrypt", data, decdata);
		}

		[Test]
		public void RijndaelZeros_MoreThanOneBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (20);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			AssertEquals ("RijndaelZeros_MoreThanOneBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-04-6C-F7-A5-DE-FF-B4-30-29-7A-0E-04-3B-D4-B8-F2", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			AssertEquals ("RijndaelZeros_MoreThanOneBlockSize_Decrypt", "00-01-02-03-04-05-06-07-08-09-0A-0B-0C-0D-0E-0F-10-11-12-13-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString (decdata));
		}

		[Test]
		public void RijndaelZeros_ExactMultipleBlockSize () 
		{
			Rijndael aes = GetAES ();
			byte[] data = GetData (48);
			byte[] encdata = Encrypt (aes, PaddingMode.Zeros, data);
			AssertEquals ("RijndaelZeros_ExactMultipleBlockSize_Encrypt", "79-42-36-2F-D6-DB-F1-0C-87-99-58-06-D5-F6-B0-BB-E1-27-3E-21-5A-BE-D5-12-F4-AF-06-8D-0A-BD-02-64-02-CB-FF-D7-32-19-5E-69-3C-54-C2-8C-A1-D7-72-FF", BitConverter.ToString (encdata));
			byte[] decdata = Decrypt (aes, PaddingMode.Zeros, encdata);
			AssertEquals ("RijndaelZeros_ExactMultipleBlockSize_Decrypt", GetData (48), decdata);
		}
	}
}