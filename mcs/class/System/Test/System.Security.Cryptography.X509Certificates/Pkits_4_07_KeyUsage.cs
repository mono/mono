//
// Pkits_4_07_KeyUsage.cs -
//	NUnit tests for Pkits 4.7 : Key Usage
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
	 * [MS/XP][!RFC3280] keyUsage for cRLsign isn't checked
	 *
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_07_KeyUsage: PkitsTest {

		public X509Certificate2 KeyUsageCriticalkeyCertSignFalseCACert {
			get { return GetCertificate ("keyUsageCriticalkeyCertSignFalseCACert.crt"); }
		}

		public X509Certificate2 KeyUsageNotCriticalkeyCertSignFalseCACert {
			get { return GetCertificate ("keyUsageNotCriticalkeyCertSignFalseCACert.crt"); }
		}

		public X509Certificate2 KeyUsageNotCriticalCACert {
			get { return GetCertificate ("keyUsageNotCriticalCACert.crt"); }
		}

		public X509Certificate2 KeyUsageCriticalcRLSignFalseCACert {
			get { return GetCertificate ("keyUsageCriticalcRLSignFalseCACert.crt"); }
		}

		public X509Certificate2 KeyUsageNotCriticalcRLSignFalseCACert {
			get { return GetCertificate ("keyUsageNotCriticalcRLSignFalseCACert.crt"); }
		}

		[Test]
		public void T1_InvalidKeyUsageCriticalKeyCertSignFalse ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidkeyUsageCriticalkeyCertSignFalseTest1EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotValidForUsage, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			// INFO: keyUsage only has CrlSign (no KeyCertSign)
			// INFO: it's critical too but that doesn't change anything
			Assert.AreEqual (KeyUsageCriticalkeyCertSignFalseCACert, chain.ChainElements[1].Certificate, "KeyUsageCriticalkeyCertSignFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.NotValidForUsage, chain.ChainElements[1].ChainElementStatus, "KeyUsageCriticalkeyCertSignFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T2_InvalidKeyUsageNotCriticalKeyCertSignFalse ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidkeyUsageNotCriticalkeyCertSignFalseTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotValidForUsage, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			// INFO: keyUsage only has CrlSign (no KeyCertSign)
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (KeyUsageNotCriticalkeyCertSignFalseCACert, chain.ChainElements[1].Certificate, "KeyUsageNotCriticalkeyCertSignFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.NotValidForUsage, chain.ChainElements[1].ChainElementStatus, "KeyUsageNotCriticalkeyCertSignFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T3_ValidKeyUsageNotCritical ()
		{
			X509Certificate2 ee = GetCertificate ("ValidkeyUsageNotCriticalTest3EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (KeyUsageNotCriticalCACert, chain.ChainElements[1].Certificate, "KeyUsageNotCriticalCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "KeyUsageNotCriticalCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T4_InvalidKeyUsageCriticalCRLSignFalse ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidkeyUsageCriticalcRLSignFalseTest4EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: keyUsage doesn't allow CRL signature verification
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (KeyUsageCriticalcRLSignFalseCACert, chain.ChainElements[1].Certificate, "KeyUsageCriticalcRLSignFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "KeyUsageCriticalcRLSignFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T4_InvalidKeyUsageCriticalCRLSignFalse_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidkeyUsageCriticalcRLSignFalseTest4EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is NOT valid wrt RFC3280
			// keyUsage doesn't allow CRL signature verification

			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (KeyUsageCriticalcRLSignFalseCACert, chain.ChainElements[1].Certificate, "KeyUsageCriticalcRLSignFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "KeyUsageCriticalcRLSignFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")] // test case is RFC3280 compliant
		public void T5_InvalidKeyUsageNotCriticalCRLSignFalse ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidkeyUsageNotCriticalcRLSignFalseTest5EE.crt");
			X509Chain chain = new X509Chain ();
			// INFO: keyUsage doesn't allow CRL signature verification
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.RevocationStatusUnknown, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (KeyUsageNotCriticalcRLSignFalseCACert, chain.ChainElements[1].Certificate, "KeyUsageNotCriticalcRLSignFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "KeyUsageNotCriticalcRLSignFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T5_InvalidKeyUsageNotCriticalCRLSignFalse_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidkeyUsageNotCriticalcRLSignFalseTest5EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD / this is NOT valid wrt RFC3280
			// keyUsage doesn't allow CRL signature verification

			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (KeyUsageNotCriticalcRLSignFalseCACert, chain.ChainElements[1].Certificate, "KeyUsageNotCriticalcRLSignFalseCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "KeyUsageNotCriticalcRLSignFalseCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
