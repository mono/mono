//
// SecurityManagerTest.cs - NUnit Test Cases for SecurityManager
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecurityManagerTest {

		static Evidence CurrentEvidence;

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			CurrentEvidence = Assembly.GetExecutingAssembly ().Evidence;
		}

		[TearDown]
		public void TearDown ()
		{
			SecurityManager.CheckExecutionRights = true;
		}

		[Test]
		public void IsGranted_Null ()
		{
			// null is always granted
			Assert.IsTrue (SecurityManager.IsGranted (null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadPolicyLevelFromFile_Null ()
		{
			SecurityManager.LoadPolicyLevelFromFile (null, PolicyLevelType.AppDomain);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadPolicyLevelFromString_Null ()
		{
			SecurityManager.LoadPolicyLevelFromString (null, PolicyLevelType.AppDomain);
		}

		[Test]
		public void PolicyHierarchy () 
		{
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			Assert.IsNotNull (e, "PolicyHierarchy");
		}

		private void ResolveEvidenceHost (SecurityZone zone, bool unrestricted, bool empty)
		{
			string prefix = zone.ToString () + "-";
			Evidence e = new Evidence ();
			e.AddHost (new Zone (zone));
			PermissionSet ps = SecurityManager.ResolvePolicy (e);
			Assert.IsTrue (ps.Count > 0, prefix + "Count");
			Assert.AreEqual (empty, ps.IsEmpty (), prefix + "IsEmpty");
			Assert.AreEqual (unrestricted, ps.IsUnrestricted (), prefix + "IsUnrestricted");
			Assert.IsNotNull (ps.GetPermission (typeof (ZoneIdentityPermission)), prefix + "GetPermission(ZoneIdentityPermission)");
		}

		[Test]
		public void ResolvePolicy_Evidence_Host_Zone () 
		{
			ResolveEvidenceHost (SecurityZone.Internet, false, false);
			ResolveEvidenceHost (SecurityZone.Intranet, false, false);
			ResolveEvidenceHost (SecurityZone.MyComputer, true, false);
			ResolveEvidenceHost (SecurityZone.Trusted, false, false);
			ResolveEvidenceHost (SecurityZone.Untrusted, false, false);
			ResolveEvidenceHost (SecurityZone.NoZone, false, true);
		}

		private void ResolveEvidenceAssembly (SecurityZone zone)
		{
			string prefix = zone.ToString () + "-";
			Evidence e = new Evidence ();
			e.AddAssembly (new Zone (zone));
			PermissionSet ps = SecurityManager.ResolvePolicy (e);
			Assert.AreEqual (0, ps.Count, prefix + "Count");
			Assert.IsTrue (ps.IsEmpty (), prefix + "IsEmpty");
			Assert.IsFalse (ps.IsUnrestricted (), prefix + "IsUnrestricted");
		}

		[Test]
		public void ResolvePolicy_Evidence_Assembly_Zone ()
		{
			ResolveEvidenceAssembly (SecurityZone.Internet);
			ResolveEvidenceAssembly (SecurityZone.Intranet);
			ResolveEvidenceAssembly (SecurityZone.MyComputer);
			ResolveEvidenceAssembly (SecurityZone.Trusted);
			ResolveEvidenceAssembly (SecurityZone.Untrusted);
			ResolveEvidenceAssembly (SecurityZone.NoZone);
		}

		[Test]
		public void ResolvePolicy_Evidence_Null ()
		{
			Evidence e = null;
			PermissionSet ps = SecurityManager.ResolvePolicy (e);
			// no exception thrown
			Assert.IsNotNull (ps);
			Assert.IsFalse (ps.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void ResolvePolicy_Evidence_CurrentAssembly ()
		{
			PermissionSet granted = SecurityManager.ResolvePolicy (CurrentEvidence);
			Assert.IsNotNull (granted);
			Assert.IsTrue (granted.IsUnrestricted (), "IsUnrestricted");
		}

#if NET_2_0
		[Test]
		public void ResolvePolicy_Evidences_Null ()
		{
			Evidence[] e = null;
			PermissionSet ps = SecurityManager.ResolvePolicy (e);
			// no exception thrown
			Assert.IsNotNull (ps);
			Assert.IsFalse (ps.IsUnrestricted (), "IsUnrestricted");
		}
#endif
		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void ResolvePolicy_Evidence_AllNull ()
		{
			PermissionSet denied = null;
			SecurityManager.ResolvePolicy (null, null, null, null, out denied);
			// null is missing the Execution right
		}

		[Test]
		public void ResolvePolicy_Evidence_AllNull_NoExecution ()
		{
			PermissionSet denied = null;
			SecurityManager.CheckExecutionRights = false;
			PermissionSet granted = SecurityManager.ResolvePolicy (null, null, null, null, out denied);
			Assert.IsNull (denied, "Denied");
			Assert.AreEqual (0, granted.Count, "Granted.Count");
			Assert.IsFalse (granted.IsUnrestricted (), "!Granted.IsUnrestricted");
		}

		[Test]
		public void ResolvePolicy_Evidence_NullRequests_CurrentAssembly ()
		{
			PermissionSet denied = null;
			PermissionSet granted = SecurityManager.ResolvePolicy (CurrentEvidence, null, null, null, out denied);
			Assert.IsNull (denied, "Denied");
			Assert.IsTrue (granted.IsUnrestricted (), "Granted.IsUnrestricted");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (PolicyException))]
#endif
		public void ResolvePolicy_Evidence_DenyUnrestricted_CurrentAssembly ()
		{
			PermissionSet deny = new PermissionSet (PermissionState.Unrestricted);
			PermissionSet denied = null;
			PermissionSet granted = SecurityManager.ResolvePolicy (CurrentEvidence, null, null, deny, out denied);
			// doing this we denied the Execution right
#if !NET_2_0
			Assert.IsNull (denied, "denied");
			Assert.IsTrue (granted.IsUnrestricted (), "Granted.IsUnrestricted");
#endif
		}

		[Test]
		[Ignore ("MS bug - throws a NullReferenceException")]
		public void ResolvePolicy_Evidence_DenyUnrestricted_NoExecution ()
		{
			PermissionSet deny = new PermissionSet (PermissionState.Unrestricted);
			PermissionSet denied = null;
			SecurityManager.CheckExecutionRights = false;
			PermissionSet granted = SecurityManager.ResolvePolicy (CurrentEvidence, null, null, deny, out denied);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolvePolicyGroups_Null ()
		{
			IEnumerator e = SecurityManager.ResolvePolicyGroups (null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void SavePolicyLevel_Null ()
		{
			SecurityManager.SavePolicyLevel (null);
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void SavePolicyLevel_AppDomain ()
		{
			PolicyLevel adl = PolicyLevel.CreateAppDomainLevel ();
			SecurityManager.SavePolicyLevel (adl);
		}
	}
}
