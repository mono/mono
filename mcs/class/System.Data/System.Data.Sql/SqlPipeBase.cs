//
// System.Data.Sql.SqlPipeBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.SqlClient;

namespace System.Data.Sql {
	public abstract class SqlPipeBase
	{
		#region Constructors

		protected SqlPipeBase ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract bool SendingResults { get; }

		#endregion // Properties

		#region Methods

		public abstract void Execute (ISqlExecutionContext request);
		public abstract void Send (ISqlReader x);
		public abstract void Send (ISqlRecord x);
		public abstract void Send (SqlError x);
		public abstract void Send (string x);

		#endregion // Methods
	}
}

#endif
