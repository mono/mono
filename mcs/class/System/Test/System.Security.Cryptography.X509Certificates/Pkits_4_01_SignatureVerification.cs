//
// Pkits_4_01_SignatureVerification.cs -
//	NUnit tests for Pkits 4.1 : Signature Verification
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
	 * [MS/XP] Everything looks to be RFC3280 compliant.
	 *
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_01_SignatureVerification: PkitsTest {

		public X509Certificate2 BadSignedCACert {
			get { return GetCertificate ("BadSignedCACert.crt"); }
		}

		public X509Certificate2 DSACACert {
			get { return GetCertificate ("DSACACert.crt"); }
		}

		public X509Certificate2 DSAParametersInheritedCACert {
			get { return GetCertificate ("DSAParametersInheritedCACert.crt"); }
		}

		[Test]
		public void T1_ValidSignature ()
		{
			X509Certificate2 ee = GetCertificate ("ValidCertificatePathTest1EE.crt");
			X509Chain chain = new X509Chain ();
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
		public void T2_InvalidCASignature ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidCASignatureTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotSignatureValid | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BadSignedCACert, chain.ChainElements[1].Certificate, "BadSignedCACert");
			CheckChainStatus (X509ChainStatusFlags.NotSignatureValid | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[1].ChainElementStatus, "BadSignedCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T3_InvalidEESignature ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidEESignatureTest3EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotSignatureValid | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NotSignatureValid | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T4_ValidDSASignatures ()
		{
			X509Certificate2 ee = GetCertificate ("ValidDSASignaturesTest4EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			Assert.AreEqual (0, chain.ChainStatus.Length, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (DSACACert, chain.ChainElements[1].Certificate, "DSACACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "DSACACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T5_ValidDSAParameterInheritance ()
		{
			X509Certificate2 ee = GetCertificate ("ValidDSAParameterInheritanceTest5EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (DSAParametersInheritedCACert, chain.ChainElements[1].Certificate, "DSAParametersInheritedCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "DSAParametersInheritedCACert.Status");
			Assert.AreEqual (DSACACert, chain.ChainElements[2].Certificate, "DSACACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "DSACACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[3].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[3].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T6_InvalidDSASignatures ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidDSASignatureTest6EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotSignatureValid | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NotSignatureValid | X509ChainStatusFlags.RevocationStatusUnknown | X509ChainStatusFlags.OfflineRevocation, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (DSACACert, chain.ChainElements[1].Certificate, "DSACACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "DSACACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
