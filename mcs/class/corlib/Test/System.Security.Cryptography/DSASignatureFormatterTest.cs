//
// DSASignatureFormatterTest.cs - NUnit Test Cases for DSASignatureFormatter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

public class DSASignatureFormatterTest : TestCase {
	protected DSASignatureFormatter fmt;
	protected static DSA dsa;
	protected static RSA rsa;

	protected override void SetUp () 
	{
		fmt = new DSASignatureFormatter ();
		// key generation is VERY long so one time is enough
		if (dsa == null)
			dsa = DSA.Create ();
		if (rsa == null)
			rsa = RSA.Create ();
	}

	protected override void TearDown () {}

	public void TestConstructors () 
	{
		// empty constructor
		DSASignatureFormatter fmt = new DSASignatureFormatter ();
		AssertNotNull ("DSASignatureFormatter()", fmt);
		// AsymmetricAlgorithm constructor (with null)
		fmt = new DSASignatureFormatter (null);
		AssertNotNull ("DSASignatureFormatter(null)", fmt);
		// AsymmetricAlgorithm constructor (with DSA)
		fmt = new DSASignatureFormatter (dsa);
		AssertNotNull ("DSASignatureFormatter(dsa)", fmt);
		// AsymmetricAlgorithm constructor (with RSA)
		try {
			fmt = new DSASignatureFormatter (rsa);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got " + e.ToString ());
		}
	}

	public void TestSetHash () 
	{
		// null is ok
		try {
			fmt.SetHashAlgorithm (null);
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got " + e.ToString ());
		}
		// SHA1
		try {
			fmt.SetHashAlgorithm ("SHA1");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
		// MD5 (bad)
		try {
			fmt.SetHashAlgorithm ("MD5");
		}
		catch (CryptographicUnexpectedOperationException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected CryptographicUnexpectedOperationException but got " + e.ToString ());
		}
	}

	public void TestSetKey () {
		// here null is ok 
		try {
			fmt.SetKey (null);
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
		// RSA (bad)
		try {
			fmt.SetKey (rsa);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}
		// DSA
		try {
			fmt.SetKey (dsa);
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
	}

	// note: There's a bug in MS Framework where you can't re-import a key into
	// the same object
	public void TestSignature () 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		byte[] sign = null;
		// no keypair
		try {
			sign = fmt.CreateSignature (hash);
		}
		catch (CryptographicUnexpectedOperationException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected CryptographicUnexpectedOperationException but got " + e.ToString ());
		}

		// try a keypair without the private key
		dsa.ImportParameters (AllTests.GetKey (false));
		fmt.SetKey (dsa);
		try {
			sign = fmt.CreateSignature (hash);
			Fail ("Expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected CryptographicException but got " + e.ToString ());
		}

		// complete keypair
		dsa.ImportParameters (AllTests.GetKey (true));
		fmt.SetKey (dsa);

		// null hash
		try {
			byte[] h = null; // overloaded method
			sign = fmt.CreateSignature (h); 
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got " + e.ToString ());
		}

		// valid
		sign = fmt.CreateSignature (hash);
		Assert ("verified signature", dsa.VerifySignature (hash, sign));
	}
}

}