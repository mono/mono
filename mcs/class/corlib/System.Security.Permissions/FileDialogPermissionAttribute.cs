//
// System.Security.Permissions.FileDialogPermissionAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot <spouliot@motus.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class FileDialogPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private bool canOpen;
		private bool canSave;
		
		// Constructor
		public FileDialogPermissionAttribute (SecurityAction action) : base (action) {}

		// Properties
		public bool Open {
			get { return canOpen; }
			set { canOpen = value; }
		} 

		public bool Save {
			get { return canSave; }
			set { canSave = value; }
		}

		// Methods
		public override IPermission CreatePermission ()
		{
			FileDialogPermissionAccess access = FileDialogPermissionAccess.None;
			if (canOpen)
				access |= FileDialogPermissionAccess.Open;
			if (canSave)
				access |= FileDialogPermissionAccess.Save;
			return new FileDialogPermission (access);
		}
	}
}
