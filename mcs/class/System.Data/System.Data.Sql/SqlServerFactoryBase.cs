//
// System.Data.Sql.SqlServerFactoryBase
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
	public abstract class SqlServerFactoryBase : DbProviderFactory
	{
		#region Constructors

		protected SqlServerFactoryBase (DbProviderSupportedClasses supportedClasses)
			: base (supportedClasses)
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract bool ProviderAvailable { get; }
		public abstract SqlContextBase SqlContext { get; }

		#endregion // Properties
	}
}

#endif
