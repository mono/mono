//
// System.Data.OleDb.OleDbPermission
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb
{
	[Serializable]
	public sealed class OleDbPermission : DBDataPermission
	{
		#region Constructors

		[MonoTODO]
		public OleDbPermission ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OleDbPermission (PermissionState state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OleDbPermission (PermissionState state, bool allowBlankPassword)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		public string Provider {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override IPermission Copy ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
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
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
