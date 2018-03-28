//------------------------------------------------------------------------------
// <copyright file="NameValueConfigurationElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * ImmutableCollections
 *
 * Copyright (c) 2004 Microsoft Corporation
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Configuration;

//
// This file contains configuration collections that are used by multiple sections
//
namespace System.Configuration
{

    public sealed class NameValueConfigurationElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propValue =
            new ConfigurationProperty("value", typeof(string), String.Empty, ConfigurationPropertyOptions.None);

        static NameValueConfigurationElement()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propValue);
        }
        
        protected internal override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        //
        // Constructor
        //
        internal NameValueConfigurationElement()
        {
        }
        
        public NameValueConfigurationElement(string name, string value) 
        {
            base[_propName] = name;
            base[_propValue] = value;
        }

        //
        // Properties
        //

        //
        // ConfigurationPropertyOptions.IsKey="true"
        //
        [ConfigurationProperty("name", IsKey = true, DefaultValue = "")]
        public string Name
        {
            get
            {
                return (string)base[_propName];
            }
        }

        [ConfigurationProperty("value", DefaultValue = "")]
        public string Value
        {
            get
            {
                return (string)base[_propValue];
            }
            set
            {
                base[_propValue] = value;
            }
        }
    }
}
