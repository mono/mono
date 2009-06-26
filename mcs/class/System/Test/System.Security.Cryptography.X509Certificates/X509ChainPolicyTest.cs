//
// X509ChainPolicyTest.cs - NUnit tests for X509ChainPolicy
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
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509ChainPolicyTest {

		static string signingTimeOid = "1.2.840.113549.1.9.5";

		private X509ChainPolicy GetPolicy () 
		{
			X509Chain c = new X509Chain ();
			return c.ChainPolicy;
		}

		[Test]
		public void Default () 
		{
			X509ChainPolicy cp = GetPolicy ();
			// default properties
			Assert.AreEqual (0, cp.ApplicationPolicy.Count, "ApplicationPolicy");
			Assert.AreEqual (0, cp.CertificatePolicy.Count, "CertificatePolicy");
			Assert.AreEqual (0, cp.ExtraStore.Count, "ExtraStore");
			Assert.AreEqual (X509RevocationFlag.ExcludeRoot, cp.RevocationFlag, "RevocationFlag");
			Assert.AreEqual (X509RevocationMode.Online, cp.RevocationMode, "RevocationMode");
			Assert.AreEqual (0, cp.UrlRetrievalTimeout.Ticks, "UrlRetrievalTimeout");
			Assert.AreEqual (X509VerificationFlags.NoFlag, cp.VerificationFlags, "VerificationFlags");
			DateTime vt = cp.VerificationTime;
			Assert.IsTrue (((vt <= DateTime.Now) && (vt > DateTime.Now.AddMinutes (-1))), "VerificationTime");
		}

		[Test]
		public void ApplicationPolicy () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ApplicationPolicy.Add (new Oid (signingTimeOid));
			Assert.AreEqual (1, cp.ApplicationPolicy.Count, "ApplicationPolicy");
		}

		[Test]
		public void ApplicationPolicy_Reset ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ApplicationPolicy.Add (new Oid (signingTimeOid));
			OidCollection oc = cp.ApplicationPolicy;
			Assert.AreEqual (1, oc.Count, "ApplicationPolicy-1");
			cp.Reset ();
			Assert.AreEqual (1, oc.Count, "ApplicationPolicy-2");
			Assert.AreEqual (0, cp.ApplicationPolicy.Count, "ApplicationPolicy-3");
		}

		[Test]
		public void CertificatePolicy () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			Assert.AreEqual (1, cp.CertificatePolicy.Count, "CertificatePolicy");
		}

		[Test]
		public void CertificatePolicy_Reset ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			OidCollection oc = cp.CertificatePolicy;
			Assert.AreEqual (1, oc.Count, "CertificatePolicy-1");
			cp.Reset ();
			Assert.AreEqual (1, oc.Count, "CertificatePolicy-2");
			Assert.AreEqual (0, cp.CertificatePolicy.Count, "CertificatePolicy-3");
		}

		[Test]
		public void ExtraStore () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ExtraStore.Add (new X509Certificate2 ());
			Assert.AreEqual (1, cp.ExtraStore.Count, "ExtraStore");
		}

		[Test]
		public void ExtraStore_Reset ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ExtraStore.Add (new X509Certificate2 ());
			X509Certificate2Collection cc = cp.ExtraStore;
			Assert.AreEqual (1, cc.Count, "ExtraStore-1");
			cp.Reset ();
			Assert.AreEqual (1, cc.Count, "ExtraStore-2");
			Assert.AreEqual (0, cp.ExtraStore.Count, "ExtraStore-3");
		}

		[Test]
		public void RevocationFlag () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.RevocationFlag = X509RevocationFlag.EndCertificateOnly;
			Assert.AreEqual (X509RevocationFlag.EndCertificateOnly, cp.RevocationFlag, "EndCertificateOnly");
			cp.RevocationFlag = X509RevocationFlag.EntireChain;
			Assert.AreEqual (X509RevocationFlag.EntireChain, cp.RevocationFlag, "EntireChain");
			cp.RevocationFlag = X509RevocationFlag.ExcludeRoot;
			Assert.AreEqual (X509RevocationFlag.ExcludeRoot, cp.RevocationFlag, "ExcludeRoot");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RevocationFlag_Invalid ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.RevocationFlag = (X509RevocationFlag) Int32.MinValue;
		}

		[Test]
		public void RevocationMode () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.RevocationMode = X509RevocationMode.NoCheck;
			Assert.AreEqual (X509RevocationMode.NoCheck, cp.RevocationMode, "NoCheck");
			cp.RevocationMode = X509RevocationMode.Offline;
			Assert.AreEqual (X509RevocationMode.Offline, cp.RevocationMode, "Offline");
			cp.RevocationMode = X509RevocationMode.Online;
			Assert.AreEqual (X509RevocationMode.Online, cp.RevocationMode, "Online");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RevocationMode_Invalid ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.RevocationMode = (X509RevocationMode) Int32.MinValue;
		}

		[Test]
		public void UrlRetrievalTimeout ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.UrlRetrievalTimeout = new TimeSpan (100);
			Assert.AreEqual (100, cp.UrlRetrievalTimeout.Ticks, "TimeSpan=100");
			cp.UrlRetrievalTimeout = new TimeSpan (0);
			Assert.AreEqual (0, cp.UrlRetrievalTimeout.Ticks, "TimeSpan=0");
			cp.UrlRetrievalTimeout = TimeSpan.MinValue;
			Assert.AreEqual (TimeSpan.MinValue, cp.UrlRetrievalTimeout, "TimeSpan=MinValue");
			cp.UrlRetrievalTimeout = TimeSpan.MaxValue;
			Assert.AreEqual (TimeSpan.MaxValue, cp.UrlRetrievalTimeout, "TimeSpan=MaxValue");
		}

		[Test]
		public void VerificationFlags () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.VerificationFlags = X509VerificationFlags.AllFlags;
			Assert.AreEqual (X509VerificationFlags.AllFlags, cp.VerificationFlags, "AllFlags");
			cp.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
			Assert.AreEqual (X509VerificationFlags.AllowUnknownCertificateAuthority, cp.VerificationFlags, "AllowUnknownCertificateAuthority");
			cp.VerificationFlags = X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown;
			Assert.AreEqual (X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown, cp.VerificationFlags, "IgnoreCertificateAuthorityRevocationUnknown");
			cp.VerificationFlags = X509VerificationFlags.IgnoreCtlNotTimeValid;
			Assert.AreEqual (X509VerificationFlags.IgnoreCtlNotTimeValid, cp.VerificationFlags, "IgnoreCtlNotTimeValid");
			cp.VerificationFlags = X509VerificationFlags.IgnoreCtlSignerRevocationUnknown;
			Assert.AreEqual (X509VerificationFlags.IgnoreCtlSignerRevocationUnknown, cp.VerificationFlags, "IgnoreCtlSignerRevocationUnknown");
			cp.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			Assert.AreEqual (X509VerificationFlags.IgnoreEndRevocationUnknown, cp.VerificationFlags, "IgnoreEndRevocationUnknown");
			cp.VerificationFlags = X509VerificationFlags.IgnoreInvalidBasicConstraints;
			Assert.AreEqual (X509VerificationFlags.IgnoreInvalidBasicConstraints, cp.VerificationFlags, "IgnoreInvalidBasicConstraints");
			cp.VerificationFlags = X509VerificationFlags.IgnoreInvalidName;
			Assert.AreEqual (X509VerificationFlags.IgnoreInvalidName, cp.VerificationFlags, "IgnoreInvalidName");
			cp.VerificationFlags = X509VerificationFlags.IgnoreInvalidPolicy;
			Assert.AreEqual (X509VerificationFlags.IgnoreInvalidPolicy, cp.VerificationFlags, "IgnoreInvalidPolicy");
			cp.VerificationFlags = X509VerificationFlags.IgnoreNotTimeNested;
			Assert.AreEqual (X509VerificationFlags.IgnoreNotTimeNested, cp.VerificationFlags, "IgnoreNotTimeNested");
			cp.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid;
			Assert.AreEqual (X509VerificationFlags.IgnoreNotTimeValid, cp.VerificationFlags, "IgnoreNotTimeValid");
			cp.VerificationFlags = X509VerificationFlags.IgnoreRootRevocationUnknown;
			Assert.AreEqual (X509VerificationFlags.IgnoreRootRevocationUnknown, cp.VerificationFlags, "IgnoreRootRevocationUnknown");
			cp.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;
			Assert.AreEqual (X509VerificationFlags.IgnoreWrongUsage, cp.VerificationFlags, "IgnoreWrongUsage");
			cp.VerificationFlags = X509VerificationFlags.NoFlag;
			Assert.AreEqual (X509VerificationFlags.NoFlag, cp.VerificationFlags, "NoFlag");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void VerificationFlags_Invalid ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.VerificationFlags = (X509VerificationFlags)Int32.MinValue;
		}

		[Test]
		public void VerificationTime ()
		{
			X509ChainPolicy cp = GetPolicy ();
			Assert.AreEqual (DateTimeKind.Local, cp.VerificationTime.Kind, "Kind=Local");
			cp.VerificationTime = DateTime.Today;
			Assert.AreEqual (DateTime.Today, cp.VerificationTime, "DateTime=Today");
			cp.VerificationTime = new DateTime (0);
			Assert.AreEqual (0, cp.VerificationTime.Ticks, "DateTime=0");
			cp.VerificationTime = DateTime.MinValue;
			Assert.AreEqual (DateTime.MinValue, cp.VerificationTime, "DateTime=MinValue");
			cp.VerificationTime = DateTime.MaxValue;
			Assert.AreEqual (DateTime.MaxValue, cp.VerificationTime, "DateTime=MaxValue");
		}

		[Test]
		public void Reset () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ApplicationPolicy.Add (new Oid (signingTimeOid));
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			cp.ExtraStore.Add (new X509Certificate2 ());
			cp.RevocationFlag = X509RevocationFlag.EndCertificateOnly;
			cp.RevocationMode = X509RevocationMode.NoCheck;
			cp.UrlRetrievalTimeout = new TimeSpan (100);
			cp.VerificationFlags = X509VerificationFlags.AllFlags;
			DateTime vt = cp.VerificationTime;
			Assert.IsTrue (((vt <= DateTime.Now) && (vt > DateTime.Now.AddMinutes (-1))), "VerificationTime");
			// wait a bit before calling Reset, otherwise we could end up with the same time value
			Thread.Sleep (100);
			cp.Reset ();
			Assert.IsTrue ((vt != cp.VerificationTime), "VerificationTime-Reset");
			// default properties
			Assert.AreEqual (0, cp.ApplicationPolicy.Count, "ApplicationPolicy");
			Assert.AreEqual (0, cp.CertificatePolicy.Count, "CertificatePolicy");
			Assert.AreEqual (0, cp.ExtraStore.Count, "ExtraStore");
			Assert.AreEqual (X509RevocationFlag.ExcludeRoot, cp.RevocationFlag, "RevocationFlag");
			Assert.AreEqual (X509RevocationMode.Online, cp.RevocationMode, "RevocationMode");
			Assert.AreEqual (0, cp.UrlRetrievalTimeout.Ticks, "UrlRetrievalTimeout");
			Assert.AreEqual (X509VerificationFlags.NoFlag, cp.VerificationFlags, "VerificationFlags");
			vt = cp.VerificationTime;
			Assert.IsTrue (((vt <= DateTime.Now) && (vt > DateTime.Now.AddMinutes (-1))), "VerificationTime");
		}
	}
}

#endif
