//
// Mono.Data.PostgreSqlClient.PgSqlRowUpdatingEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.PostgreSqlClient
{
	public sealed class PgSqlRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		[MonoTODO]
		public PgSqlRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new PgSqlCommand Command {
			get {
				throw new NotImplementedException ();
			} 
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		~PgSqlRowUpdatingEventArgs() 
		{
			throw new NotImplementedException ();
		}
	}
}
