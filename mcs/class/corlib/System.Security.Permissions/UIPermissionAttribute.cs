//
// System.Security.Permissions.UIPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class UIPermissionAttribute : CodeAccessSecurityAttribute	{

		// Fields
		private UIPermissionClipboard clipboard;
		private UIPermissionWindow window;
		
		// Constructor
		public UIPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public UIPermissionClipboard Clipboard
		{
			get { return clipboard; }
			set { clipboard = value; }
		}

		public UIPermissionWindow Window
		{
			get { return window; }
			set { window = value; }
		}

		// Methods
		public override IPermission CreatePermission ()
		{
			UIPermission perm = null;
			if (this.Unrestricted)
				perm = new UIPermission (PermissionState.Unrestricted);
			else
				perm = new UIPermission (window, clipboard);
			return perm;
		}
	}
}
