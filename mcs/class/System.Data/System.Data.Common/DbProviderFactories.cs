//
// System.Data.Common.DbProviderFactories.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.Common {
	public sealed class DbProviderFactories
	{
		#region Methods

		[MonoTODO]
		public static DbProviderFactory GetFactory (DataRow providerRow)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DbProviderFactory GetFactory (string providerInvariantName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTable GetFactoryClasses ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
