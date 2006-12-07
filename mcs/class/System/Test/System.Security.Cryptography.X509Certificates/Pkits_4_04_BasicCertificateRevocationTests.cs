//
// Pkits_4_04_BasicCertificateRevocationTests.cs -
//	NUnit tests for Pkits 4.4 : Basic Certificate Revocation Tests
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
	 * Notes
	 *
	 * [MS/XP][!RFC3280] Unknown critical extensions results in 
	 * RevocationStatusUnknown instead of Revoked - even if the CRL
	 * list the certificate serial number as revoked!
	 *
	 * [MS/XP][!RFC3280] Doesn't support having different keys for
	 * signing certificates and CRL.
	 *
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_04_BasicCertificateRevocationTests: PkitsTest {

		public X509Certificate2 NoCRLCACert {
			get { return GetCertificate ("NoCRLCACert.crt"); }
		}

		public X509Certificate2 RevokedsubCACert {
			get { return GetCertificate ("RevokedsubCACert.crt"); }
		}

		public X509Certificate2 BadCRLSignatureCACert {
			get { return GetCertificate ("BadCRLSignatureCACert.crt"); }
		}

		public X509Certificate2 BadCRLIssuerNameCACert {
			get { return GetCertificate ("BadCRLIssuerNameCACert.crt"); }
		}

		public X509Certificate2 WrongCRLCACert {
			get { return GetCertificate ("WrongCRLCACert.crt"); }
		}

		public X509Certificate2 TwoCRLsCACert {
			get { return GetCertificate ("TwoCRLsCACert.crt"); }
		}

		public X509Certificate2 UnknownCRLEntryExtensionCACert {
			get { return GetCertificate ("UnknownCRLEntryExtensionCACert.crt"); }
		}

		public X509Certificate2 UnknownCRLExtensionCACert {
			get { return GetCertificate ("UnknownCRLExtensionCACert.crt"); }
		}

		public X509Certificate2 OldCRLnextUpdateCACert {
			get { return GetCertificate ("OldCRLnextUpdateCACert.crt"); }
		}

		public X509Certificate2 Pre2000CRLnextUpdateCACert {
			get { return GetCertificate ("pre2000CRLnextUpdateCACert.crt"); }
		}

		public X509Certificate2 GeneralizedTimeCRLnextUpdateCACert {
			get { return GetCertificate ("GeneralizedTimeCRLnextUpdateCACert.crt"); }
		}

		public X509Certificate2 NegativeSerialNumberCACert {
			get { return GetCertificate ("NegativeSerialNumberCACert.crt"); }
		}

		public X509Certificate2 LongSerialNumberCACert {
			get { return GetCertificate ("LongSerialNumberCACert.crt"); }
		}

		public X509Certificate2 SeparateCertificateandCRLKeysCertificateSigningCACert {
			get { return GetCertificate ("SeparateCertificateandCRLKeysCertificateSigningCACert.crt"); }
		}

		public X509Certificate2 SeparateCertificateandCRLKeysCA2CertificateSigningCACert {
			get { return GetCertificate ("SeparateCertificateandCRLKeysCA2CertificateSigningCACert.crt"); }
		}

		[Test]
		public void T01_MissingCRL ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidMissingCRLTest1EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (NoCRLCACert, chain.ChainElements[1].Certificate, "NoCRLCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "NoCRLCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T02_InvalidRevokedCA ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidRevokedCATest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (RevokedsubCACert, chain.ChainElements[1].Certificate, "RevokedsubCACert");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[1].ChainElementStatus, "RevokedsubCACert.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[2].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T03_InvalidRevokedEE ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidRevokedEETest3EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T04_InvalidBadCrlSignature ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBadCRLSignatureTest4EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BadCRLSignatureCACert, chain.ChainElements[1].Certificate, "BadCRLSignatureCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BadCRLSignatureCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T05_InvalidBadCrlIssuerName ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBadCRLIssuerNameTest5EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BadCRLIssuerNameCACert, chain.ChainElements[1].Certificate, "BadCRLIssuerNameCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BadCRLIssuerNameCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T06_InvalidWrongCrl ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidWrongCRLTest6EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (WrongCRLCACert, chain.ChainElements[1].Certificate, "WrongCRLCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "WrongCRLCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T07_ValidTwoCrls ()
		{
			X509Certificate2 ee = GetCertificate ("ValidTwoCRLsTest7EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (TwoCRLsCACert, chain.ChainElements[1].Certificate, "TwoCRLsCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "TwoCRLsCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T08_InvalidUnknownCrlEntryExtension ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidUnknownCRLEntryExtensionTest8EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UnknownCRLEntryExtensionCACert, chain.ChainElements[1].Certificate, "UnknownCRLEntryExtensionCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UnknownCRLEntryExtensionCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T09_InvalidUnknownCrlExtension ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidUnknownCRLExtensionTest9EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UnknownCRLExtensionCACert, chain.ChainElements[1].Certificate, "UnknownCRLExtensionCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UnknownCRLExtensionCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T09_InvalidUnknownCrlExtension_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidUnknownCRLExtensionTest9EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			// MS-BAD - the certificate is REVOKED even if we don't completely understand
			// the critical extensions included in the certificate
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UnknownCRLExtensionCACert, chain.ChainElements[1].Certificate, "UnknownCRLExtensionCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UnknownCRLExtensionCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T10_InvalidUnknownCrlExtension ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidUnknownCRLExtensionTest10EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			// X.509.7.3 we should consider the EE as revoked (RevocationStatusUnknown seems fuzzy)
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (UnknownCRLExtensionCACert, chain.ChainElements[1].Certificate, "UnknownCRLExtensionCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "UnknownCRLExtensionCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T11_InvalidOldCrlNextUpdate ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidOldCRLnextUpdateTest11EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (OldCRLnextUpdateCACert, chain.ChainElements[1].Certificate, "OldCRLnextUpdateCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "OldCRLnextUpdateCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T12_InvalidPre2000CrlNextUpdate ()
		{
			X509Certificate2 ee = GetCertificate ("Invalidpre2000CRLnextUpdateTest12EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (Pre2000CRLnextUpdateCACert, chain.ChainElements[1].Certificate, "Pre2000CRLnextUpdateCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "Pre2000CRLnextUpdateCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T13_ValidGeneralizedTimeCrlNextUpdate ()
		{
			X509Certificate2 ee = GetCertificate ("ValidGeneralizedTimeCRLnextUpdateTest13EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GeneralizedTimeCRLnextUpdateCACert, chain.ChainElements[1].Certificate, "GeneralizedTimeCRLnextUpdateCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GeneralizedTimeCRLnextUpdateCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T14_ValidNegativeSerialNumber ()
		{
			X509Certificate2 ee = GetCertificate ("ValidNegativeSerialNumberTest14EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (NegativeSerialNumberCACert, chain.ChainElements[1].Certificate, "NegativeSerialNumberCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "NegativeSerialNumberCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T15_InvalidNegativeSerialNumber ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidNegativeSerialNumberTest15EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (NegativeSerialNumberCACert, chain.ChainElements[1].Certificate, "NegativeSerialNumberCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "NegativeSerialNumberCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T16_ValidLongSerialNumber ()
		{
			X509Certificate2 ee = GetCertificate ("ValidLongSerialNumberTest16EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (LongSerialNumberCACert, chain.ChainElements[1].Certificate, "LongSerialNumberCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "LongSerialNumberCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T17_ValidLongSerialNumber ()
		{
			X509Certificate2 ee = GetCertificate ("ValidLongSerialNumberTest17EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (LongSerialNumberCACert, chain.ChainElements[1].Certificate, "LongSerialNumberCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "LongSerialNumberCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T18_InvalidLongSerialNumber ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidLongSerialNumberTest18EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (LongSerialNumberCACert, chain.ChainElements[1].Certificate, "LongSerialNumberCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "LongSerialNumberCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T19_ValidSeparateCertificateAndCrlKeys ()
		{
			X509Certificate2 ee = GetCertificate ("ValidSeparateCertificateandCRLKeysTest19EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD - doesn't support different keys for signing certificates and CRL

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (SeparateCertificateandCRLKeysCertificateSigningCACert, chain.ChainElements[1].Certificate, "SeparateCertificateandCRLKeysCertificateSigningCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "SeparateCertificateandCRLKeysCertificateSigningCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T20_InvalidSeparateCertificateAndCrlKeys ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidSeparateCertificateandCRLKeysTest20EE.crt");
			X509Chain chain = new X509Chain ();
			// looks ok but in fact it's confused
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (SeparateCertificateandCRLKeysCertificateSigningCACert, chain.ChainElements[1].Certificate, "SeparateCertificateandCRLKeysCertificateSigningCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "SeparateCertificateandCRLKeysCertificateSigningCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T21_InvalidSeparateCertificateAndCrlKeys ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidSeparateCertificateandCRLKeysTest21EE.crt");
			X509Chain chain = new X509Chain ();
			// looks ok but in fact it's confused
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (SeparateCertificateandCRLKeysCA2CertificateSigningCACert, chain.ChainElements[1].Certificate, "SeparateCertificateandCRLKeysCA2CertificateSigningCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "SeparateCertificateandCRLKeysCA2CertificateSigningCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
