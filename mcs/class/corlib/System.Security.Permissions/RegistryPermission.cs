//
// System.Security.Permissions.RegistryPermission.cs
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
	public sealed class RegistryPermission
		: CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private RegistryPermissionAccess _access;
		private string _pathList;

		// Constructors

		public RegistryPermission (PermissionState state)
		{
		}

		public RegistryPermission (RegistryPermissionAccess access, string pathList)
		{
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

		[MonoTODO]
		public bool IsUnrestricted () 
		{
			return false;
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
