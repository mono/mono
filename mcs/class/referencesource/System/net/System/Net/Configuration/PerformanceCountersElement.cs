//------------------------------------------------------------------------------
// <copyright file="PerformanceCountersElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class PerformanceCountersElement : ConfigurationElement
    {
        public PerformanceCountersElement()
        {
            this.properties.Add(this.enabled);
        }
        
        [ConfigurationProperty(ConfigurationStrings.Enabled, DefaultValue=false)]
        public bool Enabled
        {
            get { return (bool) this[this.enabled]; }
            set { this[this.enabled] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get 
            {
                return this.properties;
            }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty enabled =
            new ConfigurationProperty(ConfigurationStrings.Enabled, typeof(bool), false,
                    ConfigurationPropertyOptions.None);
    }
}
