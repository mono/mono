//
// StrongNameIdentityPermissionAttributeTest.cs - NUnit Test Cases for StrongNameIdentityPermissionAttribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class StrongNameIdentityPermissionAttributeTest : Assertion {

		static string pk = "00240000048000009400000006020000002400005253413100040000010001003DBD7208C62B0EA8C1C058072B635F7C9ABDCB22DB20B2A9DADAEFE800642F5D8DEB7802F7A5367728D7558D1468DBEB2409D02B131B926E2E59544AAC18CFC909023F4FA83E94001FC2F11A27477D1084F514B861621A0C66ABD24C4B9FC90F3CD8920FF5FFCED76E5C6FB1F57DD356F96727A4A5485B079344004AF8FFA4CB";

		[Test]
		public void Default () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Name", a.Name);
			AssertNull ("PublicKey", a.PublicKey);
			AssertNull ("Version", a.Version);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			StrongNameIdentityPermission perm = (StrongNameIdentityPermission) a.CreatePermission ();
			AssertEquals ("CreatePermission.Name", String.Empty, perm.Name);
			AssertNull ("CreatePermission.PublicKey", perm.PublicKey);
			AssertEquals ("CreatePermission.Version", "0.0", perm.Version.ToString ());
		}

		[Test]
		public void Action () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("Action=Assert", SecurityAction.Assert, a.Action);
			a.Action = SecurityAction.Demand;
			AssertEquals ("Action=Demand", SecurityAction.Demand, a.Action);
			a.Action = SecurityAction.Deny;
			AssertEquals ("Action=Deny", SecurityAction.Deny, a.Action);
			a.Action = SecurityAction.InheritanceDemand;
			AssertEquals ("Action=InheritanceDemand", SecurityAction.InheritanceDemand, a.Action);
			a.Action = SecurityAction.LinkDemand;
			AssertEquals ("Action=LinkDemand", SecurityAction.LinkDemand, a.Action);
			a.Action = SecurityAction.PermitOnly;
			AssertEquals ("Action=PermitOnly", SecurityAction.PermitOnly, a.Action);
			a.Action = SecurityAction.RequestMinimum;
			AssertEquals ("Action=RequestMinimum", SecurityAction.RequestMinimum, a.Action);
			a.Action = SecurityAction.RequestOptional;
			AssertEquals ("Action=RequestOptional", SecurityAction.RequestOptional, a.Action);
			a.Action = SecurityAction.RequestRefuse;
			AssertEquals ("Action=RequestRefuse", SecurityAction.RequestRefuse, a.Action);
		}

		[Test]
		public void Name () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			AssertEquals ("Name", "mono", a.Name);
			a.Name = null;
			AssertNull ("Name", a.Name);
		}

		[Test]
		public void PublicKey () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.PublicKey = pk;
			AssertEquals ("PublicKey", pk, a.PublicKey);
			a.PublicKey = null;
			AssertNull ("PublicKey", a.PublicKey);
		}

		[Test]
		public void Version () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Version = "1.2.3.4";
			AssertEquals ("Version", "1.2.3.4", a.Version);
			a.Version = null;
			AssertNull ("Version", a.Version);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_OnlyName () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission_OnlyPublicKey () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.PublicKey = pk;
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_OnlyVersion () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission_NamePublicKey () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			a.PublicKey = pk;
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_NameVersion () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission_PublicKeyVersion () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.PublicKey = pk;
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			a.PublicKey = pk;
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Unrestricted () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}
	}
}
