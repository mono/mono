//
// HashAlgorithmTest.cs - NUnit Test Cases for HashAlgorithm
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell  http://www.novell.com
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// HashAlgorithm is a abstract class - so most of it's functionality wont
// be tested here (but will be in its descendants).

[TestFixture]
public class HashAlgorithmTest : Assertion {
	protected HashAlgorithm hash;

	[SetUp]
	protected virtual void SetUp () 
	{
		hash = HashAlgorithm.Create ();
	}

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

	[Test]
	public virtual void Create () 
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
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public virtual void CreateNull () 
	{
		// try to build null implementation
		hash = HashAlgorithm.Create (null);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Clear () 
	{
		byte[] inputABC = Encoding.Default.GetBytes ("abc");
		hash.ComputeHash (inputABC);
		hash.Clear ();
		// cannot use a disposed object
		hash.ComputeHash (inputABC);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Clear2 () 
	{
		byte[] inputABC = Encoding.Default.GetBytes ("abc");
		MemoryStream ms = new MemoryStream (inputABC);
		hash.ComputeHash (ms);
		hash.Clear ();
		// cannot use a disposed object
		hash.ComputeHash (ms);
	}

	[Test]
	[ExpectedException (typeof (NullReferenceException))]
	public void NullStream () 
	{
		Stream s = null;
		byte[] result = hash.ComputeHash (s);
	}

	[Test]
	public void Disposable () 
	{
		using (HashAlgorithm hash = HashAlgorithm.Create ()) {
			byte[] data = hash.ComputeHash (new byte [0]);
		}
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void InitializeDisposed () 
	{
		hash.ComputeHash (new byte [0]);
		hash.Clear (); // disposed
		hash.Initialize ();
		hash.ComputeHash (new byte [0]);
	}
}

}
