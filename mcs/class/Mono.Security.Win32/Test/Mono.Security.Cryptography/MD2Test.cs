//
// MD2Test.cs - NUnit Test Cases for MD2 (RFC1319)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using Mono.Security.Cryptography;
using System.Text;

namespace MonoTests.Security.Cryptography {

// References:
// a.	The MD2 Message-Digest Algorithm
//	http://www.ietf.org/rfc/rfc1319.txt

// MD2 is a abstract class - so ALL of the test included here wont be tested
// on the abstract class but should be tested in ALL its descendants.
public class MD2Test {

	protected MD2 hash;

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

	// MD2 ("") = 8350e5a3e24c153df2275c9f80692773
	[Test]
	public void RFC1319_Test1 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x83, 0x50, 0xe5, 0xa3, 0xe2, 0x4c, 0x15, 0x3d, 0xf2, 0x27, 0x5c, 0x9f, 0x80, 0x69, 0x27, 0x73 };
		byte[] input = new byte [0];
	
		string testName = className + " 1";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		// N/A RFC1319_e (testName, hash, input, result);
	}

	// MD2 ("a") = 32ec01ec4a6dac72c0ab96fb34c0b5d1
	[Test]
	public void RFC1319_Test2 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x32, 0xec, 0x01, 0xec, 0x4a, 0x6d, 0xac, 0x72, 0xc0, 0xab, 0x96, 0xfb, 0x34, 0xc0, 0xb5, 0xd1 };
		byte[] input = Encoding.Default.GetBytes ("a");
	
		string testName = className + " 2";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		RFC1319_e (testName, hash, input, result);
	}

	// MD2 ("abc") = da853b0d3f88d99b30283a69e6ded6bb
	[Test]
	public void RFC1319_Test3 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xda, 0x85, 0x3b, 0x0d, 0x3f, 0x88, 0xd9, 0x9b, 0x30, 0x28, 0x3a, 0x69, 0xe6, 0xde, 0xd6, 0xbb };
		byte[] input = Encoding.Default.GetBytes ("abc");
	
		string testName = className + " 3";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		RFC1319_e (testName, hash, input, result);
	}

	// MD2 ("message digest") = ab4f496bfb2a530b219ff33031fe06b0
	[Test]
	public void RFC1319_Test4 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xab, 0x4f, 0x49, 0x6b, 0xfb, 0x2a, 0x53, 0x0b, 0x21, 0x9f, 0xf3, 0x30, 0x31, 0xfe, 0x06, 0xb0 };
		byte[] input = Encoding.Default.GetBytes ("message digest");
	
		string testName = className + " 4";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		RFC1319_e (testName, hash, input, result);
	}

	// MD2 ("abcdefghijklmnopqrstuvwxyz") = 4e8ddff3650292ab5a4108c3aa47940b
	[Test]
	public void RFC1319_Test5 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x4e, 0x8d, 0xdf, 0xf3, 0x65, 0x02, 0x92, 0xab, 0x5a, 0x41, 0x08, 0xc3, 0xaa, 0x47, 0x94, 0x0b };
		byte[] input = Encoding.Default.GetBytes ("abcdefghijklmnopqrstuvwxyz");
	
		string testName = className + " 5";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		RFC1319_e (testName, hash, input, result);
	}

	// MD2 ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") =
	//	da33def2a42df13975352846c30338cd
	[Test]
	public void RFC1319_Test6 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xda, 0x33, 0xde, 0xf2, 0xa4, 0x2d, 0xf1, 0x39, 0x75, 0x35, 0x28, 0x46, 0xc3, 0x03, 0x38, 0xcd };
		byte[] input = Encoding.Default.GetBytes ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
	
		string testName = className + " 6";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		RFC1319_e (testName, hash, input, result);
	}

	// MD2 ("123456789012345678901234567890123456789012345678901234567890123456
	//	78901234567890") = d5976f79d83d3a0dc9806c3c66f3efd8
	[Test]
	public void RFC1319_Test7 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xd5, 0x97, 0x6f, 0x79, 0xd8, 0x3d, 0x3a, 0x0d, 0xc9, 0x80, 0x6c, 0x3c, 0x66, 0xf3, 0xef, 0xd8 };
		byte[] input = Encoding.Default.GetBytes ("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
	
		string testName = className + " 7";
		RFC1319_a (testName, hash, input, result);
		RFC1319_b (testName, hash, input, result);
		RFC1319_c (testName, hash, input, result);
		RFC1319_d (testName, hash, input, result);
		RFC1319_e (testName, hash, input, result);
	}

	public void RFC1319_a (string testName, MD2 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input); 
		AssertEquals (testName + ".a.1", result, output);
		AssertEquals (testName + ".a.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1319_b (string testName, MD2 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input, 0, input.Length); 
		AssertEquals (testName + ".b.1", result, output);
		AssertEquals (testName + ".b.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1319_c (string testName, MD2 hash, byte[] input, byte[] result) 
	{
		MemoryStream ms = new MemoryStream (input);
		byte[] output = hash.ComputeHash (ms); 
		AssertEquals (testName + ".c.1", result, output);
		AssertEquals (testName + ".c.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1319_d (string testName, MD2 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
		AssertEquals (testName + ".d.1", input, output);
		AssertEquals (testName + ".d.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void RFC1319_e (string testName, MD2 hash, byte[] input, byte[] result) 
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

	// none of those values changes for any implementation of MD2
	public virtual void StaticInfo () 
	{
		string className = hash.ToString ();
		Assert.AreEqual (128, hash.HashSize, className + ".HashSize",);
		Assert.AreEqual (1, hash.InputBlockSize, className + ".InputBlockSize");
		Assert.AreEqual (1, hash.OutputBlockSize, className + ".OutputBlockSize");
	}
}

}
