//
// DSASignatureDeformatterTest.cs - NUnit Test Cases for DSASignatureDeformatter
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

public class DSASignatureDeformatterTest : TestCase {
	protected DSASignatureDeformatter def;
	protected static DSA dsa;
	protected static RSA rsa;

	protected override void SetUp () 
	{
		def = new DSASignatureDeformatter ();
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
		DSASignatureDeformatter def = new DSASignatureDeformatter ();
		AssertNotNull ("DSASignatureDeformatter()", def);
		// AsymmetricAlgorithm constructor (with null)
		def = new DSASignatureDeformatter (null);
		AssertNotNull ("DSASignatureDeformatter(null)", def);
		// AsymmetricAlgorithm constructor (with DSA)
		def = new DSASignatureDeformatter (dsa);
		AssertNotNull ("DSASignatureDeformatter(dsa)", def);
		// AsymmetricAlgorithm constructor (with RSA)
		try {
			def = new DSASignatureDeformatter (rsa);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got " + e.ToString ());
		}
	}

	// Method is documented as unused so...
	public void TestSetHash () 
	{
		// null is ok
		try {
			def.SetHashAlgorithm (null);
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got " + e.ToString ());
		}
		// SHA1
		try {
			def.SetHashAlgorithm ("SHA1");
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
		// MD5 (bad)
		try {
			def.SetHashAlgorithm ("MD5");
		}
		catch (CryptographicUnexpectedOperationException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected CryptographicUnexpectedOperationException but got " + e.ToString ());
		}
	}

	public void TestSetKey () 
	{
		// here null is ok 
		try {
			def.SetKey (null);
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
		// RSA (bad)
		try {
			def.SetKey (rsa);
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
			def.SetKey (dsa);
		}
		catch (Exception e) {
			Fail ("Unexpected exception: " + e.ToString ());
		}
	}

	public void TestVerify () 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		byte[] sign = { 0x50, 0xd2, 0xb0, 0x8b, 0xcd, 0x5e, 0xb2, 0xc2, 0x35, 0x82, 0xd3, 0x76, 0x07, 0x79, 0xbb, 0x55, 0x98, 0x72, 0x43, 0xe8,
				      0x74, 0xc9, 0x35, 0xf8, 0xc9, 0xbd, 0x69, 0x2f, 0x08, 0x34, 0xfa, 0x5a, 0x59, 0x23, 0x2a, 0x85, 0x7b, 0xa3, 0xb3, 0x82 };
		bool ok = false;
		try {
			ok = def.VerifySignature (hash, sign);
		}
		catch (CryptographicUnexpectedOperationException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected CryptographicUnexpectedOperationException but got " + e.ToString ());
		}

		dsa.ImportParameters (AllTests.GetKey (false));
		def.SetKey (dsa);

		// missing signature
		try {
			ok = def.VerifySignature (hash, null);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got " + e.ToString ());
		}

		// missing hash
		try {
			byte[] s = null; // overloaded method
			ok = def.VerifySignature (s, sign);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect 
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got " + e.ToString ());
		}

		ok = def.VerifySignature (hash, sign);
		Assert ("verified signature", ok);

		byte[] badSign = { 0x49, 0xd2, 0xb0, 0x8b, 0xcd, 0x5e, 0xb2, 0xc2, 0x35, 0x82, 0xd3, 0x76, 0x07, 0x79, 0xbb, 0x55, 0x98, 0x72, 0x43, 0xe8,
					 0x74, 0xc9, 0x35, 0xf8, 0xc9, 0xbd, 0x69, 0x2f, 0x08, 0x34, 0xfa, 0x5a, 0x59, 0x23, 0x2a, 0x85, 0x7b, 0xa3, 0xb3, 0x82 };
		ok = def.VerifySignature (hash, badSign);
		Assert ("didn't verified bad signature", !ok);
	}
}

}