//
// System.Security.Permissions.UIPermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions {

#if NET_2_0
	[ComVisible (true)]
#endif
	[Serializable]
	public sealed class UIPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private UIPermissionWindow _window;		// Note: this (looks like) but isn't a [Flags]
		private UIPermissionClipboard _clipboard;	// Note: this (looks like) but isn't a [Flags]

		private const int version = 1;

		// Constructors

		public UIPermission (PermissionState state) 
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted) {
				_clipboard = UIPermissionClipboard.AllClipboard;
				_window = UIPermissionWindow.AllWindows;
			}
		}

		public UIPermission (UIPermissionClipboard clipboardFlag) 
		{
			// reuse validation by the Clipboard property
			Clipboard = clipboardFlag;
		}

		public UIPermission (UIPermissionWindow windowFlag) 
		{
			// reuse validation by the Window property
			Window = windowFlag;
		}

		public UIPermission (UIPermissionWindow windowFlag, UIPermissionClipboard clipboardFlag) 
		{
			// reuse validation by the Clipboard and Window properties
			Clipboard = clipboardFlag;
			Window = windowFlag;
		}

		// Properties

		public UIPermissionClipboard Clipboard {
			get { return _clipboard; }
			set {
				if (!Enum.IsDefined (typeof (UIPermissionClipboard), value)) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "UIPermissionClipboard");
				}
				_clipboard = value;
			}
		}

		public UIPermissionWindow Window { 
			get { return _window; }
			set {
				if (!Enum.IsDefined (typeof (UIPermissionWindow), value)) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "UIPermissionWindow");
				}
				_window = value;
			}
		}

		// Methods

		public override IPermission Copy () 
		{
			return new UIPermission (_window, _clipboard);
		}

		public override void FromXml (SecurityElement esd) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd)) {
				_window = UIPermissionWindow.AllWindows;
				_clipboard = UIPermissionClipboard.AllClipboard;
			}
			else {
				string w = esd.Attribute ("Window");
				if (w == null)
					_window = UIPermissionWindow.NoWindows;
				else
					_window = (UIPermissionWindow) Enum.Parse (typeof (UIPermissionWindow), w);

				string c = esd.Attribute ("Clipboard");
				if (c == null)
					_clipboard = UIPermissionClipboard.NoClipboard;
				else
					_clipboard = (UIPermissionClipboard) Enum.Parse (typeof (UIPermissionClipboard), c);
			}
		}

		public override IPermission Intersect (IPermission target) 
		{
			UIPermission uip = Cast (target);
			if (uip == null)
				return null;

			// there are not [Flags] so we can't use boolean operators
			UIPermissionWindow w = ((_window < uip._window) ? _window : uip._window);
			UIPermissionClipboard c = ((_clipboard < uip._clipboard) ? _clipboard : uip._clipboard);

			if (IsEmpty (w, c))
				return null;

			return new UIPermission (w, c);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			UIPermission uip = Cast (target);
			if (uip == null)
				return IsEmpty (_window, _clipboard);
			if (uip.IsUnrestricted ())
				return true;

			// there are not [Flags] so we can't use boolean operators
			return ((_window <= uip._window) && (_clipboard <= uip._clipboard));
		}

		public bool IsUnrestricted () 
		{
			return ((_window == UIPermissionWindow.AllWindows) &&
				(_clipboard == UIPermissionClipboard.AllClipboard));
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = Element (version);

			if (_window == UIPermissionWindow.AllWindows && _clipboard == UIPermissionClipboard.AllClipboard) {
				e.AddAttribute ("Unrestricted", "true");
			}
			else {
				if (_window != UIPermissionWindow.NoWindows)
					e.AddAttribute ("Window", _window.ToString ());

				if (_clipboard != UIPermissionClipboard.NoClipboard)
					e.AddAttribute ("Clipboard", _clipboard.ToString ());
			}
			return e;
		}

		public override IPermission Union (IPermission target)
		{
			UIPermission uip = Cast (target);
			if (uip == null)
				return Copy ();

			// there are not [Flags] so we can't use boolean operators
			UIPermissionWindow w = ((_window > uip._window) ? _window : uip._window);
			UIPermissionClipboard c = ((_clipboard > uip._clipboard) ? _clipboard : uip._clipboard);

			if (IsEmpty (w, c))
				return null;

			return new UIPermission (w, c);
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.UI;
		}

		// helpers

		private bool IsEmpty (UIPermissionWindow w, UIPermissionClipboard c)
		{
			return ((w == UIPermissionWindow.NoWindows) && (c == UIPermissionClipboard.NoClipboard));
		}

		private UIPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			UIPermission uip = (target as UIPermission);
			if (uip == null) {
				ThrowInvalidPermission (target, typeof (UIPermission));
			}

			return uip;
		}
	}
}
