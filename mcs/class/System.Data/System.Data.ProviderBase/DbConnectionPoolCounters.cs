//
// System.Data.ProviderBase.DbConnectionPoolCounters
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbConnectionPoolCounters
	{
		#region Constructors
	
		[MonoTODO]
		protected DbConnectionPoolCounters ()
		{
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public virtual void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void InitCounters ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
