// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;

namespace Microsoft.SqlServer.Server
{
	partial class SqlMetaData
	{
		public SqlMetaData (string name, SqlDbType dbType, Type userDefinedType) :
			this (name, dbType, -1, 0, 0, 0, System.Data.SqlTypes.SqlCompareOptions.None, userDefinedType) { }


		[MonoTODO]
		public System.Data.DbType DbType {
			get => throw new NotImplementedException();
		}
	}
}
