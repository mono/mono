//
// SymmetricAlgorithm2Test.cs -
//	Non generated NUnit Test Cases for SymmetricAlgorithm
//
// Author:
//	Sebastien Pouliot  <spouliot@ximian.com>
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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	// SymmetricAlgorithm is a abstract class - so most of it's functionality wont
	// be tested here (but will be in its descendants).

	[TestFixture]
	public class SymmetricAlgorithm2Test {

		[Test]
		public void KeySize_SameSize () 
		{
			using (SymmetricAlgorithm algo = SymmetricAlgorithm.Create ()) {
				// get a copy of the key
				byte[] key = algo.Key;
				int ks = algo.KeySize;
				// set the key size
				algo.KeySize = ks;
				// did it change the key ? Yes!
				Assert.AreNotEqual (BitConverter.ToString (key), BitConverter.ToString (algo.Key), "Key");
			}
		}

		[Test]
		public void BlockSize_SameSize () 
		{
			using (SymmetricAlgorithm algo = SymmetricAlgorithm.Create ()) {
				// get a copy of the IV
				byte[] iv = algo.IV;
				int bs = algo.BlockSize;
				// set the iv size
				algo.BlockSize = bs;
				// did it change the IV ? No!
				Assert.AreEqual (BitConverter.ToString (iv), BitConverter.ToString (algo.IV), "IV");
			}
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void InvalidBlockSize () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.BlockSize = 255;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void InvalidFeedbackSize () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.FeedbackSize = algo.BlockSize + 1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IV_Null () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.IV = null;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void IV_None () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.IV = new byte [0]; // e.g. stream ciphers
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void IV_TooBig () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.IV = new byte [algo.BlockSize + 1];
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Key_Null () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.Key = null;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Key_WrongSize () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.Key = new byte [255];
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void KeySize_WrongSize () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			int n = 0;
			while (algo.ValidKeySize (++n));
			algo.KeySize = n;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void InvalidCipherMode () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.Mode = (CipherMode) 255;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void InvalidPaddingMode () 
		{
			SymmetricAlgorithm algo = SymmetricAlgorithm.Create ();
			algo.Padding = (PaddingMode) 255;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FeedbackZero ()
		{
			// thanks to Yakk for the sample
			DES des = new DESCryptoServiceProvider();
			des.FeedbackSize = 0;
			des.Padding = PaddingMode.None;
			des.Mode = CipherMode.ECB;
			des.Key = new byte [8] { 8, 7, 6, 5, 4, 3, 2, 1 };

			ICryptoTransform enc = des.CreateEncryptor (); 
			byte[] response = new byte [16];
			byte[] challenge = new byte [16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

			enc.TransformBlock (challenge, 0, challenge.Length, response, 0);
			Assert.AreEqual ("7A-64-CD-1B-4F-EE-B5-92-54-90-53-E9-83-71-A6-0C", BitConverter.ToString (response));
		}
	}
}
