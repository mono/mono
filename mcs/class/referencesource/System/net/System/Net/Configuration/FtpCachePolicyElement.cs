//------------------------------------------------------------------------------
// <copyright file="FtpCachePolicyElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Cache;
    using System.Xml;
    using System.Security.Permissions;

    public sealed class FtpCachePolicyElement : ConfigurationElement
    {
        public FtpCachePolicyElement()
        {
            this.properties.Add(this.policyLevel);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.PolicyLevel, DefaultValue = RequestCacheLevel.Default)]
        public RequestCacheLevel PolicyLevel
        {
            get { return (RequestCacheLevel)this[this.policyLevel]; }
            set { this[this.policyLevel] = value; }
        }


        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            wasReadFromConfig = true;
            base.DeserializeElement(reader, serializeCollectionKey);
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            if (parentElement != null)
            {
                FtpCachePolicyElement http = (FtpCachePolicyElement)parentElement;
                this.wasReadFromConfig = http.wasReadFromConfig;
            }
            base.Reset(parentElement);
        }

        internal bool WasReadFromConfig
        {
            get { return this.wasReadFromConfig; }
        }

        bool wasReadFromConfig = false;
        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty policyLevel =
            new ConfigurationProperty(ConfigurationStrings.PolicyLevel, typeof(RequestCacheLevel), RequestCacheLevel.Default, ConfigurationPropertyOptions.None);

    }

}

