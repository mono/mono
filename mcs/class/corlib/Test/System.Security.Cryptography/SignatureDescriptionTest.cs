//
// SignatureDescriptionTest.cs - NUnit Test Cases for SignatureDescription
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

public class SignatureDescriptionTest : TestCase {

	public SignatureDescriptionTest () : base ("System.Security.Cryptography.SignatureDescription testsuite") {}
	public SignatureDescriptionTest (string name) : base (name) {}

	protected SignatureDescription sig;
	protected static DSA dsa;
	protected static RSA rsa;

	protected override void SetUp () 
	{
		sig = new SignatureDescription();
		// key generation is VERY long so one time is enough
		if (dsa == null)
			dsa = DSA.Create ();
		if (rsa == null)
			rsa = RSA.Create ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (SignatureDescriptionTest)); 
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	public void TestConstructors () 
	{
		// empty constructor
		SignatureDescription sig = new SignatureDescription ();
		AssertNotNull ("SignatureDescription()", sig);
		// null constructor
		try {
			sig = new SignatureDescription (null);
			Fail ("SignatureDescription(null): Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// LAMESPEC: Documented as CryptographicException
			// but thrown as ArgumentNullException
		}
		catch (Exception e) {
			Fail ("SignatureDescription(null): Expected ArgumentNullException but got: " + e.ToString ());
		}
		// (empty) SecurityElement constructor
		SecurityElement se = new SecurityElement ("xml");
		sig = new SignatureDescription (se);
		AssertNotNull ("SignatureDescription(SecurityElement)", sig);
	}

	public void TestProperties () 
	{
		string invalid = "invalid";
		AssertNull ("DeformatterAlgorithm 1", sig.DeformatterAlgorithm);
		sig.DeformatterAlgorithm = invalid;
		AssertNotNull ("DeformatterAlgorithm 2", sig.DeformatterAlgorithm);
		AssertEquals ("DeformatterAlgorithm 3", invalid, sig.DeformatterAlgorithm);
		sig.DeformatterAlgorithm = null;
		AssertNull ("DeformatterAlgorithm 4", sig.DeformatterAlgorithm);

		AssertNull ("DigestAlgorithm 1", sig.DigestAlgorithm);
		sig.DigestAlgorithm = invalid;
		AssertNotNull ("DigestAlgorithm 2", sig.DigestAlgorithm);
		AssertEquals ("DigestAlgorithm 3", invalid, sig.DigestAlgorithm);
		sig.DigestAlgorithm = null;
		AssertNull ("DigestAlgorithm 4", sig.DigestAlgorithm);

		AssertNull ("FormatterAlgorithm 1", sig.FormatterAlgorithm);
		sig.FormatterAlgorithm = invalid;
		AssertNotNull ("FormatterAlgorithm 2", sig.FormatterAlgorithm);
		AssertEquals ("FormatterAlgorithm 3", invalid, sig.FormatterAlgorithm);
		sig.FormatterAlgorithm = null;
		AssertNull ("FormatterAlgorithm 4", sig.FormatterAlgorithm);

		AssertNull ("KeyAlgorithm 1", sig.KeyAlgorithm);
		sig.KeyAlgorithm = invalid;
		AssertNotNull ("KeyAlgorithm 2", sig.KeyAlgorithm);
		AssertEquals ("KeyAlgorithm 3", invalid, sig.KeyAlgorithm);
		sig.KeyAlgorithm = null;
		AssertNull ("KeyAlgorithm 4", sig.KeyAlgorithm);
	}

	// I can't get any AsymmetricSignatureDeformatter out of this class!
	public void TestDeformatter () 
	{
		AsymmetricSignatureDeformatter def = null;
		// Deformatter with all properties null
		try {
			def = sig.CreateDeformatter (dsa);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}
		// Deformatter with invalid DeformatterAlgorithm property
		sig.DeformatterAlgorithm = "DSA";
		try {
			def = sig.CreateDeformatter (dsa);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}
		// Deformatter with valid DeformatterAlgorithm property
		sig.DeformatterAlgorithm = "DSASignatureDeformatter";
		try {
			def = sig.CreateDeformatter (dsa);
			Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}
		// Deformatter with valid DeformatterAlgorithm property
		sig.KeyAlgorithm = "DSA";
		sig.DigestAlgorithm = "SHA1";
		sig.DeformatterAlgorithm = "DSASignatureDeformatter";
		try {
			def = sig.CreateDeformatter (dsa);
			Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}

		// TODO: RSA
	}

	public void TestDigest ()
	{
		bool rightClass = false;
		HashAlgorithm hash = null;
		// null hash
		try {
			hash = sig.CreateDigest ();
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}

		sig.DigestAlgorithm = "SHA1";
		hash = sig.CreateDigest ();
		AssertNotNull ("CreateDigest(SHA1)", hash);
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert ("CreateDigest(SHA1)", rightClass);

		sig.DigestAlgorithm = "MD5";
		hash = sig.CreateDigest ();
		AssertNotNull ("CreateDigest(MD5)", hash);
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert ("CreateDigest(MD5)", rightClass);

		sig.DigestAlgorithm = "SHA256";
		hash = sig.CreateDigest ();
		AssertNotNull ("CreateDigest(SHA256)", hash);
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert ("CreateDigest(SHA256)", rightClass);

		sig.DigestAlgorithm = "SHA384";
		hash = sig.CreateDigest ();
		AssertNotNull ("CreateDigest(SHA384)", hash);
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert ("CreateDigest(SHA384)", rightClass);

		sig.DigestAlgorithm = "SHA512";
		hash = sig.CreateDigest ();
		AssertNotNull ("CreateDigest(SHA512)", hash);
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert ("CreateDigest(SHA512)", rightClass);

		sig.DigestAlgorithm = "bad";
		hash = sig.CreateDigest ();
		AssertNull ("CreateDigest(bad)", hash);
	}

	// I can't get any AsymmetricSignatureFormatter out of this class!
	public void TestFormatter () 
	{
		AsymmetricSignatureFormatter fmt = null;
		// Formatter with all properties null
		try {
			fmt = sig.CreateFormatter (dsa);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}
		// Formatter with invalid FormatterAlgorithm property
		sig.FormatterAlgorithm = "DSA";
		try {
			fmt = sig.CreateFormatter (dsa);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}
		// Formatter with valid FormatterAlgorithm property
		sig.FormatterAlgorithm = "DSASignatureFormatter";
		try {
			fmt = sig.CreateFormatter (dsa);
			Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}
		// Deformatter with valid DeformatterAlgorithm property
		sig.KeyAlgorithm = "DSA";
		sig.DigestAlgorithm = "SHA1";
		sig.FormatterAlgorithm = "DSASignatureFormatter";
		try {
			fmt = sig.CreateFormatter (dsa);
			Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}

		// TODO: RSA
	}
}

}