//
// RSAPKCS1SignatureFormatterTest.cs - NUnit tests for PKCS#1 v.1.5 signature.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

public class RSAPKCS1SignatureFormatterTest : TestCase {

	public RSAPKCS1SignatureFormatterTest () : base ("System.Security.Cryptography.RSAPKCS1SignatureFormatter testsuite") {}
	public RSAPKCS1SignatureFormatterTest (string name) : base(name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get {
			return new TestSuite (typeof (RSAPKCS1SignatureFormatterTest));
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	public void TestConstructors () 
	{
		RSAPKCS1SignatureFormatter fmt;
		fmt = new RSAPKCS1SignatureFormatter ();
		AssertNotNull ("RSAPKCS1SignatureFormatter()", fmt);

		fmt = new RSAPKCS1SignatureFormatter (null);
		AssertNotNull ("RSAPKCS1SignatureFormatter(null)", fmt);

		RSA rsa = RSA.Create ();
		fmt = new RSAPKCS1SignatureFormatter (rsa);
		AssertNotNull ("RSAPKCS1SignatureFormatter(rsa)", fmt);

		DSA dsa = DSA.Create ();
		try {
			fmt = new RSAPKCS1SignatureFormatter (dsa);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}
	}

	public void TestSetKey () 
	{
		RSAPKCS1SignatureFormatter fmt;
		fmt = new RSAPKCS1SignatureFormatter ();
		try {
			fmt.SetKey (RSA.Create ());
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}

		try {
			fmt.SetKey (DSA.Create ());
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}

		try {
			fmt.SetKey (null);
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
	}

	public void TestSetHashAlgorithm () 
	{
		RSAPKCS1SignatureFormatter fmt;
		fmt = new RSAPKCS1SignatureFormatter ();

		try {
			fmt.SetHashAlgorithm ("SHA1");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}

		try {
			fmt.SetHashAlgorithm ("MD5");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}

		try {
			fmt.SetHashAlgorithm ("SHA256");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}

		try {
			fmt.SetHashAlgorithm ("SHA384");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}

		try {
			fmt.SetHashAlgorithm ("SHA512");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
	}

	// see: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpcongeneratingsignatures.asp
	public void TestCreateSignature () 
	{
		RSAPKCS1SignatureFormatter fmt = new RSAPKCS1SignatureFormatter ();

		// no hash algorithm
		byte[] hash = new byte [1];
		byte[] signature = null;
		try {
			signature = fmt.CreateSignature (hash);
			Fail ("CreateSignature(?) no hash algo - Expected CryptographicUnexpectedOperationException but none");
		}
		catch (CryptographicUnexpectedOperationException) {
			// this was expected
		}
		catch (Exception e) {
			Fail ("CreateSignature(?) no hash algo - Expected CryptographicUnexpectedOperationException but got: " + e.ToString ());
		}

		// no key
		fmt.SetHashAlgorithm ("SHA1");
		hash = new byte [20];
		try {
			signature = fmt.CreateSignature (hash);
			Fail ("CreateSignature(?) no key - Expected CryptographicUnexpectedOperationException but none");
		}
		catch (CryptographicUnexpectedOperationException) {
			// this was expected
		}
		catch (Exception e) {
			Fail ("CreateSignature(?) no key - Expected CryptographicUnexpectedOperationException but got: " + e.ToString ());
		}

		// we need the private key 
		RSA rsa = RSA.Create ();
		rsa.ImportParameters (AllTests.GetRsaKey (true));
		fmt.SetKey (rsa);

		// good SHA1
		fmt.SetHashAlgorithm ("SHA1");
		hash = new byte [20];
		signature = fmt.CreateSignature (hash);
		AssertNotNull ("CreateSignature(SHA1)", signature);

		// wrong length SHA1
		fmt.SetHashAlgorithm ("SHA1");
		hash = new byte [19];
		try {
			signature = fmt.CreateSignature (hash);
			Fail ("CreateSignature(badSHA1) - Should have thrown an CryptographicException");
		}
		catch (CryptographicException) {
			// this is expected (invalid hash length)
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA1) - Unexpected exception: " + e.ToString ());
		}

		// good MD5
		fmt.SetHashAlgorithm ("MD5");
		hash = new byte [16];
		signature = fmt.CreateSignature (hash);
		AssertNotNull ("CreateSignature(MD5)", signature);

		// good SHA256
		fmt.SetHashAlgorithm ("SHA256");
		hash = new byte [32];
		try {
			signature = fmt.CreateSignature (hash);
		}
		catch (CryptographicException) {
			// unknown OID !!!
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA256) - Unexpected exception: " + e.ToString ());
		}

		// good SHA384
		fmt.SetHashAlgorithm ("SHA384");
		hash = new byte [48];
		try {
			signature = fmt.CreateSignature (hash);
		}
		catch (CryptographicException) {
			// unknown OID !!!
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA384) - Unexpected exception: " + e.ToString ());
		}

		// good SHA512
		fmt.SetHashAlgorithm ("SHA512");
		hash = new byte [64];
		try {
			signature = fmt.CreateSignature (hash);
		}
		catch (CryptographicException) {
			// unknown OID !!!
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA512) - Unexpected exception: " + e.ToString ());
		}

		// null (bad ;-)
		hash = null;
		try {
			signature  = fmt.CreateSignature (hash);
			Fail ("Expected ArgumentNullException but none");
		}
		catch (ArgumentNullException) {
			// this is was we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}
	}

	public void TestCreateSignatureHash () 
	{
		RSAPKCS1SignatureFormatter fmt = new RSAPKCS1SignatureFormatter ();
		HashAlgorithm hash = null;
		byte[] data = new byte [20];

		// no hash algorithm
		byte[] signature = null;
		try {
			signature = fmt.CreateSignature (hash);
			Fail ("CreateSignature(?) no hash algo - Expected ArgumentNullException but none");
		}
		catch (ArgumentNullException) {
			// this was expected
		}
		catch (Exception e) {
			Fail ("CreateSignature(?) no hash algo - Expected ArgumentNullException but got: " + e.ToString ());
		}

		// no key
		hash = SHA1.Create ();
		hash.ComputeHash (data);
		try {
			signature = fmt.CreateSignature (hash);
			Fail ("CreateSignature(?) no key - Expected CryptographicUnexpectedOperationException but none");
		}
		catch (CryptographicUnexpectedOperationException) {
			// this was expected
		}
		catch (Exception e) {
			Fail ("CreateSignature(?) no key - Expected CryptographicUnexpectedOperationException but got: " + e.ToString ());
		}

		// we need the private key 
		RSA rsa = RSA.Create ();
		rsa.ImportParameters (AllTests.GetRsaKey (true));
		fmt.SetKey (rsa);

		// good SHA1
		hash = SHA1.Create ();
		hash.ComputeHash (data);
		signature = fmt.CreateSignature (hash);
		AssertNotNull ("CreateSignature(SHA1)", signature);

		// good MD5
		hash = MD5.Create ();
		hash.ComputeHash (data);
		signature = fmt.CreateSignature (hash);
		AssertNotNull ("CreateSignature(MD5)", signature);

		// good SHA256
		hash = SHA256.Create ();
		hash.ComputeHash (data);
		try {
			signature = fmt.CreateSignature (hash);
		}
		catch (CryptographicException) {
			// unknown OID !!!
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA256) - Unexpected exception: " + e.ToString ());
		}

		// good SHA384
		hash = SHA384.Create ();
		hash.ComputeHash (data);
		try {
			signature = fmt.CreateSignature (hash);
		}
		catch (CryptographicException) {
			// unknown OID !!!
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA384) - Unexpected exception: " + e.ToString ());
		}

		// good SHA512
		hash = SHA512.Create ();
		hash.ComputeHash (data);
		try {
			signature = fmt.CreateSignature (hash);
		}
		catch (CryptographicException) {
			// unknown OID !!!
		}
		catch (Exception e) {
			Fail ("CreateSignature(badSHA512) - Unexpected exception: " + e.ToString ());
		}
	}
}

}
