//------------------------------------------------------------------------------
// <copyright file="BypassElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class BypassElement : ConfigurationElement
    {
        public BypassElement()
        {
            this.properties.Add(this.address);
        }

        public BypassElement(string address) : this()
        {
            this.Address = address;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Address, IsRequired=true, IsKey = true)]
        public string Address
        {
            get { return (string)this[this.address]; }
            set { this[this.address] = value; }
        }

        internal string Key
        {
            get { return this.Address; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty address =
            new ConfigurationProperty(ConfigurationStrings.Address, typeof(string), null,
                    ConfigurationPropertyOptions.IsKey);

    }
}

