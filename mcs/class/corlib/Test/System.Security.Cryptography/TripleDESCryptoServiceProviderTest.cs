//
// TripleDESCryptoServiceProviderTest.cs - Unit tests for 
//	System.Security.Cryptography.TripleDESCryptoServiceProvider
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	public class TripleDESCryptoServiceProviderTest {

		private TripleDESCryptoServiceProvider tdes;

		[SetUp]
		public void SetUp () 
		{
			tdes = new TripleDESCryptoServiceProvider ();
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void CreateEncryptor_KeyNull ()
		{
			ICryptoTransform encryptor = tdes.CreateEncryptor (null, tdes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = tdes.CreateDecryptor (tdes.Key, tdes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			// null key != SymmetricAlgorithm.Key

			// in about 1 out of 256 runs the exception will not be thrown because the padding will be
			// the same as expected - however this still won't produce the right results (next check)
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// so this case is ok too, throw the expected exception to make the unit test succeed
			throw new CryptographicException ("1/256");
		}

		[Test]
		public void CreateEncryptor_IvNull ()
		{
			ICryptoTransform encryptor = tdes.CreateEncryptor (tdes.Key, null);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = tdes.CreateDecryptor (tdes.Key, tdes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// null iv != SymmetricAlgorithm.IV
		}

		[Test]
		public void CreateEncryptor_KeyIv ()
		{
			byte[] originalKey = tdes.Key;
			byte[] originalIV = tdes.IV;

			byte[] key = (byte[]) tdes.Key.Clone ();
			Array.Reverse (key);
			byte[] iv = (byte[]) tdes.IV.Clone ();
			Array.Reverse (iv);

			Assert.IsNotNull (tdes.CreateEncryptor (key, iv), "CreateEncryptor");

			Assert.AreEqual (originalKey, tdes.Key, "Key");
			Assert.AreEqual (originalIV, tdes.IV, "IV");
			// SymmetricAlgorithm Key and IV not changed by CreateEncryptor
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void CreateDecryptor_KeyNull ()
		{
			ICryptoTransform encryptor = tdes.CreateEncryptor (tdes.Key, tdes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = tdes.CreateDecryptor (null, tdes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			// null key != SymmetricAlgorithm.Key

			// in about 1 out of 256 runs the exception will not be thrown because the padding will be
			// the same as expected - however this still won't produce the right results (next check)
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// so this case is ok too, throw the expected exception to make the unit test succeed
			throw new CryptographicException ("1/256");
		}

		[Test]
		public void CreateDecryptor_IvNull ()
		{
			ICryptoTransform encryptor = tdes.CreateEncryptor (tdes.Key, tdes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = tdes.CreateDecryptor (tdes.Key, null);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// null iv != SymmetricAlgorithm.IV
		}

		[Test]
		public void CreateDecryptor_KeyIv ()
		{
			byte[] originalKey = tdes.Key;
			byte[] originalIV = tdes.IV;

			byte[] key = (byte[]) tdes.Key.Clone ();
			Array.Reverse (key);
			byte[] iv = (byte[]) tdes.IV.Clone ();
			Array.Reverse (iv);

			Assert.IsNotNull (tdes.CreateEncryptor (key, iv), "CreateDecryptor");

			Assert.AreEqual (originalKey, tdes.Key, "Key");
			Assert.AreEqual (originalIV, tdes.IV, "IV");
			// SymmetricAlgorithm Key and IV not changed by CreateDecryptor
		}

		// Setting the IV is more restrictive than supplying an IV to
		// CreateEncryptor and CreateDecryptor. See bug #76483

		private ICryptoTransform CreateEncryptor_IV (int size)
		{
			byte[] iv = (size == -1) ? null : new byte[size];
			return tdes.CreateEncryptor (tdes.Key, iv);
		}

		[Test]
		public void CreateEncryptor_IV_Null ()
		{
			int size = (tdes.BlockSize >> 3) - 1;
			CreateEncryptor_IV (-1);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateEncryptor_IV_Zero ()
		{
			int size = (tdes.BlockSize >> 3) - 1;
			CreateEncryptor_IV (0);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateEncryptor_IV_TooSmall ()
		{
			int size = (tdes.BlockSize >> 3) - 1;
			CreateEncryptor_IV (size);
		}

		[Test]
		public void CreateEncryptor_IV_BlockSize ()
		{
			int size = (tdes.BlockSize >> 3);
			CreateEncryptor_IV (size);
		}

		[Test]
		public void CreateEncryptor_IV_TooBig ()
		{
			int size = tdes.BlockSize; // 8 times too big
			CreateEncryptor_IV (size);
		}

		private ICryptoTransform CreateDecryptor_IV (int size)
		{
			byte[] iv = (size == -1) ? null : new byte[size];
			return tdes.CreateDecryptor (tdes.Key, iv);
		}

		[Test]
		public void CreateDecryptor_IV_Null ()
		{
			int size = (tdes.BlockSize >> 3) - 1;
			CreateDecryptor_IV (-1);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateDecryptor_IV_Zero ()
		{
			int size = (tdes.BlockSize >> 3) - 1;
			CreateDecryptor_IV (0);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateDecryptor_IV_TooSmall ()
		{
			int size = (tdes.BlockSize >> 3) - 1;
			CreateDecryptor_IV (size);
		}

		[Test]
		public void CreateDecryptor_IV_BlockSize ()
		{
			int size = (tdes.BlockSize >> 3);
			CreateDecryptor_IV (size);
		}

		[Test]
		public void CreateDecryptor_IV_TooBig ()
		{
			int size = tdes.BlockSize; // 8 times too big
			CreateDecryptor_IV (size);
		}
		
		[Test]
		public void TwoKeysTripleDes ()
		{
			byte[] key = new byte [16]; // 128 bits
			Buffer.BlockCopy (tdes.Key, 0, key, 0, 16);
			
			ICryptoTransform encryptor = tdes.CreateEncryptor (key, tdes.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = tdes.CreateDecryptor (key, tdes.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsTrue (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
		}
	}
}
