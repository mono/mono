//
// System.Data.ProviderBase.DbConnectionPoolOptions
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Common;

namespace System.Data.ProviderBase {
	public sealed class DbConnectionPoolOptions
	{
		#region Fields

		bool poolByIdentity;
		int creationTimeout;
		bool hasTransactionAffinity;
		int loadBalanceTimeout;
		int maxPoolSize;
		int minPoolSize;
		bool useDeactivateQueue;

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]
		public DbConnectionPoolOptions (bool poolByIdentity, int minPoolSize, int maxPoolSize, int creationTimeout, int loadBalanceTimeout, bool hasTransactionAffinity, bool useDeactivateQueue)
		{
			this.poolByIdentity = poolByIdentity;
			this.minPoolSize = minPoolSize;
			this.maxPoolSize = maxPoolSize;
			this.creationTimeout = creationTimeout;
			this.loadBalanceTimeout = loadBalanceTimeout;
			this.hasTransactionAffinity = hasTransactionAffinity;
			this.useDeactivateQueue = useDeactivateQueue;
		}

		#endregion // Constructors

		#region Properties

		public int CreationTimeout {
			get { return creationTimeout; }
		}

		public bool HasTransactionAffinity {
			get { return hasTransactionAffinity; }
		}

		public int LoadBalanceTimeout {
			get { return loadBalanceTimeout; }
		}

		public int MaxPoolSize {
			get { return maxPoolSize; }
		}

		public int MinPoolSize {
			get { return minPoolSize; }
		}

		public bool PoolByIdentity {
			get { return poolByIdentity; }
		}

		public bool UseDeactivateQueue {
			get { return useDeactivateQueue; }
		}

		[MonoTODO]
		public bool UseLoadBalancing {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}

#endif
