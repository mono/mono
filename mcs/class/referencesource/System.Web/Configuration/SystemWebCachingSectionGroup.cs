//------------------------------------------------------------------------------
// <copyright file="SystemWebCachingSectionGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Configuration;
    using System.Security.Permissions;

    public sealed class SystemWebCachingSectionGroup : ConfigurationSectionGroup {
        public SystemWebCachingSectionGroup() {
        }

        [ConfigurationProperty("cache")]
        public CacheSection Cache 
        {
            get 
            {
                return (CacheSection) Sections["cache"];
            }
        }

        [ConfigurationProperty("outputCache")]
        public OutputCacheSection OutputCache
        {
            get 
            {
                return (OutputCacheSection) Sections["outputCache"];
            }
        }

        [ConfigurationProperty("outputCacheSettings")]
        public OutputCacheSettingsSection OutputCacheSettings
        {
            get 
            {
                return (OutputCacheSettingsSection) Sections["outputCacheSettings"];
            }
        }

        [ConfigurationProperty("sqlCacheDependency")]
        public SqlCacheDependencySection SqlCacheDependency
        {
            get 
            {
                return (SqlCacheDependencySection) Sections["sqlCacheDependency"];
            }
        }

    }
}
