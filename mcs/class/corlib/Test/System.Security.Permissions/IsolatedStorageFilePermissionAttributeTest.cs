//
// IsolatedStorageFilePermissionAttributeTest.cs - NUnit Test Cases for IsolatedStorageFilePermissionAttribute
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
	public class IsolatedStorageFilePermissionAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			AssertEquals ("UsageAllowed", IsolatedStorageContainment.None, a.UsageAllowed);
			AssertEquals ("UserQuota", 0, a.UserQuota);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			IsolatedStorageFilePermission perm = (IsolatedStorageFilePermission) a.CreatePermission ();
			AssertEquals ("CreatePermission-UsageAllowed", IsolatedStorageContainment.None, perm.UsageAllowed);
			AssertEquals ("CreatePermission-UserQuota", 0, perm.UserQuota);
		}

		[Test]
		public void Action ()
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
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
		public void UsageAllowed () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			AssertEquals ("UsageAllowed=None", IsolatedStorageContainment.None, a.UsageAllowed);
			a.UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser;
			AssertEquals ("UsageAllowed=AdministerIsolatedStorageByUser", IsolatedStorageContainment.AdministerIsolatedStorageByUser, a.UsageAllowed);
			a.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByRoamingUser;
			AssertEquals ("UsageAllowed=AssemblyIsolationByRoamingUser", IsolatedStorageContainment.AssemblyIsolationByRoamingUser, a.UsageAllowed);
			a.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
			AssertEquals ("UsageAllowed=AssemblyIsolationByUser", IsolatedStorageContainment.AssemblyIsolationByUser, a.UsageAllowed);
			a.UsageAllowed = IsolatedStorageContainment.DomainIsolationByRoamingUser;
			AssertEquals ("UsageAllowed=DomainIsolationByRoamingUser", IsolatedStorageContainment.DomainIsolationByRoamingUser, a.UsageAllowed);
			a.UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser;
			AssertEquals ("UsageAllowed=DomainIsolationByUser", IsolatedStorageContainment.DomainIsolationByUser, a.UsageAllowed);
			a.UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			AssertEquals ("UsageAllowed=UnrestrictedIsolatedStorage", IsolatedStorageContainment.UnrestrictedIsolatedStorage, a.UsageAllowed);
		}

		[Test]
		public void UserQuota () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			AssertEquals ("UserQuota=default", 0, a.UserQuota);
			a.UserQuota = Int64.MinValue;
			AssertEquals ("UserQuota=MinValue", Int64.MinValue, a.UserQuota);
			a.UserQuota = Int64.MaxValue;
			AssertEquals ("UserQuota=MaxValue", Int64.MaxValue, a.UserQuota);
		}

		[Test]
		public void Unrestricted () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			IsolatedStorageFilePermission perm = (IsolatedStorageFilePermission) a.CreatePermission ();
			Assert ("CreatePermission.IsUnrestricted", perm.IsUnrestricted ());
			AssertEquals ("CreatePermission.UsageAllowed", IsolatedStorageContainment.UnrestrictedIsolatedStorage, perm.UsageAllowed);
			AssertEquals ("CreatePermission.UserQuota", Int64.MaxValue, perm.UserQuota);
		}
	}
}
