//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class NonDualMessageSecurityOverHttpElement : MessageSecurityOverHttpElement
    {
        [ConfigurationProperty(ConfigurationStrings.EstablishSecurityContext, DefaultValue = NonDualMessageSecurityOverHttp.DefaultEstablishSecurityContext)]
        public bool EstablishSecurityContext
        {
            get { return (bool)base[ConfigurationStrings.EstablishSecurityContext]; }
            set { base[ConfigurationStrings.EstablishSecurityContext] = value; }
        }
        
        internal void ApplyConfiguration(NonDualMessageSecurityOverHttp security)
        {
            base.ApplyConfiguration(security);
            security.EstablishSecurityContext = this.EstablishSecurityContext;
        }

        internal void InitializeFrom(NonDualMessageSecurityOverHttp security)
        {
            base.InitializeFrom(security);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.EstablishSecurityContext, security.EstablishSecurityContext);
        }
    }
}
