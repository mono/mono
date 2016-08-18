//
// PasswordDeriveTest.cs - NUnit Test Cases for PasswordDerive
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004,2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

// References:
// a.	PKCS#5: Password-Based Cryptography Standard 
//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html

[TestFixture]
public class PasswordDeriveBytesTest {

	static byte[] salt = { 0xDE, 0xAD, 0xC0, 0xDE };
	static string ssalt = "DE-AD-C0-DE";

	// Constructors

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Ctor_PasswordNullSalt ()
	{
		string pwd = null;
		PasswordDeriveBytes pdb = new PasswordDeriveBytes (pwd, salt);
	}

	[Test]
	public void Ctor_PasswordSaltNull ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", null);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (100, pdb.IterationCount, "IterationCount");
		Assert.IsNull (pdb.Salt, "Salt");
	}

	[Test]
	public void Ctor_PasswordSalt ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (100, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Ctor_PasswordNullSaltCspParameters ()
	{
		string pwd = null;
		PasswordDeriveBytes pdb = new PasswordDeriveBytes (pwd, salt, new CspParameters ());
	}

#if !MOBILE
	[Test]
	[Category ("NotWorking")] // CspParameters aren't supported by Mono (requires CryptoAPI)
	public void Ctor_PasswordSaltNullCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", null, new CspParameters ());
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (100, pdb.IterationCount, "IterationCount");
		Assert.IsNull (pdb.Salt, "Salt");
	}
#endif

	[Test]
	public void Ctor_PasswordSaltCspParametersNull ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, null);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (100, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}

#if !MOBILE
	[Test]
	[Category ("NotWorking")] // CspParameters aren't supported by Mono (requires CryptoAPI)
	public void Ctor_PasswordSaltCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, new CspParameters ());
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (100, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}
#endif

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Ctor_PasswordNullSaltHashIteration ()
	{
		string pwd = null;
		PasswordDeriveBytes pdb = new PasswordDeriveBytes (pwd, salt, "SHA1", 1);
	}

	[Test]
	public void Ctor_PasswordSaltNullHashIteration ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", null, "SHA1", 1);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (1, pdb.IterationCount, "IterationCount");
		Assert.IsNull (pdb.Salt, "Salt");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Ctor_PasswordSaltHashNullIteration ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, null, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Ctor_PasswordSaltHashIterationNegative ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Ctor_PasswordSaltHashIterationZero ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", 0);
	}

	[Test]
	public void Ctor_PasswordSaltHashIterationMaxValue ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", Int32.MaxValue);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (Int32.MaxValue, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}

	[Test]
	public void Ctor_PasswordSaltHashIteration ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", 1);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (1, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Ctor_PasswordNullSaltHashIterationCspParameters ()
	{
		string pwd = null;
		PasswordDeriveBytes pdb = new PasswordDeriveBytes (pwd, salt, "SHA1", 1, new CspParameters ());
	}

#if !MOBILE
	[Test]
	[Category ("NotWorking")] // CspParameters aren't supported by Mono (requires CryptoAPI)
	public void Ctor_PasswordSaltNullHashIterationCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", null, "SHA1", 1, new CspParameters ());
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (1, pdb.IterationCount, "IterationCount");
		Assert.IsNull (pdb.Salt, "Salt");
	}
#endif

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Ctor_PasswordSaltHashNullIterationCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, null, 1, new CspParameters ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Ctor_PasswordSaltHashIterationNegativeCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", -1, new CspParameters ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Ctor_PasswordSaltHashIterationZeroCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", 0, new CspParameters ());
	}
		
#if !MOBILE
	[Test]
	[Category ("NotWorking")] // CspParameters aren't supported by Mono (requires CryptoAPI)
	public void Ctor_PasswordSaltHashIterationMaxValueCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", Int32.MaxValue, new CspParameters ());
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (Int32.MaxValue, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}
#endif

	[Test]
	public void Ctor_PasswordSaltHashIterationCspParametersNull ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", 1, null);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (1, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}

