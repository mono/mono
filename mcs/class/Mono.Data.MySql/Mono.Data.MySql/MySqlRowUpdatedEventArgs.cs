//
// Mono.Data.MySql.MySqlRowUpdatedEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.MySql {
	public sealed class MySqlRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		[MonoTODO]
		public MySqlRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new MySqlCommand Command {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		~MySqlRowUpdatedEventArgs () 
		{
			throw new NotImplementedException ();
		}

	}
}
