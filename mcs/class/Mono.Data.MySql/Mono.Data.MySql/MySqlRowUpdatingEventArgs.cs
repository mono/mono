//
// Mono.Data.MySql.MySqlRowUpdatingEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.MySql
{
	public sealed class MySqlRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		[MonoTODO]
		public MySqlRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new MySqlCommand Command {
			get { throw new NotImplementedException (); } 
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		~MySqlRowUpdatingEventArgs() {
			throw new NotImplementedException ();
		}
	}
}
