//------------------------------------------------------------------------------
// <copyright file="HttpCachePolicyElement.cs" company="Microsoft Corporation">
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

    public sealed class HttpCachePolicyElement : ConfigurationElement
    {
        public HttpCachePolicyElement()
        {
            this.properties.Add(this.maximumAge);
            this.properties.Add(this.maximumStale);
            this.properties.Add(this.minimumFresh);
            this.properties.Add(this.policyLevel);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaximumAge, DefaultValue = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan MaximumAge
        {
            get { return (TimeSpan)this[this.maximumAge]; }
            set { this[this.maximumAge] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaximumStale, DefaultValue = TimeSpanValidatorAttribute.TimeSpanMinValue)]
        public TimeSpan MaximumStale
        {
            get { return (TimeSpan)this[this.maximumStale]; }
            set { this[this.maximumStale] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MinimumFresh, DefaultValue = TimeSpanValidatorAttribute.TimeSpanMinValue)]
        public TimeSpan MinimumFresh
        {
            get { return (TimeSpan)this[this.minimumFresh]; }
            set { this[this.minimumFresh] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PolicyLevel, IsRequired = true, DefaultValue = (HttpRequestCacheLevel) HttpRequestCacheLevel.Default)]
        public HttpRequestCacheLevel PolicyLevel
        {
            get { return (HttpRequestCacheLevel)this[this.policyLevel]; }
            set { this[this.policyLevel] = value; }
        }


        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            wasReadFromConfig = true;
            base.DeserializeElement(reader, serializeCollectionKey);
        }

        protected override void Reset(ConfigurationElement parentElement )
        {
            if (parentElement != null)
            {
                HttpCachePolicyElement http = (HttpCachePolicyElement)parentElement;
                this.wasReadFromConfig = http.wasReadFromConfig;
            }
            base.Reset(parentElement );
        }

        internal bool WasReadFromConfig
        {
            get { return this.wasReadFromConfig; }
        }

        bool wasReadFromConfig = false;
        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty maximumAge =
            new ConfigurationProperty(ConfigurationStrings.MaximumAge, typeof(TimeSpan), TimeSpan.MaxValue,
                    ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty maximumStale =
            new ConfigurationProperty(ConfigurationStrings.MaximumStale, typeof(TimeSpan), TimeSpan.MinValue,
                    ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty minimumFresh =
            new ConfigurationProperty(ConfigurationStrings.MinimumFresh, typeof(TimeSpan), TimeSpan.MinValue,
                    ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty policyLevel =
            new ConfigurationProperty(ConfigurationStrings.PolicyLevel, typeof(HttpRequestCacheLevel), HttpRequestCacheLevel.Default,
                    ConfigurationPropertyOptions.None);

    }

}

