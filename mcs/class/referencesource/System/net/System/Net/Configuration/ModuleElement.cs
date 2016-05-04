//------------------------------------------------------------------------------
// <copyright file="ModuleElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class ModuleElement : ConfigurationElement
    {
        public ModuleElement()
        {
            this.properties.Add(this.type);
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Type)]
        public string Type
        {
            get { return (string)this[this.type]; }
            set { this[this.type] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty type = 
            new ConfigurationProperty(ConfigurationStrings.Type, 
                                      typeof(string), 
                                      null, 
                                      ConfigurationPropertyOptions.None);

    }
}

