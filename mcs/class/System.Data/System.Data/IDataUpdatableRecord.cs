//
// System.Data.IDataUpdatableRecord.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface IDataUpdatableRecord
	{
		#region Properties

		bool Updatable { get; }

		#endregion // Properties

		#region Methods

		int SetValues (object[] values);

		#endregion // Methods
	}
}

#endif // NET_1_2
