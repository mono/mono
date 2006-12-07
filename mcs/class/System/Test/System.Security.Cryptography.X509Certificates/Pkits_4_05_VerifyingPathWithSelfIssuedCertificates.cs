//
// Pkits_4_05_VerifyingPathWithSelfIssuedCertificates.cs -
//	NUnit tests for Pkits 4.5 : Verifying Path With Self Issued Certificates
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
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_05_VerifyingPathWithSelfIssuedCertificates: PkitsTest {

		// TODO - incomplete

		public X509Certificate2 BasicSelfIssuedNewKeyCACert {
			get { return GetCertificate ("BasicSelfIssuedNewKeyCACert.crt"); }
		}

		public X509Certificate2 BasicSelfIssuedNewKeyOldWithNewCACert {
			get { return GetCertificate ("BasicSelfIssuedNewKeyOldWithNewCACert.crt"); }
		}

		public X509Certificate2 BasicSelfIssuedOldKeyCACert {
			get { return GetCertificate ("BasicSelfIssuedOldKeyCACert.crt"); }
		}

		public X509Certificate2 BasicSelfIssuedOldKeyNewWithOldCACert {
			get { return GetCertificate ("BasicSelfIssuedOldKeyNewWithOldCACert.crt"); }
		}

		public X509Certificate2 BasicSelfIssuedCRLSigningKeyCACert {
			get { return GetCertificate ("BasicSelfIssuedCRLSigningKeyCACert.crt"); }
		}

		public X509Certificate2 BasicSelfIssuedCRLSigningKeyCRLCert {
			get { return GetCertificate ("BasicSelfIssuedCRLSigningKeyCRLCert.crt"); }
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T1_ValidBasicSelfIssuedOldWithNew ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedOldWithNewTest1EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedNewKeyOldWithNewCACert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedNewKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T1_ValidBasicSelfIssuedOldWithNew_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedOldWithNewTest1EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T2_InvalidBasicSelfIssuedOldWithNew ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedOldWithNewTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			// certificate is revoked
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedNewKeyOldWithNewCACert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedNewKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T2_InvalidBasicSelfIssuedOldWithNew_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedOldWithNewTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");

			// MS-BAD / this is valid wrt RFC3280
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T3_ValidBasicSelfIssuedNewWithOld ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedNewWithOldTest3EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedOldKeyNewWithOldCACert, chain.ChainElements[1].Certificate, "BasicSelfIssuedOldKeyNewWithOldCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedOldKeyNewWithOldCACert.Status");
			Assert.AreEqual (BasicSelfIssuedOldKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedOldKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedOldKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T3_ValidBasicSelfIssuedNewWithOld_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedNewWithOldTest3EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");

			// MS-BAD / this is valid wrt RFC3280
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		[Category ("NotWorking")] // Mono doesn't support using a different CA to sign CRL
		public void T4_ValidBasicSelfIssuedNewWithOld ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedNewWithOldTest4EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedOldKeyNewWithOldCACert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedOldKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T4_ValidBasicSelfIssuedNewWithOld_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedNewWithOldTest4EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		[Category ("NotWorking")] // Mono doesn't support using a different CA to sign CRL
		public void T5_InvalidBasicSelfIssuedNewWithOld ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedNewWithOldTest5EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedOldKeyNewWithOldCACert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedOldKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T5_InvalidBasicSelfIssuedNewWithOld_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedNewWithOldTest5EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			// EE certificate has been revoked

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		[Category ("NotWorking")] // Mono doesn't support using a different CA to sign CRL
		public void T6_ValidBasicSelfIssuedCRLSigningKey ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedCRLSigningKeyTest6EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedCRLSigningKeyCRLCert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedCRLSigningKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T6_ValidBasicSelfIssuedCRLSigningKey_MS ()
		{
			X509Certificate2 ee = GetCertificate ("ValidBasicSelfIssuedCRLSigningKeyTest6EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		[Category ("NotWorking")] // Mono doesn't support using a different CA to sign CRL
		public void T7_InvalidBasicSelfIssuedCRLSigningKey ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedCRLSigningKeyTest7EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.Revoked, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedCRLSigningKeyCRLCert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedCRLSigningKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T7_InvalidBasicSelfIssuedCRLSigningKey_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedCRLSigningKeyTest7EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is valid wrt RFC3280
			// EE certificate has been revoked

			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// Chain order is bad - it's not worth checking further
		}

		[Test]
		public void T8_InvalidBasicSelfIssuedCRLSigningKey ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidBasicSelfIssuedCRLSigningKeyTest8EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotValidForUsage | X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// hmmm... NoError ?
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BasicSelfIssuedCRLSigningKeyCRLCert, chain.ChainElements[1].Certificate, "BasicSelfIssuedNewKeyOldWithNewCACert");
			CheckChainStatus (X509ChainStatusFlags.NotValidForUsage | X509ChainStatusFlags.InvalidBasicConstraints, chain.ChainElements[1].ChainElementStatus, "BasicSelfIssuedNewKeyOldWithNewCACert.Status");
			Assert.AreEqual (BasicSelfIssuedCRLSigningKeyCACert, chain.ChainElements[2].Certificate, "BasicSelfIssuedNewKeyCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "BasicSelfIssuedNewKeyCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
