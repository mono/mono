//
// System.Security.Permissions.EnvironmentPermission.cs
//
// Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) 2002, Tim Coleman
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[Serializable]
	public sealed class EnvironmentPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		#region Fields

		EnvironmentPermissionAccess flags;
		PermissionState state;
		string pathList;

		#endregion // Fields

		#region Constructors

		public EnvironmentPermission (PermissionState state)
		{
			throw new NotImplementedException ();
		}

		public EnvironmentPermission (EnvironmentPermissionAccess flag, string pathList)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void AddPathList (EnvironmentPermissionAccess flag, string pathList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Copy ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml (SecurityElement esd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetPathList (EnvironmentPermissionAccess flag)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsUnrestricted ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetPathList (EnvironmentPermissionAccess flag, string pathList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission other)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
