//
// System.Data.SqlClient.SqlResultSet
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2
namespace System.Data.SqlClient {
	public sealed class SqlResultSet : MarshalByRefObject, IEnumerable, ISqlResultSet, ISqlReader, ISqlRecord, ISqlGetTypedData, IGetTypedData, ISqlUpdatableRecord, ISqlSetTypedData, IDataReader, IDisposable, IDataUpdatableRecord, IDataRecord, ISetTypedData
	{
	}
}

#endif
