//
// SymmetricAlgorithm2Test.cs -
//	Non generated NUnit Test Cases for SymmetricAlgorithm
//
// Author:
//	Sebastien Pouliot  <spouliot@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
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
	public class SymmetricAlgorithm2Test : Assertion {

		public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			AllTests.AssertEquals (msg, array1, array2);
		}

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
				Assert ("Key", BitConverter.ToString (key) != BitConverter.ToString (algo.Key));
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
	}
}
