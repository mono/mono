//
// System.Data.Sql.ISqlParameter
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlTypes;

namespace System.Data.Sql {
	public interface ISqlParameter : IDbDataParameter, IDataParameter
	{
		#region Properties

		SqlCompareOptions CompareInfo { get; set; }
		string DatabaseName { get; set; }
		int LocaleId { get; set; }
		SqlMetaData MetaData { get; set; }
		int Offset { get; set; }
		string SchemaName { get; set; }
		SqlDbType SqlDbType { get; set; }
		object SqlValue { get; set; }

		#endregion // Properties
	}
}

#endif
