//
// KeyedHashAlgorithmTest.cs - NUnit Test Cases for KeyedHashAlgorithm
//
// Author:
//		Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// KeyedHashAlgorithm is a abstract class - so most of it's functionality wont
// be tested here (but will be in its descendants).
public class KeyedHashAlgorithmTest : HashAlgorithmTest {
	protected override void SetUp () 
	{
		hash = KeyedHashAlgorithm.Create ();
	}

	protected override void TearDown () {}

	// Note: These tests will only be valid without a "machine.config" file
	// or a "machine.config" file that do not modify the default algorithm
	// configuration.
	private const string defaultHMACSHA1 = "System.Security.Cryptography.HMACSHA1";
	private const string defaultMACTripleDES = "System.Security.Cryptography.MACTripleDES";
	private const string defaultKeyedHash = defaultHMACSHA1;

	public override void TestCreate () 
	{
		// try the default keyed hash algorithm (created in SetUp)
		AssertEquals( "KeyedHashAlgorithm.Create()", defaultKeyedHash, hash.ToString());

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

		// try to build null implementation
		try {
			hash = KeyedHashAlgorithm.Create (null);
			Fail ("KeyedHashAlgorithm.Create(null) should throw ArgumentNullException");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("KeyedHashAlgorithm.Create(null) should throw ArgumentNullException not " + e.ToString ());
		}
	}

	public void TestKey () 
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
		// can't change a key after starting an operation
		kh.TransformBlock (key, 0, 3, keybackup, 0);
		try {
			kh.Key = keybackup;
			Fail ("KeyedHashAlgorithm.Key should throw CryptographicException but didn't");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("KeyedHashAlgorithm.Key should throw CryptographicException not " + e.ToString ());
		}
	}

}

}
