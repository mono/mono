//
// SHA1Test.cs - NUnit Test Cases for SHA1 (FIPS186)
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
// a.	FIPS PUB 180-1: Secure Hash Standard
//	http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.txt

// we inherit from SHA1Test because all SHA1 implementation must return the 
// same results (hence should run a common set of unit tests).
public class SHA1Test {

	protected SHA1 hash;

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
		msg += " -> Expected " + BitConverter.ToString (array1, 0);
		msg += " is different than " + BitConverter.ToString (array2, 0);
		Assert.IsTrue (a, msg);
	}

	[SetUp]
	protected void SetUp () 
	{
		hash = new Mono.Security.Cryptography.SHA1CryptoServiceProvider ();
	}

	// none of those values changes for a particuliar implementation of SHA1
	[Test]
	public void TestStaticInfo () 
	{
		// test all values static for SHA1
		string className = hash.ToString ();
		Assert.AreEqual (className + ".HashSize", 160, hash.HashSize);
		Assert.AreEqual (className + ".InputBlockSize", 1, hash.InputBlockSize);
		Assert.AreEqual (className + ".OutputBlockSize", 1, hash.OutputBlockSize);
		Assert.AreEqual (className + ".CanReuseTransform", true, hash.CanReuseTransform);
		Assert.AreEqual (className + ".CanTransformMultipleBlocks", true, hash.CanTransformMultipleBlocks);
		Assert.AreEqual (className + ".ToString()", "Mono.Security.Cryptography.SHA1CryptoServiceProvider", className);
	}

	// First test, we hash the string "abc"
	[Test]
	public void FIPS186_Test1 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0xa9, 0x99, 0x3e, 0x36, 0x47, 0x06, 0x81, 0x6a, 0xba, 0x3e, 0x25, 0x71, 0x78, 0x50, 0xc2, 0x6c, 0x9c, 0xd0, 0xd8, 0x9d };
		byte[] input = Encoding.Default.GetBytes ("abc");
	
		string testName = className + " 1";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	// Second test, we hash the string "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
	[Test]
	public void FIPS186_Test2 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x84, 0x98, 0x3e, 0x44, 0x1c, 0x3b, 0xd2, 0x6e, 0xba, 0xae, 0x4a, 0xa1, 0xf9, 0x51, 0x29, 0xe5, 0xe5, 0x46, 0x70, 0xf1 };
		byte[] input = Encoding.Default.GetBytes ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");
	
		string testName = className + " 2";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	// Third test, we hash 1,000,000 times the character "a"
	[Test]
	[Ignore("Much too long - must implements blocks")]
	public void FIPS186_Test3 () 
	{
		string className = hash.ToString ();
		byte[] result = { 0x34, 0xaa, 0x97, 0x3c, 0xd4, 0xc4, 0xda, 0xa4, 0xf6, 0x1e, 0xeb, 0x2b, 0xdb, 0xad, 0x27, 0x31, 0x65, 0x34, 0x01, 0x6f };
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

	public void FIPS186_a (string testName, SHA1 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input); 
		AssertEquals (testName + ".a.1", result, output);
		AssertEquals (testName + ".a.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_b (string testName, SHA1 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input, 0, input.Length); 
		AssertEquals (testName + ".b.1", result, output);
		AssertEquals (testName + ".b.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_c (string testName, SHA1 hash, byte[] input, byte[] result) 
	{
		MemoryStream ms = new MemoryStream (input);
		byte[] output = hash.ComputeHash (ms); 
		AssertEquals (testName + ".c.1", result, output);
		AssertEquals (testName + ".c.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_d (string testName, SHA1 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		// AssertEquals( testName + ".d.1", result, output );
		AssertEquals (testName + ".d", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_e (string testName, SHA1 hash, byte[] input, byte[] result) 
	{
		byte[] copy = new byte [input.Length];
		for (int i=0; i < input.Length - 1; i++)
			hash.TransformBlock (input, i, 1, copy, i);
		byte[] output = hash.TransformFinalBlock (input, input.Length - 1, 1);
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		// AssertEquals (testName + ".e.1", result, output);
		AssertEquals (testName + ".e", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}
}

}
