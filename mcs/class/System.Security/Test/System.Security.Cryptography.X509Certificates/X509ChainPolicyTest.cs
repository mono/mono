//
// X509ChainPolicyTest.cs - NUnit tests for X509ChainPolicy
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

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
		public void CertificatePolicy () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			AssertEquals ("CertificatePolicy", 1, cp.CertificatePolicy.Count);
		}

		[Test]
		public void ExtraStore () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ExtraStore.Add (new X509CertificateEx ());
			AssertEquals ("ExtraStore", 1, cp.ExtraStore.Count);
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
		public void UrlRetrievalTimeout ()
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.UrlRetrievalTimeout = new TimeSpan (100);
			AssertEquals ("TimeSpan=100", 100, cp.UrlRetrievalTimeout.Ticks);
			cp.UrlRetrievalTimeout = new TimeSpan (0);
			AssertEquals ("TimeSpan=0", 0, cp.UrlRetrievalTimeout.Ticks);
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
		public void Reset () 
		{
			X509ChainPolicy cp = GetPolicy ();
			cp.ApplicationPolicy.Add (new Oid (signingTimeOid));
			cp.CertificatePolicy.Add (new Oid (signingTimeOid));
			cp.ExtraStore.Add (new X509CertificateEx ());
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
