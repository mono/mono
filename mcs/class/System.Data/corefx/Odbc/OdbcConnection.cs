// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc
{
	partial class OdbcConnection
	{
		[MonoTODO]
		public void EnlistDistributedTransaction(System.EnterpriseServices.ITransaction transaction) => throw new NotImplementedException();
		
		[MonoTODO]
		public override void EnlistTransaction(System.Transactions.Transaction transaction) => throw new NotImplementedException();
		
		[MonoTODO]
		public override System.Data.DataTable GetSchema() => throw new NotImplementedException();

		[MonoTODO]
		public override System.Data.DataTable GetSchema(string collectionName) => throw new NotImplementedException();

		[MonoTODO]
		public override System.Data.DataTable GetSchema(string collectionName, string[] restrictionValues) => throw new NotImplementedException();
	}
}