//
// SignatureDescriptionTest.cs - NUnit Test Cases for SignatureDescription
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell  http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class SignatureDescriptionTest : Assertion {

	protected SignatureDescription sig;
	protected static DSA dsa;
	protected static RSA rsa;

	[SetUp]
	public void SetUp () 
	{
		sig = new SignatureDescription();
		// key generation is VERY long so one time is enough
		if (dsa == null)
			dsa = DSA.Create ();
		if (rsa == null)
			rsa = RSA.Create ();
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	[Test]
	public void Constructor_Default () 
	{
		// empty constructor
		SignatureDescription sig = new SignatureDescription ();
		AssertNotNull ("SignatureDescription()", sig);
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Constructor_Null () 
	{
		// null constructor
		SignatureDescription sig = new SignatureDescription (null);
		// LAMESPEC: Documented as CryptographicException
	}
	
	[Test]
	public void Constructor_SecurityElement_Empty () 
	{
		// (empty) SecurityElement constructor
		SecurityElement se = new SecurityElement ("xml");
		SignatureDescription sig = new SignatureDescription (se);
		AssertNotNull ("SignatureDescription(SecurityElement)", sig);
	}

	[Test]
	public void Constructor_SecurityElement_DSA ()
	{
		SecurityElement se = new SecurityElement ("DSASignature");
		se.AddChild (new SecurityElement ("Key", "System.Security.Cryptography.DSACryptoServiceProvider"));
		se.AddChild (new SecurityElement ("Digest", "System.Security.Cryptography.SHA1CryptoServiceProvider"));
		se.AddChild (new SecurityElement ("Formatter", "System.Security.Cryptography.DSASignatureFormatter"));
		se.AddChild (new SecurityElement ("Deformatter", "System.Security.Cryptography.DSASignatureDeformatter"));

		SignatureDescription sig = new SignatureDescription (se);
		AssertNotNull ("SignatureDescription(SecurityElement)", sig);
		AssertEquals ("Key", "System.Security.Cryptography.DSACryptoServiceProvider", sig.KeyAlgorithm);
		AssertEquals ("Digest", "System.Security.Cryptography.SHA1CryptoServiceProvider", sig.DigestAlgorithm);
		AssertEquals ("Formatter", "System.Security.Cryptography.DSASignatureFormatter", sig.FormatterAlgorithm);
		AssertEquals ("Deformatter", "System.Security.Cryptography.DSASignatureDeformatter", sig.DeformatterAlgorithm);
	}

	[Test]
	public void Constructor_SecurityElement_RSA ()
	{
		SecurityElement se = new SecurityElement ("RSASignature");
		se.AddChild (new SecurityElement ("Key", "System.Security.Cryptography.RSACryptoServiceProvider"));
		se.AddChild (new SecurityElement ("Digest", "System.Security.Cryptography.SHA1CryptoServiceProvider"));
		se.AddChild (new SecurityElement ("Formatter", "System.Security.Cryptography.RSAPKCS1SignatureFormatter"));
		se.AddChild (new SecurityElement ("Deformatter", "System.Security.Cryptography.RSAPKCS1SignatureDeformatter"));

		SignatureDescription sig = new SignatureDescription (se);
		AssertNotNull ("SignatureDescription(SecurityElement)", sig);
		AssertEquals ("Key", "System.Security.Cryptography.RSACryptoServiceProvider", sig.KeyAlgorithm);
		AssertEquals ("Digest", "System.Security.Cryptography.SHA1CryptoServiceProvider", sig.DigestAlgorithm);
		AssertEquals ("Formatter", "System.Security.Cryptography.RSAPKCS1SignatureFormatter", sig.FormatterAlgorithm);
		AssertEquals ("Deformatter", "System.Security.Cryptography.RSAPKCS1SignatureDeformatter", sig.DeformatterAlgorithm);
	}

	[Test]
	public void Properties () 
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

	[Test]
	public void Deformatter () 
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
	}

	[Test]
	public void Digest ()
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

	[Test]
	public void Formatter () 
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
	}

	[Test]
	public void DSASignatureDescription ()  
	{
		// internal class - we cannot create one without CryptoConfig
		SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName ("http://www.w3.org/2000/09/xmldsig#dsa-sha1");
		AssertEquals ("DSA.DigestAlgorithm", "System.Security.Cryptography.SHA1CryptoServiceProvider", sd.DigestAlgorithm);
		AssertEquals ("DSA.DeformatterAlgorithm", "System.Security.Cryptography.DSASignatureDeformatter", sd.DeformatterAlgorithm);
		AssertEquals ("DSA.FormatterAlgorithm", "System.Security.Cryptography.DSASignatureFormatter", sd.FormatterAlgorithm);
		AssertEquals ("DSA.KeyAlgorithm", "System.Security.Cryptography.DSACryptoServiceProvider", sd.KeyAlgorithm);

		HashAlgorithm hash = sd.CreateDigest();
		AssertEquals ("DSA.CreateDigest", "System.Security.Cryptography.SHA1CryptoServiceProvider", hash.ToString ());

		AssertEquals ("DSA.Create", dsa.ToString (), sd.KeyAlgorithm);

		AsymmetricSignatureDeformatter asd = sd.CreateDeformatter (dsa);
		AssertEquals ("DSA.CreateDeformatter", "System.Security.Cryptography.DSASignatureDeformatter", asd.ToString ());

		AsymmetricSignatureFormatter asf = sd.CreateFormatter (dsa);
		AssertEquals ("DSA.CreateFormatter", "System.Security.Cryptography.DSASignatureFormatter", asf.ToString ());
	}

	[Test]
	public void RSASignatureDescription () 
	{
		// internal class - we cannot create one without CryptoConfig
		SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName ("http://www.w3.org/2000/09/xmldsig#rsa-sha1");
		AssertEquals ("RSA.DigestAlgorithm", "System.Security.Cryptography.SHA1CryptoServiceProvider", sd.DigestAlgorithm);
		AssertEquals ("RSA.DeformatterAlgorithm", "System.Security.Cryptography.RSAPKCS1SignatureDeformatter", sd.DeformatterAlgorithm);
		AssertEquals ("RSA.FormatterAlgorithm", "System.Security.Cryptography.RSAPKCS1SignatureFormatter", sd.FormatterAlgorithm);
		AssertEquals ("RSA.KeyAlgorithm", "System.Security.Cryptography.RSACryptoServiceProvider", sd.KeyAlgorithm);

		HashAlgorithm hash = sd.CreateDigest();
		AssertEquals ("RSA.CreateDigest", "System.Security.Cryptography.SHA1CryptoServiceProvider", hash.ToString ());

		AssertEquals ("RSA.Create", rsa.ToString (), sd.KeyAlgorithm);

		AsymmetricSignatureDeformatter asd = sd.CreateDeformatter (rsa);
		AssertEquals ("RSA.CreateDeformatter", "System.Security.Cryptography.RSAPKCS1SignatureDeformatter", asd.ToString ());

		AsymmetricSignatureFormatter asf = sd.CreateFormatter (rsa);
		AssertEquals ("RSA.CreateFormatter", "System.Security.Cryptography.RSAPKCS1SignatureFormatter", asf.ToString ());
	}
}

}