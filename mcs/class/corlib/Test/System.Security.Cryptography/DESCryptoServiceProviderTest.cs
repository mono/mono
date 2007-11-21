//
// TestSuite.System.Security.Cryptography.DESCryptoServiceProviderTest.cs
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
	public class DESCryptoServiceProviderTest : DESFIPS81Test {

		[SetUp]
		public void SetUp () 
		{
			des = new DESCryptoServiceProvider ();
		}

		[Test]
		public void KeyChecks () 
		{
			byte[] key = des.Key;
			Assert.AreEqual (8, key.Length, "Key");
			Assert.IsFalse (DES.IsWeakKey (key), "IsWeakKey");
			Assert.IsFalse (DES.IsSemiWeakKey (key), "IsSemiWeakKey");
		}

		[Test]
		public void IV () 
		{
			byte[] iv = des.IV;
			Assert.AreEqual (8, iv.Length, "IV");
		}

		// other tests (test vectors) are inherited from DESFIPS81Test
		// (in DESTest.cs) but executed here

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void CreateEncryptor_KeyNull ()
		{
			ICryptoTransform encryptor = des.CreateEncryptor (null, des.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = des.CreateDecryptor (des.Key, des.IV);
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
			ICryptoTransform encryptor = des.CreateEncryptor (des.Key, null);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = des.CreateDecryptor (des.Key, des.IV);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// null iv != SymmetricAlgorithm.IV
		}

		[Test]
		public void CreateEncryptor_KeyIv ()
		{
			byte[] originalKey = des.Key;
			byte[] originalIV = des.IV;

			byte[] key = (byte[]) des.Key.Clone ();
			Array.Reverse (key);
			byte[] iv = (byte[]) des.IV.Clone ();
			Array.Reverse (iv);

			Assert.IsNotNull (des.CreateEncryptor (key, iv), "CreateEncryptor");

			Assert.AreEqual (originalKey, des.Key, "Key");
			Assert.AreEqual (originalIV, des.IV, "IV");
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
			ICryptoTransform encryptor = des.CreateEncryptor (des.Key, des.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = des.CreateDecryptor (null, des.IV);
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
			ICryptoTransform encryptor = des.CreateEncryptor (des.Key, des.IV);
			byte[] data = new byte[encryptor.InputBlockSize];
			byte[] encdata = encryptor.TransformFinalBlock (data, 0, data.Length);

			ICryptoTransform decryptor = des.CreateDecryptor (des.Key, null);
			byte[] decdata = decryptor.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.IsFalse (BitConverter.ToString (data) == BitConverter.ToString (decdata), "Compare");
			// null iv != SymmetricAlgorithm.IV
		}

		[Test]
		public void CreateDecryptor_KeyIv ()
		{
			byte[] originalKey = des.Key;
			byte[] originalIV = des.IV;

			byte[] key = (byte[]) des.Key.Clone ();
			Array.Reverse (key);
			byte[] iv = (byte[]) des.IV.Clone ();
			Array.Reverse (iv);

			Assert.IsNotNull (des.CreateEncryptor (key, iv), "CreateDecryptor");

			Assert.AreEqual (originalKey, des.Key, "Key");
			Assert.AreEqual (originalIV, des.IV, "IV");
			// SymmetricAlgorithm Key and IV not changed by CreateDecryptor
		}

		// Setting the IV is more restrictive than supplying an IV to
		// CreateEncryptor and CreateDecryptor. See bug #76483

		private ICryptoTransform CreateEncryptor_IV (int size)
		{
			byte[] iv = (size == -1) ? null : new byte[size];
			return des.CreateEncryptor (des.Key, iv);
		}

		[Test]
		public void CreateEncryptor_IV_Null ()
		{
			CreateEncryptor_IV (-1);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateEncryptor_IV_Zero ()
		{
			CreateEncryptor_IV (0);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateEncryptor_IV_TooSmall ()
		{
			int size = (des.BlockSize >> 3) - 1;
			CreateEncryptor_IV (size);
		}

		[Test]
		public void CreateEncryptor_IV_BlockSize ()
		{
			int size = (des.BlockSize >> 3);
			CreateEncryptor_IV (size);
		}

		[Test]
		public void CreateEncryptor_IV_TooBig ()
		{
			int size = des.BlockSize; // 8 times too big
			CreateEncryptor_IV (size);
		}

		private ICryptoTransform CreateDecryptor_IV (int size)
		{
			byte[] iv = (size == -1) ? null : new byte[size];
			return des.CreateDecryptor (des.Key, iv);
		}

		[Test]
		public void CreateDecryptor_IV_Null ()
		{
			CreateDecryptor_IV (-1);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateDecryptor_IV_Zero ()
		{
			CreateDecryptor_IV (0);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CreateDecryptor_IV_TooSmall ()
		{
			int size = (des.BlockSize >> 3) - 1;
			CreateDecryptor_IV (size);
		}

		[Test]
		public void CreateDecryptor_IV_BlockSize ()
		{
			int size = (des.BlockSize >> 3);
			CreateDecryptor_IV (size);
		}

		[Test]
		public void CreateDecryptor_IV_TooBig ()
		{
			int size = des.BlockSize; // 8 times too big
			CreateDecryptor_IV (size);
		}
	}
}
