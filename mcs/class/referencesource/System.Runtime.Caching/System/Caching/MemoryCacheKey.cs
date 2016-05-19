// <copyright file="MemoryCacheKey.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.Caching {
    internal class MemoryCacheKey {
        private String _key;
        private int _hash;
        protected byte _bits;

        internal int Hash { get { return _hash; } }
        internal String Key { get { return _key; } }

        internal MemoryCacheKey(String key) {       
            _key = key;
            _hash = key.GetHashCode();
        }           
    }
}
