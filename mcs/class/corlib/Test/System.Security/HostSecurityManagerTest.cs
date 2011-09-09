//
// HostSecurityManagerTest.cs - NUnit Test Cases for HostSecurityManager
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

// NET_2_1 profile lacks some (of the few) CAS features required to execute those tests
#if NET_2_0 && !NET_2_1

using NUnit.Framework;
using System;
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security {

	[TestFixture]
	public class HostSecurityManagerTest {

		[Test]
		public void Defaults ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			Assert.IsNull (hsm.DomainPolicy, "DomainPolicy");
			Assert.AreEqual (HostSecurityManagerOptions.AllFlags, hsm.Flags, "Flags");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DetermineApplicationTrust_Null_Evidence_TrustManagerContext ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			hsm.DetermineApplicationTrust (null, new Evidence (), new TrustManagerContext ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DetermineApplicationTrust_Evidence_Null_TrustManagerContext ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			hsm.DetermineApplicationTrust (new Evidence (), null, new TrustManagerContext ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DetermineApplicationTrust_Evidence_Evidence_Null ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			hsm.DetermineApplicationTrust (new Evidence (), new Evidence (), null);
		}

		[Test]
		public void ProvideAppDomainEvidence ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			Assert.IsNull (hsm.ProvideAppDomainEvidence (null), "null");

			Evidence e = new Evidence ();
			Evidence result = hsm.ProvideAppDomainEvidence (e);
			Assert.IsNotNull (result, "empty");
			Assert.AreEqual (0, result.Count, "Count-0");

			e.AddHost (new Zone (SecurityZone.Untrusted));
			result = hsm.ProvideAppDomainEvidence (e);
			Assert.AreEqual (1, result.Count, "Count-1");
		}

		[Test]
		public void ProvideAssemblyEvidence ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			Assembly a = Assembly.GetExecutingAssembly ();

			Evidence result = hsm.ProvideAssemblyEvidence (a, null);
			Assert.IsNull (result, "null");

			Evidence e = new Evidence ();
			result = hsm.ProvideAssemblyEvidence (a, e);
			Assert.AreEqual (0, result.Count, "Count-empty");

			e.AddHost (new Zone (SecurityZone.Untrusted));
			result = hsm.ProvideAssemblyEvidence (a, e);
			Assert.AreEqual (1, result.Count, "Count-1");
		}

		[Test]
		public void ProvideAssemblyEvidence_NullAssembly ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();

			Evidence result = hsm.ProvideAssemblyEvidence (null, null);
			Assert.IsNull (result, "null");

			Evidence e = new Evidence ();
			result = hsm.ProvideAssemblyEvidence (null, e);
			Assert.AreEqual (0, result.Count, "Count-empty");

			e.AddHost (new Zone (SecurityZone.Untrusted));
			result = hsm.ProvideAssemblyEvidence (null, e);
			Assert.AreEqual (1, result.Count, "Count-1");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ResolvePolicy_Null ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			PermissionSet ps = hsm.ResolvePolicy (null);
		}

		[Test]
		public void ResolvePolicy_Empty ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			PermissionSet ps = hsm.ResolvePolicy (new Evidence ());
			Assert.AreEqual (0, ps.Count, "Count");
			Assert.IsFalse (ps.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void ResolvePolicy_CurrentAssemblyEvidence ()
		{
			HostSecurityManager hsm = new HostSecurityManager ();
			Assembly a = Assembly.GetExecutingAssembly ();
			PermissionSet ps = hsm.ResolvePolicy (a.Evidence);

			PermissionSet expected = SecurityManager.ResolvePolicy (a.Evidence);
			Assert.AreEqual (expected.ToString (), ps.ToString (), "PermissionSet");
		}
	}
}

#endif
