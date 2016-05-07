//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {
    using System;
    using System.Configuration;
    using System.ComponentModel;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;
    using System.Xml;

    public sealed class SoapEnvelopeProcessingElement : ConfigurationElement {
        // These three constructors are used by the configuration system. 
        public SoapEnvelopeProcessingElement() : base() {
            this.properties.Add(this.readTimeout);
            this.properties.Add(this.strict);
        }

        public SoapEnvelopeProcessingElement(int readTimeout) : this() {
            this.ReadTimeout = readTimeout;
        }

        public SoapEnvelopeProcessingElement(int readTimeout, bool strict) : this() {
            this.ReadTimeout = readTimeout;
            this.IsStrict = strict;
        }

        [ConfigurationProperty("readTimeout", DefaultValue = int.MaxValue)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        public int ReadTimeout {
            get { return (int)base[this.readTimeout]; }
            set { base[this.readTimeout] = value; }
        }

        [ConfigurationProperty("strict", DefaultValue = false)]
        public bool IsStrict {
            get { return (bool)base[strict]; }
            set { base[strict] = value; }
        }

        protected override ConfigurationPropertyCollection Properties {
            get { return this.properties; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        readonly ConfigurationProperty readTimeout = new ConfigurationProperty("readTimeout", typeof(int), int.MaxValue, new InfiniteIntConverter(), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty strict = new ConfigurationProperty("strict", typeof(bool), false);
    }
}
