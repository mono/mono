//------------------------------------------------------------------------------
// <copyright file="QueryCacheEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data.Common;
    using System.Diagnostics;

    /// <summary> 
    /// Represents the abstract base class for all cache entry values in the query cache 
    /// </summary> 
    internal class QueryCacheEntry
    {
        #region Fields
        /// <summary>
        /// querycachekey for this entry
        /// </summary>
        readonly private QueryCacheKey _queryCacheKey;

        /// <summary> 
        /// strong reference to the target object 
        /// </summary> 
        readonly protected object _target;
        #endregion

        #region Constructors
        /// <summary> 
        /// cache entry constructor 
        /// </summary> 
        /// <param name="queryCacheKey"></param> 
        /// <param name="target"></param> 
        internal QueryCacheEntry(QueryCacheKey queryCacheKey, object target)
        {
            _queryCacheKey = queryCacheKey;
            _target = target;
        }
        #endregion

        #region Methods and Properties
        /// <summary> 
        /// The payload of this cache entry.
        /// </summary> 
        internal virtual object GetTarget()
        {
            return _target;
        }

        /// <summary>
        /// Returns the query cache key
        /// </summary>
        internal QueryCacheKey QueryCacheKey
        {
            get { return _queryCacheKey; }
        }
        #endregion
    }
}
