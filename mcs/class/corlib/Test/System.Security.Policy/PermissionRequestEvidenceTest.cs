//
// PermissionRequestEvidenceTest.cs - NUnit Test Cases for PermissionRequestEvidence
//
// Author:
//	Nick Drochak (ndrochak@gol.com)
//
// (C) 2004 Nick Drochak
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class PermissionRequestEvidenceTest : Assertion {

		[Test]
		public void NullConstructor () {
			PermissionRequestEvidence pre = new PermissionRequestEvidence (null, null, null);
			AssertNull ("Requested", pre.RequestedPermissions);
			AssertNull ("Optional", pre.OptionalPermissions);
			AssertNull ("Denied", pre.DeniedPermissions);
		}

		[Test]
		public void Constructor1 () {
			PermissionSet ps = new PermissionSet (PermissionState.None);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert ("Requested", !pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", !pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", !pre.DeniedPermissions.IsUnrestricted ());
		}

		[Test]
		public void Constructor2 () {
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert ("Requested", pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", pre.DeniedPermissions.IsUnrestricted ());
		}

		[Test]
		public void Constructor3 () {
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps1, ps2, ps1);
			Assert ("Requested", !pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", !pre.DeniedPermissions.IsUnrestricted ());
		}

		[Test]
		public void Constructor4 () {
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps2, ps1, ps2);
			Assert ("Requested", pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", !pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", pre.DeniedPermissions.IsUnrestricted ());
		}

		[Test]
		public void Copy () {
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps2, ps1, ps2);
			PermissionRequestEvidence pre2 = pre.Copy ();
			Assert ("Requested", pre2.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", !pre2.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", pre2.DeniedPermissions.IsUnrestricted ());
		}
} // class
} // namespace