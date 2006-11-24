//
// X509ChainTest.cs - NUnit tests for X509Chain
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509ChainTest {

		private X509Certificate2Collection empty;
		private X509Certificate2Collection collection;

		private X509Certificate2 cert_empty;
		private X509Certificate2 cert1;
		private X509Certificate2 cert2;
		private X509Certificate2 cert3;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cert_empty = new X509Certificate2 ();
			cert1 = new X509Certificate2 (X509Certificate2Test.farscape_pfx, "farscape", X509KeyStorageFlags.Exportable);
			cert2 = new X509Certificate2 (Encoding.ASCII.GetBytes (X509Certificate2Test.base64_cert));

			empty = new X509Certificate2Collection ();
			collection = new X509Certificate2Collection ();
			collection.Add (cert1);
			collection.Add (cert2);
		}

		private void CheckDefaultChain (X509Chain c)
		{
			Assert.AreEqual (0, c.ChainElements.Count, "ChainElements");
			Assert.IsNotNull (c.ChainPolicy, "ChainPolicy");
			Assert.AreEqual (0, c.ChainStatus.Length, "ChainStatus");
			// check default policy
			CheckDefaultPolicy (c.ChainPolicy);
		}

		private void CheckDefaultPolicy (X509ChainPolicy p)
		{
			Assert.AreEqual (0, p.ApplicationPolicy.Count, "ApplicationPolicy");
			Assert.AreEqual (0, p.CertificatePolicy.Count, "CertificatePolicy");
			Assert.AreEqual (0, p.ExtraStore.Count, "ExtraStore");
			Assert.AreEqual (X509RevocationFlag.ExcludeRoot, p.RevocationFlag, "RevocationFlag");
			Assert.AreEqual (X509RevocationMode.Online, p.RevocationMode, "RevocationMode");
			Assert.AreEqual (0, p.UrlRetrievalTimeout.Ticks, "UrlRetrievalTimeout");
			Assert.AreEqual (X509VerificationFlags.NoFlag, p.VerificationFlags, "VerificationFlags");
			Assert.IsTrue (p.VerificationTime <= DateTime.Now, "VerificationTime");
		}

		[Test]
		public void ConstructorEmpty () 
		{
			X509Chain c = new X509Chain ();
			CheckDefaultChain (c);
		}

		[Test]
		public void ConstructorMachineContextFalse () 
		{
			X509Chain c = new X509Chain (false);
			CheckDefaultChain (c);
		}

		[Test]
		public void ConstructorMachineContextTrue () 
		{
			X509Chain c = new X509Chain (true);
			CheckDefaultChain (c);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Build_Null ()
		{
			X509Chain c = new X509Chain ();
			c.Build (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Build_Empty ()
		{
			X509Chain c = new X509Chain ();
			c.Build (cert_empty);
		}

		[Test]
		public void Build_Twice_SameCertificate ()
		{
			X509Chain c = new X509Chain ();
			Assert.IsFalse (c.Build (cert1), "Build-1a");
			Assert.IsFalse (c.Build (cert1), "Build-2a");
		}

		[Test]
		public void Build_Twice_DifferentCertificate ()
		{
			X509Chain c = new X509Chain ();
			Assert.IsFalse (c.Build (cert1), "Build-1");
			Assert.IsFalse (c.Build (cert2), "Build-2");
		}

		[Test]
		public void Build_Twice_WithReset ()
		{
			X509Chain c = new X509Chain ();
			Assert.IsFalse (c.Build (cert1), "Build-1");
			c.Reset ();
			Assert.IsFalse (c.Build (cert2), "Build-2");
			c.Reset ();
			CheckDefaultChain (c);
		}

		private void CheckCert1 (X509Chain c)
		{
			X509VerificationFlags success_mask;

			switch (c.ChainPolicy.RevocationMode) {
			case X509RevocationMode.Offline:
			case X509RevocationMode.Online:
				success_mask = X509VerificationFlags.IgnoreNotTimeValid | X509VerificationFlags.AllowUnknownCertificateAuthority;
				break;
			case X509RevocationMode.NoCheck:
			default:
				success_mask = X509VerificationFlags.AllowUnknownCertificateAuthority;
				break;
			}

			Assert.AreEqual (((c.ChainPolicy.VerificationFlags & success_mask) == success_mask), c.Build (cert1), "Build");
			Assert.AreEqual (1, c.ChainElements.Count, "ChainElements");
			Assert.AreEqual (String.Empty, c.ChainElements[0].Information, "ChainElements[0].Information");
			Assert.AreEqual (cert1, c.ChainElements[0].Certificate, "ChainElements[0].Certificate");

			switch (c.ChainPolicy.RevocationMode) {
			case X509RevocationMode.Offline:
			case X509RevocationMode.Online:
				Assert.AreEqual (2, c.ChainElements[0].ChainElementStatus.Length, "ChainElements[0].ChainElementStatus");
				Assert.AreEqual (X509ChainStatusFlags.RevocationStatusUnknown, c.ChainElements[0].ChainElementStatus[0].Status, "ChainElements[0].ChainElementStatus [0].Status");
				Assert.IsNotNull (c.ChainElements[0].ChainElementStatus[0].StatusInformation, "ChainElements[0].ChainElementStatus [0].StatusInformation");
				Assert.AreEqual (X509ChainStatusFlags.OfflineRevocation, c.ChainElements[0].ChainElementStatus[1].Status, "ChainElements[0].ChainElementStatus [1].Status");
				Assert.IsNotNull (c.ChainElements[0].ChainElementStatus[1].StatusInformation, "ChainElements[0].ChainElementStatus [1].StatusInformation");

				Assert.AreEqual (3, c.ChainStatus.Length, "ChainStatus");
				Assert.AreEqual (X509ChainStatusFlags.PartialChain, c.ChainStatus[0].Status, "Status-0");
				Assert.IsNotNull (c.ChainStatus[0].StatusInformation, "StatusInformation-0");
				Assert.AreEqual (X509ChainStatusFlags.RevocationStatusUnknown, c.ChainStatus[1].Status, "Status-1");
				Assert.IsNotNull (c.ChainStatus[1].StatusInformation, "StatusInformation-1");
				Assert.AreEqual (X509ChainStatusFlags.OfflineRevocation, c.ChainStatus[2].Status, "Status-2");
				Assert.IsNotNull (c.ChainStatus[2].StatusInformation, "StatusInformation-2");
				break;
			case X509RevocationMode.NoCheck:
				Assert.AreEqual (0, c.ChainElements[0].ChainElementStatus.Length, "ChainElements[0].ChainElementStatus");
				Assert.AreEqual (1, c.ChainStatus.Length, "ChainStatus");
				Assert.AreEqual (X509ChainStatusFlags.PartialChain, c.ChainStatus[0].Status, "Status-0");
				break;
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Build_Cert1_X509RevocationMode_Offline ()
		{
			X509Chain c = new X509Chain ();
			c.ChainPolicy.RevocationMode = X509RevocationMode.Offline; // default
			foreach (X509VerificationFlags vf in Enum.GetValues (typeof (X509VerificationFlags))) {
				c.ChainPolicy.VerificationFlags = vf;
				CheckCert1 (c);
				c.Reset ();
			}
		}

		[Test]
		[Category ("InetAccess")]
		[Category ("NotWorking")]
		public void Build_Cert1_X509RevocationMode_Online ()
		{
			X509Chain c = new X509Chain ();
			c.ChainPolicy.RevocationMode = X509RevocationMode.Online;
			foreach (X509VerificationFlags vf in Enum.GetValues (typeof (X509VerificationFlags))) {
				c.ChainPolicy.VerificationFlags = vf;
				CheckCert1 (c);
				c.Reset ();
			}
		}

		[Test]
		public void Build_Cert1_X509RevocationMode_NoCheck ()
		{
			X509Chain c = new X509Chain ();
			c.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
			foreach (X509VerificationFlags vf in Enum.GetValues (typeof (X509VerificationFlags))) {
				c.ChainPolicy.VerificationFlags = vf;
				CheckCert1 (c);
				c.Reset ();
			}
		}

		private void CheckCert2 (X509Chain c)
		{
			X509VerificationFlags success_mask = X509VerificationFlags.IgnoreNotTimeValid | X509VerificationFlags.AllowUnknownCertificateAuthority;
			Assert.AreEqual (((c.ChainPolicy.VerificationFlags & success_mask) == success_mask), c.Build (cert2), "Build");
			Assert.AreEqual (1, c.ChainElements.Count, "ChainElements");
			Assert.AreEqual (String.Empty, c.ChainElements[0].Information, "ChainElements[0].Information");
			Assert.AreEqual (cert2, c.ChainElements[0].Certificate, "ChainElements[0].Certificate");

			Assert.AreEqual (2, c.ChainElements[0].ChainElementStatus.Length, "ChainElements[0].ChainElementStatus");
			Assert.AreEqual (X509ChainStatusFlags.UntrustedRoot, c.ChainElements[0].ChainElementStatus[0].Status, "ChainElements[0].ChainElementStatus [0].Status");
			Assert.IsNotNull (c.ChainElements[0].ChainElementStatus[0].StatusInformation, "ChainElements[0].ChainElementStatus [0].StatusInformation");
			Assert.AreEqual (X509ChainStatusFlags.NotTimeValid, c.ChainElements[0].ChainElementStatus[1].Status, "ChainElements[0].ChainElementStatus [1].Status");
			Assert.IsNotNull (c.ChainElements[0].ChainElementStatus[1].StatusInformation, "ChainElements[0].ChainElementStatus [1].StatusInformation");

			Assert.AreEqual (2, c.ChainStatus.Length, "ChainStatus");
			Assert.AreEqual (X509ChainStatusFlags.UntrustedRoot, c.ChainStatus[0].Status, "Status-0");
			Assert.IsNotNull (c.ChainStatus[0].StatusInformation, "StatusInformation-0");
			Assert.AreEqual (X509ChainStatusFlags.NotTimeValid, c.ChainStatus[1].Status, "Status-1");
			Assert.IsNotNull (c.ChainStatus[1].StatusInformation, "StatusInformation-1");
		}

		[Test]
		public void Build_Cert2 ()
		{
			X509Chain c = new X509Chain ();
			foreach (X509VerificationFlags vf in Enum.GetValues (typeof (X509VerificationFlags))) {
				c.ChainPolicy.VerificationFlags = vf;
				CheckCert2 (c);
				c.Reset ();
			}
			// minimal criteria for success
			c.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid | X509VerificationFlags.AllowUnknownCertificateAuthority;
			CheckCert2 (c);
		}

		[Test]
		public void Reset ()
		{
			X509Chain c = new X509Chain ();
			c.ChainPolicy.ApplicationPolicy.Add (new Oid ("1.2.3"));
			c.ChainPolicy.CertificatePolicy.Add (new Oid ("1.2.4"));
			c.ChainPolicy.ExtraStore.AddRange (collection);
			c.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
			c.ChainPolicy.RevocationMode = X509RevocationMode.Offline;
			c.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (1000);
			c.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;
			c.ChainPolicy.VerificationTime = DateTime.MinValue;
			c.Reset ();
			// resetting the chain doesn't reset the policy
			Assert.AreEqual (1, c.ChainPolicy.ApplicationPolicy.Count, "ApplicationPolicy");
			Assert.AreEqual (1, c.ChainPolicy.CertificatePolicy.Count, "CertificatePolicy");
			Assert.AreEqual (2, c.ChainPolicy.ExtraStore.Count, "ExtraStore");
			Assert.AreEqual (X509RevocationFlag.EntireChain, c.ChainPolicy.RevocationFlag, "RevocationFlag");
			Assert.AreEqual (X509RevocationMode.Offline, c.ChainPolicy.RevocationMode, "RevocationMode");
			Assert.AreEqual (1000, c.ChainPolicy.UrlRetrievalTimeout.Ticks, "UrlRetrievalTimeout");
			Assert.AreEqual (X509VerificationFlags.IgnoreWrongUsage, c.ChainPolicy.VerificationFlags, "VerificationFlags");
			Assert.AreEqual (DateTime.MinValue, c.ChainPolicy.VerificationTime, "VerificationTime");
		}

		[Test]
		public void StaticCreation () 
		{
			X509Chain c = X509Chain.Create ();
			CheckDefaultChain (c);
		}

		[Test]
		public void CreateViaCryptoConfig ()
		{
			// X509Chain can be changed using CryptoConfig
			Assert.AreEqual (typeof (X509Chain), CryptoConfig.CreateFromName ("X509Chain").GetType (), "X509Chain");
			Assert.IsNull (CryptoConfig.CreateFromName ("System.Security.Cryptography.X509Certificates.X509Chain"), "System.Security.Cryptography.X509Certificates.X509Chain");
		}
	}
}

#endif
