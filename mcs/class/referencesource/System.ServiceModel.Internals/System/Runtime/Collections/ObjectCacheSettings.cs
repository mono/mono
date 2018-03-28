//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Collections
{
    class ObjectCacheSettings
    {
        int cacheLimit;
        TimeSpan idleTimeout;
        TimeSpan leaseTimeout;
        int purgeFrequency;

        const int DefaultCacheLimit = 64;
        const int DefaultPurgeFrequency = 32; 
        static TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(2);
        static TimeSpan DefaultLeaseTimeout = TimeSpan.FromMinutes(5);

        public ObjectCacheSettings()
        {
            this.CacheLimit = DefaultCacheLimit;
            this.IdleTimeout = DefaultIdleTimeout;
            this.LeaseTimeout = DefaultLeaseTimeout;
            this.PurgeFrequency = DefaultPurgeFrequency;
        }

        ObjectCacheSettings(ObjectCacheSettings other)
        {
            this.CacheLimit = other.CacheLimit;
            this.IdleTimeout = other.IdleTimeout;
            this.LeaseTimeout = other.LeaseTimeout;
            this.PurgeFrequency = other.PurgeFrequency;
        }

        internal ObjectCacheSettings Clone()
        {
            return new ObjectCacheSettings(this);
        }

        public int CacheLimit
        {
            get
            {
                return this.cacheLimit;
            }
            set 
            {
                Fx.Assert(value >= 0, "caller should validate cache limit is non-negative");
                this.cacheLimit = value; 
            }
        }

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
            set 
            {
                Fx.Assert(value >= TimeSpan.Zero, "caller should validate cache limit is non-negative");
                this.idleTimeout = value; 
            }
        }

        public TimeSpan LeaseTimeout
        {
            get
            {
                return this.leaseTimeout;
            }
            set 
            {
                Fx.Assert(value >= TimeSpan.Zero, "caller should validate cache limit is non-negative");
                this.leaseTimeout = value; 
            }
        }

        public int PurgeFrequency
        {
            get
            {
                return this.purgeFrequency;
            }
            set
            {
                Fx.Assert(value >= 0, "caller should validate purge frequency is non-negative");
                this.purgeFrequency = value;
            }
        }

    }
}
