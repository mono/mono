//
// KeyedHashAlgorithmTest.cs - NUnit Test Cases for KeyedHashAlgorithm
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
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
	public override void SetUp () 
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
		Assert.AreEqual (defaultKeyedHash, KeyedHashAlgorithm.Create ().ToString (), "KeyedHashAlgorithm.Create()");

		// try to build all hash algorithms
		hash = KeyedHashAlgorithm.Create ("HMACSHA1");
		Assert.AreEqual (defaultHMACSHA1, hash.ToString (), "KeyedHashAlgorithm.Create('HMACSHA1')");
		hash = KeyedHashAlgorithm.Create ("System.Security.Cryptography.HMACSHA1");
		Assert.AreEqual (defaultHMACSHA1, hash.ToString (), "KeyedHashAlgorithm.Create('System.Security.Cryptography.HMACSHA1')");
		hash = KeyedHashAlgorithm.Create ("System.Security.Cryptography.KeyedHashAlgorithm" );
		Assert.AreEqual (defaultKeyedHash, hash.ToString (), "KeyedHashAlgorithm.Create('System.Security.Cryptography.KeyedHashAlgorithm')");

		hash = KeyedHashAlgorithm.Create ("MACTripleDES");
		Assert.AreEqual (defaultMACTripleDES, hash.ToString (), "KeyedHashAlgorithm.Create('MACTripleDES')");
		hash = KeyedHashAlgorithm.Create ("System.Security.Cryptography.MACTripleDES");
		Assert.AreEqual (defaultMACTripleDES, hash.ToString (), "KeyedHashAlgorithm.Create('System.Security.Cryptography.MACTripleDES')");

		// try to build invalid implementation
		hash = KeyedHashAlgorithm.Create ("InvalidKeyedHash");
		Assert.IsNull (hash, "KeyedHashAlgorithm.Create('InvalidKeyedHash')");
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
		Assert.IsNotNull (kh.Key, "KeyedHashAlgorithm.Key not null (random key)");
		byte[] key = { 0x01, 0x02, 0x03 };
		byte[] keybackup = (byte[]) key.Clone ();
		kh.Key = key;
		// the KeyedHashAlgorithm use a copy of the key (not a reference to)
		key [0] = 0x00;
		Assert.AreEqual (kh.Key, keybackup, "KeyedHashAlgorithm key[0]-before");
		// you can't change individual bytes from a key
		kh.Key [0] = 0x00;
		Assert.AreEqual (kh.Key, keybackup, "KeyedHashAlgorithm.Key[0]-after");
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
