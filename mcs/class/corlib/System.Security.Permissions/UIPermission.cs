//
// System.Security.Permissions.UIPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;
using System.Globalization;

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

		public override void FromXml (SecurityElement esd) 
		{
			if (esd == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (esd.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (esd.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));
			
			if (esd.Attribute ("Unrestricted") == "true") {
				_window = UIPermissionWindow.AllWindows;
				_clipboard = UIPermissionClipboard.AllClipboard;

			// only 2 attributes: class and version
			} else if (esd.Attributes.Count == 2) {
				_window = UIPermissionWindow.NoWindows;
				_clipboard = UIPermissionClipboard.NoClipboard;

			} else {
				_window = (UIPermissionWindow) Enum.Parse (
					typeof (UIPermissionWindow), esd.Attribute ("Window"));

				_clipboard = (UIPermissionClipboard) Enum.Parse (
					typeof (UIPermissionClipboard), esd.Attribute ("Clipboard"));
			}
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

		public override SecurityElement ToXml () 
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			if (_window == UIPermissionWindow.NoWindows && _clipboard == UIPermissionClipboard.NoClipboard)
				return e;

			if (_window == UIPermissionWindow.AllWindows && _clipboard == UIPermissionClipboard.AllClipboard) {
				e.AddAttribute ("Unrestricted", "true");
				return e;
			}

			if (_window != UIPermissionWindow.NoWindows)
				e.AddAttribute ("Window", _window.ToString ());

			if (_clipboard != UIPermissionClipboard.NoClipboard)
				e.AddAttribute ("Clipboard", _clipboard.ToString ());

			return e;
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
