//
// Pkits_4_16_PrivateCertificateExtensions.cs -
//	NUnit tests for Pkits 4.16 : PrivateCertificateExtensions
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
	 * [MS/XP][!RFC3280] Unknown critical extensions are ignored.
	 *
	 * See PkitsTest.cs for more details
	 */

	[TestFixture]
	[Category ("PKITS")]
	public class Pkits_4_16_PrivateCertificateExtensions: PkitsTest {

		[Test]
		public void T1_ValidUnknownNotCriticalCertificateExtension ()
		{
			X509Certificate2 ee = GetCertificate ("ValidUnknownNotCriticalCertificateExtensionTest1EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[1].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotDotNet")]
		public void T2_InvalidUnknownCriticalCertificateExtension ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidUnknownCriticalCertificateExtensionTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.InvalidExtension, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.InvalidExtension, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[1].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		[Category ("NotWorking")] // WONTFIX - this isn't RFC3280 compliant
		public void T2_InvalidUnknownCriticalCertificateExtension_MS ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidUnknownCriticalCertificateExtensionTest2EE.crt");
			X509Chain chain = new X509Chain ();

			// MS-BAD [XP] / this is NOT valid wrt RFC3280
			// Unknown CRITICAL extensions should not success!

			Assert.IsTrue (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[1].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "TrustAnchorRoot.Status");
		}
	}
}

#endif
