//
// System.Data.SqlClient.SqlRowUpdatedEventArgs.cs
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
	public sealed class SqlRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		#region Constructors

		public SqlRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
		}

		#endregion // Constructors

		#region Properties

		public new SqlCommand Command {
			get { return (SqlCommand) base.Command; }
		}

		#endregion // Properties
	}
}
