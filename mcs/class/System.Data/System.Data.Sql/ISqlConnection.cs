//
// System.Data.Sql.ISqlConnection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Sql {
	public interface ISqlConnection : IDbConnection, IDisposable
	{
		#region Properties

		string DataSource { get; }
		string ServerVersion { get; }

		#endregion // Properties

		#region Methods

		ISqlTransaction BeginTransaction ();
		ISqlTransaction BeginTransaction (IsolationLevel iso);
		ISqlTransaction BeginTransaction (string transactionName);
		ISqlTransaction BeginTransaction (IsolationLevel iso, string transactionName);

		#endregion // Methods
	}
}

#endif
