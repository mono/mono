//
// System.Data.Sql.ISqlReader
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Sql {
	public interface ISqlReader : IDisposable, ISqlRecord, ISqlGetTypedData, IGetTypedData
	{
		#region Properties

		int Depth { get; }
		bool HasRows { get; }
		bool IsClosed { get; }
		int RecordsAffected { get; }

		#endregion // Properties

		#region Methods

		void Close ();
		DataTable GetSchemaTable ();
		bool NextResult ();
		bool Read ();

		#endregion // Methods
	}
}

#endif
