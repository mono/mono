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

    public sealed partial class NetMsmqSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = NetMsmqSecurity.DefaultMode)]
        [ServiceModelEnumValidator(typeof(SecurityModeHelper))]
        public NetMsmqSecurityMode Mode
        {
            get { return (NetMsmqSecurityMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Transport)]
        public MsmqTransportSecurityElement Transport
        {
            get { return (MsmqTransportSecurityElement)base[ConfigurationStrings.Transport]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Message)]
        public MessageSecurityOverMsmqElement Message
        {
            get { return (MessageSecurityOverMsmqElement)base[ConfigurationStrings.Message]; }
        }

        internal void ApplyConfiguration(NetMsmqSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Transport.ApplyConfiguration(security.Transport);
            this.Message.ApplyConfiguration(security.Message);
        }

        internal void InitializeFrom(NetMsmqSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Mode, security.Mode);
            this.Transport.InitializeFrom(security.Transport);
            this.Message.InitializeFrom(security.Message);
        }
    }
}
