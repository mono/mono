//
// Mono.Data.TdsClient.TdsRowUpdatedEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.TdsClient {
	public sealed class TdsRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		[MonoTODO]
		public TdsRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new TdsCommand Command {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		~TdsRowUpdatedEventArgs () 
		{
			throw new NotImplementedException ();
		}

	}
}
