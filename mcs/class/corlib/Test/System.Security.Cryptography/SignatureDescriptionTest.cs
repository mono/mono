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
public class SignatureDescriptionTest {

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

	[Test]
	public void Constructor_Default () 
	{
		// empty constructor
		SignatureDescription sig = new SignatureDescription ();
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
		Assert.AreEqual ("System.Security.Cryptography.DSACryptoServiceProvider", sig.KeyAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.SHA1CryptoServiceProvider", sig.DigestAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.DSASignatureFormatter", sig.FormatterAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.DSASignatureDeformatter", sig.DeformatterAlgorithm);
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
		Assert.AreEqual ("System.Security.Cryptography.RSACryptoServiceProvider", sig.KeyAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.SHA1CryptoServiceProvider", sig.DigestAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.RSAPKCS1SignatureFormatter", sig.FormatterAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.RSAPKCS1SignatureDeformatter", sig.DeformatterAlgorithm);
	}

	[Test]
	public void Properties () 
	{
		string invalid = "invalid";
		Assert.IsNull (sig.DeformatterAlgorithm, "DeformatterAlgorithm 1");
		sig.DeformatterAlgorithm = invalid;
		Assert.IsNotNull (sig.DeformatterAlgorithm, "DeformatterAlgorithm 2");
		Assert.AreEqual (invalid, sig.DeformatterAlgorithm, "DeformatterAlgorithm 3");
		sig.DeformatterAlgorithm = null;
		Assert.IsNull (sig.DeformatterAlgorithm, "DeformatterAlgorithm 4");

		Assert.IsNull (sig.DigestAlgorithm, "DigestAlgorithm 1");
		sig.DigestAlgorithm = invalid;
		Assert.IsNotNull (sig.DigestAlgorithm, "DigestAlgorithm 2");
		Assert.AreEqual (invalid, sig.DigestAlgorithm, "DigestAlgorithm 3");
		sig.DigestAlgorithm = null;
		Assert.IsNull (sig.DigestAlgorithm, "DigestAlgorithm 4");

		Assert.IsNull (sig.FormatterAlgorithm, "FormatterAlgorithm 1");
		sig.FormatterAlgorithm = invalid;
		Assert.IsNotNull (sig.FormatterAlgorithm, "FormatterAlgorithm 2");
		Assert.AreEqual (invalid, sig.FormatterAlgorithm, "FormatterAlgorithm 3");
		sig.FormatterAlgorithm = null;
		Assert.IsNull (sig.FormatterAlgorithm, "FormatterAlgorithm 4");

		Assert.IsNull (sig.KeyAlgorithm, "KeyAlgorithm 1");
		sig.KeyAlgorithm = invalid;
		Assert.IsNotNull (sig.KeyAlgorithm, "KeyAlgorithm 2");
		Assert.AreEqual (invalid, sig.KeyAlgorithm, "KeyAlgorithm 3");
		sig.KeyAlgorithm = null;
		Assert.IsNull (sig.KeyAlgorithm, "KeyAlgorithm 4");
	}

	[Test]
	public void Deformatter () 
	{
		AsymmetricSignatureDeformatter def = null;
		// Deformatter with all properties null
		try {
			def = sig.CreateDeformatter (dsa);
			Assert.Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}
		// Deformatter with invalid DeformatterAlgorithm property
		sig.DeformatterAlgorithm = "DSA";
		try {
			def = sig.CreateDeformatter (dsa);
			Assert.Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}
		// Deformatter with valid DeformatterAlgorithm property
		sig.DeformatterAlgorithm = "DSASignatureDeformatter";
		try {
			def = sig.CreateDeformatter (dsa);
			Assert.Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}
		// Deformatter with valid DeformatterAlgorithm property
		sig.KeyAlgorithm = "DSA";
		sig.DigestAlgorithm = "SHA1";
		sig.DeformatterAlgorithm = "DSASignatureDeformatter";
		try {
			def = sig.CreateDeformatter (dsa);
			Assert.Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected NullReferenceException but got: " + e.ToString ());
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
			Assert.Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}

		sig.DigestAlgorithm = "SHA1";
		hash = sig.CreateDigest ();
		Assert.IsNotNull (hash, "CreateDigest(SHA1)");
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert.IsTrue (rightClass, "CreateDigest(SHA1)");

		sig.DigestAlgorithm = "MD5";
		hash = sig.CreateDigest ();
		Assert.IsNotNull (hash, "CreateDigest(MD5)");
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert.IsTrue (rightClass, "CreateDigest(MD5)");

		sig.DigestAlgorithm = "SHA256";
		hash = sig.CreateDigest ();
		Assert.IsNotNull (hash, "CreateDigest(SHA256)");
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert.IsTrue (rightClass, "CreateDigest(SHA256)");

		sig.DigestAlgorithm = "SHA384";
		hash = sig.CreateDigest ();
		Assert.IsNotNull (hash, "CreateDigest(SHA384)");
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert.IsTrue (rightClass, "CreateDigest(SHA384)");

		sig.DigestAlgorithm = "SHA512";
		hash = sig.CreateDigest ();
		Assert.IsNotNull (hash, "CreateDigest(SHA512)");
		rightClass = (hash.ToString ().IndexOf (sig.DigestAlgorithm) > 0);
		Assert.IsTrue (rightClass, "CreateDigest(SHA512)");

		sig.DigestAlgorithm = "bad";
		hash = sig.CreateDigest ();
		Assert.IsNull (hash, "CreateDigest(bad)");
	}

	[Test]
	public void Formatter () 
	{
		AsymmetricSignatureFormatter fmt = null;
		// Formatter with all properties null
		try {
			fmt = sig.CreateFormatter (dsa);
			Assert.Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}
		// Formatter with invalid FormatterAlgorithm property
		sig.FormatterAlgorithm = "DSA";
		try {
			fmt = sig.CreateFormatter (dsa);
			Assert.Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected InvalidCastException but got: " + e.ToString ());
		}
		// Formatter with valid FormatterAlgorithm property
		sig.FormatterAlgorithm = "DSASignatureFormatter";
		try {
			fmt = sig.CreateFormatter (dsa);
			Assert.Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}
		// Deformatter with valid DeformatterAlgorithm property
		sig.KeyAlgorithm = "DSA";
		sig.DigestAlgorithm = "SHA1";
		sig.FormatterAlgorithm = "DSASignatureFormatter";
		try {
			fmt = sig.CreateFormatter (dsa);
			Assert.Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is what we expect
		}
		catch (Exception e) {
			Assert.Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}
	}

	[Test]
	public void DSASignatureDescription ()  
	{
		// internal class - we cannot create one without CryptoConfig
		SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName ("http://www.w3.org/2000/09/xmldsig#dsa-sha1");
		Assert.AreEqual ("System.Security.Cryptography.SHA1CryptoServiceProvider", sd.DigestAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.DSASignatureDeformatter", sd.DeformatterAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.DSASignatureFormatter", sd.FormatterAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.DSACryptoServiceProvider", sd.KeyAlgorithm);

		HashAlgorithm hash = sd.CreateDigest();
		Assert.AreEqual ("System.Security.Cryptography.SHA1CryptoServiceProvider", hash.ToString ());

		Assert.AreEqual (dsa.ToString (), sd.KeyAlgorithm);

		AsymmetricSignatureDeformatter asd = sd.CreateDeformatter (dsa);
		Assert.AreEqual ("System.Security.Cryptography.DSASignatureDeformatter", asd.ToString ());

		AsymmetricSignatureFormatter asf = sd.CreateFormatter (dsa);
		Assert.AreEqual ("System.Security.Cryptography.DSASignatureFormatter", asf.ToString ());
	}

	[Test]
	public void RSASignatureDescription () 
	{
		// internal class - we cannot create one without CryptoConfig
		SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName ("http://www.w3.org/2000/09/xmldsig#rsa-sha1");
		Assert.AreEqual ("System.Security.Cryptography.SHA1CryptoServiceProvider", sd.DigestAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.RSAPKCS1SignatureDeformatter", sd.DeformatterAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.RSAPKCS1SignatureFormatter", sd.FormatterAlgorithm);
		Assert.AreEqual ("System.Security.Cryptography.RSACryptoServiceProvider", sd.KeyAlgorithm);

		HashAlgorithm hash = sd.CreateDigest();
		Assert.AreEqual ("System.Security.Cryptography.SHA1CryptoServiceProvider", hash.ToString ());

		Assert.AreEqual (rsa.ToString (), sd.KeyAlgorithm);

		AsymmetricSignatureDeformatter asd = sd.CreateDeformatter (rsa);
		Assert.AreEqual ("System.Security.Cryptography.RSAPKCS1SignatureDeformatter", asd.ToString ());

		AsymmetricSignatureFormatter asf = sd.CreateFormatter (rsa);
		Assert.AreEqual ("System.Security.Cryptography.RSAPKCS1SignatureFormatter", asf.ToString ());
	}
}

}