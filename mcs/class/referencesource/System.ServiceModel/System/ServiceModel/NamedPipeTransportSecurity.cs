//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Net;
    using System.Net.Security;
    using System.ComponentModel;

    public sealed class NamedPipeTransportSecurity
    {
        internal const ProtectionLevel DefaultProtectionLevel = ProtectionLevel.EncryptAndSign;
        ProtectionLevel protectionLevel;

        public NamedPipeTransportSecurity()
        {
            this.protectionLevel = DefaultProtectionLevel;
        }

        [DefaultValue(ConnectionOrientedTransportDefaults.ProtectionLevel)]
        public ProtectionLevel ProtectionLevel
        {
            get { return this.protectionLevel; }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
            }
        }

        internal WindowsStreamSecurityBindingElement CreateTransportProtectionAndAuthentication()
        {
            WindowsStreamSecurityBindingElement result = new WindowsStreamSecurityBindingElement();
            result.ProtectionLevel = this.protectionLevel;
            return result;
        }

        internal static bool IsTransportProtectionAndAuthentication(WindowsStreamSecurityBindingElement wssbe, NamedPipeTransportSecurity transportSecurity)
        {
            transportSecurity.protectionLevel = wssbe.ProtectionLevel;
            return true;
        }
    }
}
