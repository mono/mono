//
// System.Data.ProviderBase.DbConnectionPoolOptions
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

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
