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

		[MonoTODO]
		public SqlRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public new SqlCommand Command {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
