//
// SHA224Test.cs - NUnit Test Cases for SHA224
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

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Mono.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	// References:
	// a.	RFC 3874 - A 224-bit One-way Hash Function: SHA-224, September 2004
	//	http://www.faqs.org/rfc/rfc3874.txt
	// b.	FIPS PUB 180-2: Secure Hash Standard
	//	http://csrc.nist.gov/publications/fips/fips180-2/fip180-2.txt

	// SHA224 is a abstract class - so most of the test included here wont be tested
	// on the abstract class but should be tested in ALL its descendants.

	[TestFixture]
	public class SHA224Test {

		protected HashAlgorithm hash;

		[SetUp]
		public virtual void SetUp () 
		{
			hash = SHA224.Create ();
		}

		internal void AssertEquals (string msg, byte[] expected, byte[] actual)
		{
			if (expected == null) {
				if (actual != null) {
					string a =  BitConverter.ToString (actual);
					Assert.Fail (String.Format ("{0} - Expected null value but got: {1}", msg, a));
				}
			}
			else if (actual == null) {
				string e =  BitConverter.ToString (expected);
				Assert.Fail (String.Format ("{0} - Got null value but expected: {1}", msg, e));
			}
			Assert.AreEqual (BitConverter.ToString (expected), BitConverter.ToString (actual), msg);
		}

		// test vectors from RFC 3874 (as NIST FIPS 186-2 hasn't yet 
		// been officially updated for SHA224 test vectors).

		private string input1 = "abc";
		private string input2 = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";

		public void FIPS186_Test1 (SHA224 hash) 
		{
			string className = hash.ToString ();
			byte[] result = { 0x23, 0x09, 0x7D, 0x22, 0x34, 0x05, 0xD8, 0x22, 0x86, 0x42,
					0xA4, 0x77, 0xBD, 0xA2, 0x55, 0xB3, 0x2A, 0xAD, 0xBC, 0xE4, 
					0xBD, 0xA0, 0xB3, 0xF7, 0xE3, 0x6C, 0x9D, 0xA7 };
			byte[] input = Encoding.Default.GetBytes (input1);

			string testName = className + " 1";
			FIPS186_a (testName, hash, input, result);
			FIPS186_b (testName, hash, input, result);
			FIPS186_c (testName, hash, input, result);
			FIPS186_d (testName, hash, input, result);
			FIPS186_e (testName, hash, input, result);
		}

		public void FIPS186_Test2 (SHA224 hash) 
		{
			string className = hash.ToString ();
			byte[] result = { 0x75, 0x38, 0x8B, 0x16, 0x51, 0x27, 0x76, 0xCC, 0x5D, 0xBA,
					0x5D, 0xA1, 0xFD, 0x89, 0x01, 0x50, 0xB0, 0xC6, 0x45, 0x5C,
					0xB4, 0xF5, 0x8B, 0x19, 0x52, 0x52, 0x25, 0x25 };
			byte[] input = Encoding.Default.GetBytes (input2);

			string testName = className + " 2";
			FIPS186_a (testName, hash, input, result);
			FIPS186_b (testName, hash, input, result);
			FIPS186_c (testName, hash, input, result);
			FIPS186_d (testName, hash, input, result);
			FIPS186_e (testName, hash, input, result);
		}

		public void FIPS186_Test3 (SHA224 hash) 
		{
			string className = hash.ToString ();
			byte [] result = { 0x20, 0x79, 0x46, 0x55, 0x98, 0x0C, 0x91, 0xD8, 0xBB, 0xB4,
					0xC1, 0xEA, 0x97, 0x61, 0x8A, 0x4B, 0xF0, 0x3F, 0x42, 0x58,
					0x19, 0x48, 0xB2, 0xEE, 0x4E, 0xE7, 0xAD, 0x67 };
			byte[] input = new byte [1000000];
			for (int i = 0; i < 1000000; i++)
				input[i] = 0x61; // a

			string testName = className + " 3";
			FIPS186_a (testName, hash, input, result);
			FIPS186_b (testName, hash, input, result);
			FIPS186_c (testName, hash, input, result);
			FIPS186_d (testName, hash, input, result);
			FIPS186_e (testName, hash, input, result);
		}

		public void FIPS186_a (string testName, SHA224 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.ComputeHash (input); 
			AssertEquals (testName + ".a.1", result, output);
			AssertEquals (testName + ".a.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void FIPS186_b (string testName, SHA224 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.ComputeHash (input, 0, input.Length); 
			AssertEquals (testName + ".b.1", result, output);
			AssertEquals (testName + ".b.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void FIPS186_c (string testName, SHA224 hash, byte[] input, byte[] result) 
		{
			MemoryStream ms = new MemoryStream (input);
			byte[] output = hash.ComputeHash (ms); 
			AssertEquals (testName + ".c.1", result, output);
			AssertEquals (testName + ".c.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void FIPS186_d (string testName, SHA224 hash, byte[] input, byte[] result) 
		{
			hash.TransformFinalBlock (input, 0, input.Length);
			// Note: TransformFinalBlock doesn't return HashValue !
			// AssertEquals( testName + ".d.1", result, output );
			AssertEquals (testName + ".d", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void FIPS186_e (string testName, SHA224 hash, byte[] input, byte[] result) 
		{
			byte[] copy = new byte [input.Length];
			for (int i=0; i < input.Length - 1; i++)
				hash.TransformBlock (input, i, 1, copy, i);
			hash.TransformFinalBlock (input, input.Length - 1, 1);
			// Note: TransformFinalBlock doesn't return HashValue !
			// AssertEquals (testName + ".e.1", result, output);
			AssertEquals (testName + ".e", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		[Test]
		public virtual void Create () 
		{
			// Note: These tests will only be valid without a "machine.config" file
			// or a "machine.config" file that do not modify the default algorithm
			// configuration.
// FIXME: Change namespace when (or if) classes are moved into corlib
			const string defaultSHA224 = "Mono.Security.Cryptography.SHA224Managed";

			// try to build the default implementation
			SHA224 hash = SHA224.Create ();
			Assert.AreEqual (hash.ToString (), defaultSHA224, "SHA224.Create()");

			// try to build, in every way, a SHA224 implementation
			hash = SHA224.Create ("SHA224");
			Assert.AreEqual (hash.ToString (), defaultSHA224, "SHA224.Create('SHA224')");
			hash = SHA224.Create ("SHA-224");
			Assert.AreEqual (hash.ToString (), defaultSHA224, "SHA224.Create('SHA-224')");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void CreateIncorrect () 
		{
			// try to build an incorrect hash algorithms
			hash = SHA224.Create ("MD5");
		}

/* Uncomment when (or if) the SHA224 classes are moved into corlib
		[Test]
		public void CreateInvalid () 
		{
			// try to build invalid implementation
			hash = SHA224.Create ("InvalidHash");
			Assert.IsNull (hash, "SHA224.Create('InvalidHash')");
		}*/

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateNull ()
		{
			// try to build null implementation
			hash = SHA224.Create (null);
		}

		// none of those values changes for any implementation of defaultSHA224
		[Test]
		public virtual void StaticInfo () 
		{
			string className = hash.ToString ();
			Assert.AreEqual (224, hash.HashSize, className + ".HashSize");
			Assert.AreEqual (1, hash.InputBlockSize, className + ".InputBlockSize");
			Assert.AreEqual (1, hash.OutputBlockSize, className + ".OutputBlockSize");
		}
	}
}
