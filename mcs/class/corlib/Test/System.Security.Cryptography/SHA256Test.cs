//
// SHA256Test.cs - NUnit Test Cases for SHA256
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// References:
// a.	FIPS PUB 180-2: Secure Hash Standard
//	http://csrc.nist.gov/publications/fips/fips180-2/fip180-2.txt

// SHA256 is a abstract class - so most of the test included here wont be tested
// on the abstract class but should be tested in ALL its descendants.
public class SHA256Test : HashAlgorithmTest {

	protected override void SetUp () 
	{
		hash = SHA256.Create ();
	}

	protected override void TearDown () {}

	// test vectors from NIST FIPS 186-2

	private string input1 = "abc";
	private string input2 = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";

	public void FIPS186_Test1 (SHA256 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0xba, 0x78, 0x16, 0xbf, 0x8f, 0x01, 0xcf, 0xea, 
				  0x41, 0x41, 0x40, 0xde, 0x5d, 0xae, 0x22, 0x23, 
				  0xb0, 0x03, 0x61, 0xa3, 0x96, 0x17, 0x7a, 0x9c, 
				  0xb4, 0x10, 0xff, 0x61, 0xf2, 0x00, 0x15, 0xad };
		byte[] input = Encoding.Default.GetBytes (input1);

		string testName = className + " 1";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test2 (SHA256 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0x24, 0x8d, 0x6a, 0x61, 0xd2, 0x06, 0x38, 0xb8, 
				  0xe5, 0xc0, 0x26, 0x93, 0x0c, 0x3e, 0x60, 0x39, 
				  0xa3, 0x3c, 0xe4, 0x59, 0x64, 0xff, 0x21, 0x67, 
				  0xf6, 0xec, 0xed, 0xd4, 0x19, 0xdb, 0x06, 0xc1 };
		byte[] input = Encoding.Default.GetBytes (input2);

		string testName = className + " 2";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test3 (SHA256 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0xcd, 0xc7, 0x6e, 0x5c, 0x99, 0x14, 0xfb, 0x92, 
				  0x81, 0xa1, 0xc7, 0xe2, 0x84, 0xd7, 0x3e, 0x67, 
				  0xf1, 0x80, 0x9a, 0x48, 0xa4, 0x97, 0x20, 0x0e, 
				  0x04, 0x6d, 0x39, 0xcc, 0xc7, 0x11, 0x2c, 0xd0 };
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

	public void FIPS186_a (string testName, SHA256 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input); 
		AssertEquals (testName + ".a.1", result, output);
		AssertEquals (testName + ".a.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_b (string testName, SHA256 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input, 0, input.Length); 
		AssertEquals (testName + ".b.1", result, output);
		AssertEquals (testName + ".b.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_c (string testName, SHA256 hash, byte[] input, byte[] result) 
	{
		MemoryStream ms = new MemoryStream (input);
		byte[] output = hash.ComputeHash (ms); 
		AssertEquals (testName + ".c.1", result, output);
		AssertEquals (testName + ".c.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_d (string testName, SHA256 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		// AssertEquals( testName + ".d.1", result, output );
		AssertEquals (testName + ".d", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_e (string testName, SHA256 hash, byte[] input, byte[] result) 
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
		const string defaultSHA256 = "System.Security.Cryptography.SHA256Managed";

		// try to build the default implementation
		SHA256 hash = SHA256.Create ();
		AssertEquals ("SHA256.Create()", hash.ToString (), defaultSHA256);

		// try to build, in every way, a SHA256 implementation
		hash = SHA256.Create ("SHA256");
		AssertEquals ("SHA256.Create('SHA256')", hash.ToString (), defaultSHA256);
		hash = SHA256.Create ("SHA-256");
		AssertEquals ("SHA256.Create('SHA-256')", hash.ToString (), defaultSHA256);

		// try to build an incorrect hash algorithms
		try {
			hash = SHA256.Create ("MD5");
			Fail ("SHA256.Create('MD5') should throw InvalidCastException");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("SHA256.Create('MD5') should throw InvalidCastException not " + e.ToString ());
		}

		// try to build invalid implementation
		hash = SHA256.Create ("InvalidHash");
		AssertNull ("SHA256.Create('InvalidHash')", hash);

		// try to build null implementation
		try {
			hash = SHA256.Create (null);
			Fail ("SHA256.Create(null) should throw ArgumentNullException");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("SHA256.Create(null) should throw ArgumentNullException not " + e.ToString ());
		}
	}

	// none of those values changes for any implementation of defaultSHA256
	public virtual void TestStaticInfo () {
		string className = hash.ToString ();
		AssertEquals (className + ".HashSize", 256, hash.HashSize);
		AssertEquals (className + ".InputBlockSize", 1, hash.InputBlockSize);
		AssertEquals (className + ".OutputBlockSize", 1, hash.OutputBlockSize);
	}

}

}
