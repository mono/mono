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

    public sealed partial class PeerTransportSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.PeerTransportCredentialType, DefaultValue = PeerTransportSecuritySettings.DefaultCredentialType)]
        [ServiceModelEnumValidator(typeof(PeerTransportCredentialTypeHelper))]
        public PeerTransportCredentialType CredentialType
        {
            get { return (PeerTransportCredentialType)base[ConfigurationStrings.PeerTransportCredentialType]; }
            set { base[ConfigurationStrings.PeerTransportCredentialType] = value; }
        }

        internal void ApplyConfiguration(PeerTransportSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.CredentialType = this.CredentialType;
        }

        internal void InitializeFrom(PeerTransportSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.PeerTransportCredentialType, security.CredentialType);
        }
        internal void CopyFrom(PeerTransportSecurityElement security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.CredentialType = security.CredentialType;
        }
        
    }
}

