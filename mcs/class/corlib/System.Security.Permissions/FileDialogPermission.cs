//
// System.Security.Permissions.FileDialogPermission.cs
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

using System.Runtime.InteropServices;

namespace System.Security.Permissions {

	[ComVisible (true)]
	[Serializable]
	public sealed class FileDialogPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		private FileDialogPermissionAccess _access;

		// Constructors

		public FileDialogPermission (PermissionState state)
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted)
				_access = FileDialogPermissionAccess.OpenSave;
			else
				_access = FileDialogPermissionAccess.None;
		}

		public FileDialogPermission (FileDialogPermissionAccess access)
		{
			// reuse validation by the Flags property
			Access = access;
		}

		// Properties

		public FileDialogPermissionAccess Access { 
			get { return _access; }
			set {
				if (!Enum.IsDefined (typeof (FileDialogPermissionAccess), value)) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "FileDialogPermissionAccess");
				}
				_access = value;
			}
		}

		// Methods

		public override IPermission Copy () 
		{
			return new FileDialogPermission (_access);
		}

		public override void FromXml (SecurityElement esd) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd)) {
				_access = FileDialogPermissionAccess.OpenSave;
			}
			else {
				string a = esd.Attribute ("Access");
				if (a == null)
					_access = FileDialogPermissionAccess.None;
				else {
					_access = (FileDialogPermissionAccess) Enum.Parse (
						typeof (FileDialogPermissionAccess), a);
				}
			}
		}

		public override IPermission Intersect (IPermission target) 
		{
			FileDialogPermission fdp = Cast (target);
			if (fdp == null)
				return null;

			FileDialogPermissionAccess a = (_access & fdp._access);
			return ((a == FileDialogPermissionAccess.None) ? null : new FileDialogPermission (a));
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			FileDialogPermission fdp = Cast (target);
			if (fdp == null)
				return false;

			return ((_access & fdp._access) == _access);
		}

		public bool IsUnrestricted () 
		{
			return (_access == FileDialogPermissionAccess.OpenSave);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (1);
			switch (_access) {
				case FileDialogPermissionAccess.Open:
					se.AddAttribute ("Access", "Open");
					break;
				case FileDialogPermissionAccess.Save:
					se.AddAttribute ("Access", "Save");
					break;
				case FileDialogPermissionAccess.OpenSave:
					se.AddAttribute ("Unrestricted", "true");
					break;
			}
			return se;
		}

		public override IPermission Union (IPermission target)
		{
			FileDialogPermission fdp = Cast (target);
			if (fdp == null)
				return Copy ();

			if (IsUnrestricted () || fdp.IsUnrestricted ())
				return new FileDialogPermission (PermissionState.Unrestricted);

			return new FileDialogPermission (_access | fdp._access);
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.FileDialog;
		}

		// helpers

		private FileDialogPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			FileDialogPermission fdp = (target as FileDialogPermission);
			if (fdp == null) {
				ThrowInvalidPermission (target, typeof (FileDialogPermission));
			}

			return fdp;
		}
	}
}
