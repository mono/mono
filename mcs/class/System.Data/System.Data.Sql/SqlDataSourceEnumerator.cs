//
// System.Data.Sql.SqlDataSourceEnumerator
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.Common;

namespace System.Data.Sql {
	public sealed class SqlDataSourceEnumerator : DbDataSourceEnumerator
	{
		#region Properties

		[MonoTODO]
		public static SqlDataSourceEnumerator Instance { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override DataTable GetDataSources ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
