//
// System.Security.Permissions.KeyContainerPermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

	[Serializable]
	[ComVisible (true)]
	public sealed class KeyContainerPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private KeyContainerPermissionAccessEntryCollection _accessEntries;
		private KeyContainerPermissionFlags _flags;

		private const int version = 1;

		// Constructors

		public KeyContainerPermission (PermissionState state) 
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted) {
				_flags = KeyContainerPermissionFlags.AllFlags;
			}
		}

		public KeyContainerPermission (KeyContainerPermissionFlags flags)
		{
			SetFlags (flags);
		}

		public KeyContainerPermission (KeyContainerPermissionFlags flags, KeyContainerPermissionAccessEntry[] accessList) 
		{
			SetFlags (flags);
			if (accessList != null) {
				_accessEntries = new KeyContainerPermissionAccessEntryCollection ();
				foreach (KeyContainerPermissionAccessEntry kcpae in accessList) {
					_accessEntries.Add (kcpae);
				}
			}
		}

		// Properties

		public KeyContainerPermissionAccessEntryCollection AccessEntries {
			get { return _accessEntries; }
		}

		public KeyContainerPermissionFlags Flags {
			get { return _flags; }
		}

		// Methods

		public override IPermission Copy () 
		{
			if (_accessEntries.Count == 0)
				return new KeyContainerPermission (_flags);

			KeyContainerPermissionAccessEntry[] list = new KeyContainerPermissionAccessEntry [_accessEntries.Count];
			_accessEntries.CopyTo (list, 0);
			return new KeyContainerPermission (_flags, list);
		}

		[MonoTODO ("(2.0) missing support for AccessEntries")]
		public override void FromXml (SecurityElement securityElement) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (securityElement, "securityElement", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (securityElement)) {
				_flags = KeyContainerPermissionFlags.AllFlags;
			}
			else {
				// ???
				_flags = (KeyContainerPermissionFlags) Enum.Parse (
					typeof (KeyContainerPermissionFlags), securityElement.Attribute ("Flags"));
			}
		}

		[MonoTODO ("(2.0)")]
		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO ("(2.0)")]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		public bool IsUnrestricted () 
		{
			return (_flags == KeyContainerPermissionFlags.AllFlags);
		}

		[MonoTODO ("(2.0) missing support for AccessEntries")]
		public override SecurityElement ToXml () 
		{
			SecurityElement e = Element (version);
			if (IsUnrestricted ()) {
				e.AddAttribute ("Unrestricted", "true");
			} else {
				// ...
			}
			return e;
		}

		public override IPermission Union (IPermission target)
		{
			KeyContainerPermission kcp = Cast (target);
			if (kcp == null)
				return Copy ();

			KeyContainerPermissionAccessEntryCollection kcpaec = new KeyContainerPermissionAccessEntryCollection ();
			// copy first group
			foreach (KeyContainerPermissionAccessEntry kcpae in _accessEntries) {
				kcpaec.Add (kcpae);
			}
			// copy second group...
			foreach (KeyContainerPermissionAccessEntry kcpae in kcp._accessEntries) {
				// ... but only if not present in first group
				if (_accessEntries.IndexOf (kcpae) == -1)
					kcpaec.Add (kcpae);
			}

			if (kcpaec.Count == 0)
				return new KeyContainerPermission ((_flags | kcp._flags));

			KeyContainerPermissionAccessEntry[] list = new KeyContainerPermissionAccessEntry [kcpaec.Count];
			kcpaec.CopyTo (list, 0);
			return new KeyContainerPermission ((_flags | kcp._flags), list);
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.KeyContainer;
		}

		// helpers

		private void SetFlags (KeyContainerPermissionFlags flags)
		{
			if ((flags & KeyContainerPermissionFlags.AllFlags) != 0) {
				string msg = String.Format (Locale.GetText ("Invalid enum {0}"), flags);
				throw new ArgumentException (msg, "KeyContainerPermissionFlags");
			}
			_flags = flags;
		}

		private KeyContainerPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			KeyContainerPermission kcp = (target as KeyContainerPermission);
			if (kcp == null) {
				ThrowInvalidPermission (target, typeof (KeyContainerPermission));
			}

			return kcp;
		}
	}
}

