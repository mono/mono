// <copyright file="MemoryCacheSection.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.Caching.Resources;

namespace System.Runtime.Caching.Configuration {

    /* 
       <system.runtime.caching>
         <memoryCaches>
           <namedCaches>
             <add name="Default" physicalMemoryPercentage="0" pollingInterval="00:02:00"/>
             <add name="Foo" physicalMemoryPercentage="0" pollingInterval="00:02:00"/>
             <add name="Bar" physicalMemoryPercentage="0" pollingInterval="00:02:00"/>
           </namedCaches>
	     </memoryCaches>
       </system.caching>

    */

    public sealed class MemoryCacheSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propNamedCaches;

        static MemoryCacheSection() {
            _propNamedCaches = new ConfigurationProperty("namedCaches",
                                            typeof(MemoryCacheSettingsCollection),
                                            null, // defaultValue
                                            ConfigurationPropertyOptions.None);

            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propNamedCaches);
        }

        public MemoryCacheSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("namedCaches")]
        public MemoryCacheSettingsCollection NamedCaches {
            get {
                return (MemoryCacheSettingsCollection)base[_propNamedCaches];
            }
        }
    }
}
