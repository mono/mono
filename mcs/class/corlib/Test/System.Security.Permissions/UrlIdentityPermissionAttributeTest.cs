//
// UrlIdentityPermissionAttributeTest.cs - NUnit Test Cases for UrlIdentityPermissionAttribute
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
	public class UrlIdentityPermissionAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);
			AssertNull ("Url", a.Url);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DefaultPermission () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Url", a.Url);
			// UrlIdentityPermission would throw a ArgumentNullException for a null URL ...
			UrlIdentityPermission perm = (UrlIdentityPermission) a.CreatePermission ();
			// ... but this works ...
			AssertNotNull ("CreatePermission(null url)", perm);
			// ... but this doesn't!
			string url = perm.Url;
		}

		[Test]
		public void Action () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
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
		public void Url () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			a.Url = "mono";
			AssertEquals ("Url", "mono", a.Url);
			a.Url = null;
			AssertNull ("Url", a.Url);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Unrestricted () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}
	}
}
