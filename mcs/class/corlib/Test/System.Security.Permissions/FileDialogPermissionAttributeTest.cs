//
// FileDialogPermissionAttribute.cs - 
//	NUnit Test Cases for FileDialogPermissionAttribute
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
#if MOBILE
	[Ignore]
#endif
	public class FileDialogPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			FileDialogPermissionAttribute a = new FileDialogPermissionAttribute (SecurityAction.Assert);
			Assert.IsFalse (a.Open, "Open");
			Assert.IsFalse (a.Save, "Save");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			FileDialogPermission perm = (FileDialogPermission) a.CreatePermission ();
			Assert.IsFalse (perm.IsUnrestricted (), "CreatePermission-IsUnrestricted");
		}

		[Test]
		public void Action () 
		{
			FileDialogPermissionAttribute a = new FileDialogPermissionAttribute (SecurityAction.Assert);
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
			FileDialogPermissionAttribute a = new FileDialogPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void None () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = false;
			attr.Save = false;
			Assert.IsFalse (attr.Open, "None=Open");
			Assert.IsFalse (attr.Save, "None=Save");
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			Assert.AreEqual (FileDialogPermissionAccess.None, p.Access, "None=FileDialogPermission");
		}

		[Test]
		public void Open () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = true;
			attr.Save = false;
			Assert.IsTrue (attr.Open, "Open=Open");
			Assert.IsFalse (attr.Save, "Open=Save");
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			Assert.AreEqual (FileDialogPermissionAccess.Open, p.Access, "Open=FileDialogPermission");
		}

		[Test]
		public void Save () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = false;
			attr.Save = true;
			Assert.IsFalse (attr.Open, "Save=Open");
			Assert.IsTrue (attr.Save, "Save=Save");
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			Assert.AreEqual (FileDialogPermissionAccess.Save, p.Access, "Save=FileDialogPermission");
		}

		[Test]
		public void OpenSave () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = true;
			attr.Save = true;
			Assert.IsTrue (attr.Open, "OpenSave=Open");
			Assert.IsTrue (attr.Save, "OpenSave=Save");
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			Assert.AreEqual (FileDialogPermissionAccess.OpenSave, p.Access, "OpenSave=FileDialogPermission");
			Assert.IsTrue (p.IsUnrestricted (), "OpenSave=Unrestricted");
		}

		[Test]
		public void Unrestricted () 
		{
			FileDialogPermissionAttribute a = new FileDialogPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			FileDialogPermission perm = (FileDialogPermission) a.CreatePermission ();
			Assert.IsTrue (perm.IsUnrestricted (), "CreatePermission.IsUnrestricted");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (FileDialogPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object[] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
