//
// MonoTests.System.Security.SymmetricAlgorithmTransformTest.cs
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

	public abstract class SymmetricAlgorithmTransformTest {

		public abstract SymmetricAlgorithm Algorithm { get; }

		public virtual int BlockSize {
			get { return 8; }
		}


		// see bug #80439
		public void DontDecryptLastBlock (CipherMode mode, PaddingMode padding)
		{
			SymmetricAlgorithm algo = Algorithm;
			algo.Mode = mode;
			algo.Padding = padding;
			ICryptoTransform enc = algo.CreateEncryptor ();
			byte[] plaintext = new byte[BlockSize * 7];
			byte[] encdata = new byte[BlockSize * 8];
			int len = enc.TransformBlock (plaintext, 0, plaintext.Length, encdata, 0);
			Assert.AreEqual (plaintext.Length, len, "encdata");

			ICryptoTransform dec = algo.CreateDecryptor ();
			byte[] decdata = new byte[plaintext.Length];
			len = dec.TransformBlock (encdata, 0, encdata.Length, decdata, 0);
			Assert.AreEqual (plaintext.Length, len, "decdata");

			Assert.AreEqual (plaintext, decdata, "TransformBlock." + mode.ToString ());
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptLastBlock_CBC_None ()
		{
			DontDecryptLastBlock (CipherMode.CBC, PaddingMode.None);
		}

		[Test]
		public void DontDecryptLastBlock_CBC_PKCS7 ()
		{
			DontDecryptLastBlock (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptLastBlock_CBC_Zeros ()
		{
			DontDecryptLastBlock (CipherMode.CBC, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void DontDecryptLastBlock_CBC_ANSIX923 ()
		{
			DontDecryptLastBlock (CipherMode.CBC, PaddingMode.ANSIX923);
		}

		[Test]
		public void DontDecryptLastBlock_CBC_ISO10126 ()
		{
			DontDecryptLastBlock (CipherMode.CBC, PaddingMode.ISO10126);
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptLastBlock_CFB_None ()
		{
			DontDecryptLastBlock (CipherMode.CFB, PaddingMode.None);
		}

		[Test]
		public virtual void DontDecryptLastBlock_CFB_PKCS7 ()
		{
			DontDecryptLastBlock (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptLastBlock_CFB_Zeros ()
		{
			DontDecryptLastBlock (CipherMode.CFB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void DontDecryptLastBlock_CFB_ANSIX923 ()
		{
			DontDecryptLastBlock (CipherMode.CFB, PaddingMode.ANSIX923);
		}

		[Test]
		public void DontDecryptLastBlock_CFB_ISO10126 ()
		{
			DontDecryptLastBlock (CipherMode.CFB, PaddingMode.ISO10126);
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptLastBlock_ECB_None ()
		{
			DontDecryptLastBlock (CipherMode.ECB, PaddingMode.None);
		}

		[Test]
		public void DontDecryptLastBlock_ECB_PKCS7 ()
		{
			DontDecryptLastBlock (CipherMode.ECB, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptLastBlock_ECB_Zeros ()
		{
			DontDecryptLastBlock (CipherMode.ECB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void DontDecryptLastBlock_ECB_ANSIX923 ()
		{
			DontDecryptLastBlock (CipherMode.ECB, PaddingMode.ANSIX923);
		}

		[Test]
		public void DontDecryptLastBlock_ECB_ISO10126 ()
		{
			DontDecryptLastBlock (CipherMode.ECB, PaddingMode.ISO10126);
		}
#endif
		// see bug #80439 (2nd try, reopened)
		// same as DontDecryptLastBlock except
		// a. the encryption transform was final (padding was added)
		// b. we can call/test decryption TransformFinalBlock too
		public void DontDecryptLastBlock_Final (CipherMode mode, PaddingMode padding)
		{
			SymmetricAlgorithm algo = Algorithm;
			algo.Mode = mode;
			algo.Padding = padding;
			ICryptoTransform enc = algo.CreateEncryptor ();
			byte[] plaintext = new byte[BlockSize * 7];
			byte[] encdata = enc.TransformFinalBlock (plaintext, 0, plaintext.Length);
			switch (padding) {
			case PaddingMode.None:
			case PaddingMode.Zeros:
				Assert.AreEqual (BlockSize * 7, encdata.Length, "encdata");
				break;
			default:
				Assert.AreEqual (BlockSize * 8, encdata.Length, "encdata");
				break;
			}

			ICryptoTransform dec = algo.CreateDecryptor ();
			byte[] decdata = new byte[plaintext.Length];
			dec.TransformBlock (encdata, 0, encdata.Length, decdata, 0);

			Assert.AreEqual (plaintext, decdata, "TransformBlock." + mode.ToString ());

			dec = algo.CreateDecryptor ();
			byte[] final = dec.TransformFinalBlock (encdata, 0, encdata.Length);
			Assert.AreEqual (plaintext, final, "TransformFinalBlock." + mode.ToString ());
		}

		[Test]
		public void DontDecryptLastBlock_Final_CBC_None ()
		{
			DontDecryptLastBlock_Final (CipherMode.CBC, PaddingMode.None);
		}

		[Test]
		public void DontDecryptLastBlock_Final_CBC_PKCS7 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		public void DontDecryptLastBlock_Final_CBC_Zeros ()
		{
			DontDecryptLastBlock_Final (CipherMode.CBC, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void DontDecryptLastBlock_Final_CBC_ANSIX923 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CBC, PaddingMode.ANSIX923);
		}

		[Test]
		public void DontDecryptLastBlock_Final_CBC_ISO10126 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CBC, PaddingMode.ISO10126);
		}
#endif
		[Test]
		public virtual void DontDecryptLastBlock_Final_CFB_None ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.None);
		}

		[Test]
		public virtual void DontDecryptLastBlock_Final_CFB_PKCS7 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		public virtual void DontDecryptLastBlock_Final_CFB_Zeros ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void DontDecryptLastBlock_Final_CFB_ANSIX923 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.ANSIX923);
		}

		[Test]
		public void DontDecryptLastBlock_Final_CFB_ISO10126 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.ISO10126);
		}
#endif
		[Test]
		public void DontDecryptLastBlock_Final_ECB_None ()
		{
			DontDecryptLastBlock_Final (CipherMode.ECB, PaddingMode.None);
		}

		[Test]
		public void DontDecryptLastBlock_Final_ECB_PKCS7 ()
		{
			DontDecryptLastBlock_Final (CipherMode.ECB, PaddingMode.PKCS7);
		}

		[Test]
		public void DontDecryptLastBlock_Final_ECB_Zeros ()
		{
			DontDecryptLastBlock_Final (CipherMode.ECB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void DontDecryptLastBlock_Final_ECB_ANSIX923 ()
		{
			DontDecryptLastBlock_Final (CipherMode.ECB, PaddingMode.ANSIX923);
		}

		[Test]
		public void DontDecryptLastBlock_Final_ECB_ISO10126 ()
		{
			DontDecryptLastBlock_Final (CipherMode.ECB, PaddingMode.ISO10126);
		}
#endif
		// similar to previous case but here we try to skip several blocks
		// i.e. encdata.Length versus decdata.Length
		public void DontDecryptMultipleBlock (CipherMode mode, PaddingMode padding)
		{
			SymmetricAlgorithm algo = Algorithm;
			algo.Mode = mode;
			algo.Padding = padding;
			ICryptoTransform enc = algo.CreateEncryptor ();
			byte[] plaintext = new byte[BlockSize * 7];
			byte[] encdata = new byte[BlockSize * 8];
			int len = enc.TransformBlock (plaintext, 0, plaintext.Length, encdata, 0);
			Assert.AreEqual (plaintext.Length, len, "encdata");

			ICryptoTransform dec = algo.CreateDecryptor ();
			byte[] decdata = new byte[BlockSize];
			dec.TransformBlock (encdata, 0, encdata.Length, decdata, 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CBC_None ()
		{
			DontDecryptMultipleBlock (CipherMode.CBC, PaddingMode.None);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
		public virtual void DontDecryptMultipleBlock_CBC_PKCS7 ()
		{
			DontDecryptMultipleBlock (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CBC_Zeros ()
		{
			DontDecryptMultipleBlock (CipherMode.CBC, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CBC_ANSIX923 ()
		{
			DontDecryptMultipleBlock (CipherMode.CBC, PaddingMode.ANSIX923);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CBC_ISO10126 ()
		{
			DontDecryptMultipleBlock (CipherMode.CBC, PaddingMode.ISO10126);
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public virtual void DontDecryptMultipleBlock_CFB_None ()
		{
			DontDecryptMultipleBlock (CipherMode.CFB, PaddingMode.None);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
		public virtual void DontDecryptMultipleBlock_CFB_PKCS7 ()
		{
			DontDecryptMultipleBlock (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CFB_Zeros ()
		{
			DontDecryptMultipleBlock (CipherMode.CFB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CFB_ANSIX923 ()
		{
			DontDecryptMultipleBlock (CipherMode.CFB, PaddingMode.ANSIX923);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_CFB_ISO10126 ()
		{
			DontDecryptMultipleBlock (CipherMode.CFB, PaddingMode.ISO10126);
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_ECB_None ()
		{
			DontDecryptMultipleBlock (CipherMode.ECB, PaddingMode.None);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
		public virtual void DontDecryptMultipleBlock_ECB_PKCS7 ()
		{
			DontDecryptMultipleBlock (CipherMode.ECB, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_ECB_Zeros ()
		{
			DontDecryptMultipleBlock (CipherMode.ECB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_ECB_ANSIX923 ()
		{
			DontDecryptMultipleBlock (CipherMode.ECB, PaddingMode.ANSIX923);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_ECB_ISO10126 ()
		{
			DontDecryptMultipleBlock (CipherMode.ECB, PaddingMode.ISO10126);
		}
#endif
		// similar to previous case but here the encryption transform was final
		public void DontDecryptMultipleBlock_Final (CipherMode mode, PaddingMode padding)
		{
			SymmetricAlgorithm algo = Algorithm;
			algo.Mode = mode;
			algo.Padding = padding;
			ICryptoTransform enc = algo.CreateEncryptor ();
			byte[] plaintext = new byte[BlockSize * 7];
			byte[] encdata = enc.TransformFinalBlock (plaintext, 0, plaintext.Length);
			switch (padding) {
			case PaddingMode.None:
			case PaddingMode.Zeros:
				Assert.AreEqual (BlockSize * 7, encdata.Length, "encdata");
				break;
			default:
				Assert.AreEqual (BlockSize * 8, encdata.Length, "encdata");
				break;
			}

			ICryptoTransform dec = algo.CreateDecryptor ();
			byte[] decdata = new byte[BlockSize];
			dec.TransformBlock (encdata, 0, encdata.Length, decdata, 0);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CBC_None ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CBC, PaddingMode.None);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
		public virtual void DontDecryptMultipleBlock_Final_CBC_PKCS7 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CBC_Zeros ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CBC, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CBC_ANSIX923 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CBC, PaddingMode.ANSIX923);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CBC_ISO10126 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CBC, PaddingMode.ISO10126);
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public virtual void DontDecryptMultipleBlock_Final_CFB_None ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CFB, PaddingMode.None);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
		public virtual void DontDecryptMultipleBlock_Final_CFB_PKCS7 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CFB_Zeros ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CFB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CFB_ANSIX923 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CFB, PaddingMode.ANSIX923);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_CFB_ISO10126 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CFB, PaddingMode.ISO10126);
		}
#endif
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_ECB_None ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.ECB, PaddingMode.None);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (IndexOutOfRangeException))]
#endif
		public virtual void DontDecryptMultipleBlock_Final_ECB_PKCS7 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.ECB, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_ECB_Zeros ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.ECB, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_ECB_ANSIX923 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.ECB, PaddingMode.ANSIX923);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DontDecryptMultipleBlock_Final_ECB_ISO10126 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.ECB, PaddingMode.ISO10126);
		}
#endif

		private void TransformBlock_One (ICryptoTransform ct, int expected)
		{
			byte[] data = new byte[ct.InputBlockSize];
			Assert.AreEqual (expected, ct.TransformBlock (data, 0, ct.InputBlockSize, data, 0));
		}

		public void Encryptor_TransformBlock_One (PaddingMode padding, CipherMode mode, int expected)
		{
			SymmetricAlgorithm sa = Algorithm;
			sa.Padding = padding;
			sa.Mode = mode;
			TransformBlock_One (sa.CreateEncryptor (), expected);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_PKCS7_ECB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_PKCS7_CBC ()
		{
			Encryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.CBC, BlockSize);
		}

		[Test]
		public virtual void CreateEncryptor_TransformBlock_One_PKCS7_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.CFB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_Zeros_ECB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_Zeros_CBC ()
		{
			Encryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.CBC, BlockSize);
		}

		[Test]
		public virtual void CreateEncryptor_TransformBlock_One_Zeros_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.CFB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_None_ECB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.None, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_None_CBC ()
		{
			Encryptor_TransformBlock_One (PaddingMode.None, CipherMode.CBC, BlockSize);
		}

		[Test]
		public virtual void CreateEncryptor_TransformBlock_One_None_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.None, CipherMode.CFB, BlockSize);
		}

#if NET_2_0
		[Test]
		public void CreateEncryptor_TransformBlock_One_ANSIX923_ECB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.ANSIX923, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_ANSIX923_CBC ()
		{
			Encryptor_TransformBlock_One (PaddingMode.ANSIX923, CipherMode.CBC, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_ANSIX923_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.ANSIX923, CipherMode.CFB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_ISO10126_ECB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.ISO10126, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_ISO10126_CBC ()
		{
			Encryptor_TransformBlock_One (PaddingMode.ISO10126, CipherMode.CBC, BlockSize);
		}

		[Test]
		public void CreateEncryptor_TransformBlock_One_ISO10126_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.ISO10126, CipherMode.CFB, BlockSize);
		}
#endif

		public void Decryptor_TransformBlock_One (PaddingMode padding, CipherMode mode, int expected)
		{
			SymmetricAlgorithm sa = Algorithm;
			sa.Padding = padding;
			sa.Mode = mode;
			TransformBlock_One (sa.CreateDecryptor (), expected);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_PKCS7_ECB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.ECB, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_PKCS7_CBC ()
		{
			Decryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.CBC, 0);
		}

		[Test]
		public virtual void CreateDecryptor_TransformBlock_One_PKCS7_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.CFB, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_Zeros_ECB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_Zeros_CBC ()
		{
			Decryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.CBC, BlockSize);
		}

		[Test]
		public virtual void CreateDecryptor_TransformBlock_One_Zeros_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.CFB, BlockSize);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_None_ECB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.None, CipherMode.ECB, BlockSize);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_None_CBC ()
		{
			Decryptor_TransformBlock_One (PaddingMode.None, CipherMode.CBC, BlockSize);
		}

		[Test]
		public virtual void CreateDecryptor_TransformBlock_One_None_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.None, CipherMode.CFB, BlockSize);
		}
#if NET_2_0
		[Test]
		public void CreateDecryptor_TransformBlock_One_ANSIX923_ECB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.ANSIX923, CipherMode.ECB, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_ANSIX923_CBC ()
		{
			Decryptor_TransformBlock_One (PaddingMode.ANSIX923, CipherMode.CBC, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_ANSIX923_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.ANSIX923, CipherMode.CFB, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_ISO10126_ECB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.ISO10126, CipherMode.ECB, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_ISO10126_CBC ()
		{
			Decryptor_TransformBlock_One (PaddingMode.ISO10126, CipherMode.CBC, 0);
		}

		[Test]
		public void CreateDecryptor_TransformBlock_One_ISO10126_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.ISO10126, CipherMode.CFB, 0);
		}
#endif
	}

	[TestFixture]
	public class DESTransformTest: SymmetricAlgorithmTransformTest {

		public override SymmetricAlgorithm Algorithm {
			get { return DES.Create (); }
		}

		// from Lauren Bedoule
		// note: this test case works only for CBC
		private void EncryptEncryptDecryptDecrypt (CipherMode mode, PaddingMode padding)
		{
			SymmetricAlgorithm des = Algorithm;
			des.Mode = mode;
			des.Padding = padding;
			des.IV = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
			des.Key = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
			byte[] input = new byte[48] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7,
                                          0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
                                          0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                                          0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                                          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                                          0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F};
			byte[] result1 = new byte[48];
			byte[] result2 = new byte[48];
			// encrypts
			ICryptoTransform encryptor = des.CreateEncryptor ();
			int len = encryptor.TransformBlock (input, 0, input.Length, result1, 0);
			Assert.AreEqual (48, len, "enc1");
			len = encryptor.TransformBlock (result1, 0, result1.Length, result2, 0);
			Assert.AreEqual (48, len, "enc2");
			// decrypts
			des.IV = new byte[] { 0x0F, 0x93, 0x8D, 0xED, 0xE3, 0x37, 0xA3, 0x09 };
			byte[] result3 = new byte[40];
			ICryptoTransform decryptor = des.CreateDecryptor ();
			byte[] result4 = new byte[40];
			len = decryptor.TransformBlock (result2, 0, result2.Length, result3, 0);
			Assert.AreEqual (40, len, "dec1");
			for (int i = 0; i < 40; i++) {
				Assert.AreEqual (result1[i], result3[i], "dec1." + i);
			}
			len = decryptor.TransformBlock (result3, 0, len, result4, 0);
			Assert.AreEqual (40, len, "dec2");
			for (int i = 0; i < 8; i++) {
				Assert.AreEqual (result1[40 + i], result4[i], "dec2." + i);
			}
			for (int i = 8; i < 16; i++) {
				Assert.AreEqual (result2[i - 8 + 40], result4[i], "dec2." + i);
			}
			for (int i = 16; i < 40; i++) {
				Assert.AreEqual (input[i - 16 + 8], result4[i], "dec2." + i);
			}
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void EncryptEncryptDecryptDecrypt_CBC_None ()
		{
			EncryptEncryptDecryptDecrypt (CipherMode.CBC, PaddingMode.None);
		}

		[Test]
		public void EncryptEncryptDecryptDecrypt_CBC_PKCS7 ()
		{
			EncryptEncryptDecryptDecrypt (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void EncryptEncryptDecryptDecrypt_CBC_Zeros ()
		{
			EncryptEncryptDecryptDecrypt (CipherMode.CBC, PaddingMode.Zeros);
		}
#if NET_2_0
		[Test]
		public void EncryptEncryptDecryptDecrypt_CBC_ANSIX923 ()
		{
			EncryptEncryptDecryptDecrypt (CipherMode.CBC, PaddingMode.ANSIX923);
		}

		[Test]
		public void EncryptEncryptDecryptDecrypt_CBC_ISO10126 ()
		{
			EncryptEncryptDecryptDecrypt (CipherMode.CBC, PaddingMode.ISO10126);
		}
#endif
	}

	[TestFixture]
	public class RC2TransformTest: SymmetricAlgorithmTransformTest {

		public override SymmetricAlgorithm Algorithm {
			get { return RC2.Create (); }
		}
	}

	public abstract class RijndaelTransformTest: SymmetricAlgorithmTransformTest {

		// Rijndael is the only managed crypto transform implementation and, before Fx 2.0, 
		// suffers from different issues than CSP-based CryptoAPITransform. In contrast 
		// Mono has a single (managed) implementation for all crypto transforms.
#if ONLY_1_1
		// The first case is that MS reports that OFB (output feedback) mode isn't 
		// supported even if this isn't the requested mode (CFB). We ignore those cases
		// to (a) don't mess with existing Mono-only code and (b) it's the 2.0 behavior
		// anyway.

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptLastBlock_CFB_PKCS7 ()
		{
			DontDecryptLastBlock (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptLastBlock_Final_CFB_PKCS7 ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptLastBlock_Final_CFB_None ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.None);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptLastBlock_Final_CFB_Zeros ()
		{
			DontDecryptLastBlock_Final (CipherMode.CFB, PaddingMode.Zeros);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void CreateEncryptor_TransformBlock_One_PKCS7_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.CFB, BlockSize);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void CreateEncryptor_TransformBlock_One_Zeros_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.CFB, BlockSize);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void CreateEncryptor_TransformBlock_One_None_CFB ()
		{
			Encryptor_TransformBlock_One (PaddingMode.None, CipherMode.CFB, BlockSize);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void CreateDecryptor_TransformBlock_One_PKCS7_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.PKCS7, CipherMode.CFB, BlockSize);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void CreateDecryptor_TransformBlock_One_Zeros_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.Zeros, CipherMode.CFB, BlockSize);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void CreateDecryptor_TransformBlock_One_None_CFB ()
		{
			Decryptor_TransformBlock_One (PaddingMode.None, CipherMode.CFB, BlockSize);
		}

		// The second case is that the MS managed transform (Rijndael) throws a CryptographicException
		// (like 2.0) where the unmanaged one (CryptoAPITransform) throws an IndexOutOfRangeException. 
		// Since Mono has a single implementation we're ignoring this "special" case (wrong exception) 
		// in favor of the most "used" one. Note that it doesn't much affect existing code.

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptMultipleBlock_CBC_PKCS7 ()
		{
			DontDecryptMultipleBlock (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptMultipleBlock_CFB_PKCS7 ()
		{
			DontDecryptMultipleBlock (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptMultipleBlock_ECB_PKCS7 ()
		{
			DontDecryptMultipleBlock (CipherMode.ECB, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptMultipleBlock_Final_CBC_PKCS7 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CBC, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptMultipleBlock_Final_CFB_PKCS7 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.CFB, PaddingMode.PKCS7);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (CryptographicException))]
		public override void DontDecryptMultipleBlock_Final_ECB_PKCS7 ()
		{
			DontDecryptMultipleBlock_Final (CipherMode.ECB, PaddingMode.PKCS7);
		}
#endif
	}

	[TestFixture]
	public class Rijndael128TransformTest: RijndaelTransformTest {

		public override SymmetricAlgorithm Algorithm {
			get { return Rijndael.Create (); }
		}

		public override int BlockSize {
			get { return 16; }
		}
	}

	[TestFixture]
	public class Rijndael192TransformTest: RijndaelTransformTest {

		public override SymmetricAlgorithm Algorithm {
			get { 
				SymmetricAlgorithm algo = Rijndael.Create ();
				algo.BlockSize = 192;
				algo.FeedbackSize = 192;
				return algo;
			}
		}

		public override int BlockSize {
			get { return 24; }
		}
	}

	[TestFixture]
	public class Rijndael256TransformTest: RijndaelTransformTest {

		public override SymmetricAlgorithm Algorithm {
			get { 
				SymmetricAlgorithm algo = Rijndael.Create ();
				algo.BlockSize = 256;
				algo.FeedbackSize = 256;
				return algo;
			}
		}

		public override int BlockSize {
			get { return 32; }
		}
	}

	[TestFixture]
	public class TripleDESTransformTest: SymmetricAlgorithmTransformTest {

		public override SymmetricAlgorithm Algorithm {
			get { return TripleDES.Create (); }
		}
	}
}
