//
// System.Security.Permissions.RegistryPermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System.Security.AccessControl;
#endif

namespace System.Security.Permissions {

	[Serializable]
	public sealed class RegistryPermission
		: CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		private PermissionState _state;
		private RegistryPermissionAccess _access;
		private string _pathList;
#if NET_2_0
		private AccessControlActions _control;
#endif
		// Constructors

		public RegistryPermission (PermissionState state)
		{
			_state = CheckPermissionState (state, true);
		}

		public RegistryPermission (RegistryPermissionAccess access, string pathList)
		{
			_state = PermissionState.None;
			AddPathList (access, pathList);
		}
#if NET_2_0
		public RegistryPermission (RegistryPermissionAccess access, AccessControlActions control, string pathList)
		{
			if (!Enum.IsDefined (typeof (AccessControlActions), control)) {
				string msg = String.Format (Locale.GetText ("Invalid enum {0}"), control);
				throw new ArgumentException (msg, "AccessControlActions");
			}
			_state = PermissionState.None;
			AddPathList (access, control, pathList);
		}
#endif
		// Properties

		// Methods

		[MonoTODO]
		public void AddPathList (RegistryPermissionAccess access, string pathList) 
		{
		}
#if NET_2_0
		[MonoTODO]
		public void AddPathList (RegistryPermissionAccess access, AccessControlActions control, string pathList) 
		{
		}
#endif
		[MonoTODO]
		public string GetPathList (RegistryPermissionAccess access)
		{
			switch (access) {
				case RegistryPermissionAccess.Create:
					break;
				case RegistryPermissionAccess.Read:
					break;
				case RegistryPermissionAccess.Write:
					break;
				default:
					throw new ArgumentException ("Invalid flag");
			}
			return null;
		}

		[MonoTODO]
		public void SetPathList (RegistryPermissionAccess access, string pathList)
		{
		}

		public override IPermission Copy () 
		{
			return new RegistryPermission (_access, _pathList);
		}

		public override void FromXml (SecurityElement esd) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			// This serialization format stinks
			foreach (object o in esd.Attributes.Keys) {
				string key = (string) o;

				// skip over well-known attributes
				if (key == "class" || key == "version")
					continue;

				try {
					// The key is the value of the enum
					_access = (RegistryPermissionAccess) Enum.Parse (
						typeof (RegistryPermissionAccess), key);

					// The value of that attribute is the path list
					_pathList = esd.Attributes [key] as string;
				} catch {

				}
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
			return (_state == PermissionState.Unrestricted);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = Element (version);
			e.AddAttribute (_access.ToString (), _pathList);
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
			return (int) BuiltInToken.Registry;
		}

		// helpers

		private RegistryPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			RegistryPermission rp = (target as RegistryPermission);
			if (rp == null) {
				ThrowInvalidPermission (target, typeof (RegistryPermission));
			}

			return rp;
		}
	}
}
