//
// HashAlgorithmTest.cs - NUnit Test Cases for HashAlgorithm
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography
{

// HashAlgorithm is a abstract class - so most of it's functionality wont
// be tested here (but will be in its descendants).
public class HashAlgorithmTest : TestCase {
	protected HashAlgorithm hash;

	protected override void SetUp () 
	{
		hash = HashAlgorithm.Create ();
	}

	protected override void TearDown () {}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// Note: These tests will only be valid without a "machine.config" file
	// or a "machine.config" file that do not modify the default algorithm
	// configuration.
	private const string defaultSHA1 = "System.Security.Cryptography.SHA1CryptoServiceProvider";
	private const string defaultMD5 = "System.Security.Cryptography.MD5CryptoServiceProvider";
	private const string defaultSHA256 = "System.Security.Cryptography.SHA256Managed";
	private const string defaultSHA384 = "System.Security.Cryptography.SHA384Managed";
	private const string defaultSHA512 = "System.Security.Cryptography.SHA512Managed";
	private const string defaultHash = defaultSHA1;

	public virtual void TestCreate () 
	{
		// try the default hash algorithm (created in SetUp)
		AssertEquals( "HashAlgorithm.Create()", defaultHash, hash.ToString());

		// try to build all hash algorithms
		hash = HashAlgorithm.Create ("SHA");
		AssertEquals ("HashAlgorithm.Create('SHA')", defaultSHA1, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA1");
		AssertEquals ("HashAlgorithm.Create('SHA1')", defaultSHA1, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA1");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA1')", defaultSHA1, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.HashAlgorithm" );
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.HashAlgorithm')", defaultHash, hash.ToString ());

		hash = HashAlgorithm.Create ("MD5");
		AssertEquals ("HashAlgorithm.Create('MD5')", defaultMD5, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.MD5");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.MD5')", defaultMD5, hash.ToString ());

		hash = HashAlgorithm.Create ("SHA256");
		AssertEquals ("HashAlgorithm.Create('SHA256')", defaultSHA256, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA-256");
		AssertEquals ("HashAlgorithm.Create('SHA-256')", defaultSHA256, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA256");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA256')", defaultSHA256, hash.ToString ());
	
		hash = HashAlgorithm.Create ("SHA384");
		AssertEquals ("HashAlgorithm.Create('SHA384')", defaultSHA384, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA-384");
		AssertEquals ("HashAlgorithm.Create('SHA-384')", defaultSHA384, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA384");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA384')", defaultSHA384, hash.ToString ());
	
		hash = HashAlgorithm.Create ("SHA512");
		AssertEquals ("HashAlgorithm.Create('SHA512')", defaultSHA512, hash.ToString ());
		hash = HashAlgorithm.Create ("SHA-512");
		AssertEquals ("HashAlgorithm.Create('SHA-512')", defaultSHA512, hash.ToString ());
		hash = HashAlgorithm.Create ("System.Security.Cryptography.SHA512");
		AssertEquals ("HashAlgorithm.Create('System.Security.Cryptography.SHA512')", defaultSHA512, hash.ToString ());
	
		// try to build invalid implementation
		hash = HashAlgorithm.Create ("InvalidHash");
		AssertNull ("HashAlgorithm.Create('InvalidHash')", hash);

		// try to build null implementation
		try {
			hash = HashAlgorithm.Create (null);
			Fail ("HashAlgorithm.Create(null) should throw ArgumentNullException");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("HashAlgorithm.Create(null) should throw ArgumentNullException not " + e.ToString() );
		}
	}

	public void TestClear () 
	{
		byte[] inputABC = Encoding.Default.GetBytes ("abc");
		hash.ComputeHash (inputABC);
		hash.Clear ();
		// cannot use a disposed object
		try {
			hash.ComputeHash (inputABC);
			Fail ("ComputeHash after clear should throw ObjectDisposedException but didn't");
		}
		catch (ObjectDisposedException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("ComputeHash after clear should throw ObjectDisposedException not " + e.ToString ());
		}
	}

	public void TestNullStream () 
	{
		Stream s = null;
		try {
			byte[] result = hash.ComputeHash (s);
			Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected NullReferenceException but got " + e.ToString ());
		}
	}

}

}
