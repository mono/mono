//
// AuthenticationKeyTest.cs 
//	- NUnit Test Cases for AuthenticationKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class AuthenticationKeyTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorAsymmetricNull () 
		{
			// we do not want to confuse the compiler about null ;-)
			AsymmetricAlgorithm aa = null;
			AuthenticationKey ak = new AuthenticationKey (aa);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorSymmetricNull () 
		{
			// we do not want to confuse the compiler about null ;-)
			SymmetricAlgorithm sa = null;
			AuthenticationKey ak = new AuthenticationKey (sa);
		}

		[Test]
		// LAMESPEC: undocumented exception
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckSignatureNull () 
		{
			DSA dsa = DSA.Create ();
			dsa.ImportParameters (AllTests.GetDSAKey (false));
			AuthenticationKey ak = new AuthenticationKey (dsa);
			ak.CheckSignature (null);
		}

		[Test]
		public void CheckSignatureDSA () 
		{
			DSA dsa = DSA.Create ();
			dsa.ImportParameters (AllTests.GetDSAKey (false));
			AuthenticationKey ak = new AuthenticationKey (dsa);
			AssertNotNull ("AuthenticationKey(DSA)", ak);

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#dsa-sha1\" /><Reference URI=\"http://www.go-mono.com/\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>JjDjPDymSPahf7zC6CCqaTz39uQ=</DigestValue></Reference></SignedInfo><SignatureValue>j2Rl0vEKoKbRfOTtEZvYoNSdiJdkCfN+FfuMntTqVMmYFFtl/nWExg==</SignatureValue></Signature>");
			SignedXml signedXml = new SignedXml ();
			signedXml.LoadXml (doc.DocumentElement);
			Assert ("CheckSignature(DSA)", ak.CheckSignature (signedXml));
		}

		[Test]
		public void CheckSignatureRSA () 
		{
			RSA rsa = RSA.Create ();
			rsa.ImportParameters (AllTests.GetRSAKey (false));
			AuthenticationKey ak = new AuthenticationKey (rsa);
			AssertNotNull ("AuthenticationKey(RSA)", ak);

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" /><Reference URI=\"http://www.go-mono.com/\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>JjDjPDymSPahf7zC6CCqaTz39uQ=</DigestValue></Reference></SignedInfo><SignatureValue>l/Anzks1CncmNQsDb2ZgSYEcXcX7sS0jql5fmQcEVAkRDhxwV2Lb+6Z2yPO5F+QZ+54m8w/QJdbQJduyOF0w1blkWd1Iz5ubOt79Cg1f0zO8SYwB1X6j0SzXXB5tTm1hYjKzX/iAqgUJF1o8bscu74A8xCwQio2ay7TWoTEl/Ss=</SignatureValue></Signature>");
			SignedXml signedXml = new SignedXml ();
			signedXml.LoadXml (doc.DocumentElement);
			Assert ("CheckSignature(RSA)", ak.CheckSignature (signedXml));
		}

		[Test]
		public void CheckSignatureSymmetricAlgo () 
		{
			// default (should be Rjindael)
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create ();
			sa.Key = new byte [16]; // 128 bits (all zeros)
			AuthenticationKey ak = new AuthenticationKey (sa);
			AssertNotNull ("AuthenticationKey(SymmetricAlgorithm)", ak);

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#hmac-sha1\" /><Reference URI=\"http://www.go-mono.com/\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>JjDjPDymSPahf7zC6CCqaTz39uQ=</DigestValue></Reference></SignedInfo><SignatureValue>y5NfXaoCALrMxBsn/wGKNJUNJ7Y=</SignatureValue></Signature>");
			SignedXml signedXml = new SignedXml ();
			signedXml.LoadXml (doc.DocumentElement);
			Assert ("CheckSignature(HMAC)", ak.CheckSignature (signedXml));
		}
	}
}