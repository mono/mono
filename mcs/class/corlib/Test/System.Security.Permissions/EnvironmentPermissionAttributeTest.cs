//
// EnvironmentPermissionTest.cs - NUnit Test Cases for EnvironmentPermission
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
	public class EnvironmentPermissionAttributeTest : Assertion {

		private static string envar = "TMP";

		[Test]
		public void Default () 
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Read", a.Read);
			AssertNull ("Write", a.Write);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			EnvironmentPermission perm = (EnvironmentPermission) a.CreatePermission ();
			AssertNull ("GetPathList(Read)", perm.GetPathList (EnvironmentPermissionAccess.Read));
			AssertNull ("GetPathList(Write)", perm.GetPathList (EnvironmentPermissionAccess.Write));
			Assert ("CreatePermission-IsUnrestricted", !perm.IsUnrestricted ());
		}

		[Test]
		public void Action () 
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
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
		public void All () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.All = envar;
			AssertEquals ("All=Read", envar, attr.Read);
			AssertEquals ("All=Write", envar, attr.Write);
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			AssertEquals ("All=EnvironmentPermission-Read", envar, p.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("All=EnvironmentPermission-Write", envar, p.GetPathList (EnvironmentPermissionAccess.Write));
		}
#if !NET_1_0
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void All_Get () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			string s = attr.All;
		}
#endif
		[Test]
		public void Read () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.Read = envar;
			AssertEquals ("Read=Read", envar, attr.Read);
			AssertNull ("Write=null", attr.Write);
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			AssertEquals ("Read=EnvironmentPermission-Read", envar, p.GetPathList (EnvironmentPermissionAccess.Read));
			AssertNull ("Read=EnvironmentPermission-Write", p.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		public void Write ()
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.Write = envar;
			AssertNull ("Read=null", attr.Read);
			AssertEquals ("Write=Write", envar, attr.Write);
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			AssertNull ("Write=EnvironmentPermission-Read", p.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("Write=EnvironmentPermission-Write", envar, p.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		public void Unrestricted () 
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			EnvironmentPermission perm = (EnvironmentPermission) a.CreatePermission ();
			Assert ("CreatePermission.IsUnrestricted", perm.IsUnrestricted ());
			AssertEquals ("GetPathList(Read)", String.Empty, perm.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("GetPathList(Write)", String.Empty, perm.GetPathList (EnvironmentPermissionAccess.Write));
		}
	}
}
