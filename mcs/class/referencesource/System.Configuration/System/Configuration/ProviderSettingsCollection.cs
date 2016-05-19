//------------------------------------------------------------------------------
// <copyright file="ProviderSettingsCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Globalization;

    [ConfigurationCollection(typeof(ProviderSettings))]
    public sealed class ProviderSettingsCollection : ConfigurationElementCollection 
    {
        static private ConfigurationPropertyCollection _properties;
         
        static ProviderSettingsCollection() 
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        public ProviderSettingsCollection() :
            base(StringComparer.OrdinalIgnoreCase)
        {
        }
         
        protected internal override ConfigurationPropertyCollection Properties 
        {
            get
            {
                return _properties;
            }
        }
         
        public void Add(ProviderSettings provider) 
        {
            if (provider != null)
            {
                provider.UpdatePropertyCollection();
                BaseAdd(provider);
            }
        }

        public void Remove(String name) 
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() 
        {
            return new ProviderSettings();
        }
        protected override Object GetElementKey(ConfigurationElement element) 
        {
            return ((ProviderSettings)element).Name;
        }

        public new ProviderSettings this[string key]
        {
            get
            {
                return (ProviderSettings)BaseGet(key);
            }
        }
        
        public ProviderSettings this[int index]
        {
            get
            {
                return (ProviderSettings)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                BaseAdd(index,value);
            }
        }
    }
}
