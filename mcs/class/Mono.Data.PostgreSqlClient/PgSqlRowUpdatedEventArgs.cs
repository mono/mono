//
// Mono.Data.PostgreSqlClient.PgSqlRowUpdatedEventArgs.cs
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

namespace Mono.Data.PostgreSqlClient {
	public sealed class PgSqlRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		[MonoTODO]
		public PgSqlRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new PgSqlCommand Command {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		~PgSqlRowUpdatedEventArgs () 
		{
			throw new NotImplementedException ();
		}

	}
}
