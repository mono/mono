//
// System.Data.Odbc.OdbcPermission
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc
{
	[Serializable]
	public sealed class OdbcPermission : DBDataPermission
	{
		#region Constructors

		[MonoTODO]
#if NET_1_1
               [Obsolete ("use OdbcPermission(PermissionState.None)", true)]
#endif
		public OdbcPermission () : base (PermissionState.None)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OdbcPermission (PermissionState state)
			: base (state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
#if NET_1_1
		[Obsolete ("use OdbcPermission(PermissionState.None)", true)]
#endif
		public OdbcPermission (PermissionState state, bool allowBlankPassword)
			: base (state, allowBlankPassword, true)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		internal string Provider {
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
		public override void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			throw new NotImplementedException ();
		}
		

		#endregion
	}
}
