//
// RegistryPermissionAttributeTest.cs - NUnit Test Cases for RegistryPermissionAttribute
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
	public class RegistryPermissionAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Read", a.Read);
			AssertNull ("Write", a.Write);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			RegistryPermission perm = (RegistryPermission) a.CreatePermission ();
			AssertNull ("Create", perm.GetPathList (RegistryPermissionAccess.Create));
			AssertNull ("Read", perm.GetPathList (RegistryPermissionAccess.Read));
			AssertNull ("Write", perm.GetPathList (RegistryPermissionAccess.Write));
		}

		[Test]
		public void Action () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
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
		public void All_Set () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.All = "mono";
			AssertEquals ("Create", "mono", a.Create);
			AssertEquals ("Read", "mono", a.Read);
			AssertEquals ("Write", "mono", a.Write);
			a.All = null;
			AssertNull ("Create", a.Create);
			AssertNull ("Read", a.Read);
			AssertNull ("Write", a.Write);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void All_Get () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.All = "mono";
			AssertEquals ("All", "mono", a.All);
		}

		[Test]
		public void Create () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Create = "mono";
			AssertEquals ("Create", "mono", a.Create);
			AssertNull ("Read", a.Read);
			AssertNull ("Write", a.Write);
			a.Create = null;
			AssertNull ("Create", a.Create);
			AssertNull ("Read", a.Read);
			AssertNull ("Write", a.Write);
		}

		[Test]
		public void Read () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Read = "mono";
			AssertNull ("Create", a.Create);
			AssertEquals ("Read", "mono", a.Read);
			AssertNull ("Write", a.Write);
			a.Read = null;
			AssertNull ("Read", a.Read);
			AssertNull ("Create", a.Create);
			AssertNull ("Write", a.Write);
		}

		[Test]
		public void Write () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Write = "mono";
			AssertNull ("Create", a.Create);
			AssertNull ("Read", a.Read);
			AssertEquals ("Write", "mono", a.Write);
			a.Write = null;
			AssertNull ("Create", a.Create);
			AssertNull ("Read", a.Read);
			AssertNull ("Write", a.Write);
		}

		[Test]
		public void Unrestricted () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			RegistryPermission perm = (RegistryPermission) a.CreatePermission ();
			Assert ("CreatePermission.IsUnrestricted", perm.IsUnrestricted ());
		}
	}
}
