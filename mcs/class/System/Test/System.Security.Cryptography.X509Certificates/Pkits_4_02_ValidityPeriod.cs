//
// Pkits_4_02_ValidityPeriod.cs -
//	NUnit tests for Pkits 4.2 : Validity Period
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
	public class Pkits_4_02_ValidityPeriod: PkitsTest {

		public X509Certificate2 BadnotBeforeDateCACert {
			get { return GetCertificate ("BadnotBeforeDateCACert.crt"); }
		}

		public X509Certificate2 BadnotAfterDateCACert {
			get { return GetCertificate ("BadnotAfterDateCACert.crt"); }
		}

		[Test]
		public void T1_InvalidCAnotBeforeDate ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidCAnotBeforeDateTest1EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BadnotBeforeDateCACert, chain.ChainElements[1].Certificate, "BadnotBeforeDateCACert");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainElements[1].ChainElementStatus, "BadnotBeforeDateCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T2_InvalidEEnotBeforeDate ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidEEnotBeforeDateTest2EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T3_ValidPre2000UTCnotBeforeDate ()
		{
			X509Certificate2 ee = GetCertificate ("Validpre2000UTCnotBeforeDateTest3EE.crt");
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
		public void T4_ValidGeneralizedTimeNotBeforeDate ()
		{
			X509Certificate2 ee = GetCertificate ("ValidGeneralizedTimenotBeforeDateTest4EE.crt");
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
		public void T5_InvalidCAnotAfterDate ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidCAnotAfterDateTest5EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (BadnotAfterDateCACert, chain.ChainElements[1].Certificate, "BadnotAfterDateCACert");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainElements[1].ChainElementStatus, "BadnotAfterDateCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T6_InvalidEEnotAfterDate ()
		{
			X509Certificate2 ee = GetCertificate ("InvalidEEnotAfterDateTest6EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T7_InvalidPre2000UTCnotAfterDate ()
		{
			X509Certificate2 ee = GetCertificate ("Invalidpre2000UTCEEnotAfterDateTest7EE.crt");
			X509Chain chain = new X509Chain ();
			Assert.IsFalse (chain.Build (ee), "Build");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainStatus, "ChainStatus");
			Assert.AreEqual (ee, chain.ChainElements[0].Certificate, "EndEntity");
			CheckChainStatus (X509ChainStatusFlags.NotTimeValid, chain.ChainElements[0].ChainElementStatus, "EndEntity.Status");
			Assert.AreEqual (GoodCACert, chain.ChainElements[1].Certificate, "GoodCACert");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[1].ChainElementStatus, "GoodCACert.Status");
			Assert.AreEqual (TrustAnchorRoot, chain.ChainElements[2].Certificate, "TrustAnchorRoot");
			CheckChainStatus (X509ChainStatusFlags.NoError, chain.ChainElements[2].ChainElementStatus, "TrustAnchorRoot.Status");
		}

		[Test]
		public void T8_ValidGeneralizedTimeNotAfterDate ()
		{
			X509Certificate2 ee = GetCertificate ("ValidGeneralizedTimenotAfterDateTest8EE.crt");
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
	}
}

#endif