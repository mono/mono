//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;

    /// <summary>
    /// This is the in-memory nonce-cache used for turnkey replay detection.
    /// The nonce cache is based on a hashtable implementation for fast lookups.
    /// The hashcode is computed based on the nonce byte array.
    /// The nonce cache periodically purges stale nonce entries.
    /// </summary>
    sealed class InMemoryNonceCache : NonceCache
    {
        NonceCacheImpl cacheImpl;

        public InMemoryNonceCache(TimeSpan cachingTime, int maxCachedNonces) 
        {
            this.CacheSize = maxCachedNonces;
            this.CachingTimeSpan = cachingTime;
            this.cacheImpl = new NonceCacheImpl(cachingTime, maxCachedNonces);
        }

        public override bool CheckNonce(byte[] nonce)
        {
            return this.cacheImpl.CheckNonce(nonce);
        }

        public override bool TryAddNonce(byte[] nonce)
        {
            return this.cacheImpl.TryAddNonce(nonce);
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            writer.WriteLine("NonceCache:");
            writer.WriteLine("   Caching Timespan: {0}", this.CachingTimeSpan);
            writer.WriteLine("   Capacity: {0}", this.CacheSize);
            return writer.ToString();
        }

        internal sealed class NonceCacheImpl : TimeBoundedCache
        {
            static NonceKeyComparer comparer = new NonceKeyComparer();
            static object dummyItem = new Object();
            // if there are less than lowWaterMark entries, no purging is done
            static int lowWaterMark = 50;
            // We created a key for the nonce using the first 4 bytes, and hence the minimum length of nonce
            // that can be added to the cache.
            static int minimumNonceLength = 4;
            TimeSpan cachingTimeSpan;

            public NonceCacheImpl(TimeSpan cachingTimeSpan, int maxCachedNonces)
                : base(lowWaterMark, maxCachedNonces, comparer, PurgingMode.AccessBasedPurge, TimeSpan.FromTicks(cachingTimeSpan.Ticks >> 2), false)
            {
                this.cachingTimeSpan = cachingTimeSpan;
            }

            public bool TryAddNonce(byte[] nonce)
            {
                if (nonce == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
                if (nonce.Length < minimumNonceLength)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.NonceLengthTooShort));
                DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, this.cachingTimeSpan);
                return base.TryAddItem(nonce, dummyItem, expirationTime, false);
            }

            public bool CheckNonce(byte[] nonce)
            {
                if (nonce == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
                if (nonce.Length < minimumNonceLength)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.NonceLengthTooShort));
                if (base.GetItem(nonce) != null)
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// This class provides the hash-code value for the key (nonce) of the nonce cache.
            /// The hash code is obtained from the nonce byte array  by making an int of
            /// the first 4 bytes
            /// </summary>
            internal sealed class NonceKeyComparer : IEqualityComparer, System.Collections.Generic.IEqualityComparer<byte[]>
            {
                public int GetHashCode(object o)
                {
                    return GetHashCode((byte[])o);
                }
                public int GetHashCode(byte[] o)
                {
                    byte[] nonce = (byte[])o;

                    return (((int)nonce[0]) | (((int)nonce[1]) << 8) | (((int)nonce[2]) << 16) | (((int)nonce[3]) << 24));
                }

                public int Compare(object x, object y)
                {
                    return Compare((byte[])x, (byte[])y);
                }
                
                public int Compare(byte[] x, byte[] y)
                {
                    if (Object.ReferenceEquals(x, y))
                        return 0;

                    if (x == null)
                        return -1;
                    else if (y == null)
                        return 1;

                    byte[] nonce1 = (byte[])x;
                    int length1 = nonce1.Length;
                    byte[] nonce2 = (byte[])y;
                    int length2 = nonce2.Length;

                    if (length1 == length2)
                    {
                        for (int i = 0; i < length1; ++i)
                        {
                            int diff = ((int)nonce1[i] - (int)nonce2[i]);

                            if (diff != 0)
                            {
                                return diff;
                            }
                        }

                        return 0;
                    }
                    else if (length1 > length2)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }

                public new bool Equals(object x, object y)
                {
                    return (Compare(x, y) == 0);
                }
                
                public bool Equals(byte[] x, byte[] y)
                {
                    return (Compare(x, y) == 0);
                }
            }
        }
    }
}
