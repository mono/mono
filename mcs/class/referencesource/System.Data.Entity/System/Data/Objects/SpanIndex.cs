//---------------------------------------------------------------------
// <copyright file="SpanIndex.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupowner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// An index containing information about how the query was spanned
    /// This helps to determine how to materialize the query result
    /// </summary>
    internal sealed class SpanIndex
    {
        #region Nested types

        /// <summary>
        /// Helper class to compare two RowTypes using EdmEquals instead of reference equality.
        /// </summary>
        sealed private class RowTypeEqualityComparer : IEqualityComparer<RowType>
        {
            private RowTypeEqualityComparer() { }
            internal static readonly RowTypeEqualityComparer Instance = new RowTypeEqualityComparer();

            #region IEqualityComparer<RowType> Members

            public bool Equals(RowType x, RowType y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.EdmEquals(y);
            }

            public int GetHashCode(RowType obj)
            {
                return obj.Identity.GetHashCode();
            }

            #endregion
        }
        #endregion

        // When a query is spanned, the result is always a RowType
        // The _spanMap index maps RowTypes that are a span result to a map between
        // column ordinal and end member metadata of the type that is spanned
        private Dictionary<RowType, Dictionary<int, AssociationEndMember>> _spanMap;

        // A map from a spanned RowType (or parent RowType) to the original TypeUsage prior
        // to the query being rewritten
        private Dictionary<RowType, TypeUsage> _rowMap;

        internal SpanIndex()
        {
        }

        internal void AddSpannedRowType(RowType spannedRowType, TypeUsage originalRowType)
        {
            Debug.Assert(spannedRowType != null, "Spanned RowType cannot be null");
            Debug.Assert(originalRowType != null, "Original RowType cannot be null");
            Debug.Assert(originalRowType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType, "Original RowType must be a RowType");

            if (null == _rowMap)
            {
                _rowMap = new Dictionary<RowType, TypeUsage>(RowTypeEqualityComparer.Instance);
            }

            _rowMap[spannedRowType] = originalRowType;
        }

        internal TypeUsage GetSpannedRowType(RowType spannedRowType)
        {
            TypeUsage retType;
            if (_rowMap != null && _rowMap.TryGetValue(spannedRowType, out retType))
            {
                return retType;
            }
            return null;
        }

        internal bool HasSpanMap(RowType spanRowType)
        {
            Debug.Assert(spanRowType != null, "Span RowType cannot be null");
            if (null == _spanMap)
            {
                return false;
            }

            return _spanMap.ContainsKey(spanRowType);
        }

        internal void AddSpanMap(RowType rowType, Dictionary<int, AssociationEndMember> columnMap)
        {
            Debug.Assert(rowType != null, "Span row type cannot be null");
            Debug.Assert(columnMap != null, "Span column map cannot be null");

            if (null == _spanMap)
            {
                _spanMap = new Dictionary<RowType, Dictionary<int, AssociationEndMember>>(RowTypeEqualityComparer.Instance);
            }

            _spanMap[rowType] = columnMap;
        }

        internal Dictionary<int, AssociationEndMember> GetSpanMap(RowType rowType)
        {
            Dictionary<int, AssociationEndMember> retMap = null;
            if (_spanMap != null && _spanMap.TryGetValue(rowType, out retMap))
            {
                return retMap;
            }

            return null;
        }
    }
}
