//
// System.Security.Permissions.UIPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class UIPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private UIPermissionWindow _window;
		private UIPermissionClipboard _clipboard;

		// Constructors

		public UIPermission (PermissionState state) 
		{
		}

		public UIPermission (UIPermissionClipboard clipboardFlag) 
		{
			_clipboard = clipboardFlag;
		}

		public UIPermission (UIPermissionWindow windowFlag) 
		{
			_window = windowFlag;
		}

		public UIPermission (UIPermissionWindow windowFlag, UIPermissionClipboard clipboardFlag) 
		{
			_clipboard = clipboardFlag;
			_window = windowFlag;
		}

		// Properties

		public UIPermissionClipboard Clipboard {
			get { return _clipboard; }
			set { _clipboard = value; }
		}

		public UIPermissionWindow Window { 
			get { return _window; }
			set { _window = value; }
		}

		// Methods

		public override IPermission Copy () 
		{
			return new UIPermission (_window, _clipboard);
		}

		[MonoTODO]
		public override void FromXml (SecurityElement esd) 
		{
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		public bool IsUnrestricted () 
		{
			return ((_window == UIPermissionWindow.AllWindows) &&
				(_clipboard == UIPermissionClipboard.AllClipboard));
		}

		[MonoTODO]
		public override SecurityElement ToXml () 
		{
			return null;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			return null;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 7;
		}
	}
}