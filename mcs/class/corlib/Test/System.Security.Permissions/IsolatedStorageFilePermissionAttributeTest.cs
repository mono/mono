//
// IsolatedStorageFilePermissionAttributeTest.cs - NUnit Test Cases for IsolatedStorageFilePermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class IsolatedStorageFilePermissionAttributeTest {

		[Test]
		public void Default () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (IsolatedStorageContainment.None, a.UsageAllowed, "UsageAllowed");
			Assert.AreEqual (0, a.UserQuota, "UserQuota");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			IsolatedStorageFilePermission perm = (IsolatedStorageFilePermission) a.CreatePermission ();
			Assert.AreEqual (IsolatedStorageContainment.None, perm.UsageAllowed, "CreatePermission-UsageAllowed");
			Assert.AreEqual (0, perm.UserQuota, "CreatePermission-UserQuota");
		}

		[Test]
		public void Action ()
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (SecurityAction.Assert, a.Action, "Action=Assert");
			a.Action = SecurityAction.Demand;
			Assert.AreEqual (SecurityAction.Demand, a.Action, "Action=Demand");
			a.Action = SecurityAction.Deny;
			Assert.AreEqual (SecurityAction.Deny, a.Action, "Action=Deny");
			a.Action = SecurityAction.InheritanceDemand;
			Assert.AreEqual (SecurityAction.InheritanceDemand, a.Action, "Action=InheritanceDemand");
			a.Action = SecurityAction.LinkDemand;
			Assert.AreEqual (SecurityAction.LinkDemand, a.Action, "Action=LinkDemand");
			a.Action = SecurityAction.PermitOnly;
			Assert.AreEqual (SecurityAction.PermitOnly, a.Action, "Action=PermitOnly");
			a.Action = SecurityAction.RequestMinimum;
			Assert.AreEqual (SecurityAction.RequestMinimum, a.Action, "Action=RequestMinimum");
			a.Action = SecurityAction.RequestOptional;
			Assert.AreEqual (SecurityAction.RequestOptional, a.Action, "Action=RequestOptional");
			a.Action = SecurityAction.RequestRefuse;
			Assert.AreEqual (SecurityAction.RequestRefuse, a.Action, "Action=RequestRefuse");
#if NET_2_0
			a.Action = SecurityAction.DemandChoice;
			Assert.AreEqual (SecurityAction.DemandChoice, a.Action, "Action=DemandChoice");
			a.Action = SecurityAction.InheritanceDemandChoice;
			Assert.AreEqual (SecurityAction.InheritanceDemandChoice, a.Action, "Action=InheritanceDemandChoice");
			a.Action = SecurityAction.LinkDemandChoice;
			Assert.AreEqual (SecurityAction.LinkDemandChoice, a.Action, "Action=LinkDemandChoice");
#endif
		}

		[Test]
		public void Action_Invalid ()
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void UsageAllowed () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (IsolatedStorageContainment.None, a.UsageAllowed, "UsageAllowed=None");
			a.UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser;
			Assert.AreEqual (IsolatedStorageContainment.AdministerIsolatedStorageByUser, a.UsageAllowed, "UsageAllowed=AdministerIsolatedStorageByUser");
			a.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByRoamingUser;
			Assert.AreEqual (IsolatedStorageContainment.AssemblyIsolationByRoamingUser, a.UsageAllowed, "UsageAllowed=AssemblyIsolationByRoamingUser");
			a.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
			Assert.AreEqual (IsolatedStorageContainment.AssemblyIsolationByUser, a.UsageAllowed, "UsageAllowed=AssemblyIsolationByUser");
			a.UsageAllowed = IsolatedStorageContainment.DomainIsolationByRoamingUser;
			Assert.AreEqual (IsolatedStorageContainment.DomainIsolationByRoamingUser, a.UsageAllowed, "UsageAllowed=DomainIsolationByRoamingUser");
			a.UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser;
			Assert.AreEqual (IsolatedStorageContainment.DomainIsolationByUser, a.UsageAllowed, "UsageAllowed=DomainIsolationByUser");
			a.UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			Assert.AreEqual (IsolatedStorageContainment.UnrestrictedIsolatedStorage, a.UsageAllowed, "UsageAllowed=UnrestrictedIsolatedStorage");
		}

		[Test]
		public void UsageAllowed_Invalid ()
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			a.UsageAllowed = (IsolatedStorageContainment)Int32.MinValue;
			// no validation in attribute
		}

		[Test]
		public void UserQuota () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (0, a.UserQuota, "UserQuota=default");
			a.UserQuota = Int64.MinValue;
			Assert.AreEqual (Int64.MinValue, a.UserQuota, "UserQuota=MinValue");
			a.UserQuota = Int64.MaxValue;
			Assert.AreEqual (Int64.MaxValue, a.UserQuota, "UserQuota=MaxValue");
		}

		[Test]
		public void Unrestricted () 
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			IsolatedStorageFilePermission perm = (IsolatedStorageFilePermission) a.CreatePermission ();
			Assert.IsTrue (perm.IsUnrestricted (), "CreatePermission.IsUnrestricted");
			Assert.AreEqual (IsolatedStorageContainment.UnrestrictedIsolatedStorage, perm.UsageAllowed, "CreatePermission.UsageAllowed");
			Assert.AreEqual (Int64.MaxValue, perm.UserQuota, "CreatePermission.UserQuota");
		}

		[Test]
		public void Attributes ()
		{
			IsolatedStorageFilePermissionAttribute a = new IsolatedStorageFilePermissionAttribute (SecurityAction.Assert);
			Type t = typeof (IsolatedStorageFilePermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
