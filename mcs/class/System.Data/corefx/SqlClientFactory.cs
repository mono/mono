// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Data.Sql;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient
{
	partial class SqlClientFactory
	{
		public override bool CanCreateDataSourceEnumerator => true;

		public override DbDataSourceEnumerator CreateDataSourceEnumerator() => 
			SqlDataSourceEnumerator.Instance;

		public override CodeAccessPermission CreatePermission (PermissionState state) =>
			new SqlClientPermission(state);
	}
}