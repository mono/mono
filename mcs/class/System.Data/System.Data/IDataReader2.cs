//
// System.Data.IDataReader2.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data {
	public interface IDataReader2
	{
		#region Properties

		bool HasRows { get; }

		#endregion // Properties
	}
}

#endif
