//
// PermissionSetAttributeTest.cs - NUnit Test Cases for PermissionSetAttribute
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
	public class PermissionSetAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			AssertNull ("File", a.File);
			AssertNull ("Name", a.Name);
			AssertNull ("XML", a.XML);
			Assert ("UnicodeEncoded", !a.UnicodeEncoded);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			IPermission perm = a.CreatePermission ();
			AssertNull ("CreatePermission", perm);

			PermissionSet ps = a.CreatePermissionSet ();
			AssertEquals ("CreatePermissionSet", 0, ps.Count);
		}

		[Test]
		public void Action () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
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
		public void File () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.File = "mono";
			AssertEquals ("File", "mono", a.File);
			AssertNull ("Name", a.Name);
			AssertNull ("XML", a.XML);
			a.File = null;
			AssertNull ("File", a.File);
			AssertNull ("Name", a.Name);
			AssertNull ("XML", a.XML);
		}

		[Test]
		public void Name () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Name = "mono";
			AssertNull ("File", a.File);
			AssertEquals ("Name", "mono", a.Name);
			AssertNull ("XML", a.XML);
			a.Name = null;
			AssertNull ("File", a.File);
			AssertNull ("Name", a.Name);
			AssertNull ("XML", a.XML);
		}

		[Test]
		public void XML () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.XML = "mono";
			AssertNull ("File", a.File);
			AssertNull ("Name", a.Name);
			AssertEquals ("XML", "mono", a.XML);
			a.XML = null;
			AssertNull ("File", a.File);
			AssertNull ("Name", a.Name);
			AssertNull ("XML", a.XML);
		}

		[Test]
		public void UnicodeEncoded () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.UnicodeEncoded = true;
			Assert ("UnicodeEncoded", a.UnicodeEncoded);
			a.UnicodeEncoded = false;
			Assert ("UnicodeEncoded", !a.UnicodeEncoded);
		}

		[Test]
		public void Unrestricted () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			PermissionSet ps = a.CreatePermissionSet ();
			Assert ("CreatePermissionSet.IsUnrestricted", ps.IsUnrestricted ());
		}
	}
}
