//
// System.Data.IDataSources.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface IDataSources
	{
		#region Properties

		int Count { get; }
		object this [string name] { get; }

		#endregion // Properties

		#region Methods

		void Add (string name, IDbConnection connection);
		void Add (string name, IDbTransaction transaction);
		void Clear ();
		bool Contains (string name);
		void Remove (string name);

		#endregion // Methods
	}
}

#endif // NET_1_2
