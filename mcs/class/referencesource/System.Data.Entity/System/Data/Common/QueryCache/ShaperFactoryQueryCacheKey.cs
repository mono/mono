//------------------------------------------------------------------------------
// <copyright file="ShaperFactoryQueryCacheKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Data.Objects;
namespace System.Data.Common.QueryCache
{
    internal class ShaperFactoryQueryCacheKey<T> : QueryCacheKey
    {
        private readonly string _columnMapKey;
        private readonly MergeOption _mergeOption;
        private readonly bool _isValueLayer;

        internal ShaperFactoryQueryCacheKey(string columnMapKey, MergeOption mergeOption, bool isValueLayer)
        {
            Debug.Assert(null != columnMapKey, "null columnMapKey");
            _columnMapKey = columnMapKey;
            _mergeOption = mergeOption;
            _isValueLayer = isValueLayer;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ShaperFactoryQueryCacheKey<T>;
            if (null == other)
            {
                return false;
            }
            return this._columnMapKey.Equals(other._columnMapKey, _stringComparison)
                && this._mergeOption == other._mergeOption
                && this._isValueLayer == other._isValueLayer;
        }

        public override int GetHashCode()
        {
            return _columnMapKey.GetHashCode();
        }
    }
}
