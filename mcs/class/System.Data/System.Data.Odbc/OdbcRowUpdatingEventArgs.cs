//
// System.Data.Odbc.OdbcRowUpdatingEventArgs.cs
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
	public sealed class OdbcRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		#region Constructors

		public OdbcRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		#endregion // Constructors

		#region Properties

		public new OdbcCommand Command {
			get { return (OdbcCommand) base.Command; }
			set { base.Command = value; }
		}

		#endregion // Properties
	}
}