#if !MOBILE
	[Test]
	[Category ("NotWorking")] // CspParameters aren't supported by Mono (requires CryptoAPI)
	public void Ctor_PasswordSaltHashIterationCspParameters ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", 1, new CspParameters ());
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		Assert.AreEqual (1, pdb.IterationCount, "IterationCount");
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}
#endif
		
	// Properties

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Property_HashName_Null ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt, "SHA1", 1);
		Assert.AreEqual ("SHA1", pdb.HashName, "HashName");
		pdb.HashName = null;
	}

	[Test]
	public void Property_Salt ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt);
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
		pdb.Salt = null;
		Assert.IsNull (pdb.Salt, "Salt");
	}

	[Test]
	public void Property_Salt_Modify ()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes ("s3kr3t", salt);
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
		pdb.Salt [0] = 0xFF;
		// modification rejected (the property returned a copy of the salt)
		Assert.AreEqual (ssalt, BitConverter.ToString (pdb.Salt), "Salt");
	}

	// 1.0/1.1 compatibility


	// Old tests

	static int ToInt32LE(byte [] bytes, int offset)
	{
		return (bytes[offset + 3] << 24) | (bytes[offset + 2] << 16) | (bytes[offset + 1] << 8) | bytes[offset];
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
			Assert.IsTrue (compare, msg + " #" + j);
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
		Assert.AreEqual (pass, finalKey, msg);
	}

	public void Run (string password, byte[] salt, string hashname, int iterations, int getbytes, int lastFourBytes) 
	{
		PasswordDeriveBytes pd = new PasswordDeriveBytes (password, salt, hashname, iterations);
		byte[] key = pd.GetBytes (getbytes);
		string msg = "[pwd=" + password;
		msg += ", salt=" + ((salt == null) ? "null" : salt.Length.ToString ());
		msg += ", hash=" + hashname;
		msg += ", iter=" + iterations;
		msg += ", get=" + getbytes + "]";
		Assert.AreEqual (lastFourBytes, ToInt32LE (key, key.Length - 4), msg);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void TooShort () 
	{
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "SHA1", 1);
		byte[] key = pd.GetBytes (0);
	}

	public void TooLong (string hashName, int size, int lastFourBytes) 
	{
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("toolong", null, hashName, 1);
		
		// this should work (we check the last four devired bytes to be sure)
		byte[] key = pd.GetBytes (size);
		Assert.AreEqual (lastFourBytes, ToInt32LE (key, size - 4), "Last 4 bytes");

		// but we can't get another byte from it!
		try {
			key = pd.GetBytes (1);
			Assert.Fail ("Expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// LAMESPEC: no limit is documented
		}
		catch (Exception e) {
			Assert.Fail ("Expected CryptographicException but got " + e.ToString ());
		}
	}

	[Test]
	public void TooLong () 
	{
		// 1000 times hash length is the maximum
		TooLong ("MD5",    16000, 1135777886);
		TooLong ("SHA1",   20000, -1167918035);
		TooLong ("SHA256", 32000, -358766048);
		TooLong ("SHA384", 48000, 1426370534);
		TooLong ("SHA512", 64000, -1763233543);
	}

	[Test]
	public void OneIteration () 
	{
		// (1) size of hash, (2) size of 2 hash
		Run ("password", salt, "MD5",    1, 16, 986357363);
		Run ("monomono", null, "MD5",    1, 32, -1092059875);
		Run ("password", salt, "SHA1",   1, 20, -1251929751);
		Run ("monomono", null, "SHA1",   1, 40, -1148594972);
		Run ("password", salt, "SHA256", 1, 32, -1106908309);
		Run ("monomono", null, "SHA256", 1, 64, 1243724695);
		Run ("password", salt, "SHA384", 1, 48, 1338639872);
		Run ("monomono", null, "SHA384", 1, 96, -1974067932);
		Run ("password", salt, "SHA512", 1, 64, 998927776);
		Run ("monomono", null, "SHA512", 1, 128, -1082987985);
	}

	[Test]
	public void Salt () 
	{
		Run ("password", salt, "MD5",  10, 10, -1174247292);
		Run ("monomono", salt, "SHA1", 20, 20, 622814236);
		Run ("password", salt, "MD5",  30, 30, 1491759020);
		Run ("monomono", salt, "SHA1", 40, 40, 1186751819);
		Run ("password", salt, "MD5",  50, 50, -1416348895);
		Run ("monomono", salt, "SHA1", 60, 60, -1167799882);
		Run ("password", salt, "MD5",  70, 70, -695745351);
		Run ("monomono", salt, "SHA1", 80, 80, 598766793);
		Run ("password", salt, "MD5",  90, 90, -906351079);
		Run ("monomono", salt, "SHA1", 100, 100, 1247157997);
	}

	[Test]
	public void NoSalt () 
	{
		Run ("password", null, "MD5",  10, 10, -385488886);
		Run ("password", null, "SHA1", 20, 20, -385953596);
		Run ("password", null, "MD5",  30, 30, -669295228);
		Run ("password", null, "SHA1", 40, 40, -1921654064);
		Run ("password", null, "MD5",  50, 50, -1664099354);
		Run ("monomono", null, "SHA1", 60, 60, -1988511363);
		Run ("monomono", null, "MD5",  70, 70, -1326415479);
		Run ("monomono", null, "SHA1", 80, 80, 158880373);
		Run ("monomono", null, "MD5",  90, 90,  532527918);
		Run ("monomono", null, "SHA1", 100, 100, 769250758);
	}

	[Test]
	public void MD5 () 
	{
		const string hashName = "MD5";
		// getbytes less than hash size
		Run ("password", null, hashName, 10, 10, -385488886);
		// getbytes equal to hash size
		Run ("password", salt, hashName, 20, 16, -470982134);
		// getbytes more than hash size
		Run ("password", null, hashName, 30, 30, -669295228);
		Run ("password", salt, hashName, 40, 40, 892279589);
		Run ("password", null, hashName, 50, 50, -1664099354);
		Run ("monomono", salt, hashName, 60, 60, -2050574033);
		Run ("monomono", null, hashName, 70, 70, -1326415479);
		Run ("monomono", salt, hashName, 80, 80, 2047895994);
		Run ("monomono", null, hashName, 90, 90, 532527918);
		Run ("monomono", salt, hashName, 100, 100, 1522243696);
	}

	[Test]
	public void SHA1 () 
	{
		const string hashName = "SHA1";
		// getbytes less than hash size
		Run ("password", null, hashName, 10, 10, -852142057);
		// getbytes equal to hash size
		Run ("password", salt, hashName, 20, 20, -1096621819);
		// getbytes more than hash size
		Run ("password", null, hashName, 30, 30, 1748347042);
		Run ("password", salt, hashName, 40, 40, 900690664);
		Run ("password", null, hashName, 50, 50, 2125027038);
		Run ("monomono", salt, hashName, 60, 60, -1167799882);
		Run ("monomono", null, hashName, 70, 70, -1967623713);
		Run ("monomono", salt, hashName, 80, 80, 598766793);
		Run ("monomono", null, hashName, 90, 90, -1754629926);
		Run ("monomono", salt, hashName, 100, 100, 1247157997);
	}

	[Test]
	public void SHA256 () 
	{
		const string hashName = "SHA256";
		// getbytes less than hash size
		Run ("password", null, hashName, 10, 10, -1636557322);
		Run ("password", salt, hashName, 20, 20, -1403130075);
		// getbytes equal to hash size
		Run ("password", null, hashName, 30, 32, -1013167039);
		// getbytes more than hash size
		Run ("password", salt, hashName, 40, 40, 379553148);
		Run ("password", null, hashName, 50, 50, 1031928292);
		Run ("monomono", salt, hashName, 60, 60, 1933836953);
		Run ("monomono", null, hashName, 70, 70, -956782587);
		Run ("monomono", salt, hashName, 80, 80, 1239391711);
		Run ("monomono", null, hashName, 90, 90, -872090432);
		Run ("monomono", salt, hashName, 100, 100, -591569127);
	}

	[Test]
	public void SHA384 () 
	{
		const string hashName = "SHA384";
		// getbytes less than hash size
		Run ("password", null, hashName, 10, 10, 323393534);
		Run ("password", salt, hashName, 20, 20, -2034683704);
		Run ("password", null, hashName, 30, 32, 167978389);
		Run ("password", salt, hashName, 40, 40, 2123410525);
		// getbytes equal to hash size
		Run ("password", null, hashName, 50, 48, -47538843);
		// getbytes more than hash size
		Run ("monomono", salt, hashName, 60, 60, -118610774);
		Run ("monomono", null, hashName, 70, 70, 772360425);
		Run ("monomono", salt, hashName, 80, 80, -1018881215);
		Run ("monomono", null, hashName, 90, 90, -1585583772);
		Run ("monomono", salt, hashName, 100, 100, -821501990);
	}

	[Test]
	public void SHA512 () 
	{
		const string hashName = "SHA512";
		// getbytes less than hash size
		Run ("password", null, hashName, 10, 10, 708870265);
		Run ("password", salt, hashName, 20, 20, 23889227);
		Run ("password", null, hashName, 30, 32, 1718904507);
		Run ("password", salt, hashName, 40, 40, 979228711);
		Run ("password", null, hashName, 50, 48, 1554003653);
		// getbytes equal to hash size
		Run ("monomono", salt, hashName, 60, 64, 1251099126);
		// getbytes more than hash size
		Run ("monomono", null, hashName, 70, 70, 1021441810);
		Run ("monomono", salt, hashName, 80, 80, 640059310);
		Run ("monomono", null, hashName, 90, 90, 1178147201);
		Run ("monomono", salt, hashName, 100, 100, 206423887);
	}

	// get one block after the other
	[Test]
	public void OneByOne () 
	{
		byte[] key = { 0x91, 0xDA, 0xF9, 0x9D, 0x7C, 0xA9, 0xB4, 0x42, 0xB8, 0xD9, 0x45, 0xAB, 0x69, 0xEE, 0x12, 0xBC, 0x48, 0xDD, 0x38, 0x74 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "SHA1", 1);
		string msg = "PKCS#5-Long password salt SHA1 (1)";

		int bloc = key.Length;
		int iter = (int) ((1000 + bloc - 1) / bloc);
		byte[] pass = null;
		for (int i=0; i < iter; i++) {
			pass = pd.GetBytes (bloc);
		}
		Assert.AreEqual (pass, key, msg);
	}

	[Test]
	public void SHA1SaltShortRun ()
	{
		byte[] key = { 0x0B, 0x61, 0x93, 0x96, 0x3A, 0xFF, 0x0D, 0xFC, 0xF6, 0x3D, 0xA3, 0xDB, 0x34, 0xC2, 0x99, 0x71, 0x69, 0x11, 0x61, 0xB5 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "SHA1", 1);
		string msg = "PKCS#5 password salt SHA1 (1)";
		ShortRun (msg, pd, key);
	}

	[Test]
	public void SHA1SaltLongRun () 
	{
		byte[] key = { 0x91, 0xDA, 0xF9, 0x9D, 0x7C, 0xA9, 0xB4, 0x42, 0xB8, 0xD9, 0x45, 0xAB, 0x69, 0xEE, 0x12, 0xBC, 0x48, 0xDD, 0x38, 0x74 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "SHA1", 1);
		string msg = "PKCS#5-Long password salt SHA1 (1)";
		LongRun (msg, pd, key);
	}

	[Test]
	public void SHA1NoSaltShortRun ()
	{
		byte[] key = { 0x74, 0x61, 0x03, 0x6C, 0xA1, 0xFE, 0x85, 0x3E, 0xD9, 0x3F, 0x03, 0x06, 0x58, 0x45, 0xDE, 0x36, 0x52, 0xEF, 0x4B, 0x68 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", null, "SHA1", 10);
		string msg = "PKCS#5 mono null SHA1 (10)";
		ShortRun (msg, pd, key);
	}

	[Test]
	public void SHA1NoSaltLongRun () 
	{
		byte[] key = { 0x3A, 0xF8, 0x33, 0x88, 0x39, 0x61, 0x29, 0x75, 0x5C, 0x17, 0xD2, 0x9E, 0x8A, 0x78, 0xEB, 0xBD, 0x89, 0x1E, 0x4C, 0x67 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", null, "SHA1", 10);
		string msg = "PKCS#5-Long mono null SHA1 (10)";
		LongRun (msg, pd, key);
	}

	[Test]
	public void MD5SaltShortRun ()
	{
		byte[] key = { 0xA5, 0x4D, 0x4E, 0xDD, 0x3A, 0x59, 0xAC, 0x98, 0x08, 0xDA, 0xE7, 0xF2, 0x85, 0x2F, 0x7F, 0xF2 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", salt, "MD5", 100);
		string msg = "PKCS#5 mono salt MD5 (100)";
		ShortRun (msg, pd, key);
	}

	[Test]
	public void MD5SaltLongRun () 
	{
		byte[] key = { 0x92, 0x51, 0x4D, 0x10, 0xE1, 0x5F, 0xA8, 0x44, 0xEF, 0xFC, 0x0F, 0x1F, 0x6F, 0x3E, 0x40, 0x36 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", salt, "MD5", 100);
		string msg = "PKCS#5-Long mono salt MD5 (100)";
		LongRun (msg, pd, key);
	}

	[Test]
	public void MD5NoSaltShortRun ()
	{
		byte[] key = { 0x39, 0xEB, 0x82, 0x84, 0xCF, 0x1A, 0x3B, 0x3C, 0xA1, 0xF2, 0x68, 0xAF, 0xBF, 0xAC, 0x41, 0xA6 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		string msg = "PKCS#5 password null MD5 (1000)";
		ShortRun (msg, pd, key);
	}

	[Test]
	public void MD5NoSaltLongRun () 
	{
		byte[] key = { 0x49, 0x3C, 0x00, 0x69, 0xB4, 0x55, 0x21, 0xA4, 0xC9, 0x69, 0x2E, 0xFF, 0xAA, 0xED, 0x4C, 0x72 };
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		string msg = "PKCS#5-Long password null MD5 (1000)";
		LongRun (msg, pd, key);
	}

	[Test]
	public void Properties () 
	{
		// create object...
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		Assert.AreEqual ("MD5", pd.HashName, "HashName-MD5");
		Assert.AreEqual (1000, pd.IterationCount, "IterationCount-1000");
		// ...then change all its properties...
		pd.HashName = "SHA1";
		Assert.AreEqual ("SHA1", pd.HashName, "HashName-SHA1");
		pd.Salt = salt;
		Assert.AreEqual (ssalt, BitConverter.ToString (pd.Salt), "Salt");
		pd.IterationCount = 1;
		Assert.AreEqual (1, pd.IterationCount, "IterationCount-1");
		byte[] expectedKey = { 0x0b, 0x61, 0x93, 0x96 };
		// ... before using it
		Assert.AreEqual (expectedKey, pd.GetBytes (4), "PKCS#5 test properties");
		// it should work but if we try to set any properties after GetBytes
		// they should all throw an exception
		try {
			pd.HashName = "SHA256";
			Assert.Fail ("PKCS#5 can't set HashName after GetBytes - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("PKCS#5 can't set HashName after GetBytes - expected CryptographicException but got " + e.ToString ());
		}
		try {
			pd.Salt = expectedKey;
			Assert.Fail ("PKCS#5 can't set Salt after GetBytes - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("PKCS#5 can't set Salt after GetBytes - expected CryptographicException but got " + e.ToString ());
		}
		try {
			pd.IterationCount = 10;
			Assert.Fail ("PKCS#5 can't set IterationCount after GetBytes - expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("PKCS#5 can't set IterationCount after GetBytes - expected CryptographicException but got " + e.ToString ());
		}

		pd.Reset ();
		// finally a useful reset :)
		pd.HashName = "SHA256";
		pd.Salt = expectedKey;
		pd.IterationCount = 10;
	}

	// FIXME: should we treat this as a bug or as a feature ?
	[Test]
	public void StrangeBehaviour ()
	{
		// create object with a salt...
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", salt, "MD5", 1000);
		// ...then change the salt to null
		pd.Salt = null;
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void CryptDeriveKey_TooLongKey () 
	{
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("password", null, "MD5", 1000);
		pd.CryptDeriveKey ("AlgName", "MD5", -256, new byte [8]);
	}
		
#if !MOBILE
	[Test]
	[Category ("NotWorking")] // bug #79499
	public void LongMultipleGetBytes ()
	{
		// based on http://bugzilla.ximian.com/show_bug.cgi?id=79499
		PasswordDeriveBytes pd = new PasswordDeriveBytes ("mono", new byte[20]);
		string key = BitConverter.ToString (pd.GetBytes (32));
		Assert.AreEqual ("88-0A-AE-0A-41-61-02-78-FD-E2-70-9F-25-13-14-28-1F-C7-D9-72-9A-AE-CA-3F-BD-31-B4-F0-BD-8E-5B-98", key, "key");
		string iv = BitConverter.ToString (pd.GetBytes (16));
		Assert.AreEqual ("FD-E2-70-9F-25-13-14-28-4D-3F-9B-F8-EE-AA-95-ED", iv, "iv");
		pd.Reset ();
		// bytes from 32-40 are different from calling GetBytes separately
		Assert.AreEqual (key + "-F6-55-6C-3E-54-8B-F3-73-4D-3F-9B-F8-EE-AA-95-ED", BitConverter.ToString (pd.GetBytes (48)), "same");
	}
#endif
}

}
