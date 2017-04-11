//------------------------------------------------------------------------------
// <copyright file="QueryCacheKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    /// <summary>
    /// represents an abstract cache key
    /// </summary>
    internal abstract class QueryCacheKey
    {
        #region Constants
        protected const int EstimatedParameterStringSize = 20;
        #endregion

        #region Fields
        /// <summary>
        /// entry hit counter
        /// </summary>
        private uint _hitCount;

        /// <summary>
        /// aging index
        /// </summary>
        private int _agingIndex;

        /// <summary>
        /// default string comparison kind - Ordinal
        /// </summary>
        protected static StringComparison _stringComparison = StringComparison.Ordinal;
        #endregion

        #region Constructor
        protected QueryCacheKey()
        {
            _hitCount = 1;
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Determines whether two instances of QueryCacheContext are equal. 
        /// Equality is value based.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract override bool Equals( object obj );

        /// <summary>
        /// Returns QueryCacheContext instance HashCode
        /// </summary>
        /// <returns></returns>
        public abstract override int GetHashCode();
        #endregion

        #region Internal API
        /// <summary>
        /// Cache entry hit count
        /// </summary>
        internal uint HitCount
        {
            get 
            { 
                return _hitCount; 
            }
            
            set 
            { 
                _hitCount = value; 
            }
        }

        /// <summary>
        /// Gets/Sets Aging index for cache entry
        /// </summary>
        internal int AgingIndex
        {
            get 
            { 
                return _agingIndex; 
            }
            
            set 
            { 
                _agingIndex = value; 
            }
        }

        /// <summary>
        /// Updates hit count
        /// </summary>
        internal void UpdateHit()
        {
            if (uint.MaxValue != _hitCount)
            {
                unchecked { _hitCount++; }
            }
        }

        /// <summary>
        /// default string comparer
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        protected virtual bool Equals( string s, string t )
        {
            return String.Equals(s, t, _stringComparison);
        }
        #endregion
    }
}
