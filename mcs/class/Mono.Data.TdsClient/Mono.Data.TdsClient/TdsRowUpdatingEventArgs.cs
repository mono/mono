//
// Mono.Data.TdsClient.TdsRowUpdatingEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.TdsClient
{
	public sealed class TdsRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		[MonoTODO]
		public TdsRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new TdsCommand Command {
			get { throw new NotImplementedException (); } 
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		~TdsRowUpdatingEventArgs() {
			throw new NotImplementedException ();
		}
	}
}
