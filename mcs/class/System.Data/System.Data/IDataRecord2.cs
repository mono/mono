//
// System.Data.IDataReader2.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface IDataRecord2
	{
		#region Properties

		int VisibleFieldCount { get; }

		#endregion // Properties

		#region Methods

		Type GetFieldProviderSpecificType (int i);
		object GetProviderSpecificValue (int i);
		int GetProviderSpecificValues (object[] values);

		#endregion // Methods
	}
}

#endif
