//
// SiteIdentityPermissionAttributeTest.cs - NUnit Test Cases for SiteIdentityPermissionAttribute
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
	public class SiteIdentityPermissionAttributeTest : Assertion {

		[Test]
		public void Default ()
		{
			SiteIdentityPermissionAttribute a = new SiteIdentityPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Site", a.Site);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DefaultPermission () 
		{
			SiteIdentityPermissionAttribute a = new SiteIdentityPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Site", a.Site);
			// SiteIdentityPermission would throw a ArgumentException for a null site ...
			SiteIdentityPermission perm = (SiteIdentityPermission) a.CreatePermission ();
			// ... but this works ...
			AssertNotNull ("CreatePermission(null site)", perm);
			// ... but this doesn't!
			string site = perm.Site;
		}

		[Test]
		public void Action () 
		{
			SiteIdentityPermissionAttribute a = new SiteIdentityPermissionAttribute (SecurityAction.Assert);
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
		public void Site () 
		{
			SiteIdentityPermissionAttribute a = new SiteIdentityPermissionAttribute (SecurityAction.Assert);
			a.Site = "mono";
			AssertEquals ("Site", "mono", a.Site);
			a.Site = null;
			AssertNull ("Site", a.Site);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Unrestricted () 
		{
			SiteIdentityPermissionAttribute a = new SiteIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}
	}
}
