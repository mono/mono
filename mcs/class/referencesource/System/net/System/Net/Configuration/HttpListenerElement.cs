//------------------------------------------------------------------------------
// <copyright file="HttpWebRequestElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class HttpListenerElement : ConfigurationElement
    {
        internal const bool UnescapeRequestUrlDefaultValue = true;

        private static ConfigurationPropertyCollection properties;
        private static readonly ConfigurationProperty unescapeRequestUrl;
        private static readonly ConfigurationProperty timeouts;

        static HttpListenerElement()
        {
            unescapeRequestUrl = new ConfigurationProperty(ConfigurationStrings.UnescapeRequestUrl, typeof(bool),
                UnescapeRequestUrlDefaultValue, ConfigurationPropertyOptions.None);
            
            timeouts = new ConfigurationProperty(ConfigurationStrings.Timeouts, typeof(HttpListenerTimeoutsElement), null,
                ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();
            properties.Add(unescapeRequestUrl);
            properties.Add(timeouts);
        }

        [ConfigurationProperty(ConfigurationStrings.UnescapeRequestUrl, DefaultValue = UnescapeRequestUrlDefaultValue, 
            IsRequired = false)]
        public bool UnescapeRequestUrl
        {
            get { return (bool)this[unescapeRequestUrl]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Timeouts)]
        public HttpListenerTimeoutsElement Timeouts
        {
            get { return (HttpListenerTimeoutsElement)this[timeouts]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }
    }
}

