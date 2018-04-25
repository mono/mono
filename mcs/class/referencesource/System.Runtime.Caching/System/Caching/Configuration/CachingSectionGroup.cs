// <copyright file="CacheSectionGroup.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Configuration;

namespace System.Runtime.Caching.Configuration {
    public sealed class CachingSectionGroup : ConfigurationSectionGroup {
        public CachingSectionGroup() {
        }

        // public properties
        [ConfigurationProperty("memoryCache")]
        public MemoryCacheSection MemoryCaches {
            get {
                return (MemoryCacheSection)Sections["memoryCache"];
            }
        }
    }
}
