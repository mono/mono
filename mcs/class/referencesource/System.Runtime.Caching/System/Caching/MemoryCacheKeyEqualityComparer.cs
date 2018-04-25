// <copyright file="MemoryCacheEqualityComparer.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Collections;

namespace System.Runtime.Caching {
    internal class MemoryCacheEqualityComparer: IEqualityComparer {

        bool IEqualityComparer.Equals(Object x, Object y) {
            Dbg.Assert(x != null && x is MemoryCacheKey);
            Dbg.Assert(y != null && y is MemoryCacheKey);

            MemoryCacheKey a, b;
            a = (MemoryCacheKey)x;
            b = (MemoryCacheKey)y;

            return (String.Compare(a.Key, b.Key, StringComparison.Ordinal) == 0);
        }

        int IEqualityComparer.GetHashCode(Object obj) {
            MemoryCacheKey cacheKey = (MemoryCacheKey) obj;
            return cacheKey.Hash;
        }
    }
}
