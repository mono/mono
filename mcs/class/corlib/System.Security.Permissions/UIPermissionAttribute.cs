//
// System.Security.Permissions.UIPermissionAttribute.cs
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
	public sealed class UIPermissionAttribute : IsolatedStoragePermissionAttribute
	{
		// Constructor
		public UIPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		[MonoTODO]
		public UIPermissionClipboard Clipboard
		{
			get { return 0; }
			set {}
		}

		[MonoTODO]
		public UIPermissionWindow Window
		{
			get { return 0; }
			set {}
		}

		// Methods
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}
	}
}
