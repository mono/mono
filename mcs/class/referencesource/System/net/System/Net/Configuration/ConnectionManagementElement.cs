//------------------------------------------------------------------------------
// <copyright file="ConnectionManagementElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class ConnectionManagementElement : ConfigurationElement
    {
        public ConnectionManagementElement()
        {
            this.properties.Add(this.address);
            this.properties.Add(this.maxconnection);
        }

        public ConnectionManagementElement(string address, int maxConnection) : this()
        {
            this.Address = address;
            this.MaxConnection = maxConnection;
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

        [ConfigurationProperty(ConfigurationStrings.MaxConnection, IsRequired=true, DefaultValue=(int)1)]
        public int MaxConnection
        {
            get { return (int)this[this.maxconnection]; }
            set { this[this.maxconnection] = value; }
        }

        internal string Key
        {
            get { return this.Address; }
        }
        
        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty address =
            new ConfigurationProperty(ConfigurationStrings.Address, 
                                      typeof(string), 
                                      null, 
                                      ConfigurationPropertyOptions.IsKey);

        // CODE REVIEWER: Should the default value here be int.MaxInt, 2, or something else?
        readonly ConfigurationProperty maxconnection =
            new ConfigurationProperty(ConfigurationStrings.MaxConnection, 
                                      typeof(int), 
                                      1, 
                                      ConfigurationPropertyOptions.None);

    }
}

