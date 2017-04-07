//------------------------------------------------------------------------------
// <copyright file="CompiledQueryCacheKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Diagnostics;
    
    internal sealed class CompiledQueryCacheKey : QueryCacheKey
    {
        private readonly Guid _cacheIdentity;

        internal CompiledQueryCacheKey(Guid cacheIdentity)
        {
            _cacheIdentity = cacheIdentity;
        }

        /// <summary>
        /// Determines equality of this key with respect to <paramref name="compareTo"/>
        /// </summary>
        /// <param name="otherObject"></param>
        /// <returns></returns>
        public override bool Equals(object compareTo)
        {
            Debug.Assert(compareTo != null, "Comparison key should not be null");
            if (typeof(CompiledQueryCacheKey) != compareTo.GetType())
            {
                return false;
            }

            return ((CompiledQueryCacheKey)compareTo)._cacheIdentity.Equals(this._cacheIdentity);
        }

        /// <summary>
        /// Returns the hashcode for this cache key
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _cacheIdentity.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the state of this cache key
        /// </summary>
        /// <returns>
        /// A string representation that includes query text, parameter information, include path information
        /// and merge option information about this cache key.
        /// </returns>
        public override string ToString()
        {
            return _cacheIdentity.ToString();
        }
    }
}
