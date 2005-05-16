//
// TripleDESTest.cs - NUnit Test Cases for TripleDES
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class TripleDESTest {

		[Test]
		public void DefaultProperties ()
		{
			TripleDES algo = TripleDES.Create ();
			Assert.AreEqual (64, algo.BlockSize, "BlockSize");
			Assert.AreEqual (8, algo.FeedbackSize, "FeedbackSize");
			Assert.AreEqual (192, algo.KeySize, "KeySize");
			Assert.AreEqual (CipherMode.CBC, algo.Mode, "Mode");
			Assert.AreEqual (PaddingMode.PKCS7, algo.Padding, "Padding");
			Assert.AreEqual (1, algo.LegalBlockSizes.Length, "LegalBlockSizes");
			Assert.AreEqual (64, algo.LegalBlockSizes [0].MaxSize, "LegalBlockSizes.MaxSize");
			Assert.AreEqual (64, algo.LegalBlockSizes [0].MinSize, "LegalBlockSizes.MinSize");
			Assert.AreEqual (0, algo.LegalBlockSizes [0].SkipSize, "LegalBlockSizes.SkipSize");
			Assert.AreEqual (1, algo.LegalKeySizes.Length, "LegalKeySizes");
			Assert.AreEqual (192, algo.LegalKeySizes [0].MaxSize, "LegalKeySizes.MaxSize");
			Assert.AreEqual (128, algo.LegalKeySizes [0].MinSize, "LegalKeySizes.MinSize");
			Assert.AreEqual (64, algo.LegalKeySizes [0].SkipSize, "LegalKeySizes.SkipSize");
		}

		[Test]
		public void Key ()
		{
			TripleDES algo = TripleDES.Create ();
			algo.GenerateKey ();
			algo.GenerateIV ();
			Assert.AreEqual (192, algo.KeySize, "Key Size");
			Assert.AreEqual (24, algo.Key.Length, "Key Length");
			Assert.AreEqual (8, algo.IV.Length, "IV Length");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void KeyNull () 
		{
			TripleDES algo = TripleDES.Create ();
			algo.Key = null;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWeak128bits () 
		{
			byte[] wk128 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk128;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWeak192bits_AB () 
		{
			byte[] wk192 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk192;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWeak192bits_BC () 
		{
			byte[] wk192 = { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk192;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeyWrongLength () 
		{
			byte[] wk64 = { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk64;
		}

		[Test]
		public void UseOneDesWeakKeyIn128bitsKey ()
		{
			// the first 8 bytes are a DES weak key
			byte[] wk128 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk128;
		}

		[Test]
		public void UseOneDesWeakKeyIn192bitsKey ()
		{
			// the bytes 8-16 are a DES weak key
			byte[] wk192 = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
			TripleDES algo = TripleDES.Create ();
			algo.Key = wk192;
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (CryptographicException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void IsSemiWeakKey_Null () 
		{
			TripleDES.IsWeakKey (null);
		}
	}
}
