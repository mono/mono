//
// SHA384Test.cs - NUnit Test Cases for SHA384
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

// SHA384 is a abstract class - so most of the test included here wont be tested
// on the abstract class but should be tested in ALL its descendants.
public class SHA384Test : HashAlgorithmTest {

	public SHA384Test () : base ("System.Security.Cryptography.SHA384 testsuite") {}
	public SHA384Test (string name) : base (name) {}

	protected override void SetUp () 
	{
		hash = SHA384.Create ();
	}

	protected override void TearDown () {}

	public static new ITest Suite {
		get { 
			return new TestSuite (typeof (SHA384Test)); 
		}
	}

	// test vectors from NIST FIPS 186-2

	private string input1 = "abc";
	private string input2 = "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu";

	public void FIPS186_Test1 (SHA384 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0xcb, 0x00, 0x75, 0x3f, 0x45, 0xa3, 0x5e, 0x8b,
				  0xb5, 0xa0, 0x3d, 0x69, 0x9a, 0xc6, 0x50, 0x07,
				  0x27, 0x2c, 0x32, 0xab, 0x0e, 0xde, 0xd1, 0x63,
				  0x1a, 0x8b, 0x60, 0x5a, 0x43, 0xff, 0x5b, 0xed,
				  0x80, 0x86, 0x07, 0x2b, 0xa1, 0xe7, 0xcc, 0x23,
				  0x58, 0xba, 0xec, 0xa1, 0x34, 0xc8, 0x25, 0xa7 };
		byte[] input = Encoding.Default.GetBytes (input1);
	
		string testName = className + " 1";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test2 (SHA384 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0x09, 0x33, 0x0c, 0x33, 0xf7, 0x11, 0x47, 0xe8,
				  0x3d, 0x19, 0x2f, 0xc7, 0x82, 0xcd, 0x1b, 0x47,
				  0x53, 0x11, 0x1b, 0x17, 0x3b, 0x3b, 0x05, 0xd2,
				  0x2f, 0xa0, 0x80, 0x86, 0xe3, 0xb0, 0xf7, 0x12,
				  0xfc, 0xc7, 0xc7, 0x1a, 0x55, 0x7e, 0x2d, 0xb9,
				  0x66, 0xc3, 0xe9, 0xfa, 0x91, 0x74, 0x60, 0x39 };
		byte[] input = Encoding.Default.GetBytes (input2);
	
		string testName = className + " 2";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test3 (SHA384 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0x9d, 0x0e, 0x18, 0x09, 0x71, 0x64, 0x74, 0xcb,
				  0x08, 0x6e, 0x83, 0x4e, 0x31, 0x0a, 0x4a, 0x1c,
				  0xed, 0x14, 0x9e, 0x9c, 0x00, 0xf2, 0x48, 0x52,
				  0x79, 0x72, 0xce, 0xc5, 0x70, 0x4c, 0x2a, 0x5b,
				  0x07, 0xb8, 0xb3, 0xdc, 0x38, 0xec, 0xc4, 0xeb,
				  0xae, 0x97, 0xdd, 0xd8, 0x7f, 0x3d, 0x89, 0x85 };
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

	public void FIPS186_a (string testName, SHA384 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input); 
		AssertEquals (testName + ".a.1", result, output);
		AssertEquals (testName + ".a.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_b (string testName, SHA384 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input, 0, input.Length); 
		AssertEquals (testName + ".b.1", result, output);
		AssertEquals (testName + ".b.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_c (string testName, SHA384 hash, byte[] input, byte[] result) 
	{
		MemoryStream ms = new MemoryStream (input);
		byte[] output = hash.ComputeHash (ms); 
		AssertEquals (testName + ".c.1", result, output);
		AssertEquals (testName + ".c.2", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_d (string testName, SHA384 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		// AssertEquals( testName + ".d.1", result, output );
		AssertEquals (testName + ".d", result, hash.Hash);
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_e (string testName, SHA384 hash, byte[] input, byte[] result) 
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
		const string defaultSHA384 = "System.Security.Cryptography.SHA384Managed";

		// try to build the default implementation
		SHA384 hash = SHA384.Create ();
		AssertEquals ("SHA384.Create()", hash.ToString (), defaultSHA384);

		// try to build, in every way, a SHA384 implementation
		hash = SHA384.Create ("SHA384");
		AssertEquals ("SHA384.Create('SHA384')", hash.ToString (), defaultSHA384);
		hash = SHA384.Create ("SHA-384");
		AssertEquals ("SHA384.Create('SHA-384')", hash.ToString (), defaultSHA384);

		// try to build an incorrect hash algorithms
		try {
			hash = SHA384.Create ("MD5");
			Fail ("SHA384.Create('MD5') should throw InvalidCastException");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("SHA384.Create('MD5') should throw InvalidCastException not " + e.ToString ());
		}

		// try to build invalid implementation
		hash = SHA384.Create ("InvalidHash");
		AssertNull ("SHA384.Create('InvalidHash')", hash);

		// try to build null implementation
		try {
			hash = SHA384.Create (null);
			Fail ("SHA384.Create(null) should throw ArgumentNullException");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("SHA384.Create(null) should throw ArgumentNullException not " + e.ToString ());
		}
	}

	// none of those values changes for any implementation of defaultSHA384
	public virtual void TestStaticInfo () 
	{
		string className = hash.ToString ();
		AssertEquals (className + ".HashSize", 384, hash.HashSize);
		AssertEquals (className + ".InputBlockSize", 1, hash.InputBlockSize);
		AssertEquals (className + ".OutputBlockSize", 1, hash.OutputBlockSize);
	}

}

}
