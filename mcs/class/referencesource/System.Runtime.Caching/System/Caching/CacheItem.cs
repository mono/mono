// <copyright file="CacheItem.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>

using System;

namespace System.Runtime.Caching {
    public class CacheItem {
        public string Key { get; set; }
        public object Value { get; set; }
        public string RegionName { get; set; }

        private CacheItem() { } // hide default constructor

        public CacheItem(string key) {
            Key = key;
        }

        public CacheItem(string key, object value) : this(key) {
            Value = value;
        }

        public CacheItem(string key, object value, string regionName) : this(key, value) {
            RegionName = regionName;
        }

    }

}



