//
// System.Data.Sql.ISqlUpdatableRecord
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data.Sql {
	public interface ISqlUpdatableRecord : ISqlRecord, ISqlGetTypedData, ISqlSetTypedData, ISetTypedData
	{
		#region Properties

		bool Updatable { get; }

		#endregion // Properties

		#region Methods

		int SetValues (object[] values);

		#endregion // Methods
	}
}

#endif
