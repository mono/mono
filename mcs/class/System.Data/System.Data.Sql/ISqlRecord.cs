//
// System.Data.Sql.ISqlRecord
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Sql {
	public interface ISqlRecord : ISqlGetTypedData, IGetTypedData
	{
		#region Properties

		int FieldCount { get; }
		object this [int reader] { get; }
		object this [string writer] { get; }

		#endregion // Properties

		#region Methods

		string GetDataTypeName (int i);
		Type GetFieldType (int i);
		string GetName (int i);
		int GetOrdinal (string name);
		int GetSqlValues (object[] values);
		int GetValues (object[] values);

		#endregion // Methods
	}
}

#endif
