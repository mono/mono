//
// PrincipalPermissionAttributeTest.cs - NUnit Test Cases for PrincipalPermissionAttribute
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
	public class PrincipalPermissionAttributeTest {

		private static string user = "user";
		private static string role = "role";

		[Test]
		public void Default () 
		{
			PrincipalPermissionAttribute a = new PrincipalPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.Role, "Role");
			Assert.IsTrue (a.Authenticated, "Authenticated");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			PrincipalPermission perm = (PrincipalPermission) a.CreatePermission ();
			Assert.IsNotNull (perm, "CreatePermission");
		}

		[Test]
		public void Action () 
		{
			PrincipalPermissionAttribute a = new PrincipalPermissionAttribute (SecurityAction.Assert);
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
		}

		[Test]
		public void Action_Invalid ()
		{
			PrincipalPermissionAttribute a = new PrincipalPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void NameNullRoleNullAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = null;
			attr.Authenticated = true;
			Assert.IsNull (attr.Name, "NameNullRoleNullAuthenticated.Name");
			Assert.IsNull (attr.Role, "NameNullRoleNullAuthenticated.Role");
			Assert.IsTrue (attr.Authenticated, "NameNullRoleNullAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsTrue (p.IsUnrestricted (), "NameNullRoleNullAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameNullRoleNullNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = null;
			attr.Authenticated = false;
			Assert.IsNull (attr.Name, "NameNullRoleNullNonAuthenticated.Name");
			Assert.IsNull (attr.Role, "NameNullRoleNullNonAuthenticated.Role");
			Assert.IsFalse (attr.Authenticated, "NameNullRoleNullNonAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameNullRoleNullNonAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameRoleNullAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = null;
			attr.Authenticated = true;
			Assert.AreEqual (user, attr.Name, "NameRoleNullAuthenticated.Name");
			Assert.IsNull (attr.Role, "NameRoleNullAuthenticated.Role");
			Assert.IsTrue (attr.Authenticated, "NameRoleNullAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameRoleNullAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameRoleNullNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = null;
			attr.Authenticated = false;
			Assert.AreEqual (user, attr.Name, "NameRoleNullNonAuthenticated.Name");
			Assert.IsNull (attr.Role, "NameRoleNullNonAuthenticated.Role");
			Assert.IsFalse (attr.Authenticated, "NameRoleNullNonAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameRoleNullNonAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameNullRoleAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = role;
			attr.Authenticated = true;
			Assert.IsNull (attr.Name, "NameNullRoleAuthenticated.Name");
			Assert.AreEqual (role, attr.Role, "NameNullRoleAuthenticated.Role");
			Assert.IsTrue (attr.Authenticated, "NameNullRoleAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameNullRoleAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameNullRoleNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = role;
			attr.Authenticated = false;
			Assert.IsNull (attr.Name, "NameNullRoleNonAuthenticated.Name");
			Assert.AreEqual (role, attr.Role, "NameNullRoleNonAuthenticated.Role");
			Assert.IsFalse (attr.Authenticated, "NameNullRoleNonAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameNullRoleNonAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameRoleAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = role;
			attr.Authenticated = true;
			Assert.AreEqual (user, attr.Name, "NameRoleAuthenticated.Name");
			Assert.AreEqual (role, attr.Role, "NameRoleAuthenticated.Role");
			Assert.IsTrue (attr.Authenticated, "NameRoleAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameRoleAuthenticated.IsUnrestricted");
		}

		[Test]
		public void NameRoleNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = role;
			attr.Authenticated = false;
			Assert.AreEqual (user, attr.Name, "NameRoleNonAuthenticated.Name");
			Assert.AreEqual (role, attr.Role, "NameRoleNonAuthenticated.Role");
			Assert.IsFalse (attr.Authenticated, "NameRoleNonAuthenticated.Authenticated");
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert.IsFalse (p.IsUnrestricted (), "NameRoleNonAuthenticated.IsUnrestricted");
		}

		[Test]
		public void Unrestricted () 
		{
			PrincipalPermissionAttribute a = new PrincipalPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			PrincipalPermission perm = (PrincipalPermission) a.CreatePermission ();
			Assert.IsTrue (perm.IsUnrestricted (), "CreatePermission.IsUnrestricted");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (PrincipalPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object[] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Class | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
