//------------------------------------------------------------------------------
// <copyright file="LocalDB.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">antonam</owner>
//------------------------------------------------------------------------------


namespace System.Data
{
    using System.Configuration;
    using System.Collections;    

    internal sealed class LocalDBInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("version", IsRequired = true)]
        public string Version
        {
            get
            {
                return this["version"] as string;
            }
        }
    }

    internal sealed class LocalDBInstancesCollection : ConfigurationElementCollection
    {

        private class TrimOrdinalIgnoreCaseStringComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                string xStr = x as string;
                if (xStr != null)
                    x = xStr.Trim();

                string yStr = y as string;
                if (yStr != null)
                    y = yStr.Trim();

                return StringComparer.OrdinalIgnoreCase.Compare(x,y);
            }
        }

        static readonly TrimOrdinalIgnoreCaseStringComparer s_comparer = new TrimOrdinalIgnoreCaseStringComparer();

        internal LocalDBInstancesCollection()
            : base(s_comparer)
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalDBInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalDBInstanceElement)element).Name;
        }
        
    }

    internal sealed class LocalDBConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("localdbinstances", IsRequired = true)]
        public LocalDBInstancesCollection LocalDbInstances
        {
            get
            {
                return (LocalDBInstancesCollection)this["localdbinstances"] ?? new LocalDBInstancesCollection();
            }
        }
    }
}
