//
// SHA1Test.cs - NUnit Test Cases for SHA1
//
// Author:
//		Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography
{

// References:
// a.	FIPS PUB 180-1: Secure Hash Standard
//	http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.txt

// SHA1 is a abstract class - so most of the test included here wont be tested
// on the abstract class but should be tested in ALL its descendants.
public class SHA1Test : HashAlgorithmTest 
{
	protected override void SetUp () 
	{
		hash = SHA1.Create();
	}

	protected override void TearDown () {}

	public new void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// test vectors from NIST FIPS 186-2

	private string input1 = "abc";
	private string input2 = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";

	public void FIPS186_Test1 (SHA1 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0xa9, 0x99, 0x3e, 0x36, 0x47, 0x06, 0x81, 0x6a, 0xba, 0x3e, 0x25, 0x71, 0x78, 0x50, 0xc2, 0x6c, 0x9c, 0xd0, 0xd8, 0x9d };
		byte[] input = Encoding.Default.GetBytes (input1);
		
		string testName = className + " 1";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test2 (SHA1 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0x84, 0x98, 0x3e, 0x44, 0x1c, 0x3b, 0xd2, 0x6e, 0xba, 0xae, 0x4a, 0xa1, 0xf9, 0x51, 0x29, 0xe5, 0xe5, 0x46, 0x70, 0xf1 };
		byte[] input = Encoding.Default.GetBytes (input2);
		
		string testName = className + " 2";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test3 (SHA1 hash) 
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

	public override void TestCreate ()
	{
		// Note: These tests will only be valid without a "machine.config" file
		// or a "machine.config" file that do not modify the default algorithm
		// configuration.
		const string defaultSHA1 = "System.Security.Cryptography.SHA1CryptoServiceProvider";

		// try to build the default implementation
		SHA1 hash = SHA1.Create ();
		AssertEquals ("SHA1.Create()", hash.ToString (), defaultSHA1);

		// try to build, in every way, a SHA1 implementation
		// note that it's not possible to create an instance of SHA1Managed using the Create methods
		hash = SHA1.Create ("SHA");
		AssertEquals ("SHA1.Create('SHA')", hash.ToString (), defaultSHA1);
		hash = SHA1.Create ("SHA1");
		AssertEquals ("SHA1.Create('SHA1')", hash.ToString (), defaultSHA1);
		hash = SHA1.Create ("System.Security.Cryptography.SHA1");
		AssertEquals ("SHA1.Create('System.Security.Cryptography.SHA1')", hash.ToString (), defaultSHA1);
		hash = SHA1.Create ("System.Security.Cryptography.HashAlgorithm" );
		AssertEquals ("SHA1.Create('System.Security.Cryptography.HashAlgorithm')", hash.ToString (), defaultSHA1);

		// try to build an incorrect hash algorithms
		try {
			hash = SHA1.Create ("MD5");
			Fail ("SHA1.Create('MD5') should throw InvalidCastException");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("SHA1.Create(null) should throw InvalidCastException not " + e.ToString ());
		}

		// try to build invalid implementation
		hash = SHA1.Create ("InvalidHash");
		AssertNull ("SHA1.Create('InvalidHash')", hash);

		// try to build null implementation
		try {
			hash = SHA1.Create (null);
			Fail ("SHA1.Create(null) should throw ArgumentNullException");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("SHA1.Create(null) should throw ArgumentNullException not " + e.ToString ());
		}
	}

	// none of those values changes for any implementation of SHA1
	public virtual void TestStaticInfo () 
	{
		string className = hash.ToString ();
		AssertEquals (className + ".HashSize", 160, hash.HashSize);
		AssertEquals (className + ".InputBlockSize", 1, hash.InputBlockSize);
		AssertEquals (className + ".OutputBlockSize", 1, hash.OutputBlockSize);
	}

}

}
