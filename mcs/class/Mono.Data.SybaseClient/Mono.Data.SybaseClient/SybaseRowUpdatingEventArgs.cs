//
// Mono.Data.SybaseClient.SybaseRowUpdatingEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		#region Constructors

		[MonoTODO]
		public SybaseRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public new SybaseCommand Command {
			get { throw new NotImplementedException (); } 
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
