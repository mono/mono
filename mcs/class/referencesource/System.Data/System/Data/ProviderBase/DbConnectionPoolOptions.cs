//------------------------------------------------------------------------------
// <copyright file="DbConnectionPoolGroupOptions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.ProviderBase {

    using System;
    using System.Diagnostics;

    internal sealed class DbConnectionPoolGroupOptions {

        private readonly bool       _poolByIdentity;
        private readonly int        _minPoolSize;
        private readonly int        _maxPoolSize;
        private readonly int        _creationTimeout;
        private readonly TimeSpan   _loadBalanceTimeout;
        private readonly bool       _hasTransactionAffinity;
        private readonly bool       _useLoadBalancing;

        public DbConnectionPoolGroupOptions(
                                        bool poolByIdentity,
                                        int minPoolSize,
                                        int maxPoolSize,
                                        int creationTimeout,
                                        int loadBalanceTimeout,
                                        bool hasTransactionAffinity) {
            _poolByIdentity = poolByIdentity;
            _minPoolSize = minPoolSize;
            _maxPoolSize = maxPoolSize;
            _creationTimeout = creationTimeout;

            if (0 != loadBalanceTimeout) {
                _loadBalanceTimeout = new TimeSpan(0, 0, loadBalanceTimeout);
                _useLoadBalancing = true;
            }

            _hasTransactionAffinity = hasTransactionAffinity;
        }

        public int CreationTimeout {
            get { return _creationTimeout; }
        }
        public bool HasTransactionAffinity { 
            get { return _hasTransactionAffinity; } 
        }
        public TimeSpan LoadBalanceTimeout { 
            get { return _loadBalanceTimeout; } 
        }
        public int MaxPoolSize {
            get { return _maxPoolSize; } 
        }
        public int MinPoolSize { 
            get { return _minPoolSize; } 
        }
        public bool PoolByIdentity { 
            get { return _poolByIdentity; } 
        }
        public bool UseLoadBalancing { 
            get { return _useLoadBalancing; } 
        }
    }
}
 

