//
// System.Xml.Query.SqlQueryOptions
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Xml.Query {
        public class SqlQueryOptions
        {
		#region Fields

		SqlQueryPlan sqlQueryPlan;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SqlQueryOptions ()
		{
		}

		#endregion // Constructors

		#region Properties

		public SqlQueryPlan SqlQueryPlan {
			get { return sqlQueryPlan; }
			set { sqlQueryPlan = value; }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
