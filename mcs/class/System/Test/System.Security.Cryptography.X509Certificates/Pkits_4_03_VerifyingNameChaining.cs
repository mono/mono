//
// Pkits_4_03_VerifyingNameChaining.cs -
//	NUnit tests for Pkits 4.3 : Verifying Name Chaining
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	/*
	 * Notes:
	 *
	 * [MS/XP][!RFC3280] It doesn't looks like any checks is done between the 
	 * EE issuer and CA subject names.
	 *
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_03_VerifyingNameChaining: PkitsTest {

		public X509Certificate2 NameOrderingCACert {
			get { return GetCertificate ("NameOrderingCACert.crt"); }
		}

		public X509Certificate2 UIDCACert {
			get { return GetCertificate ("UIDCACert.crt"); }
		}

		public X509Certificate2 RFC3280MandatoryAttributeTypesCACert {
			get { return GetCertificate ("RFC3280MandatoryAttributeTypesCACert.crt"); }
		}
			
		public X509Certificate2 RFC3280OptionalAttributeTypesCACert {
			get { return GetCertificate ("RFC3280OptionalAttributeTypesCACert.crt"); }
		}

		public X509Certificate2 UTF8StringEncodedNamesCACert {
			get { return GetCertificate ("UTF8StringEncodedNamesCACert.crt"); }
		}

		public X509Certificate2 RolloverfromPrintableStringtoUTF8StringCACert {
			get { return GetCertificate ("RolloverfromPrintableStringtoUTF8StringCACert.crt"); }
		}

		public X509Certificate2 UTF8StringCaseInsensitiveMatchCACert {
			get { return GetCertificate ("UTF8StringCaseInsensitiveMatchCACert.crt"); }
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T01_InvalidNameChainingEE ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidNameChainingTest1EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: different ee.issuer/ca.subject names
			// ee.IssuerName.Name		"CN=Good CA Root, O=Test Certificates, C=US"
			// GoodCACert.SubjectName.Name	"CN=Good CA, O=Test Certificates, C=US"
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidNameConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.InvalidNameConstraints, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T01_InvalidNameChainingEE_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidNameChainingTest1EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is NOT valid wrt RFC3280
			// I don't like this result. MS builds the chain (using AKI/SKI) then can't find
			// the CRL (based on the wrong CA name?) which isn't what the test looks for.

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");

			// Here's a proof of this, disabling the revocation check for the end-entity results in
			// a success

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T02_InvalidNameChainingOrder ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidNameChainingOrderTest2EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: different (order) ee.issuer/ca.subject names
			// ee.Issuer			"CN=Name Ordering CA, OU=Organizational Unit Name 1, OU=Organizational Unit Name 2, O=Test Certificates, C=US"
			// NameOrderingCACert.Subject	"CN=Name Ordering CA, OU=Organizational Unit Name 2, OU=Organizational Unit Name 1, O=Test Certificates, C=US"
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.InvalidNameConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.InvalidNameConstraints, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (NameOrderingCACert, chain.ChainElements[1].Certificate, "NameOrderingCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "NameOrderingCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T02_InvalidNameChainingOrder_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidNameChainingOrderTest2EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is NOT valid wrt RFC3280
			// I don't like this result. MS builds the chain (using AKI/SKI) then can't find
			// the CRL (based on the wrong CA name?) which isn't what the test looks for.

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (NameOrderingCACert, chain.ChainElements[1].Certificate, "NameOrderingCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "NameOrderingCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");

			// Here's a proof of this, disabling the revocation check for the end-entity results in
			// a success

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T03_ValidNameChainingWhitespace ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameChainingWhitespaceTest3EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: different (spaces) ee.issuer/ca.subject names
			// ee.Issuer		"CN=Good     CA, O=Test  Certificates, C=US"
			// GoodCACert.Subject	"CN=Good CA, O=Test Certificates, C=US"
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T03_ValidNameChainingWhitespace_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameChainingWhitespaceTest3EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			// MS doesn't support internal whitespace compression. It seems MS builds the chain 
			// (using AKI/SKI) then can't find the CRL (which isn't what the test looks for).

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");

			// Here's a proof of this, disabling the revocation check for the end-entity results in
			// a success

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T04_ValidNameChainingWhitespace ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameChainingWhitespaceTest4EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: different (spaces) ee.issuer/ca.subject names
			// ee.Issuer		"CN=\"   Good CA\", O=\"Test Certificates   \", C=US"
			// GoodCACert.Subject	"CN=Good CA, O=Test Certificates, C=US"
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T04_ValidNameChainingWhitespace_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameChainingWhitespaceTest4EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			// MS doesn't support internal whitespace compression. It seems MS builds the chain 
			// (using AKI/SKI) then can't find the CRL (which isn't what the test looks for).

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");

			// Here's a proof of this, disabling the revocation check for the end-entity results in
			// a success

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T05_ValidNameChainingCapitalization ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameChainingCapitalizationTest5EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: different (capitalization) ee.issuer/ca.subject names
			// ee.Issuer		"CN=GOOD CA, O=Test Certificates, C=US"
			// GoodCACert.Subject	"CN=Good CA, O=Test Certificates, C=US"
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T05_ValidNameChainingCapitalization_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameChainingCapitalizationTest5EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			// NOTE: X509FindType.FindBySubjectDistinguishedName deals (correctly) with capitalization 
			// issues. However it seems MS can't find the CRL based on the name with a different 
			// capitalization so FALSE is returned

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");

			// Here's a proof of this, disabling the revocation check for the end-entity results in
			// a success

			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.IsTrue (chain.Build (ee), "Build-Bug");
		}

		[Test]
		public void T06_ValidNameChainingUIDs ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNameUIDsTest6EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UIDCACert, chain.ChainElements[1].Certificate, "UIDCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UIDCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T07_ValidRFC3280MandatoryAttributeTypes ()
		{
			X509Certificate2 ee = GetCertificate ("ValidRFC3280MandatoryAttributeTypesTest7EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (RFC3280MandatoryAttributeTypesCACert, chain.ChainElements[1].Certificate, "RFC3280MandatoryAttributeTypesCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "RFC3280MandatoryAttributeTypesCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T08_ValidRFC3280OptionalAttributeTypes ()
		{
			X509Certificate2 ee = GetCertificate ("ValidRFC3280OptionalAttributeTypesTest8EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (RFC3280OptionalAttributeTypesCACert, chain.ChainElements[1].Certificate, "RFC3280OptionalAttributeTypesCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "RFC3280OptionalAttributeTypesCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T09_ValidUTF8StringEncodedNames ()
		{
			X509Certificate2 ee = GetCertificate ("ValidUTF8StringEncodedNamesTest9EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UTF8StringEncodedNamesCACert, chain.ChainElements[1].Certificate, "UTF8StringEncodedNamesCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UTF8StringEncodedNamesCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T10_ValidRolloverFromPrintableStringToUTF8String ()
		{
			X509Certificate2 ee = GetCertificate ("ValidRolloverfromPrintableStringtoUTF8StringTest10EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (RolloverfromPrintableStringtoUTF8StringCACert, chain.ChainElements[1].Certificate, "RolloverfromPrintableStringtoUTF8StringCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "RolloverfromPrintableStringtoUTF8StringCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T11_ValidUTF8StringCaseInsensitiveMatch ()
		{
			X509Certificate2 ee = GetCertificate ("ValidUTF8StringCaseInsensitiveMatchTest11EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: different ee.issuer/ca.subject names (spaces & cases)
			// ee.Issuer					"CN=utf8string case  insensitive match CA, O=\"  test certificates  \", C=US"
			// UTF8StringCaseInsensitiveMatchCACert.Subject	"CN=UTF8String Case Insensitive Match CA, O=Test Certificates, C=US"
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UTF8StringCaseInsensitiveMatchCACert, chain.ChainElements[1].Certificate, "UTF8StringCaseInsensitiveMatchCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UTF8StringCaseInsensitiveMatchCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T11_ValidUTF8StringCaseInsensitiveMatch_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidUTF8StringCaseInsensitiveMatchTest11EE.crt");
			X509Chain chain = new X509Chain ();
			// MS-BAD / this is valid wrt RFC3280
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UTF8StringCaseInsensitiveMatchCACert, chain.ChainElements[1].Certificate, "UTF8StringCaseInsensitiveMatchCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UTF8StringCaseInsensitiveMatchCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
