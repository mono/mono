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

    public sealed class WsiProfilesElement : ConfigurationElement
    {
        // These three constructors are used by the configuration system. 
        public WsiProfilesElement() : base()
        {
            this.properties.Add(this.name);
        }

        public WsiProfilesElement(WsiProfiles name) : this()
        {
            this.Name = name;
        }

        [ConfigurationProperty("name", IsKey = true, DefaultValue = WsiProfiles.None)]
        public WsiProfiles Name
        {
            get { return (WsiProfiles)base[this.name]; }
            set 
            { 
                if (!IsValidWsiProfilesValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base[this.name] = value;
            }
        }
        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }

        bool IsValidWsiProfilesValue(WsiProfiles value)
        {
            return Enum.IsDefined(typeof(WsiProfiles), value);
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(WsiProfiles), WsiProfiles.None, ConfigurationPropertyOptions.IsKey);
    }
}



