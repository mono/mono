//
// FileDialogPermissionAttribute.cs - NUnit Test Cases for FileDialogPermission
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
	public class FileDialogPermissionAttributeTest : Assertion {

		[Test]
		public void None () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = false;
			attr.Save = false;
			Assert ("None=Open", !attr.Open);
			Assert ("None=Save", !attr.Save);
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			AssertEquals ("None=FileDialogPermission", FileDialogPermissionAccess.None, p.Access);
		}

		[Test]
		public void Open () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = true;
			attr.Save = false;
			Assert ("Open=Open", attr.Open);
			Assert ("Open=Save", !attr.Save);
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			AssertEquals ("Open=FileDialogPermission", FileDialogPermissionAccess.Open, p.Access);
		}

		[Test]
		public void Save () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = false;
			attr.Save = true;
			Assert ("Save=Open", !attr.Open);
			Assert ("Save=Save", attr.Save);
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			AssertEquals ("Save=FileDialogPermission", FileDialogPermissionAccess.Save, p.Access);
		}

		[Test]
		public void OpenSave () 
		{
			FileDialogPermissionAttribute attr = new FileDialogPermissionAttribute (SecurityAction.Assert);
			attr.Open = true;
			attr.Save = true;
			Assert ("OpenSave=Open", attr.Open);
			Assert ("OpenSave=Save", attr.Save);
			FileDialogPermission p = (FileDialogPermission) attr.CreatePermission ();
			AssertEquals ("OpenSave=FileDialogPermission", FileDialogPermissionAccess.OpenSave, p.Access);
			Assert ("OpenSave=Unrestricted", p.IsUnrestricted ());
		}
	}
}
