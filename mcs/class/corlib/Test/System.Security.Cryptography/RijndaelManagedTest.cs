//
// TestSuite.System.Security.Cryptography.RijndaelManaged.cs
//
// Authors:
//      Andrew Birkett (andy@nobugs.org)
//      Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright 2012 Xamarin Inc.
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class RijndaelManagedTest {

		private RijndaelManaged aes;

		[SetUp]
		public void SetUp ()
		{
			aes = new RijndaelManaged ();
		}

		public void CheckCBC(ICryptoTransform encryptor, ICryptoTransform decryptor, 
					   byte[] plaintext, byte[] expected) 
		{
	
			if ((plaintext.Length % encryptor.InputBlockSize) != 0) {
				throw new ArgumentException("Must have complete blocks");
			}
	
			byte[] ciphertext = new byte[plaintext.Length];
			for (int i=0; i < plaintext.Length; i += encryptor.InputBlockSize) {
				encryptor.TransformBlock(plaintext, i, encryptor.InputBlockSize, ciphertext, i);
			}
			Assert.AreEqual (expected, ciphertext, "CBC");
	
			byte[] roundtrip = new byte[plaintext.Length];
			for (int i=0; i < ciphertext.Length; i += decryptor.InputBlockSize) {
				decryptor.TransformBlock(ciphertext, i, decryptor.InputBlockSize, roundtrip, i);
			}
			Assert.AreEqual (plaintext, roundtrip, "CBC-rt");
		}

		[Test]
		public void CBC_0() {
	
			byte[] plaintext = new byte[32];
			for (int i=0; i < plaintext.Length; i++) plaintext[i] = 0;
	
			byte[] iv = new byte[16];
			for (byte i=0; i < iv.Length; i++) {
				iv[i] = 0;
			}
	
			RijndaelManaged r = new RijndaelManaged();
			byte[] key = new byte[16];	
	
			for (int i=0; i < 16; i++) r.Key[i] = 0;
			r.BlockSize = 128;
			r.Mode = CipherMode.CBC;
			r.Padding = PaddingMode.Zeros;
			r.Key = key;
	
			byte[] expected = { 
				0x66, 0xe9, 0x4b, 0xd4, 0xef, 0x8a, 0x2c, 0x3b, 
				0x88, 0x4c, 0xfa, 0x59, 0xca, 0x34, 0x2b, 0x2e, 
				0xf7, 0x95, 0xbd, 0x4a, 0x52, 0xe2, 0x9e, 0xd7, 
				0x13, 0xd3, 0x13, 0xfa, 0x20, 0xe9, 0x8d, 0xbc };
	
			CheckCBC(r.CreateEncryptor(key, iv), r.CreateDecryptor(key, iv), plaintext, expected);
		}

		[Test]
		public void CBC_1 ()
		{
			byte[] plaintext = new byte[32];
			for (int i=0; i < plaintext.Length; i++) plaintext[i] = 0;
	
			byte[] iv = new byte[16];
			for (byte i=0; i < iv.Length; i++) {
				iv[i] = i;
			}
	
			RijndaelManaged r = new RijndaelManaged();
			byte[] key = new byte[16];
			for (byte i=0; i < 16; i++) key[i] = 0;

			r.Key = key;
			r.BlockSize = 128;
			r.Mode = CipherMode.CBC;
			r.Padding = PaddingMode.Zeros;
	
			byte[] expected = { 
				0x7a, 0xca, 0x0f, 0xd9, 0xbc, 0xd6, 0xec, 0x7c, 
				0x9f, 0x97, 0x46, 0x66, 0x16, 0xe6, 0xa2, 0x82, 
				0x66, 0xc5, 0x84, 0x17, 0x1d, 0x3c, 0x20, 0x53, 
				0x6f, 0x0a, 0x09, 0xdc, 0x4d, 0x1e, 0x45, 0x3b };
	
			CheckCBC(r.CreateEncryptor(key, iv), r.CreateDecryptor(key, iv), plaintext, expected);
		}
	
		public void CheckECBRoundtrip(ICryptoTransform encryptor, ICryptoTransform decryptor, 
					   byte[] plaintext, byte[] expected)
		{
			byte[] ciphertext = new byte[plaintext.Length];
			int n = encryptor.TransformBlock(plaintext, 0, plaintext.Length, ciphertext, 0);

			Assert.AreEqual (expected, ciphertext, "ECB");
	
			byte[] roundtrip = new byte[plaintext.Length];
			n = decryptor.TransformBlock(ciphertext, 0, ciphertext.Length, roundtrip, 0);

			Assert.AreEqual (plaintext, roundtrip, "ECB-rt-len");
		}

		[Test]
		public void ECB ()
		{
			byte[] plaintext = new byte[16];
			byte[] iv = new byte[16];
	
			for (int i=0; i < 16; i++) {
				plaintext[i] = (byte) (i*16 + i);
			}
	
			RijndaelManaged r = new RijndaelManaged();
			r.Mode = CipherMode.ECB;
			r.Padding = PaddingMode.Zeros;
	
			byte[] key16 = new byte[16];
			byte[] key24 = new byte[24];
			byte[] key32 = new byte[32];
	
			for (int i=0; i < 32; i++) {
				if (i < 16) key16[i] = (byte) i;
				if (i < 24) key24[i] = (byte) i;
				key32[i] = (byte) i;
			}
	
				
			byte[] exp16 = { 0x69, 0xc4, 0xe0, 0xd8, 0x6a, 0x7b, 0x04, 0x30,
					 0xd8, 0xcd, 0xb7, 0x80, 0x70, 0xb4, 0xc5, 0x5a };
			byte[] exp24 = { 0xdd, 0xa9, 0x7c, 0xa4, 0x86, 0x4c, 0xdf, 0xe0,
					 0x6e, 0xaf, 0x70, 0xa0, 0xec, 0x0d, 0x71, 0x91 };
			byte[] exp32 = { 0x8e, 0xa2, 0xb7, 0xca, 0x51, 0x67, 0x45, 0xbf,
					 0xea, 0xfc, 0x49, 0x90, 0x4b, 0x49, 0x60, 0x89 }; 
	
			r.Key = key16;
			r.KeySize = 128;	
			CheckECBRoundtrip(
				r.CreateEncryptor(key16, iv), r.CreateDecryptor(key16, iv), 
				plaintext, exp16
			);
	
	
			r.Key = key24;
			r.KeySize = 192;
			CheckECBRoundtrip(
				r.CreateEncryptor(key24, iv), r.CreateDecryptor(key24, iv), 
				plaintext, exp24
			);
	
	
			r.Key = key32;
			r.KeySize = 256;
			CheckECBRoundtrip(
				r.CreateEncryptor(key32, iv), r.CreateDecryptor(key32, iv), 
				plaintext, exp32
			);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateEncryptor_KeyNull ()
		{
			ICryptoTransform encryptor = aes.CreateEncryptor (null, aes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = aes.CreateDecryptor (aes.Key, aes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			// null key != SymmetricAlgorithm.Key
		}

		[Test]
		public void CreateEncryptor_IvNull ()
		{
			ICryptoTransform encryptor = aes.CreateEncryptor (aes.Key, null);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = aes.CreateDecryptor (aes.Key, aes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// null iv != SymmetricAlgorithm.IV
		}

		[Test]
		public void CreateEncryptor_KeyIv ()
		{
			byte[] originalKey = aes.Key;
			byte[] originalIV = aes.IV;

			byte[] key = (byte[]) aes.Key.Clone ();
			Array.Reverse (key);
			byte[] iv = (byte[]) aes.IV.Clone ();
			Array.Reverse (iv);

			Assert.IsNotNull (aes.CreateEncryptor (key, iv), "CreateEncryptor");

			Assert.AreEqual (originalKey, aes.Key, "Key");
			Assert.AreEqual (originalIV, aes.IV, "IV");
			// SymmetricAlgorithm Key and IV not changed by CreateEncryptor
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateDecryptor_KeyNull ()
		{
			ICryptoTransform encryptor = aes.CreateEncryptor (aes.Key, aes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = aes.CreateDecryptor (null, aes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			// null key != SymmetricAlgorithm.Key
		}

		[Test]
		public void CreateDecryptor_IvNull ()
		{
			ICryptoTransform encryptor = aes.CreateEncryptor (aes.Key, aes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = aes.CreateDecryptor (aes.Key, null);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// null iv != SymmetricAlgorithm.IV
		}

		[Test]
		public void CreateDecryptor_KeyIv ()
		{
			byte[] originalKey = aes.Key;
			byte[] originalIV = aes.IV;

			byte[] key = (byte[]) aes.Key.Clone ();
			Array.Reverse (key);
			byte[] iv = (byte[]) aes.IV.Clone ();
			Array.Reverse (iv);

			Assert.IsNotNull (aes.CreateEncryptor (key, iv), "CreateDecryptor");

			Assert.AreEqual (originalKey, aes.Key, "Key");
			Assert.AreEqual (originalIV, aes.IV, "IV");
			// SymmetricAlgorithm Key and IV not changed by CreateDecryptor
		}

		// Setting the IV is more restrictive than supplying an IV to
		// CreateEncryptor and CreateDecryptor. See bug #76483

		private ICryptoTransform CreateEncryptor_IV (int size)
		{
			byte[] iv = (size == -1) ? null : new byte[size];
			return aes.CreateEncryptor (aes.Key, iv);
		}

		[Test]
		public void CreateEncryptor_IV_Null ()
		{
			int size = (aes.BlockSize >> 3) - 1;
			CreateEncryptor_IV (-1);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateEncryptor_IV_Zero ()
		{
			int size = (aes.BlockSize >> 3) - 1;
			CreateEncryptor_IV (0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateEncryptor_IV_TooSmall ()
		{
			int size = (aes.BlockSize >> 3) - 1;
			CreateEncryptor_IV (size);
		}

		[Test]
		public void CreateEncryptor_IV_BlockSize ()
		{
			int size = (aes.BlockSize >> 3);
			CreateEncryptor_IV (size);
		}

#if !NET_2_1
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		// Rijndael is the only implementation that has
		// this behaviour for IV that are too large
		public void CreateEncryptor_IV_TooBig ()
		{
			int size = aes.BlockSize; // 8 times too big
			CreateEncryptor_IV (size);
		}
#endif

		private ICryptoTransform CreateDecryptor_IV (int size)
		{
			byte[] iv = (size == -1) ? null : new byte[size];
			return aes.CreateDecryptor (aes.Key, iv);
		}

		[Test]
		public void CreateDecryptor_IV_Null ()
		{
			int size = (aes.BlockSize >> 3) - 1;
			CreateDecryptor_IV (-1);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateDecryptor_IV_Zero ()
		{
			int size = (aes.BlockSize >> 3) - 1;
			CreateDecryptor_IV (0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CreateDecryptor_IV_TooSmall ()
		{
			int size = (aes.BlockSize >> 3) - 1;
			CreateDecryptor_IV (size);
		}

		[Test]
		public void CreateDecryptor_IV_BlockSize ()
		{
			int size = (aes.BlockSize >> 3);
			CreateDecryptor_IV (size);
		}
#if !NET_2_1
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		// Rijndael is the only implementation that has
		// this behaviour for IV that are too large
		public void CreateDecryptor_IV_TooBig ()
		{
			int size = aes.BlockSize; // 8 times too big
			CreateDecryptor_IV (size);
		}
#endif
		[Test]
		public void CFB_7193 ()
		{
			const int size = 23; // not a block size
			byte [] original = new byte [size];
			byte [] expected = new byte [] { 0xDC, 0xA8, 0x39, 0x5C, 0xA1, 0x89, 0x3B, 0x05, 0xFA, 0xD8, 0xB5, 0x76, 0x5F, 0x8F, 0x40, 0xCF, 0xA7, 0xFF, 0x86, 0xE6, 0x30, 0x67, 0x6B };
			byte [] encdata;
			byte [] decdata;
			using (RijndaelManaged aes = new RijndaelManaged ()) {
				aes.Mode = CipherMode.CFB;
				aes.FeedbackSize = 8;
				aes.Padding = PaddingMode.None;
				aes.Key = new byte [32];
				aes.IV = new byte [16];
				using (ICryptoTransform encryptor = aes.CreateEncryptor ())
					encdata = encryptor.TransformFinalBlock (original, 0, original.Length);
				Assert.AreEqual (size, encdata.Length, "enc.Length");
				Assert.AreEqual (BitConverter.ToString (expected), BitConverter.ToString (encdata), "encrypted");
				using (ICryptoTransform decryptor = aes.CreateDecryptor ())
					decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
				Assert.AreEqual (original, decdata, "roundtrip");
			}
		}
	}
}
