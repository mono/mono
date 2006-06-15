//
// KeyedHashAlgorithmTest.cs - NUnit Test Cases for KeyedHashAlgorithm
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell  http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// KeyedHashAlgorithm is a abstract class - so most of it's functionality wont
// be tested here (but will be in its descendants).

[TestFixture]
public class KeyedHashAlgorithmTest : HashAlgorithmTest {

	[SetUp]
	protected override void SetUp () 
	{
		hash = KeyedHashAlgorithm.Create ();
		(hash as KeyedHashAlgorithm).Key = new byte [8];
	}

	// Note: These tests will only be valid without a "machine.config" file
	// or a "machine.config" file that do not modify the default algorithm
	// configuration.
	private const string defaultHMACSHA1 = "System.Security.Cryptography.HMACSHA1";
	private const string defaultMACTripleDES = "System.Security.Cryptography.MACTripleDES";
	private const string defaultKeyedHash = defaultHMACSHA1;

	[Test]
	public override void Create () 
	{
		// try the default keyed hash algorithm (created in SetUp)
		AssertEquals( "KeyedHashAlgorithm.Create()", defaultKeyedHash, KeyedHashAlgorithm.Create ().ToString());

		// try to build all hash algorithms
		hash = KeyedHashAlgorithm.Create ("HMACSHA1");
		AssertEquals ("KeyedHashAlgorithm.Create('HMACSHA1')", defaultHMACSHA1, hash.ToString ());
		hash = KeyedHashAlgorithm.Create ("System.Security.Cryptography.HMACSHA1");
		AssertEquals ("KeyedHashAlgorithm.Create('System.Security.Cryptography.HMACSHA1')", defaultHMACSHA1, hash.ToString ());
		hash = KeyedHashAlgorithm.Create ("System.Security.Cryptography.KeyedHashAlgorithm" );
		AssertEquals ("KeyedHashAlgorithm.Create('System.Security.Cryptography.KeyedHashAlgorithm')", defaultKeyedHash, hash.ToString ());

		hash = KeyedHashAlgorithm.Create ("MACTripleDES");
		AssertEquals ("KeyedHashAlgorithm.Create('MACTripleDES')", defaultMACTripleDES, hash.ToString ());
		hash = KeyedHashAlgorithm.Create ("System.Security.Cryptography.MACTripleDES");
		AssertEquals ("KeyedHashAlgorithm.Create('System.Security.Cryptography.MACTripleDES')", defaultMACTripleDES, hash.ToString ());

		// try to build invalid implementation
		hash = KeyedHashAlgorithm.Create ("InvalidKeyedHash");
		AssertNull ("KeyedHashAlgorithm.Create('InvalidKeyedHash')", hash);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public override void CreateNull () 
	{
		hash = KeyedHashAlgorithm.Create (null);
	}

	[Test]
	public void Key () 
	{
		KeyedHashAlgorithm kh = (KeyedHashAlgorithm) hash;
		AssertNotNull ("KeyedHashAlgorithm.Key not null (random key)", kh.Key);
		byte[] key = { 0x01, 0x02, 0x03 };
		byte[] keybackup = (byte[]) key.Clone ();
		kh.Key = key;
		// the KeyedHashAlgorithm use a copy of the key (not a reference to)
		key [0] = 0x00;
		AssertEquals ("KeyedHashAlgorithm key[0]", kh.Key, keybackup);
		// you can't change individual bytes from a key
		kh.Key [0] = 0x00;
		AssertEquals ("KeyedHashAlgorithm.Key[0]", kh.Key, keybackup);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ChangeKey () 
	{
		KeyedHashAlgorithm kh = (KeyedHashAlgorithm) hash;
		byte[] key = { 0x01, 0x02, 0x03 };
		byte[] keybackup = (byte[]) key.Clone ();
		kh.Key = key;

		// can't change a key after starting an operation
		kh.TransformBlock (key, 0, 3, keybackup, 0);
		kh.Key = keybackup;
	}
}

}
