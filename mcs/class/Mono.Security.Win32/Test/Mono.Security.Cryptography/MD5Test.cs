//
// MD5Test.cs - NUnit Test Cases for MD5 (RFC1321)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.Security.Cryptography {

// References:
// a.	The MD5 Message-Digest Algorithm
//	http://www.ietf.org/rfc/RFC1321.txt

// MD5 is a abstract class - so ALL of the test included here wont be tested
// on the abstract class but should be tested in ALL its descendants.
public class MD5Test {

	protected MD5 hash;

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

	// MD5 ("") = d41d8cd98f00b204e9800998ecf8427e
	[Test]
	public void RFC1321_Test1 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xd4, 0x1d, 0x8c, 0xd9, 0x8f, 0x00, 0xb2, 0x04, 0xe9, 0x80, 0x09, 0x98, 0xec, 0xf8, 0x42, 0x7e };
		byte[] input = new byte [0];

		string testName = className + " 1";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		// N/A RFC1321_e (testName, hash, input, result);
	}

	// MD5 ("a") = 0cc175b9c0f1b6a831c399e269772661
	[Test]
	public void RFC1321_Test2 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x0c, 0xc1, 0x75, 0xb9, 0xc0, 0xf1, 0xb6, 0xa8, 0x31, 0xc3, 0x99, 0xe2, 0x69, 0x77, 0x26, 0x61 };
		byte[] input = Encoding.Default.GetBytes ("a");

		string testName = className + " 2";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		RFC1321_e (testName, hash, input, result);
	}

	// MD5 ("abc") = 900150983cd24fb0d6963f7d28e17f72
	[Test]
	public void RFC1321_Test3 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x90, 0x01, 0x50, 0x98, 0x3c, 0xd2, 0x4f, 0xb0, 0xd6, 0x96, 0x3f, 0x7d, 0x28, 0xe1, 0x7f, 0x72 };
		byte[] input = Encoding.Default.GetBytes ("abc");

		string testName = className + " 3";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		RFC1321_e (testName, hash, input, result);
	}

	// MD5 ("message digest") = f96b697d7cb7938d525a2f31aaf161d0
	[Test]
	public void RFC1321_Test4 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xf9, 0x6b, 0x69, 0x7d, 0x7c, 0xb7, 0x93, 0x8d, 0x52, 0x5a, 0x2f, 0x31, 0xaa, 0xf1, 0x61, 0xd0 };
		byte[] input = Encoding.Default.GetBytes ("message digest");

		string testName = className + " 4";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		RFC1321_e (testName, hash, input, result);
	}

	// MD5 ("abcdefghijklmnopqrstuvwxyz") = c3fcd3d76192e4007dfb496cca67e13b
	[Test]
	public void RFC1321_Test5 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xc3, 0xfc, 0xd3, 0xd7, 0x61, 0x92, 0xe4, 0x00, 0x7d, 0xfb, 0x49, 0x6c, 0xca, 0x67, 0xe1, 0x3b };
		byte[] input = Encoding.Default.GetBytes ("abcdefghijklmnopqrstuvwxyz");

		string testName = className + " 5";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		RFC1321_e (testName, hash, input, result);
	}

	// MD5 ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") =
	//	d174ab98d277d9f5a5611c2c9f419d9f
	[Test]
	public void RFC1321_Test6 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xd1, 0x74, 0xab, 0x98, 0xd2, 0x77, 0xd9, 0xf5, 0xa5, 0x61, 0x1c, 0x2c, 0x9f, 0x41, 0x9d, 0x9f };
		byte[] input = Encoding.Default.GetBytes ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

		string testName = className + " 6";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		RFC1321_e (testName, hash, input, result);
	}

	// MD5 ("123456789012345678901234567890123456789012345678901234567890123456
	//	78901234567890") = 57edf4a22be3c955ac49da2e2107b67a
	[Test]
	public void RFC1321_Test7 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x57, 0xed, 0xf4, 0xa2, 0x2b, 0xe3, 0xc9, 0x55, 0xac, 0x49, 0xda, 0x2e, 0x21, 0x07, 0xb6, 0x7a };
		byte[] input = Encoding.Default.GetBytes ("12345678901234567890123456789012345678901234567890123456789012345678901234567890");

		string testName = className + " 7";
		RFC1321_a (testName, hash, input, result);
		RFC1321_b (testName, hash, input, result);
		RFC1321_c (testName, hash, input, result);
		RFC1321_d (testName, hash, input, result);
		RFC1321_e (testName, hash, input, result);
	}

	public void RFC1321_a (string testName, MD5 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input); 
		AssertEquals (testName + ".a.1", result, output);
		AssertEquals (testName + ".a.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1321_b (string testName, MD5 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input, 0, input.Length); 
		AssertEquals (testName + ".b.1", result, output);
		AssertEquals (testName + ".b.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1321_c (string testName, MD5 hash, byte[] input, byte[] result) 
	{
		MemoryStream ms = new MemoryStream (input);
		byte[] output = hash.ComputeHash (ms); 
		AssertEquals (testName + ".c.1", result, output);
		AssertEquals (testName + ".c.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1321_d (string testName, MD5 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
		AssertEquals (testName + ".d.1", input, output);
		AssertEquals (testName + ".d.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1321_e (string testName, MD5 hash, byte[] input, byte[] result) 
	{
		byte[] copy = new byte [input.Length];
		for (int i=0; i < input.Length - 1; i++)
			hash.TransformBlock (input, i, 1, copy, i);
		byte[] output = hash.TransformFinalBlock (input, input.Length - 1, 1);
		Assert.AreEqual (input [input.Length - 1], output [0], testName + ".e.1");
		AssertEquals (testName + ".e.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	// none of those values changes for any implementation of MD5
	public virtual void StaticInfo () 
	{
		string className = hash.ToString ();
		Assert.AreEqual (128, hash.HashSize, className + ".HashSize");
		Assert.AreEqual (1, hash.InputBlockSize, className + ".InputBlockSize");
		Assert.AreEqual (1, hash.OutputBlockSize, className + ".OutputBlockSize");
	}
}

}
