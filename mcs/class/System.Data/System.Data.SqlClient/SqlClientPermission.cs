//
// System.Data.SqlClient.SqlClientPermission.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient {
	[Serializable]
	public sealed class SqlClientPermission : DBDataPermission 
	{
		#region Fields

		PermissionState state;

		#endregion // Fields

		#region Constructors

		public SqlClientPermission ()
			: this (PermissionState.None, false)
		{
		}

		public SqlClientPermission (PermissionState state) 
			: this (state, false)
		{
		}

		public SqlClientPermission (PermissionState state, bool allowBlankPassword) 
		{
			AllowBlankPassword = allowBlankPassword;
		}

		#endregion // Constructors

		#region Methods

		protected override DBDataPermission CreateInstance ()
		{
			return (DBDataPermission) new SqlClientPermission ();
		}

		#endregion // Methods
	}
}
