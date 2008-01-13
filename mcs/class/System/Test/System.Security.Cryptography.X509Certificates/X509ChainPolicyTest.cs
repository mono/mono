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

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509ChainPolicyTest : Assertion {

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
			AssertEquals ("ApplicationPolicy", 0, cp.ApplicationPolicy.Count);
			AssertEquals ("CertificatePolicy", 0, cp.CertificatePolicy.Count);
			AssertEquals ("ExtraStore", 0, cp.ExtraStore.Count);
			AssertEquals ("RevocationFlag", X509RevocationFlag.ExcludeRoot, cp.RevocationFlag);
			AssertEquals ("RevocationMode", X509RevocationMode.Online, cp.RevocationMode);
			AssertEquals ("UrlRetrievalTimeout", 0, cp.UrlRetrievalTimeout.Ticks);
			AssertEquals ("VerificationFlags", X509VerificationFlags.NoFlag, cp.VerificationFlags);
			DateTime vt = cp.VerificationTime;
			Assert ("VerificationTime", ((vt <= DateTime.Now) && (vt > DateTime.Now.AddMinutes (-1))));
		}

		[Test]
		public void ApplicationPolicy () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ApplicationPolicy.Add (new Oid (signingTimeOid));
			AssertEquals ("ApplicationPolicy", 1, cp.ApplicationPolicy.Count);
		}

		[Test]
		public void ApplicationPolicy_Reset ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ApplicationPolicy.Add (new Oid (signingTimeOid));
			OidCollection oc = cp.ApplicationPolicy;
			AssertEquals ("ApplicationPolicy-1", 1, oc.Count);
			cp.Reset ();
			AssertEquals ("ApplicationPolicy-2", 1, oc.Count);
			AssertEquals ("ApplicationPolicy-3", 0, cp.ApplicationPolicy.Count);
		}

		[Test]
		public void CertificatePolicy () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			AssertEquals ("CertificatePolicy", 1, cp.CertificatePolicy.Count);
		}

		[Test]
		public void CertificatePolicy_Reset ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			OidCollection oc = cp.CertificatePolicy;
			AssertEquals ("CertificatePolicy-1", 1, oc.Count);
			cp.Reset ();
			AssertEquals ("CertificatePolicy-2", 1, oc.Count);
			AssertEquals ("CertificatePolicy-3", 0, cp.CertificatePolicy.Count);
		}

		[Test]
		public void ExtraStore () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ExtraStore.Add (new X509Certificate2 ());
			AssertEquals ("ExtraStore", 1, cp.ExtraStore.Count);
		}

		[Test]
		public void ExtraStore_Reset ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ExtraStore.Add (new X509Certificate2 ());
			X509Certificate2Collection cc = cp.ExtraStore;
			AssertEquals ("ExtraStore-1", 1, cc.Count);
			cp.Reset ();
			AssertEquals ("ExtraStore-2", 1, cc.Count);
			AssertEquals ("ExtraStore-3", 0, cp.ExtraStore.Count);
		}

		[Test]
		public void RevocationFlag () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.RevocationFlag = X509RevocationFlag.EndCertificateOnly;
			AssertEquals ("EndCertificateOnly", X509RevocationFlag.EndCertificateOnly, cp.RevocationFlag);
			cp.RevocationFlag = X509RevocationFlag.EntireChain;
			AssertEquals ("EntireChain", X509RevocationFlag.EntireChain, cp.RevocationFlag);
			cp.RevocationFlag = X509RevocationFlag.ExcludeRoot;
			AssertEquals ("ExcludeRoot", X509RevocationFlag.ExcludeRoot, cp.RevocationFlag);
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
			AssertEquals ("NoCheck", X509RevocationMode.NoCheck, cp.RevocationMode);
			cp.RevocationMode = X509RevocationMode.Offline;
			AssertEquals ("Offline", X509RevocationMode.Offline, cp.RevocationMode);
			cp.RevocationMode = X509RevocationMode.Online;
			AssertEquals ("Online", X509RevocationMode.Online, cp.RevocationMode);
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
			AssertEquals ("TimeSpan=100", 100, cp.UrlRetrievalTimeout.Ticks);
			cp.UrlRetrievalTimeout = new TimeSpan (0);
			AssertEquals ("TimeSpan=0", 0, cp.UrlRetrievalTimeout.Ticks);
			cp.UrlRetrievalTimeout = TimeSpan.MinValue;
			AssertEquals ("TimeSpan=MinValue", TimeSpan.MinValue, cp.UrlRetrievalTimeout);
			cp.UrlRetrievalTimeout = TimeSpan.MaxValue;
			AssertEquals ("TimeSpan=MaxValue", TimeSpan.MaxValue, cp.UrlRetrievalTimeout);
		}

		[Test]
		public void VerificationFlags () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.VerificationFlags = X509VerificationFlags.AllFlags;
			AssertEquals ("AllFlags", X509VerificationFlags.AllFlags, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
			AssertEquals ("AllowUnknownCertificateAuthority", X509VerificationFlags.AllowUnknownCertificateAuthority, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown;
			AssertEquals ("IgnoreCertificateAuthorityRevocationUnknown", X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreCtlNotTimeValid;
			AssertEquals ("IgnoreCtlNotTimeValid", X509VerificationFlags.IgnoreCtlNotTimeValid, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreCtlSignerRevocationUnknown;
			AssertEquals ("IgnoreCtlSignerRevocationUnknown", X509VerificationFlags.IgnoreCtlSignerRevocationUnknown, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown;
			AssertEquals ("IgnoreEndRevocationUnknown", X509VerificationFlags.IgnoreEndRevocationUnknown, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreInvalidBasicConstraints;
			AssertEquals ("IgnoreInvalidBasicConstraints", X509VerificationFlags.IgnoreInvalidBasicConstraints, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreInvalidName;
			AssertEquals ("IgnoreInvalidName", X509VerificationFlags.IgnoreInvalidName, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreInvalidPolicy;
			AssertEquals ("IgnoreInvalidPolicy", X509VerificationFlags.IgnoreInvalidPolicy, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreNotTimeNested;
			AssertEquals ("IgnoreNotTimeNested", X509VerificationFlags.IgnoreNotTimeNested, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid;
			AssertEquals ("IgnoreNotTimeValid", X509VerificationFlags.IgnoreNotTimeValid, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreRootRevocationUnknown;
			AssertEquals ("IgnoreRootRevocationUnknown", X509VerificationFlags.IgnoreRootRevocationUnknown, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;
			AssertEquals ("IgnoreWrongUsage", X509VerificationFlags.IgnoreWrongUsage, cp.VerificationFlags);
			cp.VerificationFlags = X509VerificationFlags.NoFlag;
			AssertEquals ("NoFlag", X509VerificationFlags.NoFlag, cp.VerificationFlags);
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
			AssertEquals ("Kind=Local", DateTimeKind.Local, cp.VerificationTime.Kind);
			cp.VerificationTime = DateTime.Today;
			AssertEquals ("DateTime=Today", DateTime.Today, cp.VerificationTime);
			cp.VerificationTime = new DateTime (0);
			AssertEquals ("DateTime=0", 0, cp.VerificationTime.Ticks);
			cp.VerificationTime = DateTime.MinValue;
			AssertEquals ("DateTime=MinValue", DateTime.MinValue, cp.VerificationTime);
			cp.VerificationTime = DateTime.MaxValue;
			AssertEquals ("DateTime=MaxValue", DateTime.MaxValue, cp.VerificationTime);
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
			Assert ("VerificationTime", ((vt <= DateTime.Now) && (vt > DateTime.Now.AddMinutes (-1))));
			cp.Reset ();
			Assert ("VerificationTime-Reset", (vt != cp.VerificationTime));
			// default properties
			AssertEquals ("ApplicationPolicy", 0, cp.ApplicationPolicy.Count);
			AssertEquals ("CertificatePolicy", 0, cp.CertificatePolicy.Count);
			AssertEquals ("ExtraStore", 0, cp.ExtraStore.Count);
			AssertEquals ("RevocationFlag", X509RevocationFlag.ExcludeRoot, cp.RevocationFlag);
			AssertEquals ("RevocationMode", X509RevocationMode.Online, cp.RevocationMode);
			AssertEquals ("UrlRetrievalTimeout", 0, cp.UrlRetrievalTimeout.Ticks);
			AssertEquals ("VerificationFlags", X509VerificationFlags.NoFlag, cp.VerificationFlags);
			vt = cp.VerificationTime;
			Assert ("VerificationTime", ((vt <= DateTime.Now) && (vt > DateTime.Now.AddMinutes (-1))));
		}
	}
}

#endif
