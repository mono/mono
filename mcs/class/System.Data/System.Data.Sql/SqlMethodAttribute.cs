//
// System.Data.Sql.SqlMethodAttribute
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.Sql {
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	[Serializable]
	public sealed class SqlMethodAttribute : SqlFunctionAttribute
	{
		#region Fields

		bool isMutator;
		bool onNullCall;

		#endregion // Fields

		#region Constructors

		public SqlMethodAttribute ()
			: base ()
		{
			isMutator = false;
			onNullCall = false;
		}

		#endregion // Constructors

		#region Properties

		public bool IsMutator {
			get { return isMutator; }
			set { isMutator = value; }
		}

		public bool OnNullCall {
			get { return onNullCall; }
			set { onNullCall = value; }
		}

		#endregion // Properties
	}
}

#endif
