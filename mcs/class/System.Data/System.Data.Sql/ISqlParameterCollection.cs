//
// System.Data.Sql.ISqlParameterCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.Sql {
	public interface ISqlParameterCollection : IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Properties

		ISqlParameter this [[Optional] int index] { get; set; }
		ISqlParameter this [[Optional] string name] { get; set; }

		#endregion // Properties

		#region Methods

		ISqlParameter Add (ISqlParameter objct);
		ISqlParameter Add (string name, SqlDbType tp);
		ISqlParameter Add (string name, SqlDbType tp, object value);
		ISqlParameter AddWithValue (string name, object value);
		void Remove (ISqlParameter objct);

		#endregion // Methods
	}
}

#endif
