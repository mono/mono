//
// System.Data.SybaseClient.SybaseRowUpdatedEventArgs.cs
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
	public sealed class SybaseRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		#region Constructors

		[MonoTODO]
		public SybaseRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public new SybaseCommand Command 
		{
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
