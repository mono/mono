//
// System.Data.SqlClient.SqlRowUpdatingEventArgs.cs
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

namespace System.Data.SqlClient {
	public sealed class SqlRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		#region Fields

		SqlCommand command;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SqlRowUpdatingEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			if (!(command is SqlCommand))
				throw new InvalidCastException ("Command is not a SqlCommand object.");
			this.command = (SqlCommand) command;
		}

		#endregion // Constructors

		#region Properties

		public new SqlCommand Command {
			get { return command; }
			set { command = value; }
		}

		#endregion // Properties
	}
}
