//
// MD4Test.cs - NUnit Test Cases for MD4 (RFC1320)
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Cryptography {

	// References:
	// a.	The MD4 Message-Digest Algorithm
	//	http://www.ietf.org/rfc/RFC1320.txt

	// MD4 is a abstract class - so ALL of the test included here wont be tested
	// on the abstract class but should be tested in ALL its descendants.
	public abstract class MD4Test : Assertion {

		protected MD4 hash;

		// because most crypto stuff works with byte[] buffers
		static public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return;
			if (array1 == null)
				Assertion.Fail (msg + " -> First array is NULL");
			if (array2 == null)
				Assertion.Fail (msg + " -> Second array is NULL");

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
			Assertion.Assert (msg, a);
		}

		// MD4 ("") = 31d6cfe0d16ae931b73c59d7e0c089c0
		[Test]
		public void RFC1320_Test1 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x31, 0xd6, 0xcf, 0xe0, 0xd1, 0x6a, 0xe9, 0x31, 0xb7, 0x3c, 0x59, 0xd7, 0xe0, 0xc0, 0x89, 0xc0 };
			byte[] input = new byte [0];

			string testName = className + " 1";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			// N/A RFC1320_e (testName, hash, input, result);
		}

		// MD4 ("a") = bde52cb31de33e46245e05fbdbd6fb24
		[Test]
		public void RFC1320_Test2 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xbd, 0xe5, 0x2c, 0xb3, 0x1d, 0xe3, 0x3e, 0x46, 0x24, 0x5e, 0x05, 0xfb, 0xdb, 0xd6, 0xfb, 0x24 };
			byte[] input = Encoding.Default.GetBytes ("a");

			string testName = className + " 2";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			RFC1320_e (testName, hash, input, result);
		}

		// MD4 ("abc") = a448017aaf21d8525fc10ae87aa6729d
		[Test]
		public void RFC1320_Test3 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xa4, 0x48, 0x01, 0x7a, 0xaf, 0x21, 0xd8, 0x52, 0x5f, 0xc1, 0x0a, 0xe8, 0x7a, 0xa6, 0x72, 0x9d };
			byte[] input = Encoding.Default.GetBytes ("abc");

			string testName = className + " 3";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			RFC1320_e (testName, hash, input, result);
		}

		// MD4 ("message digest") = d9130a8164549fe818874806e1c7014b
		[Test]
		public void RFC1320_Test4 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xd9, 0x13, 0x0a, 0x81, 0x64, 0x54, 0x9f, 0xe8, 0x18, 0x87, 0x48, 0x06, 0xe1, 0xc7, 0x01, 0x4b };
			byte[] input = Encoding.Default.GetBytes ("message digest");

			string testName = className + " 4";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			RFC1320_e (testName, hash, input, result);
		}

		// MD4 ("abcdefghijklmnopqrstuvwxyz") = d79e1c308aa5bbcdeea8ed63df412da9
		[Test]
		public void RFC1320_Test5 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xd7, 0x9e, 0x1c, 0x30, 0x8a, 0xa5, 0xbb, 0xcd, 0xee, 0xa8, 0xed, 0x63, 0xdf, 0x41, 0x2d, 0xa9 };
			byte[] input = Encoding.Default.GetBytes ("abcdefghijklmnopqrstuvwxyz");

			string testName = className + " 5";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			RFC1320_e (testName, hash, input, result);
		}

		// MD4 ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") =
		//	043f8582f241db351ce627e153e7f0e4
		[Test]
		public void RFC1320_Test6 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0x04, 0x3f, 0x85, 0x82, 0xf2, 0x41, 0xdb, 0x35, 0x1c, 0xe6, 0x27, 0xe1, 0x53, 0xe7, 0xf0, 0xe4 };
			byte[] input = Encoding.Default.GetBytes ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

			string testName = className + " 6";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			RFC1320_e (testName, hash, input, result);
		}

		// MD4 ("123456789012345678901234567890123456789012345678901234567890123456
		//	78901234567890") = e33b4ddc9c38f2199c3e7b164fcc0536
		[Test]
		public void RFC1320_Test7 () 
		{
			string className = hash.ToString ();
			byte[] result = { 0xe3, 0x3b, 0x4d, 0xdc, 0x9c, 0x38, 0xf2, 0x19, 0x9c, 0x3e, 0x7b, 0x16, 0x4f, 0xcc, 0x05, 0x36 };
			byte[] input = Encoding.Default.GetBytes ("12345678901234567890123456789012345678901234567890123456789012345678901234567890");

			string testName = className + " 7";
			RFC1320_a (testName, hash, input, result);
			RFC1320_b (testName, hash, input, result);
			RFC1320_c (testName, hash, input, result);
			RFC1320_d (testName, hash, input, result);
			RFC1320_e (testName, hash, input, result);
		}

		public void RFC1320_a (string testName, MD4 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.ComputeHash (input); 
			AssertEquals (testName + ".a.1", result, output);
			AssertEquals (testName + ".a.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RFC1320_b (string testName, MD4 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.ComputeHash (input, 0, input.Length); 
			AssertEquals (testName + ".b.1", result, output);
			AssertEquals (testName + ".b.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RFC1320_c (string testName, MD4 hash, byte[] input, byte[] result) 
		{
			MemoryStream ms = new MemoryStream (input);
			byte[] output = hash.ComputeHash (ms); 
			AssertEquals (testName + ".c.1", result, output);
			AssertEquals (testName + ".c.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RFC1320_d (string testName, MD4 hash, byte[] input, byte[] result) 
		{
			byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
			AssertEquals (testName + ".d.1", input, output);
			AssertEquals (testName + ".d.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		public void RFC1320_e (string testName, MD4 hash, byte[] input, byte[] result) 
		{
			byte[] copy = new byte [input.Length];
			for (int i=0; i < input.Length - 1; i++)
				hash.TransformBlock (input, i, 1, copy, i);
			byte[] output = hash.TransformFinalBlock (input, input.Length - 1, 1);
			AssertEquals (testName + ".e.1", input [input.Length - 1], output [0]);
			AssertEquals (testName + ".e.2", result, hash.Hash);
			// required or next operation will still return old hash
			hash.Initialize ();
		}

		// none of those values changes for any implementation of MD4
		[Test]
		public virtual void StaticInfo () 
		{
			string className = hash.ToString ();
			AssertEquals (className + ".HashSize", 128, hash.HashSize);
			AssertEquals (className + ".InputBlockSize", 1, hash.InputBlockSize);
			AssertEquals (className + ".OutputBlockSize", 1, hash.OutputBlockSize);
		}
		
		[Test]
		public virtual void Create () 
		{
			// create the default implementation
			HashAlgorithm h = MD4.Create ();
			Assert ("MD4Managed", (h is MD4Managed));
			// Note: will fail is default is changed in machine.config
		}
	}
}
