//
// System.Security.Permissions.RegistryPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

//
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

using System;
using System.Globalization;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class RegistryPermission
		: CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private PermissionState _state;
		private RegistryPermissionAccess _access;
		private string _pathList;

		// Constructors

		public RegistryPermission (PermissionState state)
		{
			_state = state;
		}

		public RegistryPermission (RegistryPermissionAccess access, string pathList)
		{
			_state = PermissionState.None;
			AddPathList (access, pathList);
		}

		// Properties

		// Methods

		[MonoTODO]
		public void AddPathList (RegistryPermissionAccess access, string pathList) 
		{
		}

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
			if (esd == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (esd.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (esd.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

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
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");
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
			return 5;
		}
	}
}
