//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class PeerSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = PeerSecuritySettings.DefaultMode)]
        [ServiceModelEnumValidator(typeof(SecurityModeHelper))]
        public SecurityMode Mode
        {
            get { return (SecurityMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Transport)]
        public PeerTransportSecurityElement Transport
        {
            get { return (PeerTransportSecurityElement)base[ConfigurationStrings.Transport]; }
        }

        internal void ApplyConfiguration(PeerSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            if (security.Mode != SecurityMode.None)
            {   
                this.Transport.ApplyConfiguration(security.Transport);
            }
        }

        internal void InitializeFrom(PeerSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Mode, security.Mode);
            if (security.Mode != SecurityMode.None)
            {   
                this.Transport.InitializeFrom(security.Transport);
            }
        }
        
        internal void CopyFrom(PeerSecurityElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = source.Mode;
            if (source.Mode != SecurityMode.None)
            {   
                this.Transport.CopyFrom(source.Transport);
            }
        }
    }
}

