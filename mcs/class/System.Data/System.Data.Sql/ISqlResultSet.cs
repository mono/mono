//
// System.Data.Sql.ISqlResultSet
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Sql {
	public interface ISqlResultSet : ISqlReader, IDisposable, ISqlUpdatableRecord, ISqlRecord, ISqlGetTypedData, IGetTypedData
	{
		#region Properties

		bool Scrollable { get; }
		ResultSetSensitivity Sensitivity { get; }

		#endregion // Properties

		#region Methods

		ISqlUpdatableRecord CreateRecord ();
		void Delete ();
		void Insert (ISqlRecord record);
		bool ReadAbsolute (int position);
		bool ReadFirst ();
		bool ReadLast ();
		bool ReadPrevious ();
		bool ReadRelative (int position);
		void Update ();

		#endregion // Methods
	}
}

#endif
