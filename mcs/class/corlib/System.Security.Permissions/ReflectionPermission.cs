//
// System.Security.Permissions.ReflectionPermission.cs
//
// Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) 2002, Tim Coleman
//

using System;

namespace System.Security.Permissions
{
	[Serializable]
	public sealed class ReflectionPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		#region Fields

		ReflectionPermissionFlag flags;
		PermissionState state;

		#endregion // Fields

		#region Constructors

		public ReflectionPermission (PermissionState state)
		{
			throw new NotImplementedException ();
		}

		public ReflectionPermission (ReflectionPermissionFlag flag)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public ReflectionPermissionFlag Flags {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

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
		public override bool IsUnrestricted ()
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
