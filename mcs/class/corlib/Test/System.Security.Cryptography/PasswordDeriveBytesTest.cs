//
// PasswordDeriveTest.cs - NUnit Test Cases for PasswordDerive
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography
{

// References:
// a.	PKCS#5: Password-Based Cryptography Standard 
//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html

public class PasswordDeriveBytesTest : TestCase 
{
	public PasswordDeriveBytesTest () : base ("System.Security.Cryptography.PasswordDeriveBytes testsuite") {}
	public PasswordDeriveBytesTest (string name) : base (name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (PasswordDeriveBytesTest)); 
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2)
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// generate the key up to HashSize and reset between operations
	public void ShortRun(string msg, PasswordDeriveBytes pd, byte[] finalKey)
	{
		for (int i=0; i < finalKey.Length; i++) {
			int j = 0;
			bool compare = true;
			byte[] key = pd.GetBytes (i+1);
			for (; j < i; j++) {
				if (finalKey [j] != key[j]) {
					compare = false;
					break;
				}
			}
			Assert (msg + " #" + j, compare);
			pd.Reset ();
		}
	}

	// generate a key at least 1000 bytes and don't reset between operations
	public void LongRun(string msg, PasswordDeriveBytes pd, byte[] finalKey)
	{
		int bloc = finalKey.Length;
		int iter = (int) ((1000 + bloc - 1) / bloc);
		byte[] pass = null;
		for (int i=0; i < iter; i++) {
			pass = pd.GetBytes (bloc);
		}
		AssertEquals (msg, pass, finalKey);
	}

	public void TestSHA1SaltShortRun ()
	{
		byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
		byte[] key = { 0x0B, 0x61, 0x93, 0x96, 0x3A, 0xFF, 0x0D, 0xFC, 0xF6, 0x3D, 0xA3, 0xDB, 0x34, 0xC2, 0x99, 0x71, 0x69, 0x11, 0x61, 0xB5 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "SHA1", 1);
		string msg = "PKCS#5 password salt SHA1 (1)";
		ShortRun (msg, pd, key);
	}

	public void TestSHA1SaltLongRun () 
	{
		byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
		byte[] key = { 0x91, 0xDA, 0xF9, 0x9D, 0x7C, 0xA9, 0xB4, 0x42, 0xB8, 0xD9, 0x45, 0xAB, 0x69, 0xEE, 0x12, 0xBC, 0x48, 0xDD, 0x38, 0x74 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "SHA1", 1);
		string msg = "PKCS#5-Long password salt SHA1 (1)";
		LongRun (msg, pd, key);
	}

	public void TestSHA1NoSaltShortRun ()
	{
		byte[] key = { 0x74, 0x61, 0x03, 0x6C, 0xA1, 0xFE, 0x85, 0x3E, 0xD9, 0x3F, 0x03, 0x06, 0x58, 0x45, 0xDE, 0x36, 0x52, 0xEF, 0x4B, 0x68 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", null, "SHA1", 10);
		string msg = "PKCS#5 mono null SHA1 (10)";
		ShortRun (msg, pd, key);
	}

	public void TestSHA1NoSaltLongRun () 
	{
		byte[] key = { 0x3A, 0xF8, 0x33, 0x88, 0x39, 0x61, 0x29, 0x75, 0x5C, 0x17, 0xD2, 0x9E, 0x8A, 0x78, 0xEB, 0xBD, 0x89, 0x1E, 0x4C, 0x67 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", null, "SHA1", 10);
		string msg = "PKCS#5-Long mono null SHA1 (10)";
		LongRun (msg, pd, key);
	}

	public void TestMD5SaltShortRun ()
	{
		byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
		byte[] key = { 0xA5, 0x4D, 0x4E, 0xDD, 0x3A, 0x59, 0xAC, 0x98, 0x08, 0xDA, 0xE7, 0xF2, 0x85, 0x2F, 0x7F, 0xF2 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", salt, "MD5", 100);
		string msg = "PKCS#5 mono salt MD5 (100)";
		ShortRun (msg, pd, key);
	}

	public void TestMD5SaltLongRun () 
	{
		byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
		byte[] key = { 0x92, 0x51, 0x4D, 0x10, 0xE1, 0x5F, 0xA8, 0x44, 0xEF, 0xFC, 0x0F, 0x1F, 0x6F, 0x3E, 0x40, 0x36 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", salt, "MD5", 100);
		string msg = "PKCS#5-Long mono salt MD5 (100)";
		LongRun (msg, pd, key);
	}

	public void TestMD5NoSaltShortRun ()
	{
		byte[] key = { 0x39, 0xEB, 0x82, 0x84, 0xCF, 0x1A, 0x3B, 0x3C, 0xA1, 0xF2, 0x68, 0xAF, 0xBF, 0xAC, 0x41, 0xA6 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		string msg = "PKCS#5 password null MD5 (1000)";
		ShortRun (msg, pd, key);
	}

	public void TestMD5NoSaltLongRun () 
	{
		byte[] key = { 0x49, 0x3C, 0x00, 0x69, 0xB4, 0x55, 0x21, 0xA4, 0xC9, 0x69, 0x2E, 0xFF, 0xAA, 0xED, 0x4C, 0x72 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		string msg = "PKCS#5-Long password null MD5 (1000)";
		LongRun (msg, pd, key);
	}

	public void TestProperties () 
	{
		byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
		// create object...
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		// ...then change all its properties...
		pd.HashName = "SHA1";
		pd.Salt = salt;
		pd.IterationCount = 1;
		byte[] expectedKey = { 0x0b, 0x61, 0x93, 0x96 };
		// ... before using it
		AssertEquals ("PKCS#5 test properties", expectedKey, pd.GetBytes (4));
		// it should work but if we try to set any properties after GetBytes
		// they should all throw an exception
		try {
			pd.HashName = "SHA256";
			Fail ("PKCS#5 can't set HashName after GetBytes - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set HashName after GetBytes - expected CryptographicException but got " + e.ToString ());
		}
		try {
			pd.Salt = expectedKey;
			Fail ("PKCS#5 can't set Salt after GetBytes - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set Salt after GetBytes - expected CryptographicException but got " + e.ToString ());
		}
		try {
			pd.IterationCount = 10;
			Fail ("PKCS#5 can't set IterationCount after GetBytes - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set IterationCount after GetBytes - expected CryptographicException but got " + e.ToString ());
		}
		// same thing after Reset
		pd.Reset ();
		try {
			pd.HashName = "SHA256";
			Fail ("PKCS#5 can't set HashName after Reset - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set HashName after Reset - expected CryptographicException but got " + e.ToString ());
		}
		try {
			pd.Salt = expectedKey;
			Fail ("PKCS#5 can't set Salt after Reset - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set Salt after Reset - expected CryptographicException but got " + e.ToString ());
		}
		try {
			pd.IterationCount = 10;
			Fail ("PKCS#5 can't set IterationCount after Reset - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set IterationCount after Reset - expected CryptographicException but got " + e.ToString ());
		}
	}

	// FIXME: should we treat this as a bug or as a feature ?
	public void TestStrangeBehaviour ()
	{
		byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
		// create object with a salt...
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "MD5", 1000);
		// ...then change the salt to null
		try {
			pd.Salt = null;
			Fail ("PKCS#5 can't set Salt to null - expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// do nothing, this is what we (almost) expect
		}
		catch (Exception e) {
			Fail ("PKCS#5 can't set Salt to null - expected NullReferenceException but got " + e.ToString ());
		}
	}
}

}
