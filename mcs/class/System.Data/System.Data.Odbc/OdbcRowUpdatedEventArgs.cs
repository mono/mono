//
// System.Data.Odbc.OdbcRowUpdatedEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc {
	public sealed class OdbcRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		#region Constructors

		public OdbcRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		#endregion // Constructors

		#region Properties

		public new OdbcCommand Command {
			get { return (OdbcCommand) base.Command; }
		}

		#endregion // Properties
	}
}
