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

		private string AdjustNewLine (string s) 
		{
			if (Environment.NewLine != "\r\n")
				s = s.Replace ("\r\n", Environment.NewLine);
			return s;
		}

		[Test]
		public void NullConstructor () {
			PermissionRequestEvidence pre = new PermissionRequestEvidence (null, null, null);
			AssertNull ("Requested", pre.RequestedPermissions);
			AssertNull ("Optional", pre.OptionalPermissions);
			AssertNull ("Denied", pre.DeniedPermissions);
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\"/>\r\n");
			AssertEquals ("ToString", expected, pre.ToString ());
		}

		[Test]
		public void Constructor1 () {
			PermissionSet ps = new PermissionSet (PermissionState.None);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert ("Requested", !pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", !pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", !pre.DeniedPermissions.IsUnrestricted ());
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			AssertEquals ("ToString", expected, pre.ToString ());
		}

		[Test]
		public void Constructor2 () {
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert ("Requested", pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", pre.DeniedPermissions.IsUnrestricted ());
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			AssertEquals ("ToString", expected, pre.ToString ());
		}

		[Test]
		public void Constructor3 () {
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps1, ps2, ps1);
			Assert ("Requested", !pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", !pre.DeniedPermissions.IsUnrestricted ());
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			AssertEquals ("ToString", expected, pre.ToString ());
		}

		[Test]
		public void Constructor4 () {
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps2, ps1, ps2);
			Assert ("Requested", pre.RequestedPermissions.IsUnrestricted ());
			Assert ("Optional", !pre.OptionalPermissions.IsUnrestricted ());
			Assert ("Denied", pre.DeniedPermissions.IsUnrestricted ());
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			AssertEquals ("ToString", expected, pre.ToString ());
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
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			AssertEquals ("ToString", expected, pre.ToString ());
			AssertEquals ("ToString-Copy", pre.ToString (), pre2.ToString ());
		}
	} // class
} // namespace
