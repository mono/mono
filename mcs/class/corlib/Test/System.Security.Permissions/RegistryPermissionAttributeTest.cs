//
// RegistryPermissionAttributeTest.cs - 
//	NUnit Test Cases for RegistryPermissionAttribute
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
	public class RegistryPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			RegistryPermission perm = (RegistryPermission) a.CreatePermission ();
#if NET_2_0
			Assert.AreEqual (String.Empty, perm.GetPathList (RegistryPermissionAccess.Create), "Create");
			Assert.AreEqual (String.Empty, perm.GetPathList (RegistryPermissionAccess.Read), "Read");
			Assert.AreEqual (String.Empty, perm.GetPathList (RegistryPermissionAccess.Write), "Write");
#else
			Assert.IsNull (perm.GetPathList (RegistryPermissionAccess.Create), "Create");
			Assert.IsNull (perm.GetPathList (RegistryPermissionAccess.Read), "Read");
			Assert.IsNull (perm.GetPathList (RegistryPermissionAccess.Write), "Write");
#endif
		}

		[Test]
		public void Action () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
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
			RegistryPermissionAttribute a = new RegistryPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void All_Set () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.All = "mono";
			Assert.AreEqual ("mono", a.Create, "Create");
			Assert.AreEqual ("mono", a.Read, "Read");
			Assert.AreEqual ("mono", a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif

			a.All = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void All_Get () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.All = "mono";
			Assert.AreEqual ("All", "mono", a.All);
		}

		[Test]
		public void Create () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Create = "mono";
			Assert.AreEqual ("mono", a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif

			a.Create = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif
		}

		[Test]
		public void Read () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Read = "mono";
			Assert.IsNull (a.Create, "Create");
			Assert.AreEqual ("mono", a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif

			a.Read = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif
		}

#if NET_2_0
		[Test]
		public void ChangeAccessControl ()
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.ChangeAccessControl = "mono";
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
			Assert.AreEqual ("mono", a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");

			a.ChangeAccessControl = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
		}

		[Test]
		public void ViewAccessControl ()
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.ViewAccessControl = "mono";
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.AreEqual ("mono", a.ViewAccessControl, "ViewAccessControl");

			a.ViewAccessControl = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
		}

		[Test]
		public void ViewAndModify_Set ()
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.ViewAndModify = "mono";
			Assert.AreEqual ("mono", a.Create, "Create");
			Assert.AreEqual ("mono", a.Read, "Read");
			Assert.AreEqual ("mono", a.Write, "Write");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");

			a.ViewAndModify = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ViewAndModify_Get ()
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.ViewAndModify = "mono";
			Assert.AreEqual ("ViewAndModify", "mono", a.ViewAndModify);
		}
#endif

		[Test]
		public void Write ()
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Write = "mono";
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.AreEqual ("mono", a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif

			a.Write = null;
			Assert.IsNull (a.Create, "Create");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif
		}

		[Test]
		public void Unrestricted () 
		{
			RegistryPermissionAttribute a = new RegistryPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			RegistryPermission perm = (RegistryPermission) a.CreatePermission ();
			Assert.IsTrue (perm.IsUnrestricted (), "CreatePermission.IsUnrestricted");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (RegistryPermissionAttribute);
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
