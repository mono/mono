//
// System.Security.Permissions.IsolatedStorageFilePermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class IsolatedStorageFilePermissionAttribute : IsolatedStoragePermissionAttribute
	{
		// Constructor
		public IsolatedStorageFilePermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		// Methods
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}
	}
}
