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

    public sealed partial class NetTcpSecurityElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = NetTcpSecurity.DefaultMode)]
        [ServiceModelEnumValidator(typeof(SecurityModeHelper))]
        public SecurityMode Mode
        {
            get { return (SecurityMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Transport)]
        public TcpTransportSecurityElement Transport
        {
            get { return (TcpTransportSecurityElement)base[ConfigurationStrings.Transport]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Message)]
        public MessageSecurityOverTcpElement Message
        {
            get { return (MessageSecurityOverTcpElement)base[ConfigurationStrings.Message]; }
        }

        internal void ApplyConfiguration(NetTcpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Transport.ApplyConfiguration(security.Transport);
            this.Message.ApplyConfiguration(security.Message);
        }

        internal void InitializeFrom(NetTcpSecurity security)
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
