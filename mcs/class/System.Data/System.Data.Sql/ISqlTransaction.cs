//
// System.Data.Sql.ISqlTransaction
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Sql {
	public interface ISqlTransaction : IDbTransaction, IDisposable
	{
		#region Properties

		ISqlConnection Connection { get; }

		#endregion // Properties

		#region Methods

		void Rollback (string transactionName);
		void Save (string savePoint);

		#endregion // Methods
	}
}

#endif
