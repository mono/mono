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

    public sealed class DiagnosticsElement : ConfigurationElement {
        public DiagnosticsElement() : base() {
            this.properties.Add(this.suppressReturningExceptions);
        }

        [ConfigurationProperty("suppressReturningExceptions", DefaultValue = false)]
        public bool SuppressReturningExceptions {
            get { return (bool)base[suppressReturningExceptions]; }
            set { base[suppressReturningExceptions] = value; }
        }

        protected override ConfigurationPropertyCollection Properties {
            get { return this.properties; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        readonly ConfigurationProperty suppressReturningExceptions = new ConfigurationProperty("suppressReturningExceptions", typeof(bool), false);
    }
}
