//
// System.Data.Sql.SqlFunctionAttribute
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.Sql {
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[Serializable]
	public class SqlFunctionAttribute : Attribute
	{
		#region Fields

		DataAccessKind dataAccess;
		bool isDeterministic;
		bool isPrecise;
		SystemDataAccessKind systemDataAccess;

		#endregion // Fields

		#region Constructors

		public SqlFunctionAttribute ()
		{
			dataAccess = DataAccessKind.None;
			isDeterministic = false;
			isPrecise = false;
			systemDataAccess = SystemDataAccessKind.None;
		}

		#endregion // Constructors

		#region Properties

		public DataAccessKind DataAccess {
			get { return dataAccess; }
			set { dataAccess = value; }
		}
		
		public bool IsDeterministic {
			get { return isDeterministic; }
			set { isDeterministic = value; }
		}

		public bool IsPrecise {
			get { return isPrecise; }
			set { isPrecise = value; }
		}

		public SystemDataAccessKind SystemDataAccess {
			get { return systemDataAccess; }
			set { systemDataAccess = value; }
		}

		#endregion // Properties
	}
}

#endif
