//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration
{
    using System;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    public sealed class ProtocolElement : ConfigurationElement
    {
        // These three constructors are used by the configuration system. 
        public ProtocolElement() : base()
        {
            this.properties.Add(this.name);
        }

        public ProtocolElement(WebServiceProtocols protocol) : this()
        {
            this.Name = protocol;
        }

        [ConfigurationProperty("name", IsKey = true, DefaultValue = WebServiceProtocols.Unknown)]
        public WebServiceProtocols Name
        {
            get { return (WebServiceProtocols)base[this.name]; }
            set 
            { 
                if (!IsValidProtocolsValue(value))
                {
                    value = WebServiceProtocols.Unknown;
                }
                base[this.name] = value;
            }
        }
        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }

        bool IsValidProtocolsValue(WebServiceProtocols value)
        {
            return Enum.IsDefined(typeof(WebServiceProtocols), value);
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(WebServiceProtocols), WebServiceProtocols.Unknown, ConfigurationPropertyOptions.IsKey);
    }
}



