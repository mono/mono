//
// SHA512TestBase.cs - NUnit Test Cases for SHA512
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007-2008 Novell, Inc (http://www.novell.com)
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

// SHA512 is a abstract class - so most of the test included here wont be tested
// on the abstract class but should be tested in ALL its descendants.

// we can't make this a [TestFixture] since the class is shared with System.Core
// and causes issues in XA where these are in the same process.
public abstract class SHA512TestBase : HashAlgorithmTestBase {

	[SetUp]
	public override void SetUp () 
	{
		hash = SHA512.Create ();
	}

	// the hash algorithm only exists as a managed implementation
	public override bool ManagedHashImplementation {
		get { return true; }
	}

	// test vectors from NIST FIPS 186-2

	private string input1 = "abc";
	private string input2 = "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu";

	public void FIPS186_Test1 (SHA512 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0xdd, 0xaf, 0x35, 0xa1, 0x93, 0x61, 0x7a, 0xba,
				  0xcc, 0x41, 0x73, 0x49, 0xae, 0x20, 0x41, 0x31, 
				  0x12, 0xe6, 0xfa, 0x4e, 0x89, 0xa9, 0x7e, 0xa2, 
				  0x0a, 0x9e, 0xee, 0xe6, 0x4b, 0x55, 0xd3, 0x9a, 
				  0x21, 0x92, 0x99, 0x2a, 0x27, 0x4f, 0xc1, 0xa8,
				  0x36, 0xba, 0x3c, 0x23, 0xa3, 0xfe, 0xeb, 0xbd, 
				  0x45, 0x4d, 0x44, 0x23, 0x64, 0x3c, 0xe8, 0x0e, 
				  0x2a, 0x9a, 0xc9, 0x4f, 0xa5, 0x4c, 0xa4, 0x9f };
		byte[] input = Encoding.Default.GetBytes (input1);

		string testName = className + " 1";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test2 (SHA512 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0x8e, 0x95, 0x9b, 0x75, 0xda, 0xe3, 0x13, 0xda, 
				  0x8c, 0xf4, 0xf7, 0x28, 0x14, 0xfc, 0x14, 0x3f, 
				  0x8f, 0x77, 0x79, 0xc6, 0xeb, 0x9f, 0x7f, 0xa1, 
				  0x72, 0x99, 0xae, 0xad, 0xb6, 0x88, 0x90, 0x18, 
				  0x50, 0x1d, 0x28, 0x9e, 0x49, 0x00, 0xf7, 0xe4, 
				  0x33, 0x1b, 0x99, 0xde, 0xc4, 0xb5, 0x43, 0x3a, 
				  0xc7, 0xd3, 0x29, 0xee, 0xb6, 0xdd, 0x26, 0x54, 
				  0x5e, 0x96, 0xe5, 0x5b, 0x87, 0x4b, 0xe9, 0x09 };
		byte[] input = Encoding.Default.GetBytes (input2);

		string testName = className + " 2";
		FIPS186_a (testName, hash, input, result);
		FIPS186_b (testName, hash, input, result);
		FIPS186_c (testName, hash, input, result);
		FIPS186_d (testName, hash, input, result);
		FIPS186_e (testName, hash, input, result);
	}

	public void FIPS186_Test3 (SHA512 hash) 
	{
		string className = hash.ToString ();
		byte[] result = { 0xe7, 0x18, 0x48, 0x3d, 0x0c, 0xe7, 0x69, 0x64, 
				  0x4e, 0x2e, 0x42, 0xc7, 0xbc, 0x15, 0xb4, 0x63, 
				  0x8e, 0x1f, 0x98, 0xb1, 0x3b, 0x20, 0x44, 0x28,
				  0x56, 0x32, 0xa8, 0x03, 0xaf, 0xa9, 0x73, 0xeb,
                                  0xde, 0x0f, 0xf2, 0x44, 0x87, 0x7e, 0xa6, 0x0a, 
				  0x4c, 0xb0, 0x43, 0x2c, 0xe5, 0x77, 0xc3, 0x1b, 
				  0xeb, 0x00, 0x9c, 0x5c, 0x2c, 0x49, 0xaa, 0x2e, 
				  0x4e, 0xad, 0xb2, 0x17, 0xad, 0x8c, 0xc0, 0x9b };
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

	public void FIPS186_a (string testName, SHA512 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input); 
		Assert.AreEqual (result, output, testName + ".a.1");
		Assert.AreEqual (result, hash.Hash, testName + ".a.2");
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_b (string testName, SHA512 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.ComputeHash (input, 0, input.Length); 
		Assert.AreEqual (result, output, testName + ".b.1");
		Assert.AreEqual (result, hash.Hash, testName + ".b.2");
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_c (string testName, SHA512 hash, byte[] input, byte[] result) 
	{
		MemoryStream ms = new MemoryStream (input);
		byte[] output = hash.ComputeHash (ms); 
		Assert.AreEqual (result, output, testName + ".c.1");
		Assert.AreEqual (result, hash.Hash, testName + ".c.2");
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_d (string testName, SHA512 hash, byte[] input, byte[] result) 
	{
		byte[] output = hash.TransformFinalBlock (input, 0, input.Length);
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		// AssertEquals( testName + ".d.1", result, output );
		Assert.IsNotNull (output, testName + ".d.1");
		Assert.AreEqual (result, hash.Hash, testName + ".d.2");
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	public void FIPS186_e (string testName, SHA512 hash, byte[] input, byte[] result) 
	{
		byte[] copy = new byte [input.Length];
		for (int i=0; i < input.Length - 1; i++)
			hash.TransformBlock (input, i, 1, copy, i);
		byte[] output = hash.TransformFinalBlock (input, input.Length - 1, 1);
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		// AssertEquals (testName + ".e.1", result, output);
		Assert.IsNotNull (output, testName + ".e.1");
		Assert.AreEqual (result, hash.Hash, testName + ".e.2");
		// required or next operation will still return old hash
		hash.Initialize ();
	}

	[Test]
	public override void Create () 
	{
		// Note: These tests will only be valid without a "machine.config" file
		// or a "machine.config" file that do not modify the default algorithm
		// configuration.
		const string defaultSHA512 = "System.Security.Cryptography.SHA512Managed";

		// try to build the default implementation
		SHA512 hash = SHA512.Create ();
		Assert.AreEqual (hash.ToString (), defaultSHA512, "SHA512.Create()");

		// try to build, in every way, a SHA512 implementation
		hash = SHA512.Create ("SHA512");
		Assert.AreEqual (hash.ToString (), defaultSHA512, "SHA512.Create('SHA512')");
		hash = SHA512.Create ("SHA-512");
		Assert.AreEqual (hash.ToString (), defaultSHA512, "SHA512.Create('SHA-512')");
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void CreateIncorrect () 
	{
		// try to build an incorrect hash algorithms
		hash = SHA512.Create ("MD5");
	}

	[Test]
	public void CreateInvalid () 
	{
		// try to build invalid implementation
		hash = SHA512.Create ("InvalidHash");
		Assert.IsNull (hash, "SHA512.Create('InvalidHash')");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public override void CreateNull () 
	{
		// try to build null implementation
		hash = SHA512.Create (null);
	}

	// none of those values changes for any implementation of defaultSHA512
	[Test]
	public virtual void StaticInfo () 
	{
		string className = hash.ToString ();
		Assert.AreEqual (512, hash.HashSize, className + ".HashSize");
		Assert.AreEqual (1, hash.InputBlockSize, className + ".InputBlockSize");
		Assert.AreEqual (1, hash.OutputBlockSize, className + ".OutputBlockSize");
	}
}

}
