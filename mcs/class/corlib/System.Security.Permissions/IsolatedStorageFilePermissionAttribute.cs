//
// System.Security.Permissions.IsolatedStorageFilePermissionAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class IsolatedStorageFilePermissionAttribute : IsolatedStoragePermissionAttribute
	{
		// Constructor
		public IsolatedStorageFilePermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		// Methods
		public override IPermission CreatePermission ()
		{
			IsolatedStorageFilePermission perm = null;
			if (this.Unrestricted)
				perm = new IsolatedStorageFilePermission (PermissionState.Unrestricted);
			else {
				perm = new IsolatedStorageFilePermission (PermissionState.None);
				perm.UsageAllowed = this.UsageAllowed;
				perm.UserQuota = this.UserQuota;
			}
			return perm;
		}
	}
}
