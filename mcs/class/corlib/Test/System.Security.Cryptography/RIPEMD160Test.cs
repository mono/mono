//
// RIPEMD160Test.cs - NUnit Test Cases for RIPEMD160
//	http://www.esat.kuleuven.ac.be/~bosselae/ripemd160.html
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	// RIPEMD160 is a abstract class - so ALL of the test included here wont be tested
	// on the abstract class but should be tested in ALL its descendants.
	public abstract class RIPEMD160Test {

		protected RIPEMD160 hash;

		// because most crypto stuff works with byte[] buffers
		static public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return;
			if (array1 == null)
				Assert.Fail (msg + " -> First array is NULL");
			if (array2 == null)
				Assert.Fail (msg + " -> Second array is NULL");

			bool a = (array1.Length == array2.Length);
			if (a) {
				for (int i = 0; i < array1.Length; i++) {
					if (array1 [i] != array2 [i]) {
						a = false;
						break;
					}
				}
			}
			if (array1.Length > 0) {
				msg += " -> Expected " + BitConverter.ToString (array1, 0);
				msg += " is different than " + BitConverter.ToString (array2, 0);
			}
			Assert.IsTrue (a, msg);
		}

		// RIPEMD160 ("") = 9c1185a5c5e9fc54612808977ee8f548b2258d31
		[Test]
		public void RIPEMD160_Test1 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x9c, 0x11, 0x85, 0xa5, 0xc5, 0xe9, 0xfc, 0x54, 0x61, 0x28, 0x08, 0x97, 0x7e, 0xe8, 0xf5, 0x48, 0xb2, 0x25, 0x8d, 0x31 };
			byte[] input = new byte [0];
		
			string testName = className + " 1";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			// N/A RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("a") = 0bdc9d2d256b3ee9daae347be6f4dc835a467ffe
		[Test]
		public void RIPEMD160_Test2 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x0b, 0xdc, 0x9d, 0x2d, 0x25, 0x6b, 0x3e, 0xe9, 0xda, 0xae, 0x34, 0x7b, 0xe6, 0xf4, 0xdc, 0x83, 0x5a, 0x46, 0x7f, 0xfe };
			byte[] input = Encoding.Default.GetBytes ("a");
		
			string testName = className + " 2";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("abc") = 8eb208f7e05d987a9b044a8e98c6b087f15a0bfc
		[Test]
		public void RIPEMD160_Test3 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x8e, 0xb2, 0x08, 0xf7, 0xe0, 0x5d, 0x98, 0x7a, 0x9b, 0x04, 0x4a, 0x8e, 0x98, 0xc6, 0xb0, 0x87, 0xf1, 0x5a, 0x0b, 0xfc };
			byte[] input = Encoding.Default.GetBytes ("abc");
		
			string testName = className + " 3";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("message digest") = 5d0689ef49d2fae572b881b123a85ffa21595f36
		[Test]
		public void RIPEMD160_Test4 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x5d, 0x06, 0x89, 0xef, 0x49, 0xd2, 0xfa, 0xe5, 0x72, 0xb8, 0x81, 0xb1, 0x23, 0xa8, 0x5f, 0xfa, 0x21, 0x59, 0x5f, 0x36 };
			byte[] input = Encoding.Default.GetBytes ("message digest");
		
			string testName = className + " 4";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("abcdefghijklmnopqrstuvwxyz") = f71c27109c692c1b56bbdceb5b9d2865b3708dbc
		[Test]
		public void RIPEMD160_Test5 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xf7, 0x1c, 0x27, 0x10, 0x9c, 0x69, 0x2c, 0x1b, 0x56, 0xbb, 0xdc, 0xeb, 0x5b, 0x9d, 0x28, 0x65, 0xb3, 0x70, 0x8d, 0xbc };
			byte[] input = Encoding.Default.GetBytes ("abcdefghijklmnopqrstuvwxyz");
		
			string testName = className + " 5";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq") =
		//	12a053384a9c0c88e405a06c27dcf49ada62eb2b
		[Test]
		public void RIPEMD160_Test6 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x12, 0xa0, 0x53, 0x38, 0x4a, 0x9c, 0x0c, 0x88, 0xe4, 0x05, 0xa0, 0x6c, 0x27, 0xdc, 0xf4, 0x9a, 0xda, 0x62, 0xeb, 0x2b };
			byte[] input = Encoding.Default.GetBytes ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");
		
			string testName = className + " 6";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") =
		//	b0e20b6e3116640286ed3a87a5713079b21f5189
		[Test]
		public void RIPEMD160_Test7 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xb0, 0xe2, 0x0b, 0x6e, 0x31, 0x16, 0x64, 0x02, 0x86, 0xed, 0x3a, 0x87, 0xa5, 0x71, 0x30, 0x79, 0xb2, 0x1f, 0x51, 0x89 };
			byte[] input = Encoding.Default.GetBytes ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
		
			string testName = className + " 6";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 ("123456789012345678901234567890123456789012345678901234567890123456
		//	78901234567890") = 9b752e45573d4b39f4dbd3323cab82bf63326bfb
		[Test]
		public void RIPEMD160_Test8 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x9b, 0x75, 0x2e, 0x45, 0x57, 0x3d, 0x4b, 0x39, 0xf4, 0xdb, 0xd3, 0x32, 0x3c, 0xab, 0x82, 0xbf, 0x63, 0x32, 0x6b, 0xfb };
			byte[] input = Encoding.Default.GetBytes ("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
		
			string testName = className + " 7";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		// RIPEMD160 (1000000 x 'a') = 52783243c1697bdbe16d37f97f68f08325dc1528
		[Test]
		public void RIPEMD160_Test9 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x52, 0x78, 0x32, 0x43, 0xc1, 0x69, 0x7b, 0xdb, 0xe1, 0x6d, 0x37, 0xf9, 0x7f, 0x68, 0xf0, 0x83, 0x25, 0xdc, 0x15, 0x28 };
			byte[] input = new byte [1000000];
			for (int i = 0; i < 1000000; i++)
				input[i] = 0x61; // a
		
			string testName = className + " 7";
			RIPEMD160_a (testName, hash, input, result);
			RIPEMD160_b (testName, hash, input, result);
			RIPEMD160_c (testName, hash, input, result);
			RIPEMD160_d (testName, hash, input, result);
			RIPEMD160_e (testName, hash, input, result);
		}

		public void RIPEMD160_a (string testName, RIPEMD160 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.ComputeHash (input); 
			AssertEquals (testName + ".a.1", result, output);
			AssertEquals (testName + ".a.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RIPEMD160_b (string testName, RIPEMD160 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.ComputeHash (input, 0, input.Length); 
			AssertEquals (testName + ".b.1", result, output);
			AssertEquals (testName + ".b.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RIPEMD160_c (string testName, RIPEMD160 hash, byte[] input, byte[] result) 
		{
			MemoryStream ms = new MemoryStream (input);
			byte[] output = hash.ComputeHash (ms); 
			AssertEquals (testName + ".c.1", result, output);
			AssertEquals (testName + ".c.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RIPEMD160_d (string testName, RIPEMD160 hash, byte[] input, byte[] result) 
		{
			hash.TransformFinalBlock (input, 0, input.Length);
			AssertEquals (testName + ".d", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RIPEMD160_e (string testName, RIPEMD160 hash, byte[] input, byte[] result) 
		{
			byte[] copy = new byte [input.Length];
			for (int i=0; i < input.Length - 1; i++)
				hash.TransformBlock (input, i, 1, copy, i);
			hash.TransformFinalBlock (input, input.Length - 1, 1);
			AssertEquals (testName + ".e", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		// none of those values changes for any implementation of RIPEMD160
		[Test]
		public virtual void StaticInfo () 
		{
			string className = hash.ToString ();
			Assert.AreEqual (160, hash.HashSize, className + ".HashSize");
			Assert.AreEqual (1, hash.InputBlockSize, className + ".InputBlockSize");
			Assert.AreEqual (1, hash.OutputBlockSize, className + ".OutputBlockSize");
		}
	}
}

#endif