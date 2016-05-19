//------------------------------------------------------------------------------
// <copyright file="AuthenticationModuleElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class AuthenticationModuleElement : ConfigurationElement
    {
        public AuthenticationModuleElement()
        {
            this.properties.Add(this.type);
        }

        public AuthenticationModuleElement(string typeName) : this()
        {
            if (typeName != (string)this.type.DefaultValue)
            {
                this.Type = typeName;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Type, IsRequired=true, IsKey = true)]
        public string Type
        {
            get { return (string)this[this.type]; }
            set { this[this.type] = value; }
        }

        internal string Key
        {
            get { return this.Type; }
        }
        
        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty type = 
            new ConfigurationProperty(ConfigurationStrings.Type, 
                                      typeof(string), 
                                      null, 
                                      ConfigurationPropertyOptions.IsKey);
    }
}

